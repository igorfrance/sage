namespace Kelp.Test
{
	using System;
	using System.Configuration;
	using System.IO;
	using System.Web;

	using Kelp.HttpMock;

	/// <summary>
	/// Mocks various MVC and HTTP objects.
	/// </summary>
	public static class HttpMock
	{
		public static string WebApplicationPath
		{
			get
			{
				string sitePath = ConfigurationManager.AppSettings["site-path"];
				return Path.Combine(Utilities.ApplicationPath, sitePath);
			}
		}

		public static Uri FakeUri(string url)
		{
			if (url.StartsWith("http://"))
				return new Uri(url);
			
			if (url.StartsWith("/"))
				return new Uri("http://localhost" + url);

			return new Uri("http://localhost/" + url);
		}

		public static HttpContextBase FakeHttpContext(string url)
		{
			return FakeHttpContext(url, "default.aspx");
		}

		public static HttpContextBase FakeHttpContext(string url, string physicalPath)
		{
			return new HttpContextMock(url, physicalPath, null, null, HttpMock.MapPath);
		}

		internal static string MapPath(string path)
		{
			string result = path.Replace(
				"~", WebApplicationPath).Replace(
				"//", "/").Replace(
				"/", "\\");

			try
			{
				return new FileInfo(result).FullName;
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}