namespace Sage.Xml
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Web.Caching;
	using System.Xml;

	using Kelp.Core.Extensions;

	using Sage.ResourceManagement;

	/// <summary>
	/// Provides a simple registry for caching of XSLT stylesheets.
	/// </summary>
	/// <remarks>
	///  Loading and compiling an XSLT stylesheet is a relatively expensive process, and this class provides a registry
	/// of previously loaded stylesheets so that they can be kept in memory and reused after they have been
	/// initialized. The class will automatically cache the loaded stylesheet if it hasn't been cached before.
	/// Additionally, if the stylesheet doesn't provide any templates of its own but is only including a global
	/// stylesheet, then the global stylesheet will be returned.
	/// </remarks>
	public static class XsltRegistry
	{
		private const string CacheKeyFormat = "{0}_XSLT";

		/// <summary>
		/// Loads the stylesheet from the specified path using the specified context.
		/// </summary>
		/// <param name="stylesheetPath">The path to the stylesheet to load.</param>
		/// <param name="context">The context under which this code is executing.</param>
		/// <returns>The <see cref="CacheableXslTransform"/> for the specified stylesheet.</returns>
		public static CacheableXslTransform Load(string stylesheetPath, SageContext context)
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(stylesheetPath != null);

			object cachedItem;
			string key = string.Format(CacheKeyFormat, stylesheetPath);
			if ((cachedItem = context.Cache.Get(key)) != null && cachedItem is CacheableXslTransform)
				return (CacheableXslTransform) cachedItem;

			CacheableXslTransform transform = ResourceManager.LoadXslStylesheet(stylesheetPath, context);
			IEnumerable<string> dependencies = transform.Dependencies.Where(d => new Uri(d).Scheme == "file");

			context.Cache.Insert(key, transform, new CacheDependency(dependencies.ToArray()));

			return transform;
		}

		private static bool IsEmptyStylesheet(string stylesheetPath)
		{
			XmlDocument xslDocument = new XmlDocument();
			xslDocument.Load(stylesheetPath);

			XmlNodeList templates = xslDocument.SelectNodes("/xsl:stylesheet/xsl:template", XmlNamespaces.Manager);
			XmlNodeList parameters = xslDocument.SelectNodes("/xsl:stylesheet/xsl:param", XmlNamespaces.Manager);
			XmlNodeList includes = xslDocument.SelectNodes("/xsl:stylesheet/xsl:include", XmlNamespaces.Manager);

			return
				templates.Count == 0 &&
				parameters.Count == 0 &&
				includes.Count == 1;
		}
	}
}
