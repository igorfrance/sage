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
namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Web;
	using System.Web.Hosting;

	using Kelp;
	using Kelp.Extensions;

	using Sage.Configuration;
	using Sage.Modules;
	using Sage.Rewriters;

	/// <summary>
	/// Provides a class that resolves physical file paths based on current category, locale and any fallback scenario 
	/// currently in place.
	/// </summary>
	public class PathResolver
	{
		private static Dictionary<string, string> virtualDirectories;
		private readonly SageContext context;
		private readonly ProjectConfiguration config;
		private string viewPath;
		private string modulePath;
		private string extensionPath;
		private string physicalViewPath;
		private string categoryConfigurationPath;
		private string physicalCategoryConfigurationPath;
		private string sharedViewPath;
		private string physicalSharedViewPath;
		private string siteMapPath;
		private string assetPath;
		private string sharedAssetPath;
		private string defaultStylesheetPath;

		/// <summary>
		/// Initializes a new instance of the <see cref="PathResolver"/> class using the specified <see cref="SageContext"/>
		/// to retrieve current category and locale information.
		/// </summary>
		/// <param name="context">The current <see cref="SageContext"/>.</param>
		public PathResolver(SageContext context)
		{
			this.context = context;
			config = context.ProjectConfiguration;
		}

		/// <summary>
		/// Gets the root virtual path of the current application. 
		/// </summary>
		public static string ApplicationPath
		{
			get
			{
				if (HttpContext.Current != null)
				{
					var ctx = HttpContext.Current;
					return ctx.Request.ApplicationPath;
				}

				return HostingEnvironment.ApplicationVirtualPath;
			}
		}

		/// <summary>
		/// Gets the physical path to the root folder of the current application. 
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
				return viewPath ?? (viewPath =
					context.ProjectConfiguration.PathTemplates.View
						.Replace("{assetpath}", this.AssetPath)
						.ReplaceAll("(?<!:)//", "/"));
			}
		}

		/// <summary>
		/// Gets the path to the directory from which modules are loaded.
		/// </summary>
		public string ModulePath
		{
			get
			{
				return modulePath ?? (modulePath =
					config.PathTemplates.Module
						.Replace("{assetpath}", this.AssetPath)
						.ReplaceAll("(?<!:)//", "/"));
			}
		}

		/// <summary>
		/// Gets the path in which to look for extensions.
		/// </summary>
		public string ExtensionPath
		{
			get
			{
				return extensionPath ?? (extensionPath =
					this.Resolve(config.PathTemplates.Extension));					 
			}
		}

		/// <summary>
		/// Gets the physical view path of the currently active category.
		/// </summary>
		public string PhysicalViewPath
		{
			get
			{
				return physicalViewPath ?? (physicalViewPath =
					context.MapPath(this.ViewPath));
			}
		}

		/// <summary>
		/// Gets the path to the category configuration document.
		/// </summary>
		public string CategoryConfigurationPath
		{
			get
			{
				return categoryConfigurationPath ?? (categoryConfigurationPath =
					this.Substitute(config.PathTemplates.CategoryConfiguration));
			}
		}

		/// <summary>
		/// Gets the physical category configuration path.
		/// </summary>
		public string PhysicalCategoryConfigurationPath
		{
			get
			{
				return physicalCategoryConfigurationPath ?? (physicalCategoryConfigurationPath =
					this.Resolve(this.CategoryConfigurationPath));
			}
		}

		/// <summary>
		/// Gets the path to the shared views.
		/// </summary>
		public string SharedViewPath
		{
			get
			{
				return sharedViewPath ?? (sharedViewPath =
					config.PathTemplates.View.Replace("{assetpath}", this.SharedAssetPath));
			}
		}

		/// <summary>
		/// Gets the physical path to the shared views.
		/// </summary>
		public string PhysicalSharedViewPath
		{
			get
			{
				return physicalSharedViewPath ?? (physicalSharedViewPath =
					this.Resolve(config.PathTemplates.View, new QueryString { { "category", config.SharedCategory } }));
			}
		}

		/// <summary>
		/// Gets the physical path to the sitemap file for this project.
		/// </summary>
		public string SiteMapPath
		{
			get
			{
				return siteMapPath ?? (siteMapPath =
					this.Resolve(config.PathTemplates.SiteMap));
			}
		}

		/// <summary>
		/// Gets the asset path for the current category.
		/// </summary>
		public string AssetPath
		{
			get
			{
				return assetPath ?? (assetPath = 
					this.GetAssetPath(context.Category));
			}
		}

		/// <summary>
		/// Gets the asset path for the shared category.
		/// </summary>
		public string SharedAssetPath
		{
			get
			{
				return sharedAssetPath ?? (sharedAssetPath = 
					this.GetAssetPath(config.SharedCategory));
			}
		}

		/// <summary>
		/// Gets the default style-sheet path.
		/// </summary>
		public string DefaultStylesheetPath
		{
			get
			{
				return defaultStylesheetPath ?? (defaultStylesheetPath =
					this.Resolve(config.PathTemplates.DefaultStylesheet));
			}
		}

		/// <summary>
		/// Gets the virtual directories.
		/// </summary>
		public Dictionary<string, string> VirtualDirectories
		{
			get
			{
				return virtualDirectories ?? (virtualDirectories = 
					Project.GetVirtualDirectories(context));
			}
		}

		/// <summary>
		/// Returns the path to the assets folder of the specified <paramref name="category"/>.
		/// </summary>
		/// <param name="category">The category for which to return the path</param>
		/// <returns>The physical path to the assets folder of the specified <paramref name="category"/></returns>
		public string GetAssetPath(string category)
		{
			return config.AssetPath.Replace("{category}", category).TrimEnd('/');
		}

		/// <summary>
		/// Gets the expanded and resolved path to the module with the specified <paramref name="moduleName"/>.
		/// </summary>
		/// <param name="moduleName">Name of the module whose path to get.</param>
		/// <returns>
		/// The expanded and resolved path to the module with the specified <paramref name="moduleName"/>
		/// </returns>
		public string GetModulePath(string moduleName)
		{
			string templatePath = string.Concat(this.ModulePath, moduleName);
			return this.Resolve(templatePath);
		}

		/// <summary>
		/// Expands the specified <paramref name="childPath"/> to its full path within the module with the specified
		/// <paramref name="moduleKey"/>.
		/// </summary>
		/// <param name="moduleKey">The name (or combination of category/name) of the module for which the path is being expanded.</param>
		/// <param name="childPath">The path to expand</param>
		/// <returns>The expended version of the specified <paramref name="childPath"/>. If the <paramref name="childPath"/>
		/// is either an absolute web or absolute local path, the path will not be expanded.</returns>
		public string GetModulePath(string moduleKey, string childPath)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(moduleKey));
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(childPath));

			bool isRooted = new Uri(childPath, UriKind.RelativeOrAbsolute).IsAbsoluteUri || Path.IsPathRooted(childPath);
			if (isRooted)
				return childPath;

			if (childPath.StartsWith("~/"))
				return context.MapPath(childPath);

			string templatePath = string.Concat(this.ModulePath, moduleKey.Replace("Module", string.Empty), "/", childPath);
			return this.Resolve(templatePath);
		}

		/// <summary>
		/// Expands the specified <paramref name="childPath"/> to its full path within the module specified with
		/// <paramref name="moduleType"/>.
		/// </summary>
		/// <param name="moduleType">The type of the module for which the path is being expanded.</param>
		/// <param name="childPath">The path to expand</param>
		/// <returns>The expended version of the specified <paramref name="childPath"/>. If the <paramref name="childPath"/>
		/// is either an absolute web or absolute local path, the path will not be expanded.</returns>
		public string GetModulePath(Type moduleType, string childPath)
		{
			Contract.Requires<ArgumentNullException>(moduleType != null);
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(childPath));

			return this.GetModulePath(moduleType.Name, childPath);
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

			string templatePath = string.Concat(this.ExtensionPath, pluginName, "/", childPath);
			return this.Resolve(templatePath);
		}

		/// <summary>
		/// Gets the path to the dictionary, either using the current context's locale and category, or if specified, using
		/// the <paramref name="locale"/> and <paramref name="category"/> arguments.
		/// </summary>
		/// <param name="locale">Optional locale of the dictionary path to get.</param>
		/// <param name="category">Optional category of the dictionary path to get.</param>
		/// <returns>
		/// The path to the dictionary, either using the current context's locale and category, or if specified, using
		/// the <paramref name="locale"/> and <paramref name="category"/> arguments.
		/// </returns>
		public string GetDictionaryPath(string locale = null, string category = null)
		{
			return this.Resolve(config.PathTemplates.Dictionary, 
				category ?? context.Category, 
				locale ?? context.Locale);
		}

		/// <summary>
		/// Returns the physical path to the assets folder of the specified <paramref name="category"/>.
		/// </summary>
		/// <param name="category">The category for which to return the path</param>
		/// <returns>The physical path to the assets folder of the specified <paramref name="category"/></returns>
		public string GetPhysicalCategoryPath(string category = null)
		{
			return this.Resolve(this.GetAssetPath(category ?? context.Category));
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
			return this.Resolve(this.GetViewPath(category ?? context.Category));
		}

		/// <summary>
		/// Converts, if possible, an absolute path to it's equivalent web accessible location (relative to the application path).
		/// </summary>
		/// <param name="path">The path to convert, for instance <c>c:\Inetpub\wwwroot\mysite\static\resource.xml</c></param>
		/// <param name="prependApplicationPath">if set to <c>true</c>, the path returned will be prefixed with the 
		/// application path, if the path doesn't start with it already.</param>
		/// <returns>
		/// The specified absolute path, converted to a web-accessible location. For the example above
		/// that could be something similar to <c>static/resource</c> (assuming that the project is running within 
		/// <c>c:\Inetpub\wwwroot\mysite</c>.
		/// </returns>
		public string GetRelativeWebPath(string path, bool prependApplicationPath = false)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException(path);

			path = path.TrimEnd('\\');
			if (path.Contains("\\"))
			{
				bool replaced = false;
				foreach (string key in this.VirtualDirectories.Keys)
				{
					string directoryPath = this.VirtualDirectories[key];
					if (path.IndexOf(directoryPath, StringComparison.InvariantCultureIgnoreCase) != -1)
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
				path = string.Concat(context.ApplicationPath, path);

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
			if (!config.Locales.TryGetValue(locale, out localeInfo))
				throw new UnconfiguredLocaleException(locale);

			CategoryInfo category = config.Categories[context.Category];
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
			if (relativePath.ToLower().StartsWith(config.SharedCategory + "/"))
				return this.Expand(relativePath, config.SharedCategory);

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
			var categoryAssetPath = this.GetAssetPath(category);
			var templatePath = Path.IsPathRooted(relativePath) || relativePath.StartsWith(categoryAssetPath)
				? relativePath
				: Path.Combine(categoryAssetPath, relativePath);

			var substituted = this.Substitute(templatePath, new QueryString { { "category", category } });
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

				CategoryInfo category = config.Categories[context.Category];
				return string.Concat(directory, separator, new ResourceName(path, category).ToLocale(suffix));
			}

			return mapPath ? this.Resolve(path) : path;
		}

		/// <summary>
		/// Substitutes any placeholders in the specified <paramref name="templatePath"/> with values from the current 
		/// <see cref="ProjectConfiguration"/> and the current <see cref="SageContext"/> and returns an absolute path.
		/// </summary>
		/// <param name="templatePath">The path to resolve.</param>
		/// <returns>
		/// The resolved version of the specified <paramref name="templatePath"/>.
		/// </returns>
		public string Resolve(string templatePath)
		{
			return this.Resolve(templatePath, context);
		}

		/// <summary>
		/// Substitutes any placeholders in the specified <paramref name="templatePath"/> with values from the current 
		/// <see cref="ProjectConfiguration"/>, the current <see cref="SageContext"/> and the specified 
		/// <paramref name="category"/>, and returns an absolute path.
		/// </summary>
		/// <param name="templatePath">The path to resolve.</param>
		/// <param name="category">The category to use instead of the current context's category when resolving.</param>
		/// <returns>The resolved version of the specified <paramref name="templatePath"/>.</returns>
		public string Resolve(string templatePath, string category)
		{
			return this.Resolve(templatePath, category, context.Locale);
		}

		/// <summary>
		/// Substitutes any placeholders in the specified <paramref name="templatePath"/> with values from the current 
		/// <see cref="ProjectConfiguration"/>, the current <see cref="SageContext"/> and the specified 
		/// <paramref name="category"/> and <paramref name="locale"/>, and returns an absolute path.
		/// </summary>
		/// <param name="templatePath">The path to resolve.</param>
		/// <param name="category">The category to use instead of the current context's category when resolving.</param>
		/// <param name="locale">The locale to use instead of the current context's locale when resolving.</param>
		/// <returns>The resolved version of the specified <paramref name="templatePath"/>.</returns>
		public string Resolve(string templatePath, string category, string locale)
		{
			return this.Resolve(templatePath, new QueryString { { "category", category }, { "locale", locale } });
		}

		/// <summary>
		/// Substitutes any placeholders in the specified <paramref name="templatePath"/> with values from the current 
		/// <see cref="ProjectConfiguration"/> and the specified <paramref name="context"/> and returns an absolute path.
		/// </summary>
		/// <param name="templatePath">The template path to resolve.</param>
		/// <param name="context">The context to use to get the category.</param>
		/// <returns>
		/// The resolved version of the specified template with placeholders substituted.
		/// </returns>
		public string Resolve(string templatePath, SageContext context)
		{
			return this.Resolve(templatePath, new QueryString { { "category", context.Category } });
		}

		/// <summary>
		/// Substitutes any placeholders in the specified <paramref name="templatePath"/> with values from the current 
		/// <see cref="ProjectConfiguration"/> and the specified <paramref name="values"/> and returns an absolute path.
		/// </summary>
		/// <param name="templatePath">The template path to resolve.</param>
		/// <param name="values">The name/value collection of values to use</param>
		/// <returns>
		/// The resolved version of the specified template with placeholders substituted.
		/// </returns>
		public string Resolve(string templatePath, QueryString values)
		{
			var physicalPath = context.MapPath(this.Substitute(templatePath, values));

			if (!File.Exists(physicalPath) && !Directory.Exists(physicalPath) && Project.Extensions.Count > 0)
			{
				string applicationPath = context.Request.PhysicalApplicationPath;
				string requestedFile = physicalPath.ToLower()
					.Replace(applicationPath.ToLower(), string.Empty)
					.Trim('\\');

				foreach (string extensionId in Project.InstallOrder.Reverse())
				{
					if (!Project.Extensions.ContainsKey(extensionId))
						continue;

					string extensionName = Project.Extensions[extensionId].Name;
					string rewrittenPath = Path.Combine(this.ExtensionPath, extensionName, requestedFile);
					if (File.Exists(rewrittenPath))
					{
						return rewrittenPath;
					}
				}
			}

			return physicalPath;
		}

		/// <summary>
		/// Substitutes any placeholders in the specified <paramref name="templatePath"/> with values from the current 
		/// <see cref="ProjectConfiguration"/> and the current <see cref="SageContext"/>.
		/// </summary>
		/// <param name="templatePath">The path to resolve.</param>
		/// <returns>The substituted version of the specified <paramref name="templatePath"/>.</returns>
		public string Substitute(string templatePath)
		{
			return this.Substitute(templatePath, context);
		}

		/// <summary>
		/// Substitutes any placeholders in the specified <paramref name="templatePath"/> with values from the current 
		/// <see cref="ProjectConfiguration"/> and the specified <paramref name="context"/>.
		/// </summary>
		/// <param name="templatePath">The template path to resolve.</param>
		/// <param name="context">The context to use to get the category.</param>
		/// <returns>
		/// The substituted version of the specified template with placeholders substituted.
		/// </returns>
		public string Substitute(string templatePath, SageContext context)
		{
			return this.Substitute(templatePath, new QueryString { { "category", context.Category } });
		}

		/// <summary>
		/// Substitutes any placeholders in the specified <paramref name="templatePath"/> with values from the current 
		/// <see cref="ProjectConfiguration"/>, the current <see cref="SageContext"/> and the specified 
		/// <paramref name="category"/>.
		/// </summary>
		/// <param name="templatePath">The path to resolve.</param>
		/// <param name="category">The category to use instead of the current context's category when resolving.</param>
		/// <returns>The substituted version of the specified <paramref name="templatePath"/>.</returns>
		public string Substitute(string templatePath, string category)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(templatePath));
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(category));

			QueryString values = new QueryString();
			values.Add("category", category);
			values.Add("resourcepath", this.GetAssetPath(category));

			return this.Substitute(templatePath, values);
		}

		/// <summary>
		/// Substitutes any placeholders in the specified <paramref name="templatePath"/> with values from the current 
		/// <see cref="ProjectConfiguration"/> and the specified <paramref name="values"/>.
		/// </summary>
		/// <param name="templatePath">The template path to resolve.</param>
		/// <param name="values">The name/value collection of values to use</param>
		/// <returns>
		/// The substituted version of the specified template with placeholders substituted.
		/// </returns>
		public string Substitute(string templatePath, QueryString values)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(templatePath));
			Contract.Requires<ArgumentNullException>(values != null);

			if (values["category"] == null)
				values.Add("category", context.Category);
			if (values["locale"] == null)
				values.Add("locale", context.Locale);
			if (values["assetpath"] == null)
				values.Add("assetpath", this.GetAssetPath(values["category"]));

			string result = templatePath;
			foreach (string key in values)
				result = result.Replace(string.Format("{{{0}}}", key), values[key]);

			return result;
		}
	}
}