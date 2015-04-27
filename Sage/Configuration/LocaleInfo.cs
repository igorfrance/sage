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
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.Linq;
	using System.Xml;

	using Kelp;
	using Kelp.Extensions;

	using log4net;

	using XmlNamespaces = Sage.XmlNamespaces;

	/// <summary>
	/// Contains information about a language locale.
	/// </summary>
	public class LocaleInfo : IXmlConvertible
	{
		internal const string DefaultLocale = "en-us";
		internal const string DefaultLanguage = "en";

		private static readonly ILog log = LogManager.GetLogger(typeof(LocaleInfo).FullName);

		private CultureInfo culture;
		private string shortDateFormat;
		private string longDateFormat;

		/// <summary>
		/// Initializes a new instance of the <see cref="LocaleInfo"/> class.
		/// </summary>
		public LocaleInfo()
		{
			this.Name = DefaultLocale;
			this.Language = DefaultLanguage;
			culture = new CultureInfo(DefaultLocale);
			shortDateFormat = "d";
			longDateFormat = "D";
			this.DictionaryNames = new List<string> { "en" };
			this.ResourceNames = new List<string> { "en", "default" };
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LocaleInfo"/> class, using the specified <paramref name="infoElement"/>.
		/// </summary>
		/// <param name="infoElement">The <see cref="XmlElement"/> that contains the information about the locale.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="infoElement"/> is <c>null</c>.
		/// </exception>
		public LocaleInfo(XmlElement infoElement)
		{
			Contract.Requires<ArgumentNullException>(infoElement != null);

			this.Parse(infoElement);
		}

		/// <summary>
		/// Gets the name of this locale.
		/// </summary>
		public string Name { get; internal set; }

		/// <summary>
		/// Gets the language of this locale.
		/// </summary>
		public string Language { get; internal set; }

		/// <summary>
		/// Gets the <see cref="CultureInfo"/> associated with this locale.
		/// </summary>
		public CultureInfo Culture
		{
			get
			{
				return culture;
			}

			set
			{
				culture = value;
			}
		}

		/// <summary>
		/// Gets the list of fallback dictionary names.
		/// </summary>
		/// <remarks>
		/// A locale can have one or more other locales it can fall back to in case a language phrase is not available in
		/// that locale.
		/// </remarks>
		public List<string> DictionaryNames { get; internal set; }

		/// <summary>
		/// Gets the list of fallback resource names.
		/// </summary>
		/// <remarks>
		/// A locale can have one or more other locales it can fall back to in case a resource is not available in
		/// that locale.
		/// </remarks>
		public List<string> ResourceNames { get; internal set; }

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
			return date.ToString(longDateFormat, culture);
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

			return this.FormatAsLongDate(date);
		}

		/// <summary>
		/// Formats the specified <paramref name="date"/> as a short date string for this <see cref="LocaleInfo"/>.
		/// </summary>
		/// <param name="date">The date to convert to string.</param>
		/// <returns>The specified <paramref name="date"/> converted to a short date string as configured for this locale.</returns>
		public string FormatAsShortDate(DateTime date)
		{
			return date.ToString(shortDateFormat, culture);
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

			return this.FormatAsShortDate(date);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("{0} ({1}) ({2})", this.Name,
				string.Join(",", this.DictionaryNames),
				string.Join(",", this.ResourceNames));
		}

		/// <inheritdoc/>
		public void Parse(XmlElement element)
		{
			this.Name = element.GetAttribute("name");
			this.Language = element.GetAttribute("language");

			XmlElement formatElement = element.SelectSingleElement("p:format", XmlNamespaces.Manager);
			try
			{
				culture = new CultureInfo(formatElement.GetAttribute("culture"));
			}
			catch (ArgumentException)
			{
				culture = new CultureInfo(DefaultLocale);
			}

			var shortDate = formatElement.GetAttribute("shortDate");
			var longDate = formatElement.GetAttribute("longDate");

			shortDateFormat = string.IsNullOrEmpty(shortDate) ? "d" : shortDate;
			longDateFormat = string.IsNullOrEmpty(longDate) ? "D" : longDate;

			this.DictionaryNames = new List<string>(element.GetAttribute("dictionaryNames")
				.Replace(" ", string.Empty)
				.Split(',')
				.Distinct());

			this.ResourceNames = new List<string>(element.GetAttribute("resourceNames")
				.Replace(" ", string.Empty)
				.Split(',')
				.Distinct());
		}

		/// <inheritdoc/>
		public XmlElement ToXml(XmlDocument document)
		{
			const string Ns = XmlNamespaces.ProjectConfigurationNamespace;
			XmlElement result = document.CreateElement("locale", Ns);
			XmlElement formatElem = result.AppendElement(document.CreateElement("format", Ns));

			result.SetAttribute("name", this.Name);
			result.SetAttribute("language", this.Language);
			result.SetAttribute("dictionaryNames", string.Join(",", this.DictionaryNames));
			result.SetAttribute("resourceNames", string.Join(",", this.ResourceNames));

			formatElem.SetAttribute("culture", this.Culture.Name);
			formatElem.SetAttribute("shortDate", shortDateFormat);
			formatElem.SetAttribute("longDate", longDateFormat);

			return result;
		}
	}
}
