﻿/**
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
	using System.Diagnostics.Contracts;
	using System.Xml;

	using Kelp.Extensions;
	using Sage.ResourceManagement;

	/// <summary>
	/// Implements the configuration container for configurable properties of a Sage project category.
	/// </summary>
	public class CategoryConfiguration
	{
		private const string ConfigSchemaPath = "sageresx://sage/resources/schemas/CategoryConfiguration.xsd";

		private CategoryConfiguration(XmlDocument configDocument)
		{
			XmlNamespaceManager nm = XmlNamespaces.Manager;
			XmlElement categoryElement = configDocument.SelectSingleElement("c:configuration/c:category", nm);

			this.Name = categoryElement.GetAttribute("name");
			this.VariablesElement = categoryElement.SelectSingleElement("c:variables", nm);
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
		public XmlElement VariablesElement { get; private set; }

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