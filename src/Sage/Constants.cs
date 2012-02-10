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
namespace Sage
{
	using System.Xml;

	public enum ConfigurationType
	{
		System,
		Project,
		Extension,
	}

	public enum ProblemType
	{
		Unknown = 0,
		InvalidMarkup,
		InvalidHtmlMarkup,
		MissingNamespaceDeclaration,
		MissingConfigurationFile,
		ConfigurationMissingLocales,
		ConfigurationMissingCategories,
	}

	public enum ViewSource
	{
		BuiltIn = 0,
		Specific = 1,
		Category = 2,
		Project = 3,
	}

	public enum ModuleAutoLocation
	{
		None = 0,
		Head = 1,
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
		/// Indicates that an error occured during processing of the module.
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
		Undefined = 0,

		/// <summary>
		/// Indicates a script resource.
		/// </summary>
		Script = 1,

		/// <summary>
		/// Indicates a style resource.
		/// </summary>
		Style = 2,

		/// <summary>
		/// Indicates an XML document resource.
		/// </summary>
		Document = 3,

		/// <summary>
		/// Indicates an XSLT stylesheet resource.
		/// </summary>
		Stylesheet = 4,
	}

	/// <summary>
	/// Possible render location for resource types (scripts and styles).
	/// </summary>
	public enum ResourceLocation
	{
		/// <summary>
		/// Inidicates that the resource is meant to appear in the input XML document, not in the final HTML directly.
		/// </summary>
		Data = 0,

		/// <summary>
		/// Inidicates that the resource is meant to appear in the HTML &lt;head/&gt; element.
		/// </summary>
		Head = 1,

		/// <summary>
		/// Inidicates that the resource is meant to appear in the HTML &lt;body/&gt; element.
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

		public static class Extensions
		{
			public const string IO = "http://www.cycle99.com/projects/sage/xslt/extensions/io";

			public const string Regexp = "http://www.cycle99.com/projects/sage/xslt/extensions/regexp";

			public const string Math = "http://www.cycle99.com/projects/sage/xslt/extensions/math";

			public const string String = "http://www.cycle99.com/projects/sage/xslt/extensions/string";

			public const string Set = "http://www.cycle99.com/projects/sage/xslt/extensions/set";

			public const string Date = "http://www.cycle99.com/projects/sage/xslt/extensions/date";
		}
	}
}
