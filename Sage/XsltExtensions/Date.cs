/**
 * Copyright 2012 Igor France
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Sage.XsltExtensions
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Text.RegularExpressions;

	using Sage.Extensibility;

	/// <summary>
	/// Provides several date-related utility methods for use in XSLT.
	/// </summary>
	[XsltExtensionObject(XmlNamespaces.Extensions.Date)]
	[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter",
		Justification = "This is an XSLT extension class, these methods will not be used from C#.")]
	public class Date
	{
		private const string DefaultDateFormat = "dd-MM-yyyy";
		private const string DefaultTimeFormat = "hh:mm:ss";

		private static readonly Regex dateExpr = new Regex(
			@"(?'Day'\d?\d)-(?'Month'\d?\d)-(?'Year'\d\d\d\d)(?:(?:T| )(?'Hour'\d?\d):(?'Minute'\d?\d)(?::(?'Second'\d?\d))?)?");

		/// <summary>
		/// Parses the specified <paramref name="dateTime"/> as <see cref="DateTime"/> and returns only the date as string, 
		/// formatted using the default date format
		/// </summary>
		/// <param name="dateTime">The date-time string to parse.</param>
		/// <returns>The date part of the date, or empty string, if the specified <paramref name="dateTime"/> string could 
		/// not be parsed.</returns>
		public string date(string dateTime)
		{
			return date(dateTime, DefaultDateFormat);
		}

		/// <summary>
		/// Parses the specified <paramref name="dateTime"/> as <see cref="DateTime"/> and returns only the date as string,
		/// formatted using the specified <paramref name="dateFormat"/>.
		/// </summary>
		/// <param name="dateTime">The date-time string to parse.</param>
		/// <param name="dateFormat">The string that specifies how to format the resulting date string.</param>
		/// <returns>
		/// The date part of the date, or empty string, if the specified <paramref name="dateTime"/> string could
		/// not be parsed.
		/// </returns>
		public string date(string dateTime, string dateFormat)
		{
			DateTime? date = parse(dateTime);
			return date == null ? string.Empty : date.Value.ToString(dateFormat);
		}

		/// <summary>
		/// Parses the specified <paramref name="dateTime"/> as <see cref="DateTime"/> and returns only the time as string, 
		/// formatted using the default time format
		/// </summary>
		/// <param name="dateTime">The date-time string to parse.</param>
		/// <returns>The time part of the date, or empty string, if the specified <paramref name="dateTime"/> string could 
		/// not be parsed.</returns>
		public string time(string dateTime)
		{
			return time(dateTime, DefaultTimeFormat);
		}

		/// <summary>
		/// Parses the specified <paramref name="dateTime"/> as <see cref="DateTime"/> and returns only the date as string,
		/// formatted using the specified <paramref name="timeFormat"/>.
		/// </summary>
		/// <param name="dateTime">The date-time string to parse.</param>
		/// <param name="timeFormat">The string that specifies how to format the resulting time string.</param>
		/// <returns>
		/// The time part of the date, or empty string, if the specified <paramref name="dateTime"/> string could
		/// not be parsed.
		/// </returns>
		public string time(string dateTime, string timeFormat)
		{
			DateTime? date = parse(dateTime);
			return date == null ? string.Empty : date.Value.ToString(timeFormat);
		}

		/// <summary>
		/// Parses the specified <paramref name="dateTime"/> as <see cref="DateTime"/> and returns the year.
		/// </summary>
		/// <param name="dateTime">The date-time string to parse.</param>
		/// <returns>The year part of the date, or zero, if the specified <paramref name="dateTime"/> string could 
		/// not be parsed.</returns>
		public int year(string dateTime)
		{
			DateTime? date = parse(dateTime);
			return date == null ? 0 : date.Value.Year;
		}

		/// <summary>
		/// Parses the specified <paramref name="dateTime"/> as <see cref="DateTime"/> and returns the quarter.
		/// </summary>
		/// <param name="dateTime">The date-time string to parse.</param>
		/// <returns>The year quarter, or zero, if the specified <paramref name="dateTime"/> string could 
		/// not be parsed.</returns>
		public int quarter(string dateTime)
		{
			DateTime? date = parse(dateTime);
			return date == null ? 0 : 1 + (int) Math.Floor((decimal) date.Value.Month / 4);
		}

		/// <summary>
		/// Parses the specified <paramref name="dateTime"/> as <see cref="DateTime"/> and returns the month.
		/// </summary>
		/// <param name="dateTime">The date-time string to parse.</param>
		/// <returns>The month part of the date, or zero, if the specified <paramref name="dateTime"/> string could 
		/// not be parsed.</returns>
		public int month(string dateTime)
		{
			DateTime? date = parse(dateTime);
			return date == null ? 0 : date.Value.Month;
		}

		/// <summary>
		/// Parses the specified <paramref name="dateTime"/> as <see cref="DateTime"/> and returns the day.
		/// </summary>
		/// <param name="dateTime">The date-time string to parse.</param>
		/// <returns>The day part of the date, or zero, if the specified <paramref name="dateTime"/> string could 
		/// not be parsed.</returns>
		public int day(string dateTime)
		{
			DateTime? date = parse(dateTime);
			return date == null ? 0 : date.Value.Day;
		}

		/// <summary>
		/// Parses the specified <paramref name="dateTime"/> as <see cref="DateTime"/> and returns the hour.
		/// </summary>
		/// <param name="dateTime">The date-time string to parse.</param>
		/// <returns>The hour part of the date, or zero, if the specified <paramref name="dateTime"/> string could 
		/// not be parsed, or the <paramref name="dateTime"/> string doesn't specify the time.</returns>
		public int hour(string dateTime)
		{
			DateTime? date = parse(dateTime);
			return date == null ? 0 : date.Value.Hour;
		}

		/// <summary>
		/// Parses the specified <paramref name="dateTime"/> as <see cref="DateTime"/> and returns the minute.
		/// </summary>
		/// <param name="dateTime">The date-time string to parse.</param>
		/// <returns>The minute part of the date, or zero, if the specified <paramref name="dateTime"/> string could 
		/// not be parsed, or the <paramref name="dateTime"/> string doesn't specify the time.</returns>
		public int minute(string dateTime)
		{
			DateTime? date = parse(dateTime);
			return date == null ? 0 : date.Value.Minute;
		}

		/// <summary>
		/// Parses the specified <paramref name="dateTime"/> as <see cref="DateTime"/> and returns the second.
		/// </summary>
		/// <param name="dateTime">The date-time string to parse.</param>
		/// <returns>The second part of the date, or zero, if the specified <paramref name="dateTime"/> string could 
		/// not be parsed, or the <paramref name="dateTime"/> string doesn't specify the time.</returns>
		public int second(string dateTime)
		{
			DateTime? date = parse(dateTime);
			return date == null ? 0 : date.Value.Second;
		}

		/// <summary>
		/// Gets the nearest date on which the specified <paramref name="weekDay"/> occurs, starting from and including
		/// the date parsed from <paramref name="dateTime"/>, and formatted using the default date format.
		/// </summary>
		/// <param name="dateTime">The date-time string to parse.</param>
		/// <param name="weekDay">The week day.</param>
		/// <returns>
		/// The date part of the nearest date on which the specified <paramref name="weekDay"/> occurs, 
		/// starting from and including the date parsed from <paramref name="dateTime"/>, formatted using the default date 
		/// format, or empty string,  if the specified <paramref name="dateTime"/> 
		/// string could not be parsed.
		/// </returns>
		public string nearestWeekday(string dateTime, byte weekDay)
		{
			return nearestWeekday(dateTime, weekDay, DefaultDateFormat);
		}

		/// <summary>
		/// Gets the nearest date on which the specified <paramref name="weekDay"/> occurs, starting from and including
		/// the date parsed from <paramref name="dateTime"/>, and formatted using the specified <paramref name="dateFormat"/>.
		/// </summary>
		/// <param name="dateTime">The date-time string to parse.</param>
		/// <param name="weekDay">The week day.</param>
		/// <param name="dateFormat">The string that specifies how to format the resulting date string.</param>
		/// <returns>
		/// The date part of the nearest date on which the specified <paramref name="weekDay"/> occurs, 
		/// starting from and including the date parsed from <paramref name="dateTime"/>, formatted using the 
		/// specified <paramref name="dateFormat"/>, or empty string,  if the specified <paramref name="dateTime"/> 
		/// string could not be parsed.
		/// </returns>
		public string nearestWeekday(string dateTime, byte weekDay, string dateFormat)
		{
			DateTime? date = parse(dateTime);
			if (date == null)
				return string.Empty;

			while ((byte) date.Value.DayOfWeek != weekDay)
				date = date.Value.Subtract(TimeSpan.FromDays(1));

			return date.Value.ToString(dateFormat);
		}

		/// <summary>
		/// Returns the difference, in days, between <paramref name="dateTime1"/> and <paramref name="dateTime2"/>.
		/// </summary>
		/// <param name="dateTime1">The first date</param>
		/// <param name="dateTime2">The second date</param>
		/// <returns>The difference, in days, between <paramref name="dateTime1"/> and <paramref name="dateTime2"/>.</returns>
		public int difference(string dateTime1, string dateTime2)
		{
			return difference(dateTime1, dateTime2, "days");
		}

		/// <summary>
		/// Returns the difference, in the specfied <paramref name="unit"/>, between <paramref name="dateTime1"/> and
		/// <paramref name="dateTime2"/>.
		/// </summary>
		/// <param name="dateTime1">The first date</param>
		/// <param name="dateTime2">The second date</param>
		/// <param name="unit">The unit in which to return the difference. Valid values are 'days', 'weeks', 'months' 
		/// and 'years'.</param>
		/// <returns>
		/// The difference, in the specfied <paramref name="unit"/>, between <paramref name="dateTime1"/> and 
		/// <paramref name="dateTime2"/>.
		/// </returns>
		public int difference(string dateTime1, string dateTime2, string unit)
		{
			DateTime? date1 = parse(dateTime1);
			DateTime? date2 = parse(dateTime2);

			if (date1 == null || date2 == null)
				return -1;

			TimeSpan difference = date2.Value - date1.Value;
			switch (unit)
			{
				case "years":
					return (int) Math.Floor(difference.TotalDays / 365);

				case "months":
					return (int) Math.Floor(difference.TotalDays / 30);

				case "weeks":
					return (int) Math.Floor(difference.TotalDays / 7);

				default:
					return (int) difference.TotalDays;
			}
		}

		private static DateTime? parse(string dateTime)
		{
			Match match;
			DateTime? result = null;

			if ((match = dateExpr.Match(dateTime)).Success)
			{
				int year = int.Parse(match.Groups["Year"].Value);
				int month = int.Parse(match.Groups["Month"].Value);
				int day = int.Parse(match.Groups["Day"].Value);
				int hour = match.Groups["Hour"].Success ? int.Parse(match.Groups["Hour"].Value) : 0;
				int minute = match.Groups["Minute"].Success ? int.Parse(match.Groups["Minute"].Value) : 0;
				int second = match.Groups["Second"].Success ? int.Parse(match.Groups["Second"].Value) : 0;

				result = new DateTime(year, month, day, hour, minute, second);
			}

			return result;
		}
	}
}
