namespace Sage.Modules
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Xml;

	using Sage.Views;

	using log4net;

	using Sage.Extensibility;
	using Sage.ResourceManagement;

	/// <summary>
	/// Contains module configuration information.
	/// </summary>
	public class ModuleConfiguration
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

			var scriptNodes = configElement.SelectNodes("p:resources/p:script", XmlNamespaces.Manager);
			foreach (XmlElement scriptNode in scriptNodes)
			{
				this.resources.Add(new ModuleResource(scriptNode, this.Name));
			}

			var styleNodes = configElement.SelectNodes("p:resources/p:style", XmlNamespaces.Manager);
			foreach (XmlElement styleNode in styleNodes)
			{
				this.resources.Add(new ModuleResource(styleNode, this.Name));
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
		}

		private ModuleConfiguration()
		{
			this.TagNames = new List<string>();
			this.Stylesheets = new List<string>();
			this.Dependencies = new List<string>();
			this.Libraries = new List<string>();
		}

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
						if (result.Where(r => r.Path == resource.Path).Count() != 0)
							continue;

						result.Add(resource);
					}
				}

				return result;
			}
		}

		public List<string> Libraries
		{
			get;
			private set;
		}

		public string Name { get; private set; }

		public Type Type
		{
			get
			{
				return moduleType ?? (moduleType = Application.GetType(this.TypeName));
			}
		}

		public string TypeName { get; private set; }

		public IList<string> TagNames { get; private set; }

		public IList<string> Stylesheets { get; private set; }

		public IList<string> Dependencies { get; private set; }

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
					CacheableXmlDocument stylesheet = context.Resources.LoadXml(stylesheetPath);

					CopyXslElements(context, stylesheet, resultDoc);
					resultDoc.AddDependencies(stylesheet.Dependencies);
				}
			}

			XsltTransform.ExcludeNamespacesPrefixResults(resultDoc);
			return resultDoc;
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

		private static void CopyXslElements(SageContext context, XmlDocument fromDocument, XmlDocument toDocument)
		{
			XmlNodeList paramNodes = fromDocument.SelectNodes("/*/xsl:param", XmlNamespaces.Manager);
			XmlNodeList variableNodes = fromDocument.SelectNodes("/*/xsl:variable", XmlNamespaces.Manager);
			XmlNodeList templateNodes = fromDocument.SelectNodes("/*/xsl:template", XmlNamespaces.Manager);
			XmlNodeList includeNodes = fromDocument.SelectNodes("/*/xsl:include", XmlNamespaces.Manager);

			// recursively add any includes
			foreach (XmlElement includeElem in includeNodes)
			{
				CacheableXmlDocument includedDoc = context.Resources.LoadXml(includeElem.GetAttribute("href"));
				CopyXslElements(context, includedDoc, toDocument);
			}

			// templates
			foreach (XmlNode xslNode in templateNodes)
				toDocument.DocumentElement.AppendChild(toDocument.ImportNode(xslNode, true));

			// variables before templates
			XmlNode templateNode = toDocument.DocumentElement.SelectSingleNode("/*/xsl:template[1]", XmlNamespaces.Manager);
			foreach (XmlNode xslNode in variableNodes)
				toDocument.DocumentElement.InsertBefore(toDocument.ImportNode(xslNode, true), templateNode);

			// params before variables or templates
			XmlNode variableNode = toDocument.DocumentElement.SelectSingleNode("/*/xsl:variable[1]", XmlNamespaces.Manager);
			foreach (XmlNode xslNode in paramNodes)
				toDocument.DocumentElement.InsertBefore(toDocument.ImportNode(xslNode, true), variableNode ?? templateNode);

			foreach (XmlAttribute attrNode in fromDocument.DocumentElement.Attributes)
			{
				toDocument.DocumentElement.SetAttribute(attrNode.Name, attrNode.InnerText);
			}
		}
	}
}
