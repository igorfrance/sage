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
	using System.Xml;

	using Kelp;

	/// <summary>
	/// Contains configuration information about a category (such as 'football' or 'outdoor').
	/// </summary>
	public class CategoryInfo : IXmlConvertible
	{
		internal const string DefaultLocale = "default";

		/// <summary>
		/// Initializes a new instance of the <see cref="CategoryInfo"/> class.
		/// </summary>
		/// <param name="categoryName">The name of this category.</param>
		public CategoryInfo(string categoryName)
			: this()
		{
			this.Name = categoryName;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CategoryInfo"/> class. 
		/// </summary>
		/// <param name="element">The <see cref="XmlElement"/> that contains this category's configuration.</param>
		public CategoryInfo(XmlElement element)
			: this()
		{
			this.Parse(element);
		}

		internal CategoryInfo()
		{
			this.Name = CategoryInfo.DefaultLocale;
			this.Locales = new List<string>();
		}

		/// <summary>
		/// Gets the name of this category.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the locales that are applicable to this category.
		/// </summary>
		public List<string> Locales { get; private set; }

		/// <inheritdoc/>
		public void Parse(XmlElement element)
		{
			this.Name = element.GetAttribute("name");
			this.Locales = new List<string>(element.GetAttribute("locales").Split(','));
		}

		/// <inheritdoc/>
		public XmlElement ToXml(XmlDocument document)
		{
			XmlElement result = document.CreateElement("category", Sage.XmlNamespaces.ProjectConfigurationNamespace);
			result.SetAttribute("name", this.Name);
			result.SetAttribute("locales", string.Join(",", this.Locales));
			return result;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Name;
		}
	}
}