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
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Web.Caching;

	using log4net;
	using Sage.ResourceManagement;

	/// <summary>
	/// Provides a simple mechanism for saving to and restoring from the ASP.NET cache the last modification dates
	/// of local resources.
	/// </summary>
	public class LastModifiedCache
	{
		private const string CacheKeyName = "DATE_{0}";
		private static readonly ILog log = LogManager.GetLogger(typeof(LastModifiedCache).FullName);
		private readonly SageContext context;

		/// <summary>
		/// Initializes a new instance of the <see cref="LastModifiedCache"/> class, using the specified
		/// <paramref name="context"/>.
		/// </summary>
		/// <param name="context">The current context under which this code is being executed.</param>
		internal LastModifiedCache(SageContext context)
		{
			Contract.Requires<ArgumentNullException>(context != null);

			this.context = context;
		}

		/// <summary>
		/// Gets the cached last modification date for the specified resource if present, otherwise
		/// <c>null</c>.
		/// </summary>
		/// <param name="resourcePath">The resource path for which to get the last modification date.</param>
		/// <returns>The cached last modification date for the specified resource if present, otherwise
		/// <c>null</c>.</returns>
		public DateTime? Get(string resourcePath)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(resourcePath));

			string itemKey = string.Format(CacheKeyName, resourcePath);
			object cachedItem = context.Cache.Get(itemKey);
			if (cachedItem != null)
				return (DateTime) cachedItem;

			return null;
		}

		/// <summary>
		/// Saves the last modification of the specified <paramref name="resourcePath"/>.
		/// </summary>
		/// <param name="resourcePath">The resource path for which to store the date.</param>
		/// <param name="lastModificationDate">The last modification date to store.</param>
		/// <param name="dependencies">The dependencies of the <paramref name="resourcePath"/>. If any one of
		/// these files changes, the cached item will expire and be removed from cache, thereby ensuring that
		/// the cached date is correct; its absence would indicate that the last modification date has expired
		/// (or has never been saved, in any case causing the caller to get a fresh last modification date)</param>
		public void Put(string resourcePath, DateTime lastModificationDate, List<string> dependencies)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(resourcePath));
			Contract.Requires<ArgumentNullException>(dependencies != null);
			Contract.Requires<ArgumentNullException>(dependencies.Count > 0);

			string itemKey = string.Format(CacheKeyName, resourcePath);
			IEnumerable<string> files = dependencies.Where(d => UrlResolver.GetScheme(d) == "file");
			this.context.Cache.Insert(itemKey, lastModificationDate, new CacheDependency(files.ToArray()), Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, OnCacheItemExpired);
		}

		private static void OnCacheItemExpired(string key, CacheItemUpdateReason reason, out object expensiveObject, out CacheDependency dependency, out DateTime absoluteExpiration, out TimeSpan slidingExpiration)
		{
			log.DebugFormat("Cache item '{0}' expired, reason: {1}.", key, reason);

			expensiveObject = null;
			dependency = null;
			absoluteExpiration = Cache.NoAbsoluteExpiration;
			slidingExpiration = Cache.NoSlidingExpiration;
		}
	}
}
