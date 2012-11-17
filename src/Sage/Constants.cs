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
namespace Sage
{
	using System.Xml;

	/// <summary>
	/// Defines the user agent types.
	/// </summary>
	public enum UserAgentType
	{
		/// <summary>
		/// Represents no user agent.
		/// </summary>
		None = 0,

		/// <summary>
		/// Signifies a web browser, such as Mozilla Firefox.
		/// </summary>
		Browser,

		/// <summary>
		/// Signifies a crawler, such as <c>Googlebot</c>.
		/// </summary>
		Crawler,
	}

	/// <summary>
	/// Defines configuration types.
	/// </summary>
	public enum ConfigurationType
	{
		/// <summary>
		/// System configuration
		/// </summary>
		System,

		/// <summary>
		/// Project configuration
		/// </summary>
		Project,

		/// <summary>
		/// Extension configuration
		/// </summary>
		Extension,
	}

	/// <summary>
	/// Defines the problem types that Sage tries to recognize and assist the user with.
	/// </summary>
	public enum ProblemType
	{
		/// <summary>
		/// Represents initial state, no error
		/// </summary>
		None = -1,

		/// <summary>
		/// Represents an unknown error
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Indicates an error that occurred during initialization of the project configuration.
		/// </summary>
		ConfigurationInitializationError,

		/// <summary>
		/// Indicates a schema validation error within the project configuration.
		/// </summary>
		ConfigurationSchemaError,

		/// <summary>
		/// Indicates an error that occurs during contextualization step of view transform.
		/// </summary>
		ContextualizeError,

		/// <summary>
		/// Indicates that an error occurred during extension installation.
		/// </summary>
		ExtensionInstallError,

		/// <summary>
		/// Indicates an error that occurred while parsing the schema of extension configuration file.
		/// </summary>
		ExtensionSchemaValidationError,

		/// <summary>
		/// Occurs if the expression specified with the <c>xpointer</c> attribute of an 
		/// <c>xi:include</c> element fails to select something.
		/// </summary>
		IncludeFragmentNotFound,

		/// <summary>
		/// Occurs if the resource specified with an <c>xi:include</c> element can't be found.
		/// </summary>
		IncludeNotFound,

		/// <summary>
		/// Occurs if the expression specified with the <c>xpointer</c> attribute generates a syntax error.
		/// </summary>
		IncludeSyntaxError,

		/// <summary>
		/// Indicates invalid markup in XML
		/// </summary>
		InvalidMarkup,

		/// <summary>
		/// Indicates invalid markup in HTML
		/// </summary>
		InvalidHtmlMarkup,

		/// <summary>
		/// Occurs when the system fails to find any configuration files in a project.
		/// </summary>
		MissingConfigurationFile,

		/// <summary>
		/// Indicates that the project is missing an essential dependency.
		/// </summary>
		MissingDependency,

		/// <summary>
		/// Indicates that one of the project extensions is missing an essential dependency.
		/// </summary>
		MissingExtensionDependency,

		/// <summary>
		/// Occurs when a qualified name is used in XML without providing the corresponding namespace declaration.
		/// </summary>
		MissingNamespaceDeclaration,

		/// <summary>
		/// Indicates a generic error that occurs during the project initialization phase.
		/// </summary>
		ProjectInitializationError,

		/// <summary>
		/// Indicates an error that occurred while parsing the schema of project configuration file.
		/// </summary>
		ProjectSchemaValidationError,

		/// <summary>
		/// Indicates an error that occurs during processing of view modules.
		/// </summary>
		ResourceProcessingError,

		/// <summary>
		/// Occurs if an exception is raised during the XSLT transform step.
		/// </summary>
		TransformError,

		/// <summary>
		/// Occurs if the XSLT transform fails to produce anything.
		/// </summary>
		TransformResultMissingRootElement,

		/// <summary>
		/// Occurs if an exception is raised during initialization of the view document.
		/// </summary>
		ViewDocumentInitError,

		/// <summary>
		/// Indicates an error that occurs during processing of view modules.
		/// </summary>
		ViewProcessingError,

		/// <summary>
		/// Indicates an error that occurs during processing of view filters.
		/// </summary>
		ViewXmlFilteringError,

		/// <summary>
		/// Occurs if an exception is raised during loading of an XSLT style-sheet.
		/// </summary>
		XsltLoadError,
	}

	/// <summary>
	/// Defines the project types.
	/// </summary>
	public enum ProjectType
	{
		/// <summary>
		/// Represents the master project.
		/// </summary>
		Project = 0,

		/// <summary>
		/// Represents an extension project.
		/// </summary>
		Extension = 1,
	}

	/// <summary>
	/// Specifies the possible sources of an XSLT view; where a view's style-sheet is loaded from.
	/// </summary>
	public enum ViewSource
	{
		/// <summary>
		/// Indicates the default, built-in XSLT style-sheet.
		/// </summary>
		BuiltIn = 0,

		/// <summary>
		/// Indicates a specific style-sheet created specifically for the current view.
		/// </summary>
		Specific = 1,

