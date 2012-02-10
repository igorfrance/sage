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
namespace Kelp.ResourceHandling
{
	using System;
	using System.Web;
	using System.Web.Caching;

	using log4net;

	/// <summary>
	/// Provides shared database of file modification dates.
	/// </summary>
	/// <remarks>
	/// ASP.NET caching mechanism is used as a database store, because it provides neat file modification based cache
	/// dependency which will automatically expire a key if a file changes.
	/// </remarks>
	public static class LastModifiedDateCache
	{
		private const string Key = "LASTMODIFIED:{0}";
		private static readonly ILog log = LogManager.GetLogger(typeof(Cache).FullName);

		/// <summary>
		/// Gets the last modified date and time from cache.
		/// </summary>
		/// <param name="context">The context; for accessing the cache.</param>
		/// <param name="relativePath">The relative path of the file.</param>
		/// <returns>
		/// The last modified date and time as present in the cache
		/// </returns>
		/// <remarks>
		/// If the no entry for the relative path can be found in the cache, a 1-1-0001 0:00:00 date is returned which is really old
		/// </remarks>
		public static DateTime Get(HttpContextBase context, string relativePath)
		{
			var cache = context.Cache;
			object cacheObject = null;
			string cacheKey = string.Format(Key, relativePath);

			// Try to get the Last-Modified from the cache
			try
			{
				cacheObject = cache.Get(cacheKey);
			}
			catch (Exception e)
			{
				log.ErrorFormat("Could not access the cache '{0}'", e);
			}

			// Is there a cache entry of the Last-Modified for this file?
			if (cacheObject != null && cacheObject.GetType() == typeof(DateTime))
			{
				return (DateTime) cacheObject;
			}

			return new DateTime();
		}

		/// <summary>
		/// Removes the last modified date and time entry from cache for a particular  <paramref name="relativePath"/>.
		/// </summary>
		/// <param name="context">The context; for accessing the cache.</param>
		/// <param name="relativePath">The relative path.</param>
		public static void Remove(HttpContextBase context, string relativePath)
		{
			var cache = context.Cache;
			string cacheKey = string.Format(Key, relativePath);
			cache.Remove(cacheKey);
		}

		/// <summary>
		/// Puts the last modified date and time in cache for a particular <paramref name="relativePath"/>.
		/// </summary>
		/// <param name="context">The context; for accessing the cache.</param>
		/// <param name="relativePath">The relative path.</param>
		/// <param name="lastModified">The last modified.</param>
		/// <param name="fileDependencies">The file dependencies.</param>
		/// <remarks>
		/// when supplying file dependencies, the cache entry is automatically removed if one of these files change on disk
		/// </remarks>
		public static void Store(HttpContextBase context, string relativePath, DateTime lastModified, string[] fileDependencies)
		{
			var cache = context.Cache;
			string cacheKey = string.Format(Key, relativePath);
			try
			{
				cache.Insert(cacheKey, lastModified, new CacheDependency(fileDependencies));
			}
			catch (Exception e)
			{
				log.ErrorFormat("Could not access the cache '{0}'", e);
			}
		}
	}
}
