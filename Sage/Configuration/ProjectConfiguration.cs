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
	using System.Collections.Specialized;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Web.Routing;
	using System.Xml;

	using Kelp.Extensions;

	using log4net;
	using Sage.Extensibility;
	using Sage.Modules;
	using Sage.ResourceManagement;
	using Sage.Routing;

	/// <summary>
	/// Provides a configuration container for all configurable properties of this project.
	/// </summary>
	public class ProjectConfiguration
	{
		/// <summary>
		/// Specifies the path to the configuration schema document.
		/// </summary>
		public const string ConfigSchemaPath = "sageresx://sage/resources/schemas/sage/configuration/project.xsd";

		/// <summary>
		/// Specifies the file name of the extension configuration document.
		/// </summary>
		public const string ExtensionConfigName = "Extension.config";

		/// <summary>
		/// Specifies the file name of the project configuration document.
		/// </summary>
		public const string ProjectConfigName = "Project.config";

		/// <summary>
		/// Specifies the file name of the system configuration document.
		/// </summary>
		public const string SystemConfigName = "System.config";

		private static readonly Dictionary<string, ProjectConfiguration> extensions = new Dictionary<string, ProjectConfiguration>();
		private static readonly ILog log = LogManager.GetLogger(typeof(ProjectConfiguration).FullName);
		private static readonly ProjectConfiguration defaultConfiguration;
		private readonly List<XmlElement> customElements;

		static ProjectConfiguration()
		{
			defaultConfiguration = new ProjectConfiguration();
			if (File.Exists(ProjectConfiguration.SystemConfigurationPath))
				defaultConfiguration.Parse(ProjectConfiguration.SystemConfigurationPath);
		}

		private ProjectConfiguration(ProjectConfiguration initConfiguration = null)
		{
			this.AssetPath = initConfiguration != null 
				? initConfiguration.AssetPath 
				: "~/assets";

			this.Modules = initConfiguration != null
				? new Dictionary<string, ModuleConfiguration>(initConfiguration.Modules)
				: new Dictionary<string, ModuleConfiguration>();

			this.MetaViews = initConfiguration != null
				? new MetaViewDictionary(initConfiguration.MetaViews)
				: new MetaViewDictionary();

			this.ErrorViews = initConfiguration != null
				? new Dictionary<string, ErrorViewInfo>(initConfiguration.ErrorViews)
				: new Dictionary<string, ErrorViewInfo>();

			this.Environment = initConfiguration != null
				? new EnvironmentConfiguration(initConfiguration.Environment)
				: new EnvironmentConfiguration();

			this.Linking = initConfiguration != null
				? new LinkingConfiguration(initConfiguration.Linking)
				: new LinkingConfiguration();

			this.Routing = initConfiguration != null
				? new RoutingConfiguration(initConfiguration.Routing)
				: new RoutingConfiguration();

			this.PathTemplates = initConfiguration != null
				? new PathTemplates(initConfiguration.PathTemplates)
				: new PathTemplates();

			this.ResourceLibraries = initConfiguration != null
				? new Dictionary<string, ResourceLibraryInfo>(initConfiguration.ResourceLibraries)
				: new Dictionary<string, ResourceLibraryInfo>();

			this.ViewCaching = initConfiguration != null
				? initConfiguration.ViewCaching
				: new CachingConfiguration();

			this.Type = initConfiguration != null
				? initConfiguration.Type
				: ProjectType.Project;

			this.SharedCategory = initConfiguration != null
				? initConfiguration.SharedCategory
				: "shared";

			this.DefaultLocale = initConfiguration != null
				? initConfiguration.DefaultLocale
				: "us";

			this.DefaultCategory = initConfiguration != null
				? initConfiguration.DefaultCategory
				: "default";

			this.AutoInternationalize = initConfiguration == null || initConfiguration.AutoInternationalize;

			this.ValidationResult = initConfiguration != null
				? new ValidationResult(initConfiguration.ValidationResult)
				: new ValidationResult();

			this.Files = initConfiguration != null
				? new List<string>(initConfiguration.Files)
				: new List<string>();
			
			this.Dependencies = initConfiguration != null
				? new List<string>(initConfiguration.Dependencies)
				: new List<string>();

			this.Locales = initConfiguration != null
				? new Dictionary<string, LocaleInfo>(initConfiguration.Locales)
				: new Dictionary<string, LocaleInfo> { { "us", new LocaleInfo 
					{ 
						Name = "us",
						DictionaryNames = new List<string> { "us", "en" },
						ResourceNames = new List<string> { "us", "en", "default" },
					}}};

			this.Categories = initConfiguration != null
				? new Dictionary<string, CategoryInfo>(initConfiguration.Categories)
				: new Dictionary<string, CategoryInfo>();

			this.Categories[this.DefaultCategory] = new CategoryInfo(this.DefaultCategory) 
			{ 
				Locales = this.Locales.Keys.ToList() 
			};

			customElements = initConfiguration != null
				? new List<XmlElement>(initConfiguration.customElements)
				: new List<XmlElement>();

			this.Variables = new Dictionary<string, NameValueCollection>();
		}

		/// <summary>
		/// Occurs when the project configuration is changed.
		/// </summary>
		public event EventHandler Changed;

		/// <summary>
		/// Gets the path of system configuration file, used to initialize the default settings.
		/// </summary>
		public static string SystemConfigurationPath
		{
			get
			{
				return Path.Combine(Project.AssemblyCodeBaseDirectory, SystemConfigName);
			}
		}

		/// <summary>
		/// Gets the asset path configuration variable; this is the base path for all resources.
		/// </summary>
		public string AssetPath { get; private set; }

		/// <summary>
		/// Gets a value indicating whether Sage should automatically globalize any non-globalized XML resources
		/// that reference the internationalization namespace.
		/// </summary>
		public bool AutoInternationalize { get; private set; }

		/// <summary>
		/// Gets the list of categories available within this project.
		/// </summary>
		public Dictionary<string, CategoryInfo> Categories { get; private set; }

		/// <summary>
		/// Gets the default category to fall back to if the URL doesn't specify a category.
		/// </summary>
		public string DefaultCategory { get; private set; }

		/// <summary>
		/// Gets the default locale to fall back to if the URL doesn't specify a locale.
		/// </summary>
		public string DefaultLocale { get; private set; }

		/// <summary>
		/// Gets the list of extensions that this project depends on.
		/// </summary>
		/// <value>The extensions this project depends on.</value>
		public List<string> Dependencies { get; private set; } 

		/// <summary>
		/// Gets the environment configuration.
		/// </summary>
		public EnvironmentConfiguration Environment { get; private set; }

		/// <summary>
		/// Gets the view caching configuration.
		/// </summary>
		internal CachingConfiguration ViewCaching { get; private set; }

		/// <summary>
		/// Gets the identification string of the project this configuration represents.
		/// </summary>
		/// <value>
		/// The project identification string is used to identify resource origin and be able to
		/// subsequently order them on project dependency. The format of the identifier is: 
		/// <code>[ProjectType].[ProjectName]</code>
		/// </value>
		public string Id
		{
			get
			{
				return string.Concat(this.Type.ToString(), ".", this.Name);
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current project has been configured for debugging.
		/// </summary>
		public bool IsDebugEnabled { get; private set; }

		/// <summary>
		/// Gets the linking configuration.
		/// </summary>
		public LinkingConfiguration Linking { get; private set; }

		/// <summary>
		/// Gets the dictionary of defined locales.
		/// </summary>
		public Dictionary<string, LocaleInfo> Locales { get; private set; }

		/// <summary>
		/// Gets the collection of global, shared meta views as defined in the configuration file.
		/// </summary>
		public MetaViewDictionary MetaViews { get; private set; }

		/// <summary>
		/// Gets the collection of global, shared error views as defined in the configuration file.
		/// </summary>
		public Dictionary<string, ErrorViewInfo> ErrorViews { get; private set; }

		/// <summary>
		/// Gets the dictionary of module configurations
		/// </summary>
		public Dictionary<string, ModuleConfiguration> Modules { get; private set; }

		/// <summary>
		/// Gets the name of this project.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the path templates for various system-required files.
		/// </summary>
		public PathTemplates PathTemplates { get; private set; }

		/// <summary>
		/// Gets the resource library dictionary.
		/// </summary>
		public Dictionary<string, ResourceLibraryInfo> ResourceLibraries { get; private set; }

		/// <summary>
		/// Gets the routing configuration for the current project.
		/// </summary>
		public RoutingConfiguration Routing { get; private set; }

		/// <summary>
		/// Gets the name of the category that is shared (common) with other categories.
		/// </summary>
		public string SharedCategory { get; private set; }

		/// <summary>
		/// Gets the type of project that this configuration defines.
		/// </summary>
		public ProjectType Type { get; private set; }

		internal List<string> Files
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the extension package configuration for packing this project as an extension.
		/// </summary>
		/// <remarks>
		/// This value is only applicable for <see cref="ConfigurationType.Extension"/> type projects.
		/// </remarks>
		internal PackageConfiguration Package { get; private set; }

		internal ValidationResult ValidationResult { get; set; }

		/// <summary>
		/// Gets the element that contains the definition of project-global internationalization variables. Used with
		/// internationalization.
		/// </summary>
		internal XmlNode VariablesNode { get; private set; }

		internal Dictionary<string, NameValueCollection> Variables
		{
			get;
			private set;
		}

		/// <summary>
		/// Creates a <see cref="ProjectConfiguration"/> instance with all settings initialized to their defaults.
		/// </summary>
		/// <returns>A new instance of <see cref="ProjectConfiguration"/>.</returns>
		public static ProjectConfiguration Create()
		{
			return new ProjectConfiguration(defaultConfiguration);
		}

		/// <summary>
		/// Creates a new <see cref="ProjectConfiguration"/> instance using the file from the specified <paramref name="configStream"/>.
		/// </summary>
		/// <param name="configStream">The stream to the configuration file to use.</param>
		/// <returns>A new instance of <see cref="ProjectConfiguration"/>.</returns>
		public static ProjectConfiguration Create(Stream configStream)
		{
			Contract.Requires<ArgumentNullException>(configStream != null);

			XmlDocument document = new XmlDocument { PreserveWhitespace = true };
			document.Load(configStream);
			return ProjectConfiguration.Create(document);
		}

		/// <summary>
		/// Creates a new <see cref="ProjectConfiguration"/> instance using the file from the specified <paramref name="configPath"/>.
		/// </summary>
		/// <param name="configPath">The path to the configuration file to use.</param>
		/// <returns>A new instance of <see cref="ProjectConfiguration"/>.</returns>
		public static ProjectConfiguration Create(string configPath)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(configPath));

			XmlDocument document = ResourceManager.LoadXmlDocument(configPath);
			return ProjectConfiguration.Create(document);
		}

		/// <summary>
		/// Creates a new <see cref="ProjectConfiguration"/> instance using the <paramref name="configDoc"/>.
		/// </summary>
		/// <param name="configDoc">The configuration file to use.</param>
		/// <returns>A new instance of <see cref="ProjectConfiguration"/>.</returns>
		public static ProjectConfiguration Create(XmlDocument configDoc)
		{
			Contract.Requires<ArgumentNullException>(configDoc != null);

			var result = ProjectConfiguration.Create();
			result.Parse(configDoc.DocumentElement);
			return result;
		}

		/// <summary>
		/// Gets the value of the project variable with the specified <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The name of the variable.</param>
		/// <param name="locale">Optional locale to use to select the variable value.</param>
		/// <returns>The value of the project variable with the specified <paramref name="name"/>.</returns>
		public string GetVariable(string name, string locale = null)
		{
			NameValueCollection variable;
			if (!this.Variables.TryGetValue(name, out variable))
				return null;

			var result = variable["default"];
			if (locale != null && this.Locales.ContainsKey(locale))
			{
				var localeInfo = this.Locales[locale];
				foreach (string localeName in localeInfo.ResourceNames)
				{
					if (variable[localeName] != null)
					{
						result = variable[localeName];
						break;
					}
				}
			}

			return result;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("Project {0}", this.Name);
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
			XmlElement result = ownerDoc.CreateElement("project", Ns);

			if (!string.IsNullOrWhiteSpace(this.Name))
				result.SetAttribute("name", this.Name);

			if (!string.IsNullOrWhiteSpace(this.SharedCategory))
				result.SetAttribute("sharedCategory", this.SharedCategory);

			if (!string.IsNullOrWhiteSpace(this.DefaultLocale))
				result.SetAttribute("defaultLocale", this.DefaultLocale);

			if (!string.IsNullOrWhiteSpace(this.DefaultCategory))
				result.SetAttribute("defaultCategory", this.DefaultCategory);

			result.SetAttribute("autoInternationalize", this.AutoInternationalize ? "1" : "0");
			result.SetAttribute("debugMode", this.IsDebugEnabled ? "1" : "0");

			result.AppendChild(this.PathTemplates.ToXml(ownerDoc));
			result.AppendChild(this.Routing.ToXml(ownerDoc));
			result.AppendChild(this.Linking.ToXml(ownerDoc));
			result.AppendChild(this.Environment.ToXml(ownerDoc));

			if (this.Modules.Count != 0)
			{
				XmlNode target = result.AppendChild(ownerDoc.CreateElement("modules", Ns));
				foreach (ModuleConfiguration module in this.Modules.Values)
					target.AppendChild(module.ToXml(ownerDoc));
			}

			if (this.ResourceLibraries.Count != 0)
			{
				XmlNode target = result.AppendChild(ownerDoc.CreateElement("libraries", Ns));
				foreach (ResourceLibraryInfo library in this.ResourceLibraries.Values)
					target.AppendChild(library.ToXml(ownerDoc));
			}

			if (this.Locales.Count != 0)
			{
				XmlNode target = result.AppendChild(ownerDoc.CreateElement("internationalization", Ns));
				foreach (LocaleInfo locale in this.Locales.Values)
					target.AppendChild(locale.ToXml(ownerDoc));
			}

			if (this.Categories.Count != 0)
			{
				XmlNode target = result.AppendChild(ownerDoc.CreateElement("categories", Ns));
				foreach (CategoryInfo category in this.Categories.Values)
					target.AppendChild(category.ToXml(ownerDoc));
			}

			if (this.MetaViews.Count != 0)
			{
				XmlNode target = result.AppendChild(ownerDoc.CreateElement("metaViews", Ns));
				foreach (MetaViewInfo metaViewInfo in this.MetaViews.Values)
					target.AppendChild(metaViewInfo.ToXml(ownerDoc));
			}

			if (this.ErrorViews.Count != 0)
			{
				XmlNode target = result.AppendChild(ownerDoc.CreateElement("errorViews", Ns));
				foreach (ErrorViewInfo info in this.ErrorViews.Values)
					target.AppendChild(info.ToXml(ownerDoc));
			}

			foreach (XmlElement custom in customElements)
				result.AppendChild(ownerDoc.ImportNode(custom, true));

			return result;
		}

		internal void Parse(string configPath)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(configPath));

			if (!this.ValidationResult.Success)
				return;

			CacheableXmlDocument document = ResourceManager.LoadXmlDocument(configPath);
			this.Parse(document.DocumentElement);
			this.ValidationResult.SourceFile = configPath;
			this.Files.AddRange(document.Dependencies.Where(d => !this.Files.Contains(d)));
		}

		internal void Parse(XmlElement configNode)
		{
			Contract.Requires<ArgumentNullException>(configNode != null);

			this.ValidationResult = ResourceManager.ValidateElement(configNode, ConfigSchemaPath);
			if (!this.ValidationResult.Success)
				return;

			var nm = XmlNamespaces.Manager;
			var packageElement = configNode.SelectSingleNode("p:package", nm);

			this.Type = (ProjectType) Enum.Parse(typeof(ProjectType), configNode.Name, true);
			if (this.Type == ProjectType.Project && packageElement != null)
			{
				this.Type = ProjectType.ExtensionProject;
			}

			foreach (XmlElement child in configNode.SelectNodes(
				string.Format("*[namespace-uri() != '{0}']", XmlNamespaces.ProjectConfigurationNamespace)))
			{
				customElements.Add(child);
			}

			foreach (XmlElement element in configNode.SelectNodes("p:dependencies/p:extension", nm))
			{
				string extensionName = element.GetAttribute("name");
				if (!this.Dependencies.Contains(extensionName))
					this.Dependencies.Add(extensionName);
			}

			XmlNode variablesNode = configNode.SelectSingleNode("p:variables", nm);
			if (variablesNode != null)
			{
				if (this.VariablesNode != null)
				{
					foreach (XmlElement variableNode in variablesNode.SelectNodes("*"))
					{
						var importReady = this.VariablesNode.OwnerDocument.ImportNode(variableNode, true);
						var id = variableNode.GetAttribute("id");
						var current = this.VariablesNode.SelectSingleNode(
							string.Format("intl:variable[@id='{0}']", id), nm);

						if (current != null)
							this.VariablesNode.ReplaceChild(importReady, current);
						else
							this.VariablesNode.AppendChild(importReady);
					}
				}
				else
				{
					this.VariablesNode = variablesNode;
				}

				this.Variables = new Dictionary<string, NameValueCollection>();

				var variables = variablesNode.SelectNodes("intl:variable", nm);
				foreach (XmlElement elem in variables)
				{
					var values = elem.SelectNodes("intl:value", nm);
					var valueDictionary = new NameValueCollection();

					if (values.Count == 0)
					{
						valueDictionary["default"] = elem.InnerText;
					}
					else
					{
						foreach (XmlElement val in values)
							valueDictionary.Add(val.GetAttribute("locale"), val.InnerText.Trim());
					}

					this.Variables[elem.GetAttribute("id")] = valueDictionary;
				}
			}

			XmlElement routingNode = configNode.SelectSingleElement("p:routing", nm);
			XmlElement pathsNode = configNode.SelectSingleElement("p:paths", nm);

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

			nodeValue = configNode.GetAttribute("autoInternationalize");
			if (!string.IsNullOrEmpty(nodeValue))
				this.AutoInternationalize = nodeValue.ContainsAnyOf("yes", "1", "true");

			nodeValue = configNode.GetAttribute("debugMode");
			if (!string.IsNullOrEmpty(nodeValue))
				this.IsDebugEnabled = nodeValue.ContainsAnyOf("yes", "1", "true");

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
				this.Routing.Parse(routingNode);

			XmlElement linkingNode = configNode.SelectSingleElement("p:linking", nm);
			if (linkingNode != null)
				this.Linking.Parse(linkingNode);

			XmlElement environmentNode = configNode.SelectSingleElement("p:environment", nm);
			if (environmentNode != null)
				this.Environment.Parse(environmentNode);

			XmlElement cachingNode = configNode.SelectSingleElement("p:viewcaching", nm);
			if (cachingNode != null)
				this.ViewCaching.Parse(cachingNode);

			foreach (XmlElement libraryNode in configNode.SelectNodes("p:libraries/p:library", nm))
			{
				ResourceLibraryInfo info = new ResourceLibraryInfo(libraryNode, this.Id);
				this.ResourceLibraries.Add(info.Name, info);
			}

			foreach (XmlElement moduleNode in configNode.SelectNodes("p:modules/p:module", nm))
			{
				ModuleConfiguration moduleConfig = new ModuleConfiguration(moduleNode, this.Id);

				var moduleKey = string.IsNullOrWhiteSpace(moduleConfig.Category)
					? moduleConfig.Name
					: moduleConfig.Category + "/" + moduleConfig.Name;

				this.Modules.Add(moduleKey, moduleConfig);
			}

			var localeNodes = configNode.SelectNodes("p:internationalization/p:locale", nm);
			if (localeNodes.Count != 0)
			{
				this.Locales = new Dictionary<string, LocaleInfo>();
				this.Categories[this.DefaultCategory].Locales.Clear();
				foreach (XmlElement locale in localeNodes)
				{
					var name = locale.GetAttribute("name");
					var info = new LocaleInfo(locale);
					this.Locales[name] = info;
					this.Categories[this.DefaultCategory].Locales.Add(name);
				}
			}

			if (this.Locales.Count != 0 && !this.Locales.ContainsKey(this.DefaultLocale))
			{
				string firstLocale = this.Locales.First().Key;
				log.ErrorFormat("The default locale '{0}' doesn't exist, resetting it to '{1}'.", this.DefaultLocale, firstLocale);
				this.DefaultLocale = firstLocale;
			}

			var categoryNodes = configNode.SelectNodes("p:categories/p:category", nm);
			if (categoryNodes.Count != 0)
			{
				this.Categories.Clear();
				foreach (XmlElement categoryElement in categoryNodes)
				{
					var category = new CategoryInfo(categoryElement);
					this.Categories[category.Name] = category;

					var undefinedLocales = category.Locales.Where(name => !this.Locales.ContainsKey(name)).ToList();
					if (undefinedLocales.Count != 0)
					{
						log.ErrorFormat("Category '{0}' is configured to use these unsupported locales: {1}.",
							category.Name, string.Join(",", undefinedLocales));
					}
				}
			}

			if (this.Categories.Count != 0 && !this.Categories.ContainsKey(this.DefaultCategory))
			{
				string firstCategory = this.Categories.First().Key;
				log.ErrorFormat("The default category '{0}' doesn't exist, resetting it to '{1}'.", this.DefaultCategory, firstCategory);
				this.DefaultCategory = firstCategory;
			}

			foreach (XmlElement viewNode in configNode.SelectNodes("p:metaViews/p:metaView", nm))
			{
				var name = viewNode.GetAttribute("name");
				var info = new MetaViewInfo(viewNode);
				this.MetaViews[name] = info;
			}

			foreach (XmlElement viewNode in configNode.SelectNodes("p:errorViews/p:view", nm))
			{
				var error = viewNode.GetAttribute("error");
				var info = new ErrorViewInfo(viewNode);
				this.ErrorViews[error] = info;
			}

			this.Package = new PackageConfiguration(packageElement);

			if (this.Changed != null)
				this.Changed(this, EventArgs.Empty);
		}

		internal void RegisterExtension(ProjectConfiguration extensionConfig)
		{
			Contract.Requires<ArgumentNullException>(extensionConfig != null);
			Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(extensionConfig.Name));

			string extensionName = extensionConfig.Name;

			extensions[extensionName] = extensionConfig;
			this.MergeExtension(extensionConfig);
		}

		internal void RegisterRoutes()
		{
			foreach (RouteInfo route in this.Routing.Values)
			{
				string[] namespaces = { route.Namespace ?? this.Routing.DefaultNamespace };
				RouteTable.Routes.MapRouteLowercase(route.Name, route.Path,
					route.Defaults,
					route.Constraints,
					namespaces);

				log.DebugFormat("Automatically registering route '{0}' to {1}.{2}",
					route.Path,
					route.Controller,
					route.Action);
			}
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

				extensionConfig.Routing[name].Extension = extensionName;
				this.Routing.Add(name, extensionConfig.Routing[name]);
			}

			foreach (KeyValuePair<string, ExtensionString> pair in extensionConfig.Linking.Links)
			{
				if (this.Linking.Links.ContainsKey(pair.Key))
				{
					log.WarnFormat("Skipped registering link '{0}' from extension '{1}' because a link with the same name already exists.", pair.Key, extensionName);
					continue;
				}

				var newLink = this.Linking.AddLink(pair.Key, pair.Value.Value);
				newLink.Extension = extensionName;
			}

			foreach (KeyValuePair<string, ExtensionString> pair in extensionConfig.Linking.Formats)
			{
				if (this.Linking.Formats.ContainsKey(pair.Key))
				{
					log.WarnFormat("Skipped registering format '{0}' from extension '{1}' because a link with the same name already exists.", pair.Key, extensionName);
					continue;
				}

				var newFormat = this.Linking.AddFormat(pair.Key, pair.Value.Value);
				newFormat.Extension = extensionName;
			}

			foreach (string name in extensionConfig.ResourceLibraries.Keys)
			{
				if (this.ResourceLibraries.ContainsKey(name))
				{
					log.WarnFormat("Skipped registering script library '{0}' from extension '{1}' because a script library with the same name already exists.", name, extensionName);
					continue;
				}

				ResourceLibraryInfo library = extensionConfig.ResourceLibraries[name];
				library.Resources.Each(r => r.Extension = extensionName);
				library.Extension = extensionName;
				this.ResourceLibraries.Add(name, library);
			}

			foreach (string name in extensionConfig.MetaViews.Keys)
			{
				if (this.MetaViews.ContainsKey(name))
				{
					log.WarnFormat("Skipped registering meta view '{0}' from extension '{1}' because a meta view with the same name already exists.", name, extensionName);
					continue;
				}

				MetaViewInfo info = extensionConfig.MetaViews[name];
				info.Extension = extensionName;
				this.MetaViews.Add(name, info);
			}

			foreach (string number in extensionConfig.ErrorViews.Keys)
			{
				if (this.ErrorViews.ContainsKey(number))
				{
					log.WarnFormat("Skipped registering error view '{0}' from extension '{1}' because an error view with the same number already exists.", number, extensionName);
					continue;
				}

				ErrorViewInfo info = extensionConfig.ErrorViews[number];
				info.Extension = extensionName;
				this.ErrorViews.Add(number, info);
			}

			foreach (string name in extensionConfig.Modules.Keys)
			{
				if (this.Modules.ContainsKey(name))
				{
					log.WarnFormat("Skipped registering module '{0}' from extension '{1}' because a module with the same name already exists.", name, extensionName);
					continue;
				}

				ModuleConfiguration module = extensionConfig.Modules[name];
				module.Resources.Each(r => r.Extension = extensionName);
				module.Extension = extensionName;
				this.Modules.Add(name, module);
			}
		}
	}
}
