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
namespace Sage.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Xml;

	using log4net;

	/// <summary>
	/// Contains information about a language locale.
	/// </summary>
	public struct LocaleInfo
	{
		internal const string DefaultLocale = "en-us";

		private static readonly ILog log = LogManager.GetLogger(typeof(LocaleInfo).FullName);

		private readonly CultureInfo culture;
		private readonly string shortDateFormat;
		private readonly string longDateFormat;

		/// <summary>
		/// Initializes a new instance of the <see cref="LocaleInfo"/> struct, using the specified <paramref name="infoElement"/>.
		/// </summary>
		/// <param name="infoElement">The <see cref="XmlElement"/> that contains the information about the locale.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="infoElement"/> is <c>null</c>.
		/// </exception>
		public LocaleInfo(XmlElement infoElement)
			: this()
		{
			if (infoElement == null)
				throw new ArgumentNullException("infoElement");

			this.Name = infoElement.GetAttribute("name");

			try
			{
				this.culture = new CultureInfo(infoElement["format"].Attributes["culture"].InnerText);
			}
			catch (ArgumentException)
			{
				this.culture = new CultureInfo(DefaultLocale);
			}

			this.shortDateFormat = infoElement["format"].Attributes["shortDate"].InnerText;
			this.longDateFormat = infoElement["format"].Attributes["longDate"].InnerText;

			this.DictionaryNames = new List<string>(infoElement.GetAttribute("dictionaryNames")
				.Replace(" ", string.Empty)
				.Split(',')
				.Distinct());

			this.ResourceNames = new List<string>(infoElement.GetAttribute("resourceNames")
				.Replace(" ", string.Empty)
				.Split(',')
				.Distinct());
		}

		/// <summary>
		/// Gets the name of this locale.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the <see cref="CultureInfo"/> associated with this locale.
		/// </summary>
		public CultureInfo Culture
		{
			get
			{
				return culture;
			}
		}

		/// <summary>
		/// Gets the list of fallback dictionary names.
		/// </summary>
		/// <remarks>
		/// A locale can have one or more other locales it can fall back to in case a language phrase is not available in
		/// that locale.
		/// </remarks>
		public List<string> DictionaryNames { get; private set; }

		/// <summary>
		/// Gets the list of fallback resource names.
		/// </summary>
		/// <remarks>
		/// A locale can have one or more other locales it can fall back to in case a resource is not available in
		/// that locale.
		/// </remarks>
		public List<string> ResourceNames { get; private set; }

		/// <summary>
		/// Determines whether this locale is a subset of the specified <paramref name="locale"/>.
		/// </summary>
		/// <param name="locale">The locale to check against.</param>
		/// <returns>
		/// <c>true</c> if this locale is or is a subset of the specified <paramref name="locale"/>; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// If the specified <paramref name="locale"/> is <c>null</c> or empty.
		/// </exception>
		public bool IsSubsetOf(string locale)
		{
			if (string.IsNullOrEmpty(locale))
				throw new ArgumentNullException("locale");

			return this.Name == locale || this.ResourceNames.ToList().Contains(locale);
		}

		/// <summary>
		/// Formats a <paramref name="date"/> as a long date for this <see cref="LocaleInfo"/>.
		/// </summary>
		/// <param name="date">The date to convert to string.</param>
		/// <returns>The specified <paramref name="date"/> converted to a long date string as configured for this locale.</returns>
		public string FormatAsLongDate(DateTime date)
		{
			return date.ToString(this.longDateFormat, this.culture);
		}

		/// <summary>
		/// Formats the specified <paramref name="dateString"/> as a long date for this <see cref="LocaleInfo"/>.
		/// </summary>
		/// <param name="dateString">The string to format.</param>
		/// <returns>The specified <paramref name="dateString"/> converted to a date and formatted as a long date 
		/// string configured  for this locale.</returns>
		/// <exception cref="ArgumentNullException">
		/// If the specified <paramref name="dateString"/> is <c>null</c> or empty.
		/// </exception>
		public string FormatAsLongDate(string dateString)
		{
			if (string.IsNullOrEmpty(dateString))
				throw new ArgumentNullException("dateString");

			DateTime date;
			if (!DateTime.TryParse(dateString, out date))
			{
				log.ErrorFormat("Could not parse '{0}' to a date", dateString);
				return dateString;
			}

			return FormatAsLongDate(date);
		}

		/// <summary>
		/// Formats the specified <paramref name="date"/> as a short date string for this <see cref="LocaleInfo"/>.
		/// </summary>
		/// <param name="date">The date to convert to string.</param>
		/// <returns>The specified <paramref name="date"/> converted to a short date string as configured for this locale.</returns>
		public string FormatAsShortDate(DateTime date)
		{
			return date.ToString(this.shortDateFormat, this.culture);
		}

		/// <summary>
		/// Formats the specified <paramref name="dateString"/> to a short date for this <see cref="LocaleInfo"/>.
		/// </summary>
		/// <param name="dateString">The string to format.</param>
		/// <returns>The specified <paramref name="dateString"/> converted to a date and formatted as a short date 
		/// string configured  for this locale.</returns>
		/// <exception cref="ArgumentNullException">
		/// If the specified <paramref name="dateString"/> is <c>null</c> or empty.
		/// </exception>
		public string FormatAsShortDate(string dateString)
		{
			if (string.IsNullOrEmpty(dateString))
				throw new ArgumentNullException("dateString");

			DateTime date;
			if (!DateTime.TryParse(dateString, out date))
			{
				log.ErrorFormat("Could not parse '{0}' to a date", dateString);
				return dateString;
			}

			return FormatAsShortDate(date);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("{0} ({1}) ({2})", this.Name,
				string.Join(",", this.DictionaryNames),
				string.Join(",", this.ResourceNames));
		}
	}
}
