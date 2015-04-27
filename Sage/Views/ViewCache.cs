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
	using System.Text;
	using System.Text.RegularExpressions;
	using Kelp.Extensions;
	using log4net;
	using Sage.Configuration;

	/// <summary>
	/// Implements a simple cache store for saving and restoring transformed views.
	/// </summary>
	public class ViewCache
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ViewCache).FullName);
		private readonly SageContext context;
		private readonly CachingConfiguration config;

		/// <summary>
		/// Initializes a new instance of the <see cref="ViewCache"/> class.
		/// </summary>
		/// <param name="context">The current context.</param>
		public ViewCache(SageContext context)
		{
			this.context = context;
			if (context.ProjectConfiguration != null)
			{
				config = context.ProjectConfiguration.ViewCaching;
				this.Directory = context.MapPath(config.Directory);
			}
		}

		/// <summary>
		/// Gets the physical path of the directory used as the cache store.
		/// </summary>
		public string Directory { get; private set; }

		/// <summary>
		/// Determines whether the specified view path has been cached, and is still valid.
		/// </summary>
		/// <param name="viewPath">The view path to check.</param>
		/// <param name="groupName">Optional name of the cache group that the <paramref name="viewPath"/> belongs to.</param>
		/// <returns><c>true</c> if the specified view path exists in cache, and is valid; otherwise, <c>false</c>.</returns>
		public bool Has(string viewPath, string groupName = null)
		{
			var cachePath = this.GetCachePath(viewPath, groupName);
			return File.Exists(cachePath);
		}

		/// <summary>
		/// Saves the specified <paramref name="content"/> to the cache.
		/// </summary>
		/// <param name="viewPath">The view path.</param>
		/// <param name="groupName">Optional name of the cache group that the <paramref name="viewPath"/> belongs to.</param>
		/// <param name="content">The content.</param>
		public void Save(string viewPath, string content, string groupName = null)
		{
			var cachePath = this.GetCachePath(viewPath, groupName);
			var group = groupName != null && config.Groups.ContainsKey(groupName) ? config.Groups[groupName] : null;
			try
			{
				if (group != null)
					content = group.ApplySaveFilters(content, context);

				File.WriteAllText(cachePath, content, Encoding.UTF8);
			}
			catch (Exception ex)
			{
				log.ErrorFormat("Failed caching '{0}' to '{1}': {2}", viewPath, cachePath, ex.Message);
			}
		}

		/// <summary>
		/// Gets the specified view path.
		/// </summary>
		/// <param name="viewPath">The view path.</param>
		/// <param name="groupName">Optional name of the cache group that the <paramref name="viewPath"/> belongs to.</param>
		/// <returns>System.String.</returns>
		public string Get(string viewPath, string groupName = null)
		{
			string result;
			var cachePath = this.GetCachePath(viewPath, groupName);

			if (!this.Has(viewPath, groupName))
				return null;

			try
			{
				result = File.ReadAllText(cachePath);

				var group = groupName != null && config.Groups.ContainsKey(groupName) ? config.Groups[groupName] : null;
				if (group != null)
					result = group.ApplyLoadFilters(result, context);
			}
			catch (Exception ex)
			{
				log.ErrorFormat("Failed getting '{0}' from '{1}': {2}", viewPath, cachePath, ex.Message);
				return null;
			}

			return result;
		}

		/// <summary>
		/// Gets the last modified date of the specified <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The source path of the cached item.</param>
		/// <param name="groupName">Optional name of the cache group that the <paramref name="path"/> belongs to.</param>
		/// <returns>The last modified date of the specified <paramref name="path"/>, or <c>null</c> is no entry exists for that
		/// <paramref name="path"/>.</returns>
		public DateTime? GetLastModified(string path, string groupName = null)
		{
			if (!this.Has(path, groupName))
				return null;

			return Kelp.Util.GetDateLastModified(this.GetCachePath(path, groupName));
		}

		private string GetCachePath(string viewPath, string groupName = null)
		{
			if (groupName != null && config.Groups.ContainsKey(groupName))
				viewPath = Path.ChangeExtension(viewPath, config.Groups[groupName].Extension);

			return Path.Combine(this.Directory, viewPath.ReplaceAll(@"[\\/:]", "_"));
		}
	}
}