		/// <summary>
		/// Indicates a style-sheet that is shared across the whole project category (in a multi-category setup).
		/// </summary>
		Category = 2,

		/// <summary>
		/// Indicates a style-sheet that is shared across the project.
		/// </summary>
		Project = 3,
	}

	/// <summary>
	/// Specifies the location of a module that will be auto included.
	/// </summary>
	public enum ModuleAutoLocation
	{
		/// <summary>
		/// Nowhere
		/// </summary>
		None = 0,

		/// <summary>
		/// Within the head of the HTML document
		/// </summary>
		Head = 1,

		/// <summary>
		/// Within the body of the HTML document
		/// </summary>
		Body = 2,
	}

	/// <summary>
	/// Defines the module result types.
	/// </summary>
	public enum ModuleResultStatus
	{
		/// <summary>
		/// Indicates the default, zero-state.
		/// </summary>
		None = 0,

		/// <summary>
		/// Indicates that the operation completed successfully.
		/// </summary>
		Ok = 1,

		/// <summary>
		/// Indicates that the operation completed successfully, but yielded no results.
		/// </summary>
		NoData = 2,

		/// <summary>
		/// Indicates that the operation could not be completed because one or more required parameters
		/// were missing from the context.
		/// </summary>
		MissingParameters = 3,

		/// <summary>
		/// Indicates that an error in module's configuration is preventing the module from operating normally.
		/// </summary>
		ConfigurationError = 4,

		/// <summary>
		/// Indicates that an error occurred during processing of the module.
		/// </summary>
		ModuleError = 5,

		/// <summary>
		/// Indicates that the module even though module processing completed normally, there was a warning.
		/// </summary>
		ModuleWarning = 6,
	}

	/// <summary>
	/// Defines the message types that controllers may send to views.
	/// </summary>
	public enum MessageType
	{
		/// <summary>
		/// Indicates an informational message.
		/// </summary>
		Info = 1,

		/// <summary>
		/// Indicates a warning message.
		/// </summary>
		Warning = 2,

		/// <summary>
		/// Indicates an error message.
		/// </summary>
		Error = 3,

		/// <summary>
		/// Indicates an development specific message.
		/// </summary>
		Development = 10
	}

	/// <summary>
	/// Indicates recognized resource types
	/// </summary>
	public enum ResourceType
	{
		/// <summary>
		/// Indicates an unknown resource type.
		/// </summary>
		Undefined = 0,

		/// <summary>
		/// Indicates an icon resource (usually the <c>favicon.ico</c>)
		/// </summary>
		Icon = 1,

		/// <summary>
		/// Indicates a style resource.
		/// </summary>
		Style = 2,

		/// <summary>
		/// Indicates a script resource.
		/// </summary>
		Script = 3,

		/// <summary>
		/// Indicates an XML document resource.
		/// </summary>
		Document = 4,
	}

	/// <summary>
	/// Possible render location for resource types (scripts and styles).
	/// </summary>
	public enum ResourceLocation
	{
		/// <summary>
		/// Indicates that the resource is meant to appear in the input XML document, not in the final HTML directly.
		/// </summary>
		Data = 0,

		/// <summary>
		/// Indicates that the resource is meant to appear in the HTML &lt;head/&gt; element.
		/// </summary>
		Head = 1,

		/// <summary>
		/// Indicates that the resource is meant to appear in the HTML &lt;body/&gt; element.
		/// </summary>
		Body = 2,
	}

	/// <summary>
	/// Defines the namespaces and the namespace manager in use throughout the system.
	/// </summary>
	public static class XmlNamespaces
	{
		/// <summary>
		/// Specifies the category configuration namespace.
		/// </summary>
		public const string CategoryConfigurationNamespace = "http://www.cycle99.com/schemas/sage/configuration/category.xsd";

		/// <summary>
		/// Specifies the prefix for the category configuration namespace.
		/// </summary>
		public const string CategoryConfigurationPrefix = "c";

		/// <summary>
		/// Defines the contextualization namespace.
		/// </summary>
		public const string ContextualizationNamespace = "http://www.cycle99.com/schemas/sage/contextualization.xsd";

		/// <summary>
		/// Defines the contextualization namespace prefix.
		/// </summary>
		public const string ContextualizationNsPrefix = "context";

		/// <summary>
		/// Defines the <c>intenationalization</c> namespace.
		/// </summary>
		public const string InternationalizationNamespace = "http://www.cycle99.com/schemas/sage/internationalization.xsd";

		/// <summary>
		/// Defines the prefix for the <c>intenationalization</c> namespace.
		/// </summary>
		public const string InternationalizationNsPrefix = "intl";

		/// <summary>
		/// Defines the modules namespace.
		/// </summary>
		public const string ModulesNamespace = "http://www.cycle99.com/schemas/sage/modules.xsd";

		/// <summary>
		/// Defines the prefix for the modules namespace.
		/// </summary>
		public const string ModulesNsPrefix = "mod";

