﻿namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Web;
	using System.Web.Hosting;

	using Kelp.Core;
	using Sage.Configuration;

	/// <summary>
	/// Provides a class that resolves physical file paths based on current category, locale and any fallback scenario 
	/// currently in place.
	/// </summary>
	public class PathResolver
	{
		private readonly SageContext context;
		private static Dictionary<string, string> virtualDirectories;

		/// <summary>
		/// Initializes a new instance of the <see cref="PathResolver"/> class using the specified <see cref="SageContext"/>
		/// to retrieve current category and locale information.
		/// </summary>
		/// <param name="context">The current <see cref="SageContext"/>.</param>
		public PathResolver(SageContext context)
		{
			this.context = context;
		}

		/// <summary>
		/// Returns the physical path to the root folder of the current application. 
		/// </summary>
		public static string ApplicationPhysicalPath
		{
			get
			{
				if (HttpContext.Current != null)
				{
					var ctx = HttpContext.Current;
					return ctx.Server.MapPath(ctx.Request.ApplicationPath);
				}

				return HostingEnvironment.ApplicationPhysicalPath;
			}
		}

		/// <summary>
		/// Gets the view path of the currently active category.
		/// </summary>
		public string ViewPath
		{
			get
			{
				return context.ProjectConfiguration.PathTemplates.View.Replace("{assetpath}", this.AssetPath);
			}
		}

		/// <summary>
		/// Gets the view path of the currently active category.
		/// </summary>
		public string ModulePath
		{
			get
			{
				return context.ProjectConfiguration.PathTemplates.Module.Replace("{assetpath}", this.AssetPath);
			}
		}

		/// <summary>
		/// Gets the path in which to look for extensions.
		/// </summary>
		public string ExtensionPath
		{
			get
			{
				return this.Resolve(context.ProjectConfiguration.PathTemplates.Extension);
			}
		}

		/// <summary>
		/// Gets the physical view path of the currently active category.
		/// </summary>
		public string PhysicalViewPath
		{
			get
			{
				return context.MapPath(ViewPath);
			}
		}

		/// <summary>
		/// Gets the path to the category configuration document.
		/// </summary>
		public string CategoryConfigurationPath
		{
			get
			{
				return this.Substitute(context.ProjectConfiguration.PathTemplates.CategoryConfiguration);
			}
		}

		public string PhysicalCategoryConfigurationPath
		{
			get
			{
				return this.Resolve(CategoryConfigurationPath);
			}
		}

		/// <summary>
		/// Gets the path to the shared views.
		/// </summary>
		public string SharedViewPath
		{
			get
			{
				return context.ProjectConfiguration.PathTemplates.View.Replace("{assetpath}", this.SharedAssetPath);
			}
		}

		/// <summary>
		/// Gets the physical path to the shared views.
		/// </summary>
		public string PhysicalSharedViewPath
		{
			get
			{
				return this.Resolve(context.ProjectConfiguration.PathTemplates.View, 
					new QueryString { { "category", context.ProjectConfiguration.SharedCategory } });
			}
		}

		/// <summary>
		/// Gets the asset path for the current category.
		/// </summary>
		public string AssetPath
		{
			get
			{
				return this.GetAssetPath(context.Category);
			}
		}

		/// <summary>
		/// Gets the asset path for the shared category.
		/// </summary>
		public string SharedAssetPath
		{
			get
			{
				return this.GetAssetPath(context.ProjectConfiguration.SharedCategory);
			}
		}

		public string DefaultStylesheetPath
		{
			get
			{
				return this.Resolve(context.ProjectConfiguration.PathTemplates.DefaultStylesheet);
			}
		}

		public Dictionary<string, string> VirtualDirectories
		{
			get
			{
				return virtualDirectories ?? (virtualDirectories = Application.GetVirtualDirectories(this.context));
			}
		}

		/// <summary>
		/// Returns the path to the assets folder of the specified <paramref name="category"/>.
		/// </summary>
		/// <param name="category">The category for which to return the path</param>
		/// <returns>The physical path to the assets folder of the specified <paramref name="category"/></returns>
		public string GetAssetPath(string category)
		{
			return context.ProjectConfiguration.AssetPath.Replace("{category}", category);
		}

		public string GetModulePath(string moduleName)
		{
			string templatePath = string.Concat(this.ModulePath.TrimEnd('/'), "/", moduleName);
			return this.Resolve(templatePath);
		}

		/// <summary>
		/// Expands the specified <paramref name="childPath"/> to its full path within the module with the specified
		/// <paramref name="moduleName"/>.
		/// </summary>
		/// <param name="moduleName">The name of the module for which the path is being expanded.</param>
		/// <param name="childPath">The path to expand</param>
		/// <returns>The expended version of the specified <paramref name="childPath"/>. If the <paramref name="childPath"/>
		/// is either an absolute web or absolute local path, the path will not be expanded.</returns>
		public string GetModulePath(string moduleName, string childPath)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(moduleName));
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(childPath));

			bool isRooted = new Uri(childPath, UriKind.RelativeOrAbsolute).IsAbsoluteUri || Path.IsPathRooted(childPath);
			if (isRooted)
				return childPath;

			if (childPath.StartsWith("~/"))
				return context.MapPath(childPath);

			string templatePath = string.Concat(this.ModulePath.TrimEnd('/'), "/", moduleName, "/", childPath);
			return this.Resolve(templatePath);
		}

		/// <summary>
		/// Expands the specified <paramref name="childPath"/> to its full path within the module with the specified
		/// <paramref name="pluginName"/>.
		/// </summary>
		/// <param name="pluginName">The name of the plugin for which the path is being expanded.</param>
		/// <param name="childPath">The path to expand</param>
		/// <returns>The expended version of the specified <paramref name="childPath"/>. If the <paramref name="childPath"/>
		/// is either an absolute web or absolute local path, the path will not be expanded.</returns>
		public string GetPluginPath(string pluginName, string childPath)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(pluginName));
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(childPath));

			bool isRooted = new Uri(childPath, UriKind.RelativeOrAbsolute).IsAbsoluteUri || Path.IsPathRooted(childPath);
			if (isRooted)
				return childPath;

			if (childPath.StartsWith("~/"))
				return context.MapPath(childPath);

			string templatePath = string.Concat(this.ExtensionPath.TrimEnd('/'), "/", pluginName, "/", childPath);
			return this.Resolve(templatePath);
		}

		public string GetDictionaryPath(string locale = null, string category = null)
		{
			return this.Resolve(context.ProjectConfiguration.PathTemplates.Dictionary, category ?? context.Category, locale ?? context.Locale);
		}

		/// <summary>
		/// Returns the physical path to the assets folder of the specified <paramref name="category"/>.
		/// </summary>
		/// <param name="category">The category for which to return the path</param>
		/// <returns>The physical path to the assets folder of the specified <paramref name="category"/></returns>
		public string GetPhysicalCategoryPath(string category = null)
		{
			return this.Resolve(GetAssetPath(category ?? context.Category));
		}

		/// <summary>
		/// Returns the path to the views folder of the specified <paramref name="category"/>.
		/// </summary>
		/// <param name="category">The category for which to return the path</param>
		/// <returns>The physical path to the views folder of the specified <paramref name="category"/></returns>
		public string GetViewPath(string category = null)
		{
			return this.Substitute("{assetpath}", category ?? context.Category);
		}

		/// <summary>
		/// Returns the physical path to the views folder of the specified <paramref name="category"/>.
		/// </summary>
		/// <param name="category">The category for which to return the path</param>
		/// <returns>The physical path to the views folder of the specified <paramref name="category"/></returns>
		public string GetPhysicalViewPath(string category = null)
		{
			return this.Resolve(GetViewPath(category ?? context.Category));
		}

		/// <summary>
		/// Converts, if possible, an absolute path to it's equivalent web accessible location (relative to the application path).
		/// </summary>
		/// <param name="path">The path to convert, for instance 'c:\Inetpub\wwwroot\mysite\static\resource.xml'</param>
		/// <param name="prependApplicationPath">if set to <c>true</c>, the path returned will be prefixed with the 
		/// application path, if the path doesn't start with it already.</param>
		/// <returns>
		/// The specified absolute path, converted to a web-accessible location. For the example above
		/// that could be something similar to 'static/resource' (assuming that the project is running within c:\Inetpub\wwwroot\mysite.
		/// </returns>
		public string GetRelativeWebPath(string path, bool prependApplicationPath = false)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException(path);

			path = path.ToLower().TrimEnd('\\');
			if (path.Contains("\\"))
			{
				bool replaced = false;
				foreach (string key in VirtualDirectories.Keys)
				{
					string directoryPath = VirtualDirectories[key];
				    if (path.Contains(directoryPath))
				    {
				        path = path.Replace(directoryPath, key).Replace("\\", "/");
						replaced = true;
						break;
				    }
				}

				if (!replaced)
				{
					var applicationPath = context.PhysicalApplicationPath.ToLower();
					path = path.ToLower().Replace(applicationPath, string.Empty).Replace("\\", "/");
				}
			}

			path = path.Replace("~", context.ApplicationPath.TrimEnd('/'));
			if (path.Contains(Uri.SchemeDelimiter))
				return path;

			if (prependApplicationPath && !path.StartsWith(context.ApplicationPath))
				path = string.Concat(context.ApplicationPath, "/", path).Replace("//", "/");

			return path;
		}

		/// <summary>
		/// Gets the suffix of an existing file most closely matching the specified <paramref name="locale"/>.
		/// </summary>
		/// <param name="path">The path to the file, without any suffix.</param>
		/// <param name="locale">The locale for which to search.</param>
		/// <returns>
		/// The suffix of an existing file most closely matching the current locale.
		/// </returns>
		/// <remarks>
		/// If the resource name suffix fallback hierarchy is configured to be: <c>ch,de,default</c>, while the 
		/// specified <paramref name="locale"/> is <c>ch</c>, given the <paramref name="path"/> 
		/// <c>g:\projects\p1\resources\image.png</c>, the method looks for files in this order:
		/// <list type="bullet">
		/// <item>
		/// <description><c>g:\projects\p1\resources\image-ch.png</c></description>
		/// </item>
		/// <item>
		/// <description><c>g:\projects\p1\resources\image-de.png</c></description>
		/// </item>
		/// <item>
		/// <description><c>g:\projects\p1\resources\image-default.png</c></description>
		/// </item>
		/// <item>
		/// <description><c>g:\projects\p1\resources\image.png</c></description>
		/// </item>
		/// </list>
		/// As soon as a match is found, the search is over and the found file's suffix (e.g. 'de') is returned.
		/// </remarks>
		public string GetClosestLocaleSuffix(string path, string locale)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));

			LocaleInfo localeInfo;
			if (!context.ProjectConfiguration.Locales.TryGetValue(locale, out localeInfo))
				throw new UnconfiguredLocaleException(locale);

			CategoryInfo category = context.ProjectConfiguration.Categories[context.Category];
			foreach (string suffix in localeInfo.ResourceNames)
			{
				string localizedName = new ResourceName(path, category).ToLocalePath(suffix);
				string attemptPath = context.MapPath(localizedName);
				if (File.Exists(attemptPath))
					return suffix;
			}

			return string.Empty;
		}

		/// <summary>
		/// Expands the specified <paramref name="relativePath"/> to a physical path in the the current context category.
		/// </summary>
		/// <param name="relativePath">The path to expand</param>
		/// <returns>The expanded relative path</returns>
		public string Expand(string relativePath)
		{
			if (relativePath.ToLower().StartsWith(context.ProjectConfiguration.SharedCategory + "/"))
				return this.Expand(relativePath, context.ProjectConfiguration.SharedCategory);

			return this.Expand(relativePath, context.Category);
		}

		/// <summary>
		/// Expands the specified <paramref name="relativePath"/> to a physical path in the specified <paramref name="category"/>.
		/// </summary>
		/// <param name="relativePath">The path to expand</param>
		/// <param name="category">The category under which to expand the path.</param>
		/// <returns>The expanded relative path.</returns>
		public string Expand(string relativePath, string category)
		{
			var templatePath = Path.Combine(GetAssetPath(category), relativePath);
			var substituted = Substitute(templatePath, new QueryString { { "category", category } });

			return context.MapPath(substituted);
		}

		/// <summary>
		/// Returns a localized version of the path, if such a file exists, searching
		/// for the name that most closely matches the current locale.
		/// </summary>
		/// <param name="path">The path, without any localization.</param>
		/// <returns>The localized version of the path</returns>
		/// <seealso cref="Localize(string, bool)"/>
		/// <seealso cref="Localize(string, string, bool)"/>
		public string Localize(string path)
		{
			return this.Localize(path, false);
		}

		/// <summary>
		/// Returns a localized version of the path, if such a file exists, searching
		/// for the name that most closely matches the current locale.
		/// </summary>
		/// <param name="path">The path, without any localization.</param>
		/// <param name="mapPath">If true, the resulting path will be converted to a physical path.</param>
		/// <returns>The localized version of the path</returns>
		/// <seealso cref="Localize(string, string, bool)"/>
		public string Localize(string path, bool mapPath)
		{
			return this.Localize(path, context.Locale, mapPath);
		}

		/// <summary>
		/// Returns a localized version of the path, if such a file exists, searching
		/// for the name that most closely matches the current locale.
		/// </summary>
		/// <param name="path">The path, without any localization.</param>
		/// <param name="locale">The locale for which to search.</param>
		/// <param name="mapPath">If true, the resulting path will be converted to a physical path.</param>
		/// <returns>The localized version of the path</returns>
		/// <remarks>
		/// If the resource name suffix fallback hierarchy is configured to be: <c>ch,de,default</c>, while the 
		/// specified <paramref name="locale"/> is <c>ch</c>, given the <paramref name="path"/> 
		/// <c>g:\projects\p1\resources\image.png</c>, the method looks for files in this order:
		/// <list type="bullet">
		/// <item>
		/// <description><c>g:\projects\p1\resources\image-ch.png</c></description>
		/// </item>
		/// <item>
		/// <description><c>g:\projects\p1\resources\image-de.png</c></description>
		/// </item>
		/// <item>
		/// <description><c>g:\projects\p1\resources\image-default.png</c></description>
		/// </item>
		/// <item>
		/// <description><c>g:\projects\p1\resources\image.png</c></description>
		/// </item>
		/// </list>
		/// As soon as a match is found, the search is over and the found file's path is returned.
		/// <para>
		/// The resulting path will not be mapped from a web to a physical path, it will be localized as-is.
		/// </para>
		/// </remarks>
		public string Localize(string path, string locale, bool mapPath = false)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));

			string suffix = this.GetClosestLocaleSuffix(path, locale);
			if (!string.IsNullOrEmpty(suffix))
			{
				string separator = path.Contains("\\") ? "\\" : "/";
				string directory = Path.GetDirectoryName(path);

				CategoryInfo category = context.ProjectConfiguration.Categories[context.Category];
				return string.Concat(directory, separator, new ResourceName(path, category).ToLocale(suffix));
			}

			return mapPath ? Resolve(path) : path;
		}

		public string Resolve(string template)
		{
			return this.Resolve(template, this.context);
		}

		public string Resolve(string template, string category)
		{
			return this.Resolve(template, category, context.Locale);
		}

		public string Resolve(string template, string category, string locale)
		{
			return this.Resolve(template, new QueryString { { "category", category }, { "locale", locale } });
		}

		/// <summary>
		/// Substitutes any placeholders in the specified template path with values from the specified
		/// <paramref name="context"/> and returns an absolute path.
		/// </summary>
		/// <param name="template">The template path to resolve.</param>
		/// <param name="context">The context to use to get the category.</param>
		/// <returns>
		/// The absolute version of the specified template with placeholders substituted.
		/// </returns>
		public string Resolve(string template, SageContext context)
		{
			return this.Resolve(template, new QueryString { { "category", context.Category } });
		}

		public string Resolve(string template, QueryString values)
		{
			var substituted = this.Substitute(template, values);
			return context.MapPath(substituted);
		}

		public string Substitute(string template)
		{
			return this.Substitute(template, this.context);
		}

		public string Substitute(string template, SageContext context)
		{
			return Substitute(template, new QueryString { { "category", context.Category } });
		}

		/// <summary>
		/// Substitutes the placeholders the specified template path using the specified <paramref name="category"/>.
		/// </summary>
		/// <param name="template">The template path to resolve.</param>
		/// <param name="category">The category substitution value.</param>
		/// <returns>
		/// The absolute version of the specified template with placeholders substituted.
		/// </returns>
		public string Substitute(string template, string category)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(template));
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(category));

			QueryString values = new QueryString();
			values.Add("category", category);
			values.Add("resourcepath", GetAssetPath(category));

			return Substitute(template, values);
		}

		public string Substitute(string template, QueryString values)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(template));
			Contract.Requires<ArgumentNullException>(values != null);

			if (values["category"] == null)
				values.Add("category", context.Category);
			if (values["locale"] == null)
				values.Add("locale", context.Locale);
			if (values["assetpath"] == null)
				values.Add("assetpath", GetAssetPath(values["category"]));

			string result = template;
			foreach (string key in values)
				result = result.Replace(string.Format("{{{0}}}", key), values[key]);

			return result;
		}
	}
}