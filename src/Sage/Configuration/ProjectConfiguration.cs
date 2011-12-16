namespace Sage.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Xml;
	using System.Xml.Schema;

	using Kelp.Core.Extensions;
	using log4net;
	using Sage.ResourceManagement;

	/// <summary>
	/// Implements the configuration container for configurable properties of this project.
	/// </summary>
	public class ProjectConfiguration
	{
		private const string RewriteOnFile = "Rewrite.ON";
		private const string SystemConfigName = "System.config";
		private const string ProjectConfigName = "Project.config";
		private const string ConfigSchemaPath = "sageresx://sage/resources/schemas/projectconfiguration.xsd";

		private static readonly ILog log = LogManager.GetLogger(typeof(ProjectConfiguration).FullName);

		private static volatile ProjectConfiguration current;

		private ProjectConfiguration()
		{
			this.Modules = new List<ModuleConfiguration>();
			this.Categories = new Dictionary<string, CategoryInfo>();
			this.Locales = new Dictionary<string, LocaleInfo>();
			this.MetaViews = new MetaViewDictionary();
			this.DeveloperIps = new List<IpAddress>();
			this.AssetPrefixes = new Dictionary<string, string>();
			this.Links = new LinkConfiguration();
			this.Routing = new RoutingConfiguration();
			this.PathTemplates = new PathTemplates();

			this.SharedCategory = "shared";
			this.DefaultLocale = "default";
			this.DefaultCategory = "default";
			this.AreResourcesPreGenerated = false;
			this.MergeResources = false;
		}

		/// <summary>
		/// Gets the current global <see cref="ProjectConfiguration"/>.
		/// </summary>
		public static ProjectConfiguration Current
		{
			get
			{
				if (current == null)
				{
					lock (ProjectConfigName)
					{
						if (current == null)
						{
							current = ProjectConfiguration.Create();
						}
					}
				}

				return current;
			}
		}

		/// <summary>
		/// Gets the physical path of the currently executing assembly.
		/// </summary>
		public static string AssemblyPath
		{
			get
			{
				return Path.GetDirectoryName(
					Assembly.GetExecutingAssembly()
						.CodeBase
						.Replace("file:///", string.Empty)
						.Replace("/", "\\"));
			}
		}

		/// <summary>
		/// Gets a list with the current assembly and all assemblies loaded from the <see cref="AssemblyPath"/> that 
		/// reference the current assembly.
		/// </summary>
		public static List<Assembly> RelevantAssemblies
		{
			get
			{
				var currentAssembly = Assembly.GetExecutingAssembly();
				List<Assembly> result = new List<Assembly> { currentAssembly };
				try
				{
					var files = Directory.GetFiles(ProjectConfiguration.AssemblyPath, "*.dll", SearchOption.AllDirectories);
					foreach (string path in files)
					{
						Assembly asmb = Assembly.LoadFrom(path);
						if (asmb.GetReferencedAssemblies().Where(a => a.FullName == currentAssembly.FullName).Count() != 0)
						{
							result.Add(asmb);
						}
					}
				}
				catch (Exception ex)
				{
					log.ErrorFormat("Failed to count the numer of assemblies: {0}", ex.Message);
				}

				return result;
			}
		}

		public XmlElement ConfigurationElement { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the resources have been pre-generated.
		/// </summary>
		/// <value>
		/// <c>true</c> if the resources have been pre-generated; otherwise, <c>false</c>.
		/// </value>
		public bool AreResourcesPreGenerated
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the default category to fall back to if not specific category has been specified.
		/// </summary>
		public string DefaultCategory
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the default locale to fall back to if not specific locale has been specified.
		/// </summary>
		public string DefaultLocale
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a value indicating whether the resources (script and styles) should be merged.
		/// </summary>
		/// <value>
		/// <c>true</c> if the resources should be merged; otherwise, <c>false</c>.
		/// </value>
		public bool MergeResources
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a value indicating whether the current project runs in multi-category mode.
		/// </summary>
		/// <value>
		/// <c>true</c> if the current project runs in multi-category mode; otherwise, <c>false</c>.
		/// </value>
		public bool MultiCategory
		{
			get;
			private set;
		}

		/// <summary>
		/// Path templates for various system-required files.
		/// </summary>
		public PathTemplates PathTemplates
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the routing configuration for the current project.
		/// </summary>
		public RoutingConfiguration Routing
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the resource path configuration variable; this is the base path for all resources.
		/// </summary>
		public string ResourcePath
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the name of the shared category.
		/// </summary>
		public string SharedCategory
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets the base pattern for constructing URLs.
		/// </summary>
		public string UrlRewritePrefix
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether URL rewriting is on.
		/// </summary>
		/// <value><c>true</c> if URL rewriting is on; otherwise, <c>false</c>.</value>
		public bool UrlRewritingOn
		{
			get
			{
				if (PathResolver.ApplicationPhysicalPath != null)
					return File.Exists(Path.Combine(PathResolver.ApplicationPhysicalPath, RewriteOnFile));

				return false;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating the current project has been configured for debugging.
		/// </summary>
		/// <value><c>true</c> if debugging is on; otherwise, <c>false</c>.</value>
		public bool IsDebugMode
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the dictionary of asset resource mappings; where keys are asset aliases, and values are the actual paths to these assets.
		/// </summary>
		/// <remarks>
		/// This dictionary is used with <see cref="XsltIncludeResolver"/> to substitute aliases with actual paths to resources.
		/// </remarks>
		public Dictionary<string, string> AssetPrefixes
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the list of locales available within categories.
		/// </summary>
		/// <remarks>
		/// The keys in this dictionaries are the keys of the categories ('running', 'football'...) and the values are 
		/// comma-separated lists of locale identifiers.
		/// </remarks>
		/// <see cref="CategoryInfo"/>
		public Dictionary<string, CategoryInfo> Categories
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the list of IP addresses or address ranges to be considered as developers.
		/// </summary>
		public List<IpAddress> DeveloperIps
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the <see cref="NameValueCollection"/> of name/pattern link values as parsed from the configuration node.
		/// </summary>
		public LinkConfiguration Links
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the dictionary of defined locales.
		/// </summary>
		/// <see cref="LocaleInfo"/>
		public Dictionary<string, LocaleInfo> Locales
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the collection of global, shared meta views as defined in the configuration file.
		/// </summary>
		/// <see cref="MetaViewInfo"/>
		public MetaViewDictionary MetaViews
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the dictionary that defines how module names map to module classes that implement them.
		/// </summary>
		public IList<ModuleConfiguration> Modules
		{
			get;
			private set;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="clientIpAddress"/> is configured as a developer IP address.
		/// </summary>
		/// <param name="clientIpAddress">The client IP address to test.</param>
		/// <returns>
		/// <c>true</c> if the specified <paramref name="clientIpAddress"/> is configured as a developer IP address; otherwise, <c>false</c>.
		/// </returns>
		public bool IsDeveloperIp(string clientIpAddress)
		{
			return this.DeveloperIps.Where(a => a.Matches(clientIpAddress)).Count() != 0;
		}

		/// <summary>
		/// Gets a value indicating whether the specified <paramref name="locale"/> uses a latin character subset.
		/// </summary>
		/// <param name="locale">The name of the locale to verify</param>
		/// <returns>
		/// <c>true</c> if the specified locale uses a latin character subset, otherwise <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// If the argument <paramref name="locale"/> is empty or a <c>null</c>.
		/// </exception>
		public bool IsLatinLocale(string locale)
		{
			if (locale == null)
				throw new ArgumentNullException("locale");

			bool result = false;

			LocaleInfo info;
			if (this.Locales.TryGetValue(locale, out info))
				result = info.IsLatinCharset;

			return result;
		}

		/// <summary>
		/// Creates an instance of <see cref="ProjectConfiguration"/> using the configuration section in the current configuration file.
		/// </summary>
		/// <returns>
		/// An instance of <see cref="ProjectConfiguration"/> initialized using the configuration section in the current
		/// configuration file.
		/// </returns>
		/// <exception cref="ApplicationException">
		/// The required configuration file could not be opened.
		/// </exception>
		/// <exception cref="ConfigurationError">
		/// The configuration file doesn't contain the right node.
		/// </exception>
		/// <exception cref="XmlSchemaException">
		/// The format of the configuration file doesn't satisfy the schema criteria.
		/// </exception>
		private static ProjectConfiguration Create()
		{
			string projectConfigPath = Path.Combine(AssemblyPath, ProjectConfigName);
			string systemConfigPath = Path.Combine(AssemblyPath, SystemConfigName);
			if (!File.Exists(projectConfigPath) && !File.Exists(systemConfigPath))
			{
				string errorMessage =
					string.Format(
						string.Concat(
							"The required configuration file '{0}' was not found in the running application directory '{2}'.\n",
							"System configuration file '{1}' couldn't be found either. ",
							"Please ensure that either file exists and try again"),
						ProjectConfigName,
						SystemConfigName,
						AssemblyPath);

				log.Fatal(errorMessage);
				throw new ApplicationException(errorMessage);
			}

			ProjectConfiguration config = new ProjectConfiguration();
			if (File.Exists(systemConfigPath))
				config.LoadValidateAndParseConfig(systemConfigPath, true);

			if (File.Exists(projectConfigPath))
				config.LoadValidateAndParseConfig(projectConfigPath, false);

			return config;
		}

		private void LoadValidateAndParseConfig(string configPath, bool isSystemConfig)
		{
			XmlDocument configDoc = ResourceManager.LoadXmlDocument(configPath, null, ConfigSchemaPath);
			XmlNode configNode = configDoc.SelectSingleNode("/p:configuration", XmlNamespaces.Manager);

			if (configNode == null)
			{
				string errorMessage =
					string.Format(
						string.Concat(
							"The configuration file {0} doesn't contain the required node ",
							"/p:configuration.\n",
							"Please ensure that the file is properly formed and try again"),
					configPath);

				log.Fatal(errorMessage);
				throw new ConfigurationError(errorMessage);
			}

			this.ParseConfig(configNode, isSystemConfig);
		}

		/// <summary>
		/// Parses the specified configuration node.
		/// </summary>
		/// <param name="configNode">The XML configuration node to parse.</param>
		/// <param name="isSystemConfig">if set to <c>true</c> [is system config].</param>
		/// <exception cref="ArgumentNullException"><paramref name="configNode"/> is <c>null</c></exception>
		///   
		/// <exception cref="ArgumentException">
		/// The <paramref name="configNode"/> is missing the <c>project</c> node
		/// - or -
		/// The <paramref name="configNode"/> is missing the <c>modules</c> node
		/// - or -
		/// The <paramref name="configNode"/> is missing the <c>urls</c> node.
		/// </exception>
		private void ParseConfig(XmlNode configNode, bool isSystemConfig)
		{
			if (configNode == null)
				throw new ArgumentNullException("configNode");

			XmlNamespaceManager nm = XmlNamespaces.Manager;

			this.ConfigurationElement = configNode.SelectSingleElement("p:project", nm);
			this.Categories = new Dictionary<string, CategoryInfo>();
			this.Locales = new Dictionary<string, LocaleInfo>();

			string nodeValue;
			XmlElement projectNode = this.ConfigurationElement;
			XmlElement modulesNode = projectNode.SelectSingleElement("p:modules", nm);
			XmlElement routingNode = projectNode.SelectSingleElement("p:routing", nm);
			XmlElement linksNode = projectNode.SelectSingleElement("p:links", nm);
			XmlElement categoriesNode = projectNode.SelectSingleElement("p:categories", nm);
			XmlElement globalizationNode = projectNode.SelectSingleElement("p:globalization", nm);
			XmlElement pathsNode = projectNode.SelectSingleElement("p:paths", nm);
			XmlElement viewsNode = projectNode.SelectSingleElement("p:metaViews", nm);
			XmlElement developerAddresses = projectNode.SelectSingleElement("p:developers", nm);
			XmlElement assetsNode = projectNode.SelectSingleElement("p:assets", nm);

			nodeValue = projectNode.GetAttribute("sharedCategory");
			if (!string.IsNullOrEmpty(nodeValue))
				this.SharedCategory = nodeValue;

			nodeValue = projectNode.GetAttribute("defaultLocale");
			if (!string.IsNullOrEmpty(nodeValue))
				this.DefaultLocale = nodeValue;

			nodeValue = projectNode.GetAttribute("defaultCategory");
			if (!string.IsNullOrEmpty(nodeValue))
				this.DefaultCategory = nodeValue;

			nodeValue = projectNode.GetAttribute("resourcesPregenerated");
			if (!string.IsNullOrEmpty(nodeValue))
				this.AreResourcesPreGenerated = nodeValue.ContainsAnyOf("yes", "1", "true");

			nodeValue = projectNode.GetAttribute("multiCategory");
			if (!string.IsNullOrEmpty(nodeValue))
				this.MultiCategory = nodeValue.ContainsAnyOf("yes", "1", "true");

			nodeValue = projectNode.GetAttribute("mergeResources");
			if (!string.IsNullOrEmpty(nodeValue))
				this.MergeResources = nodeValue.ContainsAnyOf("yes", "1", "true");

			nodeValue = projectNode.GetAttribute("debugMode");
			if (!string.IsNullOrEmpty(nodeValue))
				this.IsDebugMode = nodeValue.ContainsAnyOf("yes", "1", "true");

			if (pathsNode != null)
			{
				this.PathTemplates.Parse(pathsNode);

				nodeValue = pathsNode.SelectSingleNode("p:ResourcePath", nm).InnerText;
				if (!string.IsNullOrEmpty(nodeValue))
					this.ResourcePath = nodeValue;
			}

			if (routingNode != null)
				this.Routing.ParseConfiguration(routingNode);

			if (linksNode != null)
			{
				this.Links.ParseConfiguration(linksNode);
				nodeValue = linksNode.GetAttribute("rewritePrefix");
				if (!string.IsNullOrEmpty(nodeValue))
					this.UrlRewritePrefix = nodeValue;
			}

			if (modulesNode != null)
			{
				foreach (XmlElement moduleNode in modulesNode.SelectNodes("p:module", nm))
				{
					ModuleConfiguration moduleConfig = new ModuleConfiguration(moduleNode);
					this.Modules.Add(moduleConfig);
				}
			}

			if (globalizationNode != null)
			{
				foreach (XmlElement locale in globalizationNode.SelectNodes("p:locale", nm))
				{
					var name = locale.GetAttribute("name");
					var info = new LocaleInfo(locale);
					if (this.Locales.ContainsKey(name))
						this.Locales[name] = info;
					else
						this.Locales.Add(name, info);
				}
			}

			if (!isSystemConfig && this.Locales.Count == 0)
			{
				throw new ConfigurationError(string.Concat(
					"The current project doesn't specify any locales, make sure at least ",
					"one locale is defined in the project configuration."));
			}

			if (this.MultiCategory)
			{
				if (categoriesNode != null)
				{
					foreach (XmlElement category in categoriesNode.SelectNodes("p:category", nm))
					{
						var info = new CategoryInfo(category, this.Locales);
						this.Categories.Add(info.Name, info);
						if (this.Categories.ContainsKey(info.Name))
							this.Categories[info.Name] = info;
						else
							this.Categories.Add(info.Name, info);
					}
				}
				else if (!isSystemConfig)
				{
					throw new ConfigurationError(string.Concat(
						"The current project is defined as a multi-category project, but the ",
						"configuration doesn't define any category. Define at least one category, ",
						"or redefine this project as a single-category project"));
				}
			}
			else
			{
				this.Categories.Add(this.DefaultCategory, new CategoryInfo(this.DefaultCategory, this.Locales));
			}

			if (viewsNode != null)
			{
				foreach (XmlElement viewNode in viewsNode.SelectNodes("p:view", nm))
				{
					var name = viewNode.GetAttribute("name");
					var info = new MetaViewInfo(viewNode);
					if (this.MetaViews.ContainsKey(name))
						this.MetaViews[name] = info;
					else
						this.MetaViews.Add(name, info);
				}
			}

			if (developerAddresses != null)
			{
				foreach (XmlElement elem in developerAddresses.SelectNodes("p:ip", nm))
				{
					IpAddress address = new IpAddress(elem);
					this.DeveloperIps.Add(address);
				}
			}

			if (assetsNode != null)
			{
				foreach (XmlElement elem in assetsNode.SelectNodes("p:prefix", nm))
				{
					var key = elem.GetAttribute("key");
					var value = elem.GetAttribute("value");
					if (this.AssetPrefixes.ContainsKey(key))
						this.AssetPrefixes[key] = value;
					else
						this.AssetPrefixes.Add(key, value);
				}
			}
		}
	}
}