		/// <summary>
		/// Specifies the project configuration namespace.
		/// </summary>
		public const string ProjectConfigurationNamespace = "http://www.cycle99.com/schemas/sage/configuration/project.xsd";

		/// <summary>
		/// Specifies the prefix for the project configuration namespace.
		/// </summary>
		public const string ProjectConfigurationPrefix = "p";

		/// <summary>
		/// Defines the main Sage namespace.
		/// </summary>
		public const string SageNamespace = "http://www.cycle99.com/schemas/sage/sage.xsd";

		/// <summary>
		/// Defines the prefix for the main Sage namespace.
		/// </summary>
		public const string SageNsPrefix = "sage";

		/// <summary>
		/// Specifies the sitemap configuration namespace.
		/// </summary>
		public const string SitemapConfigurationNamespace = "http://www.cycle99.com/schemas/sage/configuration/sitemap.xsd";

		/// <summary>
		/// Specifies the prefix for the sitemap configuration namespace.
		/// </summary>
		public const string SitemapConfigurationPrefix = "s";

		/// <summary>
		/// Defines the standard XHTML namespace.
		/// </summary>
		public const string XHtmlNamespace = "http://www.w3.org/1999/xhtml";

		/// <summary>
		/// Defines the prefix for the standard XHTML namespace.
		/// </summary>
		public const string XHtmlPrefix = "xhtml";

		/// <summary>
		/// Defines the standard X-Include namespace.
		/// </summary>
		public const string XIncludeNamespace = "http://www.w3.org/2003/XInclude";

		/// <summary>
		/// Defines the prefix for the standard X-Include namespace.
		/// </summary>
		public const string XIncludePrefix = "xi";

		/// <summary>
		/// Defines the standard XSL namespace.
		/// </summary>
		public const string XslNamespace = "http://www.w3.org/1999/XSL/Transform";

		/// <summary>
		/// Defines the prefix for the standard XSL namespace.
		/// </summary>
		public const string XslPrefix = "xsl";

		private static volatile XmlNamespaceManager nsman;

		/// <summary>
		/// Gets the <see cref="XmlNamespaceManager"/> that can be used everywhere where selecting with namespaces needs to be done.
		/// </summary>
		public static XmlNamespaceManager Manager
		{
			get
			{
				if (nsman == null)
				{
					lock (SageNsPrefix)
					{
						if (nsman == null)
						{
							var kelpManager = Kelp.XmlNamespaces.Manager;
							var tempManager = new XmlNamespaceManager(kelpManager.NameTable);
							var nspairs = Kelp.XmlNamespaces.Manager.GetNamespacesInScope(XmlNamespaceScope.Local);
							foreach (string key in nspairs.Keys)
								tempManager.AddNamespace(key, nspairs[key]);

							tempManager.AddNamespace(SageNsPrefix, SageNamespace);
							tempManager.AddNamespace(ContextualizationNsPrefix, ContextualizationNamespace);
							tempManager.AddNamespace(ProjectConfigurationPrefix, ProjectConfigurationNamespace);
							tempManager.AddNamespace(CategoryConfigurationPrefix, CategoryConfigurationNamespace);
							tempManager.AddNamespace(SitemapConfigurationPrefix, SitemapConfigurationNamespace);
							tempManager.AddNamespace(InternationalizationNsPrefix, InternationalizationNamespace);
							tempManager.AddNamespace(ModulesNsPrefix, ModulesNamespace);
							tempManager.AddNamespace(XHtmlPrefix, XHtmlNamespace);
							tempManager.AddNamespace(XIncludePrefix, XIncludeNamespace);
							tempManager.AddNamespace(XslPrefix, XslNamespace);
							tempManager.AddNamespace("msxsl", "urn:schemas-microsoft-com:xslt");

							nsman = tempManager;
						}
					}
				}

				return nsman;
			}
		}

		/// <summary>
		/// Defines the namespaces for the XSLT
		/// </summary>
		public static class Extensions
		{
			/// <summary>
			/// Defines the namespace for the Basic extensions.
			/// </summary>
			public const string Basic = "http://www.cycle99.com/schemas/sage/xslt/extensions/basic.xsd";

			/// <summary>
			/// Defines the namespace for the Date extensions.
			/// </summary>
			public const string Date = "http://www.cycle99.com/schemas/sage/xslt/extensions/date.xsd";

			/// <summary>
			/// Defines the namespace for the IO extensions.
			/// </summary>
			public const string IO = "http://www.cycle99.com/schemas/sage/xslt/extensions/io.xsd";

			/// <summary>
			/// Defines the namespace for regular expression extensions.
			/// </summary>
			public const string Regex = "http://www.cycle99.com/schemas/sage/xslt/extensions/regex.xsd";

			/// <summary>
			/// Defines the namespace for the Set extensions.
			/// </summary>
			public const string Set = "http://www.cycle99.com/schemas/sage/xslt/extensions/set.xsd";

			/// <summary>
			/// Defines the namespace for the String extensions.
			/// </summary>
			public const string String = "http://www.cycle99.com/schemas/sage/xslt/extensions/string.xsd";
		}
	}
}
