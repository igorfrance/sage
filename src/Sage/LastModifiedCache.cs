namespace Sage
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Web.Caching;

	/// <summary>
	/// Provides a simple mechanism for saving to and restoring from the ASP.NET cache the last modification dates
	/// of local resources.
	/// </summary>
	public class LastModifiedCache
	{
		private const string CacheKeyName = "{0}_DATE";
		private readonly SageContext context;

		/// <summary>
		/// Initializes a new instance of the <see cref="LastModifiedCache"/> class, usinng the specified
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
		/// Saves the last modification of the specified <see cref="resourcePath"/>.
		/// </summary>
		/// <param name="resourcePath">The resource path for which to store the date.</param>
		/// <param name="lastModificationDate">The last modification date to store.</param>
		/// <param name="dependencies">The dependencies of the <see cref="resourcePath"/>. If any one of
		/// these files changes, the cached item will expire and be removed from cache, thereby ensuring that
		/// the cached date is correct; its absence would indicate that the last modification date has expired
		/// (or has never been saved, in any case causing the caller to get a fresh last modification date)</param>
		public void Put(string resourcePath, DateTime lastModificationDate, List<string> dependencies)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(resourcePath));
			Contract.Requires<ArgumentNullException>(dependencies != null);
			Contract.Requires<ArgumentNullException>(dependencies.Count > 0);

			string itemKey = string.Format(CacheKeyName, resourcePath);
			IEnumerable<string> files = dependencies.Where(d => new Uri(d).Scheme == "file");
			context.Cache.Insert(itemKey, lastModificationDate, new CacheDependency(files.ToArray()));
		}
	}
}
