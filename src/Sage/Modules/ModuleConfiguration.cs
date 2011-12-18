namespace Sage.Modules
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Xml;

	using Sage.Configuration;
	using Sage.ResourceManagement;

	using log4net;

	public class ModuleConfiguration
	{
		private const string DefaultXslt = "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" xmlns=\"http://www.w3.org/1999/xhtml\"/>";
		private static readonly ILog log = LogManager.GetLogger(typeof(ModuleConfiguration).FullName);

		private readonly List<ModuleResource> resources = new List<ModuleResource>();

		internal ModuleConfiguration(Type moduleType, params string[] tagNames)
			: this()
		{
			if (moduleType == null)
				throw new ArgumentNullException("moduleType");

			this.Type = moduleType;
			this.Name = moduleType.Name.Replace("Module", string.Empty);

			if (tagNames.Length == 0)
			{
				this.TagNames.Add(this.Type.Name.Replace("Module", string.Empty));
			}
			else
			{
				this.TagNames = new List<string>(tagNames);
			}
		}

		internal ModuleConfiguration(XmlElement configElement)
			: this()
		{
			if (configElement == null)
				throw new ArgumentNullException("configElement");

			this.Name = configElement.GetAttribute("name");

			Type moduleType = typeof(NullModule);
			string typeName = configElement.GetAttribute("type");
			if (!string.IsNullOrEmpty(typeName))
			{
				Type t = Type.GetType(typeName, false);
				if (t == null)
				{
					log.ErrorFormat(string.Format(string.Concat(
						"The type '{0}' specified for module '{1}' could not be created and the system will use '{2}' instead. ",
						"To use a custom type for this module, make sure the type name is fully qualified with the name of the ",
						"assembly it lives in."), typeName, this.Name, moduleType.FullName));
				}
				else
					moduleType = t;
			}

			this.Type = moduleType;
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
				this.Resources.Add(new ModuleResource(scriptNode, this.Name));
			}

			var styleNodes = configElement.SelectNodes("p:resources/p:style", XmlNamespaces.Manager);
			foreach (XmlElement styleNode in styleNodes)
			{
				this.Resources.Add(new ModuleResource(styleNode, this.Name));
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
		}

		public List<ModuleResource> Resources
		{
			get
			{
				List<ModuleResource> result = new List<ModuleResource>(this.resources);
				List<ModuleConfiguration> configs = ResolveDependencies(this);

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

		public string Name { get; private set; }

		public Type Type { get; private set; }

		public IList<string> TagNames { get; private set; }

		public IList<string> Stylesheets { get; private set; }

		public IList<string> Dependencies { get; private set; }

		private static List<ModuleConfiguration> ResolveDependencies(ModuleConfiguration config)
		{
			List<ModuleConfiguration> result = new List<ModuleConfiguration>();
			foreach (string name in config.Dependencies)
			{
				ModuleConfiguration reference;
				if (SageModuleFactory.Modules.TryGetValue(name, out reference))
				{
					result.Add(reference);
					List<ModuleConfiguration> innerDependencies = ResolveDependencies(reference);
					result.AddRange(innerDependencies.Where(innerConfig => !result.Contains(innerConfig)));
				}
			}

			return result;
		}

		[SageResourceProvider("modules.xslt")]
		internal static XmlReader GetModulesXslt(SageContext context, string resourceUri)
		{
			CacheableXmlDocument resultDoc = new CacheableXmlDocument();
			resultDoc.LoadXml(DefaultXslt);

			foreach (ModuleConfiguration config in ProjectConfiguration.Current.Modules)
			{
				foreach (string path in config.Stylesheets)
				{
					string stylesheetPath = context.Path.GetModulePath(config.Name, path);
					CacheableXmlDocument stylesheet = context.Resources.LoadXml(stylesheetPath);

					CopyXslElements(context, stylesheet, resultDoc);
					resultDoc.AddDependencies(stylesheet.Dependencies);
				}
			}

			UrlResolver resolver = new UrlResolver(context);
			XmlReaderSettings settings = CacheableXmlDocument.CreateReaderSettings(resolver);
			XmlReader result = XmlReader.Create(new StringReader(resultDoc.OuterXml), settings, resourceUri);
			return result;
		}

		private static void CopyXslElements(SageContext context, CacheableXmlDocument fromDocument, CacheableXmlDocument toDocument)
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
