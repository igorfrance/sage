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
	using System.Web;
	using System.Web.Caching;

	using log4net;

	/// <summary>
	/// Provides a wrapper around the current <see cref="System.Web.Caching.Cache"/>, as well as a plain
	/// key/value dictionary, so that if <see cref="System.Web.Caching.Cache"/> is <c>null</c> (as in unit 
	/// test scenarios), the code using the cache can run without exceptions.
	/// </summary>
	public class CacheWrapper
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(CacheWrapper).FullName);
		private readonly Cache cache;
		private readonly Dictionary<string, object> dictionary;

		/// <summary>
		/// Initializes a new instance of the <see cref="CacheWrapper"/> class.
		/// </summary>
		public CacheWrapper()
		{
			dictionary = new Dictionary<string, object>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CacheWrapper"/> class using the specified <paramref name="context"/>.
		/// </summary>
		/// <param name="context">The current HTTP context.</param>
		public CacheWrapper(HttpContextBase context)
			: this()
		{
			if (context != null && context.Cache != null)
			{
				try
				{
					log.DebugFormat("The context cache has {0} items in it", context.Cache.Count);
					this.cache = context.Cache;
				}
				catch (Exception ex)
				{
					log.ErrorFormat("The context cache hasn't been initialized fully: {0}", ex.Message);
				}
			}
		}

		/// <summary>
		/// Gets the number of items stored in the cache.
		/// </summary>
		public int Count
		{
			get
			{
				if (this.cache != null)
					return this.cache.Count;

				return this.dictionary.Count;
			}
		}

		/// <summary>
		/// Gets or sets the cache item at the specified key.
		/// </summary>
		/// <param name="key">The key of the item to get.</param>
		/// <returns>The item with the corresponding key.</returns>
		public object this[string key]
		{
			get
			{
				return this.Get(key);
			}
		}

		/// <summary>
		/// Retrieves the specified item from the cache.
		/// </summary>
		/// <param name="key">The identifier for the cache item to retrieve.</param>
		/// <returns>The retrieved cache item, or null if the key is not found</returns>
		public object Get(string key)
		{
			if (this.cache != null)
				return this.cache.Get(key);

			if (this.dictionary.ContainsKey(key))
				return dictionary[key];

			return null;
		}

		/// <summary>
		/// Removes the specified item from the cache.
		/// </summary>
		/// <param name="key">An identifier for the cache item to remove.</param>
		/// <returns>The item removed from the cache. If the value in the key parameter is not found, returns null</returns>
		public object Remove(string key)
		{
			if (this.cache != null)
				return this.cache.Get(key);

			if (this.dictionary.ContainsKey(key))
			{
				object result = this.dictionary[key];
				this.dictionary.Remove(key);
				return result;
			}

			return null;
		}

		/// <summary>
		/// Inserts an object into the cache together with dependencies, expiration policies, and a delegate that 
		/// you can use to notify the application before the item is removed from the cache.
		/// </summary>
		/// <param name="key">The cache key that is used to reference the object.</param>
		/// <param name="value">The object to insert into the cache.</param>
		/// <param name="dependencies">The file or cache key dependencies for the item. When any dependency changes, 
		/// the object becomes invalid and is removed from the cache. If there are no dependencies, this parameter 
		/// contains null.</param>
		/// <param name="absoluteExpiration">The time at which the inserted object expires and is removed from the 
		/// cache. To avoid possible issues with local time such as changes from standard time to daylight saving 
		/// time, use <see cref="DateTime.UtcNow"/> instead of <see cref="DateTime.Now"/> for this parameter value. 
		/// If you are using absolute expiration, the slidingExpiration parameter must be set to 
		/// <see cref="Cache.NoSlidingExpiration"/>.</param>
		/// <param name="slidingExpiration">The interval between the time that the cached object was last accessed 
		/// and the time at which that object expires. If this value is the equivalent of 20 minutes, the object 
		/// will expire and be removed from the cache 20 minutes after it was last accessed. If you are using 
		/// sliding expiration, the absoluteExpiration parameter must be set to <see cref="Cache.NoAbsoluteExpiration"/>.</param>
		/// <param name="callback">A delegate that will be called before the object is removed from the cache. 
		/// You can use this to update the cached item and ensure that it is not removed from the cache.</param>
		public void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemUpdateCallback callback)
		{
			if (this.cache != null)
			{
				this.cache.Insert(key, value, dependencies, absoluteExpiration, slidingExpiration, callback);
				return;
			}

			this.InsertToDictionary(key, value);
		}

		/// <summary>
		/// Inserts an object into the cache with dependencies, expiration and priority policies, and a delegate you can use to notify your application when the inserted item is removed from the Cache.
		/// </summary>
		/// <param name="key">The cache key that is used to reference the object.</param>
		/// <param name="value">The object to insert into the cache.</param>
		/// <param name="dependencies">The file or cache key dependencies for the item. When any dependency changes, 
		/// the object becomes invalid and is removed from the cache. If there are no dependencies, this parameter 
		/// contains null.</param>
		/// <param name="absoluteExpiration">The time at which the inserted object expires and is removed from the 
		/// cache. To avoid possible issues with local time such as changes from standard time to daylight saving 
		/// time, use <see cref="DateTime.UtcNow"/> instead of <see cref="DateTime.Now"/> for this parameter value. 
		/// If you are using absolute expiration, the slidingExpiration parameter must be set to 
		/// <see cref="Cache.NoSlidingExpiration"/>.</param>
		/// <param name="slidingExpiration">The interval between the time that the cached object was last accessed 
		/// and the time at which that object expires. If this value is the equivalent of 20 minutes, the object 
		/// will expire and be removed from the cache 20 minutes after it was last accessed. If you are using 
		/// sliding expiration, the absoluteExpiration parameter must be set to <see cref="Cache.NoAbsoluteExpiration"/>.</param>
		/// <param name="priority">The cost of the object relative to other items stored in the cache, as expressed 
		/// by the <see cref="CacheItemPriority"/> enumeration. This value is used by the cache when it evicts 
		/// objects; objects with a lower cost are removed from the cache before objects with a higher cost.</param>
		/// <param name="callback">A delegate that will be called before the object is removed from the cache. 
		/// You can use this to update the cached item and ensure that it is not removed from the cache.</param>
		public void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback callback)
		{
			if (this.cache != null)
			{
				this.cache.Insert(key, value, dependencies, absoluteExpiration, slidingExpiration, priority, callback);
				return;
			}

			this.InsertToDictionary(key, value);
		}

		/// <summary>
		/// Inserts an object into the cache with dependencies and expiration policies.
		/// </summary>
		/// <param name="key">The cache key that is used to reference the object.</param>
		/// <param name="value">The object to insert into the cache.</param>
		/// <param name="dependencies">The file or cache key dependencies for the item. When any dependency changes, 
		/// the object becomes invalid and is removed from the cache. If there are no dependencies, this parameter 
		/// contains null.</param>
		/// <param name="absoluteExpiration">The time at which the inserted object expires and is removed from the 
		/// cache. To avoid possible issues with local time such as changes from standard time to daylight saving 
		/// time, use <see cref="DateTime.UtcNow"/> instead of <see cref="DateTime.Now"/> for this parameter value. 
		/// If you are using absolute expiration, the slidingExpiration parameter must be set to 
		/// <see cref="Cache.NoSlidingExpiration"/>.</param>
		/// <param name="slidingExpiration">The interval between the time that the cached object was last accessed 
		/// and the time at which that object expires. If this value is the equivalent of 20 minutes, the object 
		/// will expire and be removed from the cache 20 minutes after it was last accessed. If you are using 
		/// sliding expiration, the absoluteExpiration parameter must be set to <see cref="Cache.NoAbsoluteExpiration"/>.</param>
		public void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration)
		{
			if (this.cache != null)
			{
				this.cache.Insert(key, value, dependencies, absoluteExpiration, slidingExpiration);
				return;
			}

			this.InsertToDictionary(key, value);
		}

		/// <summary>
		/// Inserts an object into the cache that has file or key dependencies.
		/// </summary>
		/// <param name="key">The cache key that is used to reference the object.</param>
		/// <param name="value">The object to insert into the cache.</param>
		/// <param name="dependencies">The file or cache key dependencies for the item. When any dependency changes, 
		/// the object becomes invalid and is removed from the cache. If there are no dependencies, this parameter 
		/// contains null.</param>
		public void Insert(string key, object value, CacheDependency dependencies)
		{
			if (this.cache != null)
			{
				this.cache.Insert(key, value, dependencies);
				return;
			}

			this.InsertToDictionary(key, value);
		}

		private void InsertToDictionary(string key, object value)
		{
			if (this.dictionary.ContainsKey(key))
				dictionary[key] = value;
			else
				dictionary.Add(key, value);
		}
	}
}
