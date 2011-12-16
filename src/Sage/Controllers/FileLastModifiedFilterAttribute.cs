namespace Sage.Controllers
{
	using System;
	using System.IO;
	using System.Net;
	using System.Web;
	using System.Web.Mvc;

	using Kelp.Http;
	using Kelp.ResourceHandling;

	using Microsoft.Win32;

	/// <summary>
	/// Ensures that a controller or a controller method gets cached by using the last-modified and etag headers.
	/// </summary>
	/// <remarks>
	/// This is done by determining the latest last-modified DateTime of the Dependencies property of the <see cref="SageController"/>
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class FileLastModifiedFilterAttribute : ActionFilterAttribute
	{
		/// <summary>
		/// Called by the MVC framework before the action method executes.
		/// </summary>
		/// <param name="filterContext">The filter context.</param>
		/// <remarks>
		/// First the Last-Modified DateTime is fetched from the cache. This cache-entry automatically invalidates 
		/// When one of the dependend files change, so no cache entry is present.
		/// If there is a Last-Modified DateTime entry and it is the same as the if-not-modified-since request header
		/// and also the etags match a HTTP 304 Not modified is send and the action is cancelled.
		/// </remarks>
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (!filterContext.Controller.GetType().IsSubclassOf(typeof(SageController)))
				return;

			var context = filterContext.HttpContext;
			var response = context.Response;


			var relativePath = GetPath(context);
			var lastModified = LastModifiedDateCache.Get(context, relativePath);
			var lastCachedString = context.Request.Headers["If-Modified-Since"];

			if (!string.IsNullOrEmpty(lastCachedString))
			{
				DateTime cachedDate = DateTime.Parse(lastCachedString);
				if (lastModified <= cachedDate)
				{
					response.SuppressContent = true;
					response.StatusCode = (int) HttpStatusCode.NotModified;
					response.StatusDescription = "Not Modified";
					response.AddHeader("Content-Length", "0");
					response.ContentType = GetContentType(Path.GetExtension(relativePath));
					response.Cache.SetCacheability(HttpCacheability.Public);
					response.Cache.SetLastModified(lastModified);

					filterContext.Result = new EmptyResult();
				}
			}
		}

		/// <summary>
		/// Called by the MVC framework after the action method executes.
		/// </summary>
		/// <param name="filterContext">The filter context.</param>
		/// <remarks>
		/// Gets the latest Last-Modified of the <see cref="SageController.Dependencies"/> and 
		/// puts it in the cache for this Request.
		/// All <see cref="SageController.Dependencies"/> are added as filedependencies to the cache, so
		/// if any of these files change the cache-entry is automatically deleted from the cache
		/// </remarks>
		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			if (!filterContext.Controller.GetType().IsSubclassOf(typeof(SageController)))
				return;

			if (filterContext.Exception != null)
				return;

			var context = filterContext.HttpContext;
			string relativePath = GetPath(context);

			var dependencies = ((SageController)filterContext.Controller).Dependencies;

			if (dependencies == null || dependencies.Count == 0)
				return;

			DateTime lastModified = Util.GetDateLastModified(dependencies).ToUniversalTime();

			LastModifiedDateCache.Store(context, relativePath, lastModified, dependencies.ToArray());
			context.Response.ContentType = GetContentType(Path.GetExtension(relativePath));
			context.Response.Cache.SetCacheability(HttpCacheability.Public);
			context.Response.Cache.SetLastModified(lastModified);
		}

		/// <summary>
		/// Reconstruct the request path for the <paramref name="context"/>
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns>The reconstructed request path</returns>
		protected virtual string GetPath(HttpContextBase context)
		{
			return string.Format("/{1}/{2}{0}", context.Request.Path, context.Request.Params[SageContext.CategoryVariableName], context.Request.Params[SageContext.LocaleVariableName]);
		}

		/// <summary>
		/// Gets the HTTP Content-Type for the specified <paramref name="extension"/>.
		/// </summary>
		/// <param name="extension">The extension of a file</param>
		/// <returns>
		/// The HTTP Content-Type that matches the specified <paramref name="extension"/>.
		/// </returns>
		private static string GetContentType(string extension)
		{
			// set a default mimetype if not found.
			string contentType = "application/octet-stream";

			try
			{
				// get the registry classes root
				RegistryKey classes = Registry.ClassesRoot;

				// find the sub key based on the file extension
				RegistryKey fileClass = classes.OpenSubKey(extension);
				contentType = fileClass.GetValue("Content Type").ToString();
			}
			catch
			{
			}

			return contentType;
		}
	}
}