namespace Sage
{
	using System.Xml;

	public enum ViewSource
	{
		BuiltIn = 0,
		Specific = 1,
		Category = 2,
		Project = 3,
	}

	/// <summary>
	/// Defines the module result types.
	/// </summary>
	public enum ModuleResultStatus
	{
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
		/// Inidicates a script resource.
		/// </summary>
		Script = 1,

		/// <summary>
		/// Inidicates a style resource.
		/// </summary>
		Style = 2,
	}

	/// <summary>
	/// Possible render location for resource types (scripts and styles).
	/// </summary>
	public enum ResourceLocation
	{
		/// <summary>
		/// Inidicates the HTML &lt;head/&gt; element.
		/// </summary>
		Head = 1,

		/// <summary>
		/// Inidicates the HTML &lt;body/&gt; element.
		/// </summary>
		Body = 2,
	}

	/// <summary>
	/// Defines the namespaces and the namespace manager in use throughout the system.
	/// </summary>
	public static class XmlNamespaces
	{
		/// <summary>
		/// Defines the prefix for the main White Label namespace.
		/// </summary>
		public const string SageNsPrefix = "sage";

		/// <summary>
		/// Defines the main White Label namespace.
		/// </summary>
		public const string SageNamespace = "http://www.cycle99.com/projects/sage";

		/// <summary>
		/// Defines the contextualization namespace prefix.
		/// </summary>
		public const string ContextualizationNsPrefix = "context";
		
		/// <summary>
		/// Defines the contextualization namespace.
		/// </summary>
		public const string ContextualizationNamespace = "http://www.cycle99.com/projects/sage/contextualization";

		/// <summary>
		/// Specifies the prefix for the project configuration namespace.
		/// </summary>
		public const string ProjectConfigurationPrefix = "p";

		/// <summary>
		/// Specifies the project configuration namespace.
		/// </summary>
		public const string ProjectConfigurationNamespace = "http://www.cycle99.com/projects/sage/configuration/project";

		/// <summary>
		/// Specifies the prefix for the category configuration namespace.
		/// </summary>
		public const string CategoryConfigurationPrefix = "c";

		/// <summary>
		/// Specifies the category configuration namespace.
		/// </summary>
		public const string CategoryConfigurationNamespace = "http://www.cycle99.com/projects/sage/configuration/category";

		/// <summary>
		/// Specifies the prefix for the developer (dev tools) configuration namespace.
		/// </summary>
		public const string DeveloperConfigurationPrefix = "dev";

		/// <summary>
		/// Specifies the developer (dev tools) configuration namespace.
		/// </summary>
		public const string DeveloperConfigurationNamespace = "http://www.cycle99.com/projects/sage/configuration/devtools";

		/// <summary>
		/// Defines the prefix for the modules namespace.
		/// </summary>
		public const string ModulesNsPrefix = "mod";

		/// <summary>
		/// Defines the modules namespace.
		/// </summary>
		public const string ModulesNamespace = "http://www.cycle99.com/projects/sage/modules";

		/// <summary>
		/// Defines the prefix for the intenationalization namespace.
		/// </summary>
		public const string InternationalizationNsPrefix = "intl";

		/// <summary>
		/// Defines the intenationalization namespace.
		/// </summary>
		public const string InternationalizationNamespace = "http://www.cycle99.com/projects/sage/internationalization";

		/// <summary>
		/// Defines the prefix for the standard XHTML namespace.
		/// </summary>
		public const string XHtmlPrefix = "xhtml";

		/// <summary>
		/// Defines the standard XHTML namespace.
		/// </summary>
		public const string XHtmlNamespace = "http://www.w3.org/1999/xhtml";

		/// <summary>
		/// Defines the prefix for the standard XSL namespace.
		/// </summary>
		public const string XslPrefix = "xsl";

		/// <summary>
		/// Defines the standard XSL namespace.
		/// </summary>
		public const string XslNamespace = "http://www.w3.org/1999/XSL/Transform";

		/// <summary>
		/// Defines the prefix for the standard X-Include namespace.
		/// </summary>
		public const string XIncludePrefix = "xi";

		/// <summary>
		/// Defines the standard X-Include namespace.
		/// </summary>
		public const string XIncludeNamespace = "http://www.w3.org/2003/XInclude";

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
							tempManager.AddNamespace(DeveloperConfigurationPrefix, DeveloperConfigurationNamespace);
							tempManager.AddNamespace(InternationalizationNsPrefix, InternationalizationNamespace);
							tempManager.AddNamespace(ModulesNsPrefix, ModulesNamespace);
							tempManager.AddNamespace(XHtmlPrefix, XHtmlNamespace);
							tempManager.AddNamespace(XIncludePrefix, XIncludeNamespace);
							tempManager.AddNamespace(XslPrefix, XslNamespace);

							nsman = tempManager;
						}
					}
				}

				return nsman;
			}
		}
	}
}
