/**
 * Open Source Initiative OSI - The MIT License (MIT):Licensing
 * [OSI Approved License]
 * The MIT License (MIT)
 *
 * Copyright (c) 2011 Igor France
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */
namespace Sage.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml;

	/// <summary>
	/// Contains configuration information about a category (such as 'football' or 'outdoor').
	/// </summary>
	public class CategoryInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CategoryInfo"/> class.
		/// </summary>
		/// <param name="categoryName">The name of this category.</param>
		/// <param name="definedLocales">The locales defined for this caategory.</param>
		public CategoryInfo(string categoryName, Dictionary<string, LocaleInfo> definedLocales)
		{
			this.Name = categoryName;
			this.Locales = new List<string>();
			this.Suffixes = new List<string>();

			foreach (string name in definedLocales.Keys)
			{
				LocaleInfo locale = definedLocales[name];
				this.Locales.Add(name);
				foreach (string suffix in locale.ResourceNames.Where(suffix => !this.Suffixes.Contains(suffix)))
				{
					this.Suffixes.Add(suffix);
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CategoryInfo"/> class. 
		/// </summary>
		/// <param name="categoryElement">
		/// The <see cref="XmlElement"/> that contains information about this category.
		/// </param>
		/// <param name="definedLocales">
		/// The defined locales configured for the whole project.
		/// </param>
		/// <exception cref="ConfigurationError">
		/// If the path configuration variable 'GlobalizedNameFormat' is missing from the project configuration.
		/// </exception>
		/// <exception cref="ConfigurationError">
		/// If the project's globalization configuration doesn't contain one of the 
		/// locales specified for this category.
		/// </exception>
		public CategoryInfo(XmlElement categoryElement, IDictionary<string, LocaleInfo> definedLocales)
		{
			this.Name = categoryElement.GetAttribute("name");
			this.Locales = new List<string>(categoryElement.GetAttribute("locales").Split(','));
			
			this.Suffixes = new List<string>();
			foreach (string localeName in this.Locales)
			{
				if (!definedLocales.ContainsKey(localeName))
					throw new ConfigurationError(string.Format(
						"The locale '{0}', defined for the project, hasn't been defined within the globalization section of the project configuration.", localeName));

				foreach (string suffix in definedLocales[localeName].ResourceNames
					.Where(suffix => !this.Suffixes.Contains(suffix)))
				{
					this.Suffixes.Add(suffix);
				}
			}
		}

		/// <summary>
		/// Gets the name of this category.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the locales that are applicable to this category.
		/// </summary>
		public List<string> Locales { get; private set; }

		/// <summary>
		/// Gets the list of localized file name suffixes valid for this category.
		/// </summary>
		public List<string> Suffixes { get; private set; }

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Name;
		}
	}
}