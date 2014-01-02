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
	using System.Diagnostics.Contracts;
	using System.Xml;

	using Kelp.Extensions;
	using Sage.ResourceManagement;

	/// <summary>
	/// Implements the configuration container for configurable properties of a Sage project category.
	/// </summary>
	public class CategoryConfiguration
	{
		private const string ConfigSchemaPath = "sageresx://sage/resources/schemas/sage/configuration/category.xsd";

		private CategoryConfiguration(XmlDocument configDocument)
		{
			XmlNamespaceManager nm = XmlNamespaces.Manager;
			XmlElement categoryElement = configDocument.SelectSingleElement("c:configuration/c:category", nm);

			this.Name = categoryElement.GetAttribute("name");
			this.Variables = categoryElement.SelectSingleElement("c:variables", nm);
			this.ConfigurationElement = categoryElement;
		}

		/// <summary>
		/// Gets the name of this category.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the configuration element.
		/// </summary>
		public XmlElement ConfigurationElement { get; private set; }

		/// <summary>
		/// Gets the xml element containing the localization variables for use in internationalization.
		/// </summary>
		public XmlElement Variables { get; private set; }

		/// <summary>
		/// Creates a <see cref="CategoryConfiguration"/> using the specified <paramref name="configurationPath"/>.
		/// </summary>
		/// <param name="configurationPath">The configuration path.</param>
		/// <returns>A new instance of <see cref="CategoryConfiguration"/>, initialized from the file located at
		/// the specified <paramref name="configurationPath"/>.</returns>
		public static CategoryConfiguration Create(string configurationPath)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(configurationPath));

			CacheableXmlDocument schemaDocument = ResourceManager.LoadXmlDocument(configurationPath, null, ConfigSchemaPath);
			return new CategoryConfiguration(schemaDocument);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Name;
		}
	}
}