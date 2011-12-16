namespace Sage.Controllers.Dev
{
	using System;
	using System.IO;
	using System.Web.Mvc;

	using Sage.Routing;

	/// <summary>
	/// Serves file contents from any location available to the current web application's identity.
	/// </summary>
	public class FileViewController : SageController
	{
		/// <summary>
		/// Views the content of the specified file.
		/// </summary>
		/// <param name="path">The path to the file to view.</param>
		/// <returns>A new <see cref="FileContentResult"/>, initialized with the file content and content type</returns>
		[UrlRoute(Path = "dev/file/{*path}")]
		public ActionResult ViewFile(string path)
		{
			if (!Context.IsDeveloperRequest || string.IsNullOrEmpty(path))
				return PageNotFound();

			string resolvedPath = path;
			if (!Path.IsPathRooted(path))
				resolvedPath = Context.MapPath("~/" + path);
			
			if (!System.IO.File.Exists(resolvedPath))
				return PageNotFound();

			string extension = Path.GetExtension(resolvedPath).Replace(".", string.Empty);
			string contentType = GetContentType(extension);

			byte[] data = System.IO.File.ReadAllBytes(resolvedPath);
			FileContentResult result = new FileContentResult(data, contentType);
			return result;
		}

		/// <summary>
		/// Gets the content type associated with the specified <paramref name="extension"/>.
		/// </summary>
		/// <returns>Content type associated with the specified <paramref name="extension"/></returns>
		/// <remarks>
		/// The extensions resolve to content type as follows:
		/// <list type="table">
		/// <listheader>
		/// <term>Extension(s)</term>
		/// <description>Content type</description>
		/// </listheader>
		/// <item><term>jpg, jpeg</term>
		/// <description>image/jpeg</description></item>
		/// <item><term>png</term>
		/// <description>image/png</description></item>
		/// <item><term>bmp</term>
		/// <description>image/bmp</description></item>
		/// <item><term>gif</term>
		/// <description>image/gif</description></item>
		/// <item><term>xml, xsl, xslt</term>
		/// <description>text/xml</description></item>
		/// <item><term>js</term>
		/// <description>text/javascript</description></item>
		/// <item><term>css</term>
		/// <description>text/css</description></item>
		/// <item><term>html</term>
		/// <description>text/html</description></item>
		/// <item><term>txt</term>
		/// <description>text/plain</description></item>
		/// <item><term>swf</term>
		/// <description>application/x-shockwave-flash</description></item>
		/// <item><term>(Any other) &lt;extension&gt;</term>
		/// <description>application/&lt;extension&gt;</description></item>
		/// </list>
		/// </remarks>
		internal static string GetContentType(string extension)
		{
			switch (extension)
			{
				case "jpg":
				case "jpeg":
					return "image/jpeg";

				case "png":
					return "image/png";

				case "bmp":
					return "image/bmp";

				case "gif":
					return "image/gif";

				case "xml":
				case "xsl":
				case "xslt":
					return "text/xml";

				case "js":
					return "text/javascript";

				case "css":
					return "text/css";

				case "html":
					return "text/html";

				case "txt":
					return "text/plain";

				case "swf":
					return "application/x-shockwave-flash";

				default:
					return "application/" + extension;
			}
		}
	}
}
