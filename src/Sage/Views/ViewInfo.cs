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
namespace Sage.Views
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Xml;

	using Kelp.Extensions;

	using Mvp.Xml.XInclude;
	using Mvp.Xml.XPointer;

	using Sage.Controllers;
	using Sage.ResourceManagement;

	/// <summary>
	/// Provides information about view template and configuration file corresponding to a controller and an action.
	/// </summary>
	public class ViewInfo
	{
		internal const string DefaultBuiltInStylesheetPath = "sageresx://sage/resources/xslt/global.xslt";
		internal const string DefaultStylesheet = "default";

		internal static readonly string[] ConfigExtensions = 
			new[] { ".xml", ".html", ".txt" };

		internal static readonly string[] ViewExtensions =
			new[] { ".aspx", ".xsl", ".xslt" };

		private readonly SageContext context;
		private CacheableXmlDocument configDoc;
		private XsltTransform processor;

		/// <summary>
		/// Initializes a new instance of the <see cref="ViewInfo"/> class, using the specified 
		/// <paramref name="controller"/> and <paramref name="action"/>.
		/// </summary>
		/// <param name="controller">The controller for which to create this instance.</param>
		/// <param name="action">The controller action for which to create this instance.</param>
		public ViewInfo(SageController controller, string action)
		{
			this.context = controller.Context;
			this.ContentType = "text/html";
			this.Controller = controller;
			this.Action = action;
			this.ConfigPath = null;

			this.ViewSource = ViewSource.BuiltIn;
			this.ViewPath = DefaultBuiltInStylesheetPath;

			string pathTemplate = controller.IsShared ? context.Path.SharedViewPath : context.Path.ViewPath;
			string folderPath = context.MapPath(string.Format("{0}/{1}", pathTemplate, controller.ControllerName));
			string basePath = context.MapPath(string.Format("{0}/{1}", folderPath, action));

			foreach (string extension in ConfigExtensions)
			{
				var path = basePath + extension;
				if (!File.Exists(path))
					continue;

				this.ConfigPath = path;
				this.ConfigExists = true;
				break;
			}

			foreach (string extension in ViewExtensions)
			{
				var path = basePath + extension;
				if (!File.Exists(path))
					continue;

				this.ViewPath = path;
				this.ViewSource = ViewSource.Specific;
				break;
			}

			if (this.ViewSource == ViewSource.BuiltIn && File.Exists(context.Path.DefaultStylesheetPath))
			{
				this.ViewPath = context.Path.DefaultStylesheetPath;
				this.ViewSource = ViewSource.Project;
			}
		}

		/// <summary>
		/// Gets the controller associated with the view this object represents.
		/// </summary>
		public SageController Controller { get; private set; }

		/// <summary>
		/// Gets the action associated with the view this object represents.
		/// </summary>
		public string Action { get; private set; }

		/// <summary>
		/// Gets the path to the view file (either the XML template or XSLY stylesheet) 
		/// corresponding to this object.
		/// </summary>
		public string ViewPath { get; private set; }

		/// <summary>
		/// Gets a number indicating the source of the view stylesheet (specific, global or category).
		/// </summary>
		public ViewSource ViewSource { get; private set; }

		/// <summary>
		/// Gets the path to the view configuration file associated with the view this object represents.
		/// </summary>
		public string ConfigPath { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this view's configuration file exists.
		/// </summary>
		public bool ConfigExists { get; private set; }

		/// <summary>
		/// Gets or sets the content type associated with the view this object represents.
		/// </summary>
		public string ContentType { get; set; }

		/// <summary>
		/// Gets the file extension of this view.
		/// </summary>
		public string ViewExtension
		{
			get
			{
				return Path.GetExtension(this.ViewPath);
			}
		}

		/// <summary>
		/// Gets the file extension associated with this view's configuration file.
		/// </summary>
		public string ConfigExtension
		{
			get
			{
				return Path.GetExtension(this.ConfigPath);
			}
		}

		/// <summary>
		/// Gets a value indicating whether the view or view configuration exist.
		/// </summary>
		public bool Exists
		{
			get
			{
				return this.ConfigExists || this.ViewSource == Sage.ViewSource.Specific;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance represents an XSLT view.
		/// </summary>
		public bool IsXsltView
		{
			get
			{
				return this.Exists && this.ViewExtension.IndexOf("xsl", System.StringComparison.Ordinal) != -1;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the view specifies that it should not be cached.
		/// </summary>
		public bool IsNoCacheView
		{
			get
			{
				if (!this.ConfigExists)
					return false;

				XmlNamespaceManager nm = XmlNamespaces.Manager;
				return
					ConfigDocument.SelectSingleNode("//xhtml:meta[@http-equiv='cache-control' and @content='no-cache']", nm) != null ||
					ConfigDocument.SelectSingleNode("//xhtml:meta[@http-equiv='pragma' and @content='no-cache']", nm) != null;
			}
		}

		/// <summary>
		/// Gets the last modified date associated with the view this object represents.
		/// </summary>
		public DateTime? LastModified
		{
			get
			{
				if (!this.Exists || this.IsNoCacheView)
					return null;

				DateTime? lastModified1 = null;
				DateTime? lastModified2 = null;

				if (this.ViewSource != 0)
					lastModified1 = context.LmCache.Get(this.ViewPath);

				if (this.ConfigExists)
					lastModified2 = context.LmCache.Get(this.ConfigPath);

				if (lastModified1 == null)
				{
					if (this.ViewPath != null)
					{
						lastModified1 = this.Processor.LastModified;
						context.LmCache.Put(this.ViewPath, lastModified1.Value, this.Processor.Dependencies.ToList());
					}
					else
					{
						lastModified1 = DateTime.MinValue;
					}
				}

				if (lastModified2 == null)
				{
					if (this.ConfigExists)
					{
						lastModified2 = this.ConfigDocument.LastModified;
						context.LmCache.Put(this.ConfigPath, lastModified2.Value, this.ConfigDocument.Dependencies.ToList());
					}
					else
					{
						lastModified2 = DateTime.MinValue;
					}
				}
	
				return new DateTime(Math.Max(lastModified1.Value.Ticks, lastModified2.Value.Ticks));
			}
		}

		/// <summary>
		/// Gets the view configuration document associated with the view this object represents.
		/// </summary>
		public CacheableXmlDocument ConfigDocument
		{
			get
			{
				if (configDoc == null)
				{
					if (ConfigExists)
					{
						try
						{
							configDoc = context.Resources.LoadXml(ConfigPath);
							string defaultNamespace = configDoc.DocumentElement.GetAttribute("xmlns");
							if (string.IsNullOrEmpty(defaultNamespace))
								configDoc.DocumentElement.SetAttribute("xmlns", XmlNamespaces.XHtmlNamespace);

							configDoc.LoadXml(configDoc.InnerXml);
						}
						catch (Exception ex)
						{
							SageHelpException helpError = SageHelpException.Create(ex, this.ConfigPath);
							if (helpError.Problem.Type == ProblemType.Unknown)
								throw;

							throw helpError;
						}
					}
				}

				return configDoc;
			}

			internal set
			{
				configDoc = value;
			}
		}

		/// <summary>
		/// Gets the XSLT processor associated with the view this object represents.
		/// </summary>
		public XsltTransform Processor
		{
			get
			{
				if (processor == null)
				{
					if (this.ViewSource != ViewSource.BuiltIn)
					{
						try
						{
							processor = XsltTransform.Create(this.context, this.ViewPath);
						}
						catch (Exception ex)
						{
							ProblemInfo problem = new ProblemInfo(ProblemType.XsltLoadError, this.ViewPath);
							throw new SageHelpException(problem, ex);
						}
					}
					else
					{
						processor = XsltTransform.Create(this.context, DefaultBuiltInStylesheetPath);
					}
				}

				return processor;
			}

			internal set
			{
				processor = value;
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("{0}/{1} ({2})", Controller, Action, ViewPath);
		}
	}
}
