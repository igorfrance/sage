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
namespace Sage.Modules
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Xml;

	using Kelp.Extensions;
	using Sage.Extensibility;
	using Sage.ResourceManagement;
	using Sage.Views;

	/// <summary>
	/// Contains module configuration information.
	/// </summary>
	public class ModuleConfiguration
	{
		private const string DefaultXslt = @"<xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" xmlns=""http://www.w3.org/1999/xhtml""/>";
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
		/// Initializes a new instance of the <see cref="ModuleConfiguration"/> class, using the specified
		/// <paramref name="configElement"/>.
		/// </summary>
		/// <param name="configElement">The module configuration element.</param>
		internal ModuleConfiguration(XmlElement configElement)
			: this()
		{
			Contract.Requires<ArgumentNullException>(configElement != null);

			this.Name = configElement.GetAttribute("name");

			this.TypeName = configElement.GetAttribute("type");
			this.TagNames = new List<string>();

			var tagNameNodes = configElement.SelectNodes("p:tags/p:tag", XmlNamespaces.Manager);
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

			var dependencyNodes = configElement.SelectNodes("p:dependencies/p:dependency", XmlNamespaces.Manager);
			foreach (XmlElement dependencyNode in dependencyNodes)
			{
				this.Dependencies.Add(dependencyNode.GetAttribute("ref"));
			}

			var resourceNodes = configElement.SelectNodes("p:resources/p:resource", XmlNamespaces.Manager);
			foreach (XmlElement scriptNode in resourceNodes)
			{
				this.resources.Add(new ModuleResource(scriptNode, this.Name));
			}

			var libraryNodes = configElement.SelectNodes("p:resources/p:library", XmlNamespaces.Manager);
			foreach (XmlElement libraryNode in libraryNodes)
			{
				this.Libraries.Add(libraryNode.GetAttribute("ref"));
			}

			var stylesheetNodes = configElement.SelectNodes("p:stylesheets/p:stylesheet", XmlNamespaces.Manager);
			foreach (XmlElement stylesheetNode in stylesheetNodes)
			{
				this.Stylesheets.Add(stylesheetNode.GetAttribute("path"));
			}

			this.AutoLocation = (ModuleAutoLocation) Enum.Parse(typeof(ModuleAutoLocation), 
				configElement.GetAttribute("auto"), true);
		}

		private ModuleConfiguration()
		{
			this.TagNames = new List<string>();
			this.Stylesheets = new List<string>();
			this.Dependencies = new List<string>();
			this.Libraries = new List<string>();
			this.AutoLocation = ModuleAutoLocation.None;
		}

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
		/// Gets the list of shared libaries this module uses / depends on.
		/// </summary>
		public List<string> Libraries { get; private set; }

		/// <summary>
		/// Gets the nameof this module.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the location where this module should be automatically inserted in, if any.
		/// </summary>
		public ModuleAutoLocation AutoLocation { get; private set; }

		/// <summary>
		/// Gets the type that implements this module.
		/// </summary>
		public Type Type
		{
			get
			{
				if (moduleType == null)
				{
					if (!string.IsNullOrEmpty(this.TypeName))
						moduleType = Application.GetType(this.TypeName);
					else
						moduleType = typeof(NullModule);
				}

				return moduleType;
			}
		}

		/// <summary>
		/// Gets the name of the type that implements this module.
		/// </summary>
		public string TypeName { get; private set; }

		/// <summary>
		/// Gets the tag names associated with this module.
		/// </summary>
		public IList<string> TagNames { get; private set; }

		/// <summary>
		/// Gets the XSLT stylesheets this module uses.
		/// </summary>
		public IList<string> Stylesheets { get; private set; }

		/// <summary>
		/// Gets this module's dependencies (list of other module's names) 
		/// </summary>
		public IList<string> Dependencies { get; private set; }

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("{0} ({1}) ({2})", this.Name, string.Join(",", this.TagNames), this.Type);
		}

		[SageResourceProvider("modules.xslt")]
		internal static CacheableXmlDocument GetModulesXslt(SageContext context, string resourceUri)
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

			XsltTransform.ExcludeNamespacesPrefixResults(resultDoc);
			return resultDoc;
		}

		[NodeHandler(XmlNodeType.Element, "head", XmlNamespaces.XHtmlNamespace)]
		[NodeHandler(XmlNodeType.Element, "body", XmlNamespaces.XHtmlNamespace)]
		internal static XmlNode ProcessHtmlElement(XmlNode htmlNode, SageContext context)
		{
			Contract.Requires<ArgumentNullException>(htmlNode != null);

			if (htmlNode.SelectSingleNode("ancestor::xhtml:body", XmlNamespaces.Manager) != null)
				return htmlNode;

			ModuleAutoLocation location = htmlNode.LocalName == "head" ? ModuleAutoLocation.Head : ModuleAutoLocation.Body;

			IEnumerable<ModuleConfiguration> autoModules =
				context.ProjectConfiguration.Modules.Values.Where(m => m.AutoLocation == location);

			XmlNode resultNode = ResourceManager.CopyTree(htmlNode, context);
			foreach (ModuleConfiguration module in autoModules)
				resultNode.AppendElement("mod:" + module.TagNames[0], XmlNamespaces.ModulesNamespace);

			return resultNode;
		}

		private static IEnumerable<ModuleConfiguration> ResolveDependencies(ModuleConfiguration config)
		{
			List<ModuleConfiguration> result = new List<ModuleConfiguration>();
			foreach (string name in config.Dependencies)
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

		private static void CopyXslElements(SageContext context, string stylesheetPath, CacheableXmlDocument targetDocument)
		{
			CacheableXmlDocument fromDocument = context.Resources.LoadXml(stylesheetPath);
			targetDocument.AddDependencies(fromDocument.Dependencies);

			XmlNodeList paramNodes = fromDocument.SelectNodes("/*/xsl:param", XmlNamespaces.Manager);
			XmlNodeList variableNodes = fromDocument.SelectNodes("/*/xsl:variable", XmlNamespaces.Manager);
			XmlNodeList templateNodes = fromDocument.SelectNodes("/*/xsl:template", XmlNamespaces.Manager);
			XmlNodeList includeNodes = fromDocument.SelectNodes("/*/xsl:include", XmlNamespaces.Manager);
			XmlNodeList scriptNodes = fromDocument.SelectNodes("/*/msxsl:script", XmlNamespaces.Manager);
			XmlNodeList otherNodes = fromDocument.SelectNodes(string.Join(" | ", new[] 
			{
				"/*/xsl:preserve-space", 
				"/*/xsl:strip-space",
				"/*/xsl:namespace-alias",
				"/*/xsl:attribute-set",
			}), XmlNamespaces.Manager);

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
	}
}
