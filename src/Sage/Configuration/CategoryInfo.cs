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