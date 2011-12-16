namespace Sage.Views
{
	using System;
	using System.IO;
	using System.Linq;

	using Sage.Controllers;
	using Sage.ResourceManagement;
	using Sage.Xml;

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
		private readonly bool directoryExists;
		private CacheableXmlDocument configDoc;
		private CacheableXslTransform transform;

		/// <summary>
		/// Initializes a new instance of the <see cref="ViewInfo"/> class, using the specified 
		/// <paramref name="controller"/> and <paramref name="action"/>.
		/// </summary>
		/// <param name="controller">The controller for which to create this object.</param>
		/// <param name="action">The action for which to create this object.</param>
		public ViewInfo(SageController controller, string action)
		{
			this.context = controller.Context;
			this.ContentType = "text/html";
			this.Controller = controller;
			this.Action = action;
			this.ConfigPath = null;
			this.ViewSource = ViewSource.BuiltIn;

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

			this.directoryExists = Directory.Exists(folderPath);

			if (this.ViewSource == ViewSource.BuiltIn && File.Exists(context.Path.CategoryStylesheetPath))
			{
				this.ViewPath = context.Path.CategoryStylesheetPath;
				this.ViewSource = ViewSource.Category;
			}

			if (this.ViewSource == ViewSource.BuiltIn && File.Exists(context.Path.DefaultStylesheetPath))
			{
				this.ViewPath = context.Path.CategoryStylesheetPath;
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
		/// Gets the path to the view file corresponding to this object.
		/// </summary>
		public string ViewPath
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a number indicating the source of the view stylesheet (specific, global or category).
		/// </summary>
		public ViewSource ViewSource
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the path to the view configuration file associated with the view this object represents.
		/// </summary>
		public string ConfigPath { get; private set; }

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
		/// Returns true if either the view or view configuration exist.
		/// </summary>
		public bool Exists
		{
			get
			{
				return this.directoryExists;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this view's configuration file exists.
		/// </summary>
		public bool ConfigExists { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this instance represents an XSLT view.
		/// </summary>
		public bool IsXsltView
		{
			get
			{
				return this.Exists && this.ViewExtension.IndexOf("xsl") != -1;
			}
		}

		/// <summary>
		/// Gets the last modified date associated with the view this object represents.
		/// </summary>
		public DateTime? LastModified
		{
			get
			{
				if (!this.Exists)
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
						lastModified1 = this.CacheableTransform.LastModified;
						context.LmCache.Put(this.ViewPath, lastModified1.Value, this.CacheableTransform.Dependencies.ToList());
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
						configDoc = context.Resources.LoadXml(ConfigPath);
				}

				return configDoc;
			}

			internal set
			{
				configDoc = value;
			}
		}

		/// <summary>
		/// Gets the XSLT transform associated with the view this object represents.
		/// </summary>
		public CacheableXslTransform CacheableTransform
		{
			get
			{
				if (transform == null)
				{
					if (this.ViewSource != 0)
					{
						transform = XsltRegistry.Load(this.ViewPath, this.context);
					}
					else
					{
						transform = XsltRegistry.Load(DefaultBuiltInStylesheetPath, this.context);
					}
				}

				return transform;
			}

			internal set
			{
				transform = value;
			}
		}

		/// <summary>
		/// Returns a <see cref="String"/> that represents this instance.
		/// </summary>
		public override string ToString()
		{
			return string.Format("{0}/{1} ({2})", Controller, Action, ViewPath);
		}

	}
}
