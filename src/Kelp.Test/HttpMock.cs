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