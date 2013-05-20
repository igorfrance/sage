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