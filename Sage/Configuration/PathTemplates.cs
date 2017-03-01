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

	using Sage.Modules;

	/// <summary>
	/// Provides path templates for various system-required files.
	/// </summary>
	public class PathTemplates
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PathTemplates"/> class.
		/// </summary>
		public PathTemplates()
		{
			// initialize with default values.
			this.View = "{assetpath}/views/";
			this.Module = "{assetpath}/modules/";
			this.Extension = "{assetpath}/extensions/";
			this.CategoryConfiguration = "{assetpath}/configuration/Category.config";
			this.DefaultStylesheet = "{assetpath}/views/xslt/default.xsl";
			this.Dictionary = "{assetpath}/configuration/dictionary/{locale}.xml";
			this.CacheDirectory = "~/tmp/cache/";
			this.SiteMap = "{assetpath}/configuration/sitemap.xml";
			this.GlobalizedDirectory = "_target/";
			this.GlobalizedDirectoryForNonFileResources = "~/_target/";
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PathTemplates"/> class, using the specified
		/// <paramref name="configElement"/> to parse its values from.
		/// </summary>
		/// <param name="configElement">The configuration element containing the definition of the path templates.</param>
		public PathTemplates(XmlElement configElement)
			: this()
		{
			this.Parse(configElement);
		}

		internal PathTemplates(PathTemplates init)
		{
			this.View = init.View;
			this.Module = init.Module;
			this.Extension = init.Extension;
			this.CategoryConfiguration = init.CategoryConfiguration;
			this.DefaultStylesheet = init.DefaultStylesheet;
			this.CacheDirectory = init.CacheDirectory;
			this.Dictionary = init.Dictionary;
			this.SiteMap = init.SiteMap;
			this.GlobalizedDirectory = init.GlobalizedDirectory;
			this.GlobalizedDirectoryForNonFileResources = init.GlobalizedDirectoryForNonFileResources;
		}

		/// <summary>
		/// Gets the template for constructing paths to views.
		/// </summary>
		public string View { get; private set; }

		/// <summary>
		/// Gets the path to the cache directory.
		/// </summary>
		public string CacheDirectory { get; private set; }

		/// <summary>
		/// Gets the template for constructing paths to module directories.
		/// </summary>
		public string Module { get; private set; }

		/// <summary>
		/// Gets the template for constructing paths to plugin directories.
		/// </summary>
		public string Extension { get; private set; }

		/// <summary>
		/// Gets the template for constructing paths to language dictionaries.
		/// </summary>
		public string Dictionary { get; private set; }

		/// <summary>
		/// Gets the template for constructing paths to sitemap files.
		/// </summary>
		public string SiteMap { get; private set; }

		/// <summary>
		/// Gets the path template to the default XSLT style sheet that can be used if a view
		/// doesn't have it's own, specific style sheet.
		/// </summary>
		public string DefaultStylesheet { get; private set; }

		/// <summary>
		/// Gets the path template to the XSLT style sheet that contains all modules xslt templates.
		/// </summary>
		/// <remarks>
		/// If this path is defined, it will be used as the xml content for <see cref="ModuleConfiguration.GetModulesDocumentXslt"/>,
		/// rather than having it invoke <see cref="ModuleConfiguration.CombineModuleXslt"/> as it would do otherwise. This provides
		/// a way to use a pre-generated stylesheet that can be deployed without having the full module source structure on the deploy
		/// location.
		/// </remarks>
		public string ModuleStylesheet { get; private set; }

		/// <summary>
		/// Gets the directory in which the globalized resources are saved.
		/// </summary>
		public string GlobalizedDirectory { get; private set; }

		/// <summary>
		/// Gets the directory in which the globalized non-file resources are saved.
		/// </summary>
		public string GlobalizedDirectoryForNonFileResources { get; private set; }

		/// <summary>
		/// Gets the template for constructing paths to category configuration files.
		/// </summary>
		public string CategoryConfiguration { get; private set; }

		/// <summary>
		/// Parses the specified configuration element and copies any applicable values into the current instance.
		/// </summary>
		/// <param name="configElement">The configuration element to parse.</param>
		public void Parse(XmlElement configElement)
		{
			Contract.Requires<ArgumentNullException>(configElement != null);

			XmlNamespaceManager nm = XmlNamespaces.Manager;
			string testValue;

			XmlNode testNode = configElement.SelectSingleNode("p:View", nm);
			if (testNode != null && !string.IsNullOrEmpty(testValue = testNode.InnerText))
				this.View = testValue.TrimEnd('/') + "/";

			testNode = configElement.SelectSingleNode("p:Module", nm);
			if (testNode != null && !string.IsNullOrEmpty(testValue = testNode.InnerText))
				this.Module = testValue.TrimEnd('/') + "/";

			testNode = configElement.SelectSingleNode("p:Extension", nm);
			if (testNode != null && !string.IsNullOrEmpty(testValue = testNode.InnerText))
				this.Extension = testValue.TrimEnd('/') + "/";

			testNode = configElement.SelectSingleNode("p:CategoryConfiguration", nm);
			if (testNode != null && !string.IsNullOrEmpty(testValue = testNode.InnerText))
				this.CategoryConfiguration = testValue;

			testNode = configElement.SelectSingleNode("p:DefaultStylesheet", nm);
			if (testNode != null && !string.IsNullOrEmpty(testValue = testNode.InnerText))
				this.DefaultStylesheet = testValue;

			testNode = configElement.SelectSingleNode("p:ModuleStylesheet", nm);
			if (testNode != null && !string.IsNullOrEmpty(testValue = testNode.InnerText))
				this.ModuleStylesheet = testValue;

			testNode = configElement.SelectSingleNode("p:Dictionary", nm);
			if (testNode != null && !string.IsNullOrEmpty(testValue = testNode.InnerText))
				this.Dictionary = testValue;

			testNode = configElement.SelectSingleNode("p:SiteMap", nm);
			if (testNode != null && !string.IsNullOrEmpty(testValue = testNode.InnerText))
				this.SiteMap = testValue;

			testNode = configElement.SelectSingleNode("p:GlobalizedDirectory", nm);
			if (testNode != null && !string.IsNullOrEmpty(testValue = testNode.InnerText))
				this.GlobalizedDirectory = testValue;

			testNode = configElement.SelectSingleNode("p:GlobalizedDirectoryForNonFileResources", nm);
			if (testNode != null && !string.IsNullOrEmpty(testValue = testNode.InnerText))
				this.GlobalizedDirectoryForNonFileResources = testValue;
		}

		/// <summary>
		/// Generates an XML element that represents this instance.
		/// </summary>
		/// <param name="ownerDoc">The document to use to create the element with.</param>
		/// <returns>The element that represents this instance.</returns>
		public XmlElement ToXml(XmlDocument ownerDoc)
		{
			Contract.Requires<ArgumentNullException>(ownerDoc != null);

			const string Ns = XmlNamespaces.ProjectConfigurationNamespace;

			XmlElement result = ownerDoc.CreateElement("paths", Ns);
			result.AppendElement(ownerDoc.CreateElement("View", Ns)).InnerText = this.View;
			result.AppendElement(ownerDoc.CreateElement("Module", Ns)).InnerText = this.Module;
			result.AppendElement(ownerDoc.CreateElement("Extension", Ns)).InnerText = this.Extension;
			result.AppendElement(ownerDoc.CreateElement("CategoryConfiguration", Ns)).InnerText = this.CategoryConfiguration;
			result.AppendElement(ownerDoc.CreateElement("DefaultStylesheet", Ns)).InnerText = this.DefaultStylesheet;
			result.AppendElement(ownerDoc.CreateElement("Dictionary", Ns)).InnerText = this.Dictionary;
			result.AppendElement(ownerDoc.CreateElement("SiteMap", Ns)).InnerText = this.SiteMap;
			result.AppendElement(ownerDoc.CreateElement("GlobalizedDirectory", Ns)).InnerText = this.GlobalizedDirectory;
			result.AppendElement(ownerDoc.CreateElement("GlobalizedDirectoryForNonFileResources", Ns)).InnerText = this.GlobalizedDirectoryForNonFileResources;

			return result;
		}
	}
}
