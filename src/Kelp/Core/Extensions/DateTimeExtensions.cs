﻿namespace Kelp.Core.Extensions
{
	using System;
	using System.Text.RegularExpressions;

	/// <summary>
	/// Provides <see cref="DateTime"/> extension methods.
	/// </summary>
	public static class DateTimeExtensions
	{
		private static readonly Regex offsetValue = new Regex(@"^(?'value'[+-]?(?:\d+|\d*\.\d*))(?'unit'[mhdMy]?)$");

		/// <summary>
		/// Offsets the date by the specified time.
		/// </summary>
		/// <remarks><para>
		/// The time to offset this <c>DateTime</c> with is expressed as a string written in a specific format. This
		/// is useful in situations where relative date values need to be expressed as strings, for instance in configuration
		/// files.
		/// </para>
		/// <para>The format of this string is: <c>([+/-]\d+)(mhdMy)</c>, where the first part is the amount 
		/// (positive or negative), and the second part is the unit ([m]inutes, [h]ours, [d]ays, [M]onths or [y]ears).</para>
		/// <para>For instance, to offset a given date by 2 hours (to set it 2 hours in the future), you would specify it with
		/// <c>"+2h"</c>: <code>DateTime d2 = d1.Offset("+2h");</code></para>
		/// <para>Similarly, to set a given date to a year earlier, you would specify it with <c>"-1y"</c>: 
		/// <code>DateTime d2 = d1.Offset("-1y");</code></para>
		/// </remarks>
		/// <param name="dt">The value to offset</param>
		/// <param name="offset">The string that specifies the time with which to offset the value.</param>
		/// <returns>A new date, offset from the specified date by the specified value.</returns>
		/// <exception cref="ArgumentNullException">If offset is <see langword="null"/> or empty.</exception>
		public static DateTime Offset(this DateTime dt, string offset)
		{
			if (string.IsNullOrEmpty(offset))
				throw new ArgumentNullException("offset");

			Match m;
			DateTime result = new DateTime(dt.Ticks);
			if ((m = offsetValue.Match(offset)).Success)
			{
				string unit = m.Groups["unit"].Value;
				long value = Int64.Parse(m.Groups["value"].Value);

				switch (unit)
				{
					case "s":
						result = dt.AddSeconds(value);
						break;
					case "m":
						result = dt.AddMinutes(value);
						break;
					case "h":
						result = dt.AddHours(value);
						break;
					case "d":
						result = dt.AddDays(value);
						break;
					case "M":
						result = dt.AddMonths((int) value);
						break;
					case "y":
						result = dt.AddYears((int) value);
						break;
				}
			}

			return result;
		}

		/// <summary>
		/// Determines whether instance <paramref name="dt"/> is equal to <paramref name="other"/>, comparing
		/// only the year, month and date, disregarding the time completely.
		/// </summary>
		/// <param name="dt">The current instance.</param>
		/// <param name="other">The date to compare to.</param>
		/// <returns>
		/// <c>true</c> if instance <paramref name="dt"/> is equal to <paramref name="other"/>; 
		///  otherwise, <c>false</c>.
		/// </returns>
		public static bool IsEqualByDateTo(this DateTime dt, DateTime other)
		{
			return dt.Year == other.Year && dt.Month == other.Month && dt.Date == other.Date;
		}
	}
}
