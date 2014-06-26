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
namespace Sage.Modules
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Xml;

	using Kelp;
	using Kelp.Extensions;

	using log4net;

	using Sage.Extensibility;
	using Sage.ResourceManagement;
	using Sage.Views;

	using XmlNamespaces = Sage.XmlNamespaces;

	/// <summary>
	/// Contains module configuration information.
	/// </summary>
	public class ModuleConfiguration : IXmlConvertible
	{
		private const string DefaultXslt = @"<xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" xmlns=""http://www.w3.org/1999/xhtml""/>";
		private static readonly ILog log = LogManager.GetLogger(typeof(ModuleConfiguration).FullName);

		private readonly List<ModuleResource> resources = new List<ModuleResource>();
		private Type moduleType;

		/// <summary>
		/// Initializes a new instance of the <see cref="ModuleConfiguration"/> class, using the specified 
		/// <paramref name="moduleType"/> and <paramref name="tagNames"/>.
		/// </summary>
		/// <param name="moduleType">The type of the module.</param>
		/// <param name="tagNames">The tag names associated with the module.</param>
		internal ModuleConfiguration(Type moduleType, params string[] tagNames)
			: this()
		{
			Contract.Requires<ArgumentNullException>(moduleType != null);

			this.moduleType = moduleType;
			this.Name = moduleType.Name.Replace("Module", string.Empty);

			if (tagNames.Length == 0)
				this.TagNames.Add(this.Type.Name.Replace("Module", string.Empty));
			else
				this.TagNames = new List<string>(tagNames);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ModuleConfiguration" /> class, using the specified
		/// <paramref name="configElement" />.
		/// </summary>
		/// <param name="configElement">The module configuration element.</param>
		/// <param name="projectId">The identification string of the project this library belongs to.</param>
		internal ModuleConfiguration(XmlElement configElement, string projectId)
			: this()
		{
			this.ProjectId = projectId;
			this.Parse(configElement);
		}

		private ModuleConfiguration()
		{
			this.Category = string.Empty;
			this.TagNames = new List<string>();
			this.Stylesheets = new List<string>();
			this.ModuleDependencies = new List<string>();
			this.LibraryDependencies = new List<string>();
			this.AutoLocation = ModuleAutoLocation.None;
		}

		/// <summary>
		/// Gets the location where this module should be automatically inserted in, if any.
		/// </summary>
		public ModuleAutoLocation AutoLocation { get; private set; }

		/// <summary>
		/// Gets the category of this module.
		/// </summary>
		public string Category { get; private set; }

		/// <summary>
		/// Gets the list of names of libraries this module uses or depends on.
		/// </summary>
		public List<string> LibraryDependencies { get; private set; }

		/// <summary>
		/// Gets the list of other modules that this module uses / depends on.
		/// </summary>
		public IList<string> ModuleDependencies { get; private set; }

		/// <summary>
		/// Gets the name of this module.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Optional name of extension that defines this module.
		/// </summary>
		public string Extension { get; internal set; }

		/// <summary>
		/// Gets the identification string of the project this library belongs to.
		/// </summary>
		public string ProjectId { get; private set; }

		/// <summary>
		/// Gets the list of resources this module uses / depends on.
		/// </summary>
		public List<ModuleResource> Resources
		{
			get
			{
				List<ModuleResource> result = new List<ModuleResource>(this.resources);
				IEnumerable<ModuleConfiguration> configs = ResolveDependencies(this);

				foreach (ModuleConfiguration config in configs)
				{
					foreach (ModuleResource resource in config.Resources)
					{
						if (result.Count(r => r.Path == resource.Path) != 0)
							continue;

						result.Add(resource);
					}
				}

				return result;
			}
		}

		/// <summary>
		/// Gets the XSLT style-sheets this module uses.
		/// </summary>
		public IList<string> Stylesheets { get; private set; }

		/// <summary>
		/// Gets the tag names associated with this module.
		/// </summary>
		public IList<string> TagNames { get; private set; }

		/// <summary>
		/// Gets the type that implements this module.
		/// </summary>
		public Type Type
		{
			get
			{
				if (this.moduleType == null)
				{
					if (!string.IsNullOrEmpty(this.TypeName))
					{
						this.moduleType = Project.GetType(this.TypeName);
						if (this.moduleType == null)
						{
							log.ErrorFormat("The specified type '{0}' for module '{1}' could not be loaded", this.TypeName, this.Name);
							this.moduleType = typeof(NullModule);
						}
					}
					else
						this.moduleType = typeof(NullModule);
				}

				return this.moduleType;
			}
		}

		/// <summary>
		/// Gets the name of the type that implements this module.
		/// </summary>
		public string TypeName { get; private set; }

		/// <inheritdoc/>
		public void Parse(XmlElement element)
		{
			this.Name = element.GetAttribute("name");
			this.Category = element.GetAttribute("category");

			this.TypeName = element.GetAttribute("type");
			this.TagNames = new List<string>();

			var tagNameNodes = element.SelectNodes("p:tags/p:tag", XmlNamespaces.Manager);
			if (tagNameNodes.Count == 0)
			{
				this.TagNames.Add(this.Name);
			}
			else
			{
				foreach (XmlElement tagNode in tagNameNodes)
				{
					this.TagNames.Add(tagNode.GetAttribute("name"));
				}
			}

			var dependencyNodes = element.SelectNodes("p:dependencies/p:module", XmlNamespaces.Manager);
			foreach (XmlElement dependencyNode in dependencyNodes)
			{
				this.ModuleDependencies.Add(dependencyNode.GetAttribute("ref"));
			}

			var libraryNodes = element.SelectNodes("p:dependencies/p:library", XmlNamespaces.Manager);
			foreach (XmlElement libraryNode in libraryNodes)
			{
				this.LibraryDependencies.Add(libraryNode.GetAttribute("ref"));
			}

			var resourceNodes = element.SelectNodes("p:resources/p:resource", XmlNamespaces.Manager);
			foreach (XmlElement scriptNode in resourceNodes)
			{
				this.resources.Add(new ModuleResource(scriptNode, this.Name, this.ProjectId));
			}

			var stylesheetNodes = element.SelectNodes("p:stylesheets/p:stylesheet", XmlNamespaces.Manager);
			foreach (XmlElement stylesheetNode in stylesheetNodes)
			{
				this.Stylesheets.Add(stylesheetNode.GetAttribute("path"));
			}

			var autoLocation = element.GetAttribute("auto");
			if (!string.IsNullOrWhiteSpace(autoLocation))
			{
				this.AutoLocation = (ModuleAutoLocation) Enum.Parse(
					typeof(ModuleAutoLocation),
					element.GetAttribute("auto"),
					true);
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("{0} ({1}) ({2})", this.Name, 
				string.Join(",", this.TagNames), this.TypeName ?? typeof(NullModule).Name);
		}

		/// <inheritdoc/>
		public XmlElement ToXml(XmlDocument document)
		{
			const string Ns = XmlNamespaces.ProjectConfigurationNamespace;
			XmlElement result = document.CreateElement("module", Ns);

			result.SetAttribute("name", this.Name);

			if (!string.IsNullOrWhiteSpace(this.Category))
				result.SetAttribute("category", this.Category);

			if (!string.IsNullOrWhiteSpace(this.TypeName))
				result.SetAttribute("type", this.TypeName);

			if (this.TagNames.Count > 1 || this.TagNames[0] != this.Name)
			{
				XmlNode tagsNode = result.AppendChild(document.CreateElement("tags", Ns));
				foreach (string name in this.TagNames)
				{
					tagsNode.AppendElement(document.CreateElement("p:tag", Ns)).SetAttribute("name", name);
				}
			}

			XmlNode dependenciesNode = result.AppendChild(document.CreateElement("dependencies", Ns));
			foreach (string name in this.ModuleDependencies)
				dependenciesNode.AppendElement(document.CreateElement("module", Ns)).SetAttribute("ref", name);

			foreach (string name in this.LibraryDependencies)
				dependenciesNode.AppendElement(document.CreateElement("library", Ns)).SetAttribute("ref", name);

			XmlNode resourcesNode = result.AppendChild(document.CreateElement("resources", Ns));
			foreach (ModuleResource resource in this.resources)
				resourcesNode.AppendElement(resource.ToXml(document));

			XmlNode stylesheetsNode = result.AppendChild(document.CreateElement("stylesheets", Ns));
			foreach (string path in this.Stylesheets)
				stylesheetsNode.AppendElement(document.CreateElement("stylesheet", Ns)).SetAttribute("path", path);

			if (this.AutoLocation != ModuleAutoLocation.None)
				result.SetAttribute("auto", this.AutoLocation.ToString().ToLower());

			return result;
		}

		[XmlProvider("modules.xslt")]
		internal static CacheableXmlDocument CombineModuleXslt(SageContext context, string resourceUri)
		{
			CacheableXmlDocument resultDoc = new CacheableXmlDocument();
			resultDoc.LoadXml(DefaultXslt);

			foreach (ModuleConfiguration config in context.ProjectConfiguration.Modules.Values)
			{
				foreach (string path in config.Stylesheets)
				{
					string stylesheetPath = context.Path.GetModulePath(config.Name, path);
					CopyXslElements(context, stylesheetPath, resultDoc);
				}
			}

			XsltTransform.OmitNamespacePrefixResults(resultDoc);
			return resultDoc;
		}

		[NodeHandler(XmlNodeType.Element, "head", XmlNamespaces.XHtmlNamespace)]
		[NodeHandler(XmlNodeType.Element, "body", XmlNamespaces.XHtmlNamespace)]
		internal static XmlNode ProcessHtmlElement(SageContext context, XmlNode htmlNode)
		{
			Contract.Requires<ArgumentNullException>(htmlNode != null);

			if (htmlNode.SelectSingleNode("ancestor::xhtml:body", XmlNamespaces.Manager) != null)
				return htmlNode;

			ModuleAutoLocation location = htmlNode.LocalName == "head" ? ModuleAutoLocation.Head : ModuleAutoLocation.Body;

			IEnumerable<ModuleConfiguration> autoModules =
				context.ProjectConfiguration.Modules.Values.Where(m => m.AutoLocation == location);

			XmlNode resultNode = context.ProcessNode(htmlNode);
			foreach (ModuleConfiguration module in autoModules)
				resultNode.AppendElement("mod:" + module.TagNames[0], XmlNamespaces.ModulesNamespace);

			return resultNode;
		}

		/// <summary>
		/// Gets the element with module's defaults, if one is provided in the module's resource directory.
		/// </summary>
		/// <param name="context">The context under which the code is executing.</param>
		/// <returns>The element with module's defaults, if one is provided in the module's resource directory.</returns>
		internal XmlElement GetDefault(SageContext context)
		{
			string documentPath = context.Path.GetModulePath(this.Name, this.Name + ".xml");
			if (File.Exists(documentPath))
			{
				XmlDocument document = context.Resources.LoadXml(documentPath);
				return document.SelectSingleElement(string.Format("/mod:{0}", this.Name), XmlNamespaces.Manager);
			}

			return null;
		}

		private static void CopyXslElements(SageContext context, string stylesheetPath, CacheableXmlDocument targetDocument)
		{
			CacheableXmlDocument fromDocument = context.Resources.LoadXml(stylesheetPath);
			targetDocument.AddDependencies(fromDocument.Dependencies);

			string xpathOthers = string.Join(" | ", 
				new[] { "/*/xsl:preserve-space", "/*/xsl:strip-space", "/*/xsl:namespace-alias", "/*/xsl:attribute-set" });

			XmlNodeList paramNodes = fromDocument.SelectNodes("/*/xsl:param", XmlNamespaces.Manager);
			XmlNodeList variableNodes = fromDocument.SelectNodes("/*/xsl:variable", XmlNamespaces.Manager);
			XmlNodeList templateNodes = fromDocument.SelectNodes("/*/xsl:template", XmlNamespaces.Manager);
			XmlNodeList includeNodes = fromDocument.SelectNodes("/*/xsl:include", XmlNamespaces.Manager);
			XmlNodeList scriptNodes = fromDocument.SelectNodes("/*/msxsl:script", XmlNamespaces.Manager);
			XmlNodeList otherNodes = fromDocument.SelectNodes(xpathOthers, XmlNamespaces.Manager);

			string stylesheetDirectory = Path.GetDirectoryName(stylesheetPath);

			// recursively add any includes
			foreach (XmlElement includeElem in includeNodes)
			{
				string includeHref = includeElem.GetAttribute("href");
				string includePath = Path.Combine(stylesheetDirectory, includeHref);

				CopyXslElements(context, includePath, targetDocument);
				targetDocument.AddDependencies(includePath);
			}

			// templates
			foreach (XmlNode xslNode in templateNodes)
				targetDocument.DocumentElement.AppendChild(targetDocument.ImportNode(xslNode, true));

			foreach (XmlNode xslNode in scriptNodes)
				targetDocument.DocumentElement.AppendChild(targetDocument.ImportNode(xslNode, true));

			XmlNode firstNode = targetDocument.SelectSingleNode("/*/xsl:template[1]", XmlNamespaces.Manager);
			foreach (XmlNode xslNode in variableNodes)
				firstNode = targetDocument.DocumentElement.InsertBefore(targetDocument.ImportNode(xslNode, true), firstNode);

			// other nodes before variables or templates, params before other nodes
			foreach (XmlNode xslNode in otherNodes)
				firstNode = targetDocument.DocumentElement.InsertBefore(targetDocument.ImportNode(xslNode, true), firstNode);

			foreach (XmlNode xslNode in paramNodes)
				targetDocument.DocumentElement.InsertBefore(targetDocument.ImportNode(xslNode, true), targetDocument.DocumentElement.SelectSingleNode("*"));

			foreach (XmlAttribute attrNode in fromDocument.DocumentElement.Attributes)
				targetDocument.DocumentElement.SetAttribute(attrNode.Name, attrNode.InnerText);
		}

		private static IEnumerable<ModuleConfiguration> ResolveDependencies(ModuleConfiguration config)
		{
			List<ModuleConfiguration> result = new List<ModuleConfiguration>();
			foreach (string name in config.ModuleDependencies)
			{
				ModuleConfiguration reference;
				if (SageModuleFactory.Modules.TryGetValue(name, out reference))
				{
					result.Add(reference);
					IEnumerable<ModuleConfiguration> innerDependencies = ResolveDependencies(reference);
					result.AddRange(innerDependencies.Where(innerConfig => !result.Contains(innerConfig)));
				}
			}

			return result;
		}
	}
}
