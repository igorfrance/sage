namespace Sage.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Web.Mvc;

	using Kelp.ResourceHandling;

	using Sage.Routing;

	using log4net;

	using Sage.Configuration;
	using Sage.Controllers.Dev;
	using Sage.ResourceManagement;

	/// <summary>
	/// Implements a controller that acts as a proxy that returns specified files.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The (partial) path to the resource follows the route name of the controller and must include the file extension: 
	/// <c>fileproxy/{path_to_desired_resource}.{extension}</c>
	/// </para>
	/// <para>
	/// If the file begins with <see cref="ProjectConfiguration.SharedCategory"/>, then the resource will be looked up in the
	/// shared category directory. In all other cases, the current category will be prepended to the specified resource path.
	/// </para>
	/// <para>
	/// The <c>path_to_desired_resource</c> never specifies the locale; the proxy will always use the current locale 
	/// when looking for the actual resource, by calling <c>ResolvePath</c> method of <see cref="PathResolver"/>.
	/// </para>
	/// <para>
	/// In addition to going through the standard localization lookup procedure, different resources types will be handled 
	/// differently:
	/// <list type="bullet">
	/// <listheader>
	/// <item>
	/// <term>Extension: Content Type</term>
	/// <description>Description</description>
	/// </item>
	/// </listheader>
	/// <item>
	/// <term>xml,xsl,xslt: text/xml</term>
	/// <description>Xml files will be globalized and any link nodes that contain <c>linkId</c> and optionally <c>linkValues</c> 
	/// attributes)  will be replaced with the result of calling <see cref="UrlGenerator.GetUrl(string, string)"/> passing in 
	/// the values of these two attributes.</description>
	/// </item>
	/// <item>
	/// <term>css: text/css</term>
	/// <description>Css files will be retrieved through <see cref="CodeFile.Content"/> property.</description>
	/// </item>
	/// <item>
	/// <term>js: text/javascript</term>
	/// <description>Script files will be retrieved through <see cref="CodeFile.Content"/> property.</description>
	/// </item>
	/// <item>
	/// <term>*: all other files</term>
	/// <description>All other files are served raw as they are.</description>
	/// </item>
	/// </list>
	/// </para>
	/// </remarks>
	/// <seealso cref="CssFile"/>
	/// <seealso cref="ScriptFile"/>
	/// <seealso cref="UrlGenerator"/>
	/// <seealso cref="PathResolver"/>
	/// <seealso cref="ResourceManager"/>
	[FileLastModifiedFilter]
	public class FileProxyController : SageController
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(FileProxyController).FullName);

		/// <summary>
		/// Default action that processes each file request.
		/// </summary>
		/// <param name="requestPath">The requested file path.</param>
		/// <returns>The contents of the specified file if found, or an <c>EmptyResult</c> if no matching file has been found.</returns>
		public ActionResult GetFile(string requestPath)
		{
			if (string.IsNullOrEmpty(requestPath))
			{
				log.ErrorFormat("File proxy controller invoked without file name. Exiting");
				return PageNotFound();
			}

			string extension = Path.GetExtension(requestPath) ?? string.Empty;
			extension = extension.Replace(".", string.Empty);

			string resolvedPath = Context.Path.Localize(requestPath, true);
			if (!System.IO.File.Exists(resolvedPath))
				return PageNotFound();

			if (resolvedPath != requestPath && Context.IsDeveloperRequest)
				Response.AddHeader("OriginalFilePath", resolvedPath);

			string contentType = FileViewController.GetContentType(extension);

			switch (extension)
			{
				case "xml":
					return GetXmlContent(resolvedPath);

				case "js":
					return GetScriptContent(resolvedPath);

				case "css":
					return GetCssContent(resolvedPath);

				default:
					return GetFileContent(resolvedPath, contentType);
			}
		}

		/// <summary>
		/// Gets the content of the specified file.
		/// </summary>
		/// <param name="resolvedPath">The resolved path of the file to get.</param>
		/// <param name="contentType">Type content type of the file to get.</param>
		/// <returns>The content of the specified file.</returns>
		/// <exception cref="ArgumentNullException">
		/// 	<paramref name="resolvedPath"/> is <c>null</c>.
		/// - or -
		/// <paramref name="contentType"/> is <c>null</c>.
		/// </exception>
		internal FileContentResult GetFileContent(string resolvedPath, string contentType)
		{
			byte[] data = System.IO.File.ReadAllBytes(resolvedPath);
			FileContentResult result = new FileContentResult(data, contentType);

			this.Dependencies = new List<string> { resolvedPath };

			return result;
		}

		/// <summary>
		/// Gets the content of the xml file.
		/// </summary>
		/// <param name="resolvedPath">The resolved path of the xml file to get.</param>
		/// <returns>The content of the specified xml file.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="resolvedPath"/> is <c>null</c>.
		/// </exception>
		/// <remarks>
		/// xinclude paths are substituted, and all included files are referenced in the Dependencies property
		/// </remarks>
		internal ContentResult GetXmlContent(string resolvedPath)
		{
			if (string.IsNullOrEmpty(resolvedPath))
				throw new ArgumentNullException("resolvedPath");

			CacheableXmlDocument cacheable = Context.Resources.LoadXml(resolvedPath);

			this.Dependencies = new List<string>(cacheable.Dependencies);

			ContentResult result = new ContentResult
				{
					Content = cacheable.InnerXml, 
					ContentType = "text/xml", 
					ContentEncoding = Encoding.UTF8 
				};
			return result;
		}

		/// <summary>
		/// Gets the content of the script.
		/// </summary>
		/// <param name="resolvedPath">The resolved path of the script to get.</param>
		/// <returns>The content of the specified script.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="resolvedPath"/> is <c>null</c>.
		/// </exception>
		/// <remarks>
		/// files will be merged and minified depending on project.config/web.config setting.
		/// All included files are referenced in the Dependencies property
		/// </remarks>
		/// <seealso cref="ScriptFile"/>
		internal ContentResult GetScriptContent(string resolvedPath)
		{
			if (string.IsNullOrEmpty(resolvedPath))
				throw new ArgumentNullException("resolvedPath");

			ScriptFile script = new ScriptFile(resolvedPath, resolvedPath, Context.MapPath);

			ContentResult result = new ContentResult 
			{ 
				Content = script.Content, 
				ContentType = "text/javascript"
			};

			this.Dependencies = script.Dependencies;

			return result;
		}

		/// <summary>
		/// Gets the content of the style sheet.
		/// </summary>
		/// <param name="resolvedPath">The resolved path of the style sheet to get.</param>
		/// <returns>The content of the specified style sheet.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="resolvedPath"/> is <c>null</c>.
		/// </exception>
		/// <remarks>
		/// files will be merged and minified depending on project.config/web.config setting.
		/// All included files are referenced in the Dependencies property
		/// </remarks>
		/// <seealso cref="CssFile"/>
		internal ContentResult GetCssContent(string resolvedPath)
		{
			if (string.IsNullOrEmpty(resolvedPath))
				throw new ArgumentNullException("resolvedPath");

			CssFile script = new CssFile(resolvedPath, resolvedPath, Context.MapPath);
			ContentResult result = new ContentResult
			{
				Content = script.Content,
				ContentType = "text/javascript"
			};

			this.Dependencies = script.Dependencies;

			return result;
		}
	}
}
