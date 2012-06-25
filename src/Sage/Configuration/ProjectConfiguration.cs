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
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Xml;

	using Kelp.Extensions;
	using log4net;
	using Sage.Modules;
	using Sage.ResourceManagement;

	/// <summary>
	/// Provides a configuration container for all configurable properties of this project.
	/// </summary>
	public class ProjectConfiguration
	{
		/// <summary>
		/// Specifies the file name of the system configuration document.
		/// </summary>
		public const string SystemConfigName = "System.config";

		/// <summary>
		/// Specifies the file name of the project configuration document.
		/// </summary>
		public const string ProjectConfigName = "Project.config";

		/// <summary>
		/// Specifies the file name of the extension configuration document.
		/// </summary>
		public const string ExtensionConfigName = "Extension.config";

		/// <summary>
		/// Specifies the path to the configuration schema document.
		/// </summary>
		public const string ConfigSchemaPath = "sageresx://sage/resources/schemas/projectconfiguration.xsd";

		internal const string RewriteOnFile = "Rewrite.ON";
		private static readonly ILog log = LogManager.GetLogger(typeof(ProjectConfiguration).FullName);
		private static readonly Dictionary<string, ProjectConfiguration> extensions = new Dictionary<string, ProjectConfiguration>();

		private static volatile ProjectConfiguration systemConfig;
		private static volatile ProjectConfiguration projectConfig;
		private static DateTime lastUpdated;
		private static string projectLoadPath;

		private bool considerAllRequestsAsDevelopers;

		private ProjectConfiguration()
		{
			this.Modules = new Dictionary<string, ModuleConfiguration>();
			this.Categories = new Dictionary<string, CategoryInfo>();
			this.Locales = new Dictionary<string, LocaleInfo>();
			this.MetaViews = new MetaViewDictionary();
			this.DeveloperIps = new List<IpAddress>();
			this.Links = new Dictionary<string, LinkInfo>();
			this.Routing = new RoutingConfiguration();
			this.PathTemplates = new PathTemplates();
			this.ResourceLibraries = new Dictionary<string, ResourceLibraryInfo>();

			this.SharedCategory = "shared";
			this.DefaultLocale = "default";
			this.DefaultCategory = "default";
			this.AutoGlobalize = true;
		}

		/// <summary>
		/// Gets the current global <see cref="ProjectConfiguration"/>.
		/// </summary>
		public static ProjectConfiguration Current
		{
			get
			{
				ResetIfConfigurationChanged();

				lock (SystemConfigName)
				{
					if (projectConfig == null)
					{
						lock (ProjectConfigName)
						{
							Initialize();
						}
					}
				}

				return projectConfig;
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
		/// Gets the XMLconfiguration element that was used for initialization of this instance.
		/// </summary>
		public XmlElement ConfigurationElement { get; private set; }

		/// <summary>
		/// Gets the name of this project.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the type of project configuration.
		/// </summary>
		public ConfigurationType Type { get; private set; }

		/// <summary>
		/// Gets a value indicating whether Sage should automatically globalize any non-globalized XML resources
		/// that reference the globalization namespace.
		/// </summary>
		public bool AutoGlobalize { get; private set; }

		/// <summary>
		/// Gets the default category to fall back to if the URL doesn't specify a category.
		/// </summary>
		/// <remarks>
		/// This value is only applicable in project is <see cref="MultiCategory"/>.
		/// </remarks>
		public string DefaultCategory { get; private set; }

		/// <summary>
		/// Gets the default locale to fall back to if the URL doesn't specify a locale.
		/// </summary>
		public string DefaultLocale { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the current project runs in multi-category mode.
		/// </summary>
		public bool MultiCategory { get; private set; }

		/// <summary>
		/// Gets the path templates for various system-required files.
		/// </summary>
		public PathTemplates PathTemplates { get; private set; }

		/// <summary>
		/// Gets the routing configuration for the current project.
		/// </summary>
		public RoutingConfiguration Routing { get; private set; }

		/// <summary>
		/// Gets the asset path configuration variable; this is the base path for all resources.
		/// </summary>
		public string AssetPath { get; private set; }

		/// <summary>
		/// Gets the name of the category that is shared (common) with other categories.
		/// </summary>
		/// <remarks>
		/// This value is only applicable in project is <see cref="MultiCategory"/>.
		/// </remarks>
		public string SharedCategory { get; private set; }

		/// <summary>
		/// Gets the URL prefix to use if <see cref="UrlRewritingOn"/> is <c>true</c>.
		/// </summary>
		/// <remarks>
		/// This value will typically contain placeholders for category and locale, for instance 
		/// <c>{locale}/{category}/</c>
		/// </remarks>
		public string UrlRewritePrefix { get; private set; }

		/// <summary>
		/// Gets a value indicating whether URL rewriting is on.
		/// </summary>
		/// <remarks>
		/// If this value is <c>true</c>, a rewriting prefix (as specified with <see cref="UrlRewritePrefix"/>)
		/// will be prepended when generating links.
		/// </remarks>
		/// <seealso cref="UrlGenerator"/>
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
		/// Gets a value indicating whether the current project has been configured for debugging.
		/// </summary>
		public bool IsDebugMode { get; private set; }

		/// <summary>
		/// Gets the resource library dictionary.
		/// </summary>
		public Dictionary<string, ResourceLibraryInfo> ResourceLibraries { get; private set; }

		/// <summary>
		/// Gets the list of categories available within this project.
		/// </summary>
		/// <remarks>
		/// This value is only applicable in project is <see cref="MultiCategory"/>.
		/// </remarks>
		public Dictionary<string, CategoryInfo> Categories { get; private set; }

		/// <summary>
		/// Gets the list of IP addresses or address ranges to be considered as developers.
		/// </summary>
		public List<IpAddress> DeveloperIps { get; private set; }

		/// <summary>
		/// Gets the dictionary of links
		/// </summary>
		public Dictionary<string, LinkInfo> Links { get; private set; }

		/// <summary>
		/// Gets the dictionary of defined locales.
		/// </summary>
		public Dictionary<string, LocaleInfo> Locales { get; private set; }

		/// <summary>
		/// Gets the collection of global, shared meta views as defined in the configuration file.
		/// </summary>
		public MetaViewDictionary MetaViews { get; private set; }

		/// <summary>
		/// Gets the dictionary of module confiurations
		/// </summary>
		public Dictionary<string, ModuleConfiguration> Modules { get; private set; }

		/// <summary>
		/// Gets the extension package configuration for packing this project as an extension.
		/// </summary>
		/// <remarks>
		/// This value is only applicable for <see cref="ConfigurationType.Extension"/> type projects.
		/// </remarks>
		internal ExtensionPackageConfiguration Package { get; private set; }

		/// <summary>
		/// Creates a new <see cref="ProjectConfiguration"/> instance using the file from the specified 
		/// <paramref name="configStream"/>, optionally using the values from file located at 
		/// <paramref name="parentConfigPath"/> to initialize values not present in <paramref name="configStream"/>.
		/// </summary>
		/// <param name="configStream">The stream to the configuration file to use.</param>
		/// <param name="parentConfigPath">Optional path to the parent configuration file to use.</param>
		/// <returns>A new instance of <see cref="ProjectConfiguration"/>.</returns>
		public static ProjectConfiguration Create(Stream configStream, string parentConfigPath = null)
		{
			Contract.Requires<ArgumentNullException>(configStream != null);

			XmlDocument document = new XmlDocument();
			document.Load(configStream);

			XmlDocument parentDoc = null;
			if (parentConfigPath != null)
				parentDoc = ResourceManager.LoadXmlDocument(parentConfigPath);

			return Create(document, parentDoc);
		}

		/// <summary>
		/// Creates a new <see cref="ProjectConfiguration"/> instance using the file from the specified 
		/// <paramref name="configPath"/>, optionally using the values from file located at 
		/// <paramref name="parentConfigPath"/> to initialize values not present in <paramref name="configPath"/>.
		/// </summary>
		/// <param name="configPath">The path to the configuration file to use.</param>
		/// <param name="parentConfigPath">Optional path to the parent configuration file to use.</param>
		/// <returns>A new instance of <see cref="ProjectConfiguration"/>.</returns>
		public static ProjectConfiguration Create(string configPath, string parentConfigPath = null)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(configPath));

			XmlDocument document = ResourceManager.LoadXmlDocument(configPath);
			XmlDocument parentDoc = null;
			if (parentConfigPath != null)
				parentDoc = ResourceManager.LoadXmlDocument(parentConfigPath);

			return Create(document, parentDoc);
		}

		/// <summary>
		/// Creates a new <see cref="ProjectConfiguration"/> instance using the <paramref name="configDoc"/>, 
		/// optionally using the values from <paramref name="parentConfigDoc"/> to initialize values not present in 
		/// <paramref name="configDoc"/>.
		/// </summary>
		/// <param name="configDoc">The configuration file to use.</param>
		/// <param name="parentConfigDoc">Optional parent configuration file to use.</param>
		/// <returns>A new instance of <see cref="ProjectConfiguration"/>.</returns>
		public static ProjectConfiguration Create(XmlDocument configDoc, XmlDocument parentConfigDoc = null)
		{
			Contract.Requires<ArgumentNullException>(configDoc != null);

			var result = new ProjectConfiguration();
			if (parentConfigDoc != null)
				result.Parse(parentConfigDoc);

			result.Parse(configDoc);
			return result;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="clientIpAddress"/> is configured to be treated as a developer.
		/// </summary>
		/// <param name="clientIpAddress">The client IP address to test.</param>
		/// <returns>
		/// <c>true</c> if the specified <paramref name="clientIpAddress"/> is configured to be treated as a developer; 
		/// otherwise, <c>false</c>.
		/// </returns>
		public bool IsDeveloperIp(string clientIpAddress)
		{
			if (considerAllRequestsAsDevelopers)
				return true;

			return this.DeveloperIps.Count(a => a.Matches(clientIpAddress)) != 0;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("{0} ({1})", this.Name, this.Type);
		}

		internal static void Initialize()
		{
			if (projectConfig != null)
				return;

			string systemConfigPath = Path.Combine(AssemblyPath, SystemConfigName);
			string projectConfigPath1 = Path.Combine(AssemblyPath, ProjectConfigName);
			string projectConfigPath2 = Path.Combine(AssemblyPath, "..\\" + ProjectConfigName);

			string projectConfigPath = projectConfigPath1;
			if (File.Exists(projectConfigPath2))
			{
				projectConfigPath = projectConfigPath2;
				projectLoadPath = projectConfigPath2;
				lastUpdated = DateTime.Now;
			}

			if (!File.Exists(projectConfigPath) && !File.Exists(systemConfigPath))
				throw new SageHelpException(ProblemType.MissingConfigurationFile);
		
			systemConfig = new ProjectConfiguration();
			if (File.Exists(systemConfigPath))
				systemConfig.Parse(systemConfigPath);

			projectConfig = new ProjectConfiguration();
			if (File.Exists(systemConfigPath))
				projectConfig.Parse(systemConfigPath);

			if (File.Exists(projectConfigPath))
				projectConfig.Parse(projectConfigPath);

			if (projectConfig.Locales.Count == 0)
				throw new SageHelpException(ProblemType.ConfigurationMissingLocales);

			if (projectConfig.MultiCategory && projectConfig.Categories.Count == 0)
				throw new SageHelpException(ProblemType.ConfigurationMissingCategories);

			projectConfig.MergeRegisteredExtensions();
		}

		internal void Parse(string configPath)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(configPath));

			XmlDocument document = ResourceManager.LoadXmlDocument(configPath);
			this.Parse(document);
		}

		internal void Parse(XmlDocument configDoc)
		{
			Contract.Requires<ArgumentNullException>(configDoc != null);

			ResourceManager.ValidateDocument(configDoc, ConfigSchemaPath);
			XmlNamespaceManager nm = XmlNamespaces.Manager;

			XmlElement configNode = configDoc.SelectSingleElement("/p:configuration/*", nm);

			XmlElement routingNode = configNode.SelectSingleElement("p:routing", nm);
			XmlElement pathsNode = configNode.SelectSingleElement("p:paths", nm);

			this.Type = (ConfigurationType) Enum.Parse(typeof(ConfigurationType), configNode.Name, true);
			this.ConfigurationElement = configNode;

			string nodeValue = configNode.GetAttribute("name");
			if (!string.IsNullOrEmpty(nodeValue))
				this.Name = nodeValue;

			nodeValue = configNode.GetAttribute("sharedCategory");
			if (!string.IsNullOrEmpty(nodeValue))
				this.SharedCategory = nodeValue;

			nodeValue = configNode.GetAttribute("defaultLocale");
			if (!string.IsNullOrEmpty(nodeValue))
				this.DefaultLocale = nodeValue;

			nodeValue = configNode.GetAttribute("defaultCategory");
			if (!string.IsNullOrEmpty(nodeValue))
				this.DefaultCategory = nodeValue;

			nodeValue = configNode.GetAttribute("autoGlobalize");
			if (!string.IsNullOrEmpty(nodeValue))
				this.AutoGlobalize = nodeValue.ContainsAnyOf("yes", "1", "true");

			nodeValue = configNode.GetAttribute("multiCategory");
			if (!string.IsNullOrEmpty(nodeValue))
				this.MultiCategory = nodeValue.ContainsAnyOf("yes", "1", "true");

			nodeValue = configNode.GetAttribute("debugMode");
			if (!string.IsNullOrEmpty(nodeValue))
				this.IsDebugMode = nodeValue.ContainsAnyOf("yes", "1", "true");

			if (pathsNode != null)
			{
				this.PathTemplates.Parse(pathsNode);

				XmlNode node = pathsNode.SelectSingleNode("p:AssetPath", nm);
				if (node != null)
				{
					nodeValue = node.InnerText;
					if (!string.IsNullOrEmpty(nodeValue))
						this.AssetPath = nodeValue.TrimEnd('/') + "/";
				}
			}

			if (routingNode != null)
				this.Routing.ParseConfiguration(routingNode);

			XmlElement linksNode = configNode.SelectSingleElement("p:links", nm);
			if (linksNode != null)
				this.UrlRewritePrefix = linksNode.GetAttribute("rewritePrefix");

			foreach (XmlElement linkNode in configNode.SelectNodes("p:links/p:link", nm))
			{
				LinkInfo linkInfo = new LinkInfo(linkNode);
				this.Links[linkInfo.Name] = linkInfo;
			}

			foreach (XmlElement moduleNode in configNode.SelectNodes("p:modules/p:module", nm))
			{
				ModuleConfiguration moduleConfig = new ModuleConfiguration(moduleNode);
				this.Modules.Add(moduleConfig.Name, moduleConfig);
			}

			foreach (XmlElement libraryNode in configNode.SelectNodes("p:libraries/p:library", nm))
			{
				ResourceLibraryInfo info = new ResourceLibraryInfo(libraryNode);
				this.ResourceLibraries.Add(info.Name, info);
			}

			foreach (XmlElement locale in configNode.SelectNodes("p:globalization/p:locale", nm))
			{
				var name = locale.GetAttribute("name");
				var info = new LocaleInfo(locale);
				this.Locales[name] = info;
			}

			if (this.MultiCategory)
			{
				foreach (XmlElement category in configNode.SelectNodes("p:categories/p:category", nm))
				{
					var info = new CategoryInfo(category, this.Locales);
					this.Categories[info.Name] = info;
				}
			}
			else
			{
				this.Categories[this.DefaultCategory] = new CategoryInfo(this.DefaultCategory, this.Locales);
			}

			foreach (XmlElement viewNode in configNode.SelectNodes("p:metaViews/p:view", nm))
			{
				var name = viewNode.GetAttribute("name");
				var info = new MetaViewInfo(viewNode);
				this.MetaViews[name] = info;
			}

			foreach (XmlElement elem in configNode.SelectNodes("p:developers/p:ip", nm))
			{
				if (elem.GetAttribute("address") == "*")
				{
					this.considerAllRequestsAsDevelopers = true;
					break;
				}

				IpAddress address = new IpAddress(elem);
				this.DeveloperIps.Add(address);
			}

			this.Package = new ExtensionPackageConfiguration(configNode.SelectSingleNode("p:package", nm));
		}

		internal void RegisterExtension(ProjectConfiguration extensionConfig)
		{
			Contract.Requires<ArgumentNullException>(extensionConfig != null);
			Contract.Requires<ArgumentException>(extensionConfig.Type == ConfigurationType.Extension);
			Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(extensionConfig.Name));

			string extensionName = extensionConfig.Name;

			ProjectConfiguration.extensions[extensionName] = extensionConfig;
			this.MergeExtension(extensionConfig);
		}

		private void MergeExtension(ProjectConfiguration extensionConfig)
		{
			string extensionName = extensionConfig.Name;
			foreach (string name in extensionConfig.Routing.Keys)
			{
				if (this.Routing.ContainsKey(name))
				{
					log.WarnFormat("Skipped registering route '{0}' from extension '{1}' because a route with the same name already exists.", name, extensionName);
					continue;
				}

				this.Routing.Add(name, extensionConfig.Routing[name]);
			}

			foreach (string name in extensionConfig.Links.Keys)
			{
				if (this.Links.ContainsKey(name))
				{
					log.WarnFormat("Skipped registering link '{0}' from extension '{1}' because a link with the same name already exists.", name, extensionName);
					continue;
				}

				this.Links.Add(name, extensionConfig.Links[name]);
			}

			foreach (string name in extensionConfig.ResourceLibraries.Keys)
			{
				if (this.ResourceLibraries.ContainsKey(name))
				{
					log.WarnFormat("Skipped registering script library '{0}' from extension '{1}' because a script library with the same name already exists.", name, extensionName);
					continue;
				}

				this.ResourceLibraries.Add(name, extensionConfig.ResourceLibraries[name]);
			}

			foreach (string name in extensionConfig.MetaViews.Keys)
			{
				if (this.MetaViews.ContainsKey(name))
				{
					log.WarnFormat("Skipped registering meta view '{0}' from extension '{1}' because a meta view with the same name already exists.", name, extensionName);
					continue;
				}

				this.MetaViews.Add(name, extensionConfig.MetaViews[name]);
			}

			foreach (string name in extensionConfig.Modules.Keys)
			{
				if (this.Modules.ContainsKey(name))
				{
					log.WarnFormat("Skipped registering module '{0}' from extension '{1}' because a module with the same name already exists.", name, extensionName);
					continue;
				}

				this.Modules.Add(name, extensionConfig.Modules[name]);
			}
		}

		private void MergeRegisteredExtensions()
		{
			foreach (ProjectConfiguration config in ProjectConfiguration.extensions.Values)
				this.MergeExtension(config);
		}

		private static void ResetIfConfigurationChanged()
		{
			if (projectLoadPath == null)
				return;

			if (!File.Exists(projectLoadPath))
				return;

			DateTime created = File.GetCreationTime(projectLoadPath);
			DateTime modified = File.GetLastWriteTime(projectLoadPath);

			if (created > lastUpdated || modified > lastUpdated)
				projectConfig = null;
		}
	}
}