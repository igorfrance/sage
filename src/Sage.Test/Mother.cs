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
namespace Sage.Test
{
	using System;
	using System.Collections.Specialized;
	using System.Configuration;
	using System.IO;
	using System.Web;
	using System.Web.Routing;

	using Kelp;
	using Kelp.HttpMock;
	using Rhino.Mocks;
	using Sage.Controllers;

	/// <summary>
	/// Mocks various MVC and HTTP objects.
	/// </summary>
	public static class Mother
	{
		public static string WebApplicationPath
		{
			get
			{
				string sitePath = ConfigurationManager.AppSettings["site-path"];
				return Path.Combine(Utilities.ApplicationPath, sitePath);
			}
		}

		public static Uri CreateUri(string url)
		{
			if (url.StartsWith("http://"))
				return new Uri(url);

			if (url.StartsWith("/"))
				return new Uri("http://localhost" + url);

			return new Uri("http://localhost/" + url);
		}

		public static HttpContextBase CreateHttpContext(string url)
		{
			return CreateHttpContext(url, "default.aspx");
		}

		public static HttpContextBase CreateHttpContext(string url, string physicalPath)
		{
			return new HttpContextMock(url, physicalPath, null, null, Mother.MapPath);
		}

		/// <summary>
		/// Creates a new fake HTTP request with its <c>url</c> and <c>physicalPath</c> properties set to <c>null</c>.
		/// </summary>
		/// <returns>A fake HTTP request object.</returns>
		public static HttpRequestBase CreateHttpRequest()
		{
			return CreateHttpRequest(null, null);
		}

		/// <summary>
		/// Creates a new fake HTTP request with its <c>url</c> and <c>physicalPath</c> properties set to specified values.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="physicalPath">The physical path.</param>
		/// <returns>A fake HTTP request object.</returns>
		public static HttpRequestBase CreateHttpRequest(string url, string physicalPath)
		{
			HttpRequestBase request = MockRepository.GenerateStub<HttpRequestBase>();

			if (url != null)
				request.SetupRequestUrl(url);

			if (physicalPath != null)
				request.Stub(r => r.PhysicalPath).Return(physicalPath);

			return request;
		}

		/// <summary>
		/// Creates a new request context, using the specified request <paramref name="url"/>.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <returns>
		/// A new request context.
		/// </returns>
		public static RequestContext CreateRequestContext(string url)
		{
			return CreateRequestContext(CreateHttpContext(url));
		}

		/// <summary>
		/// Creates a new request context, using the specified category and locale.
		/// </summary>
		/// <param name="category">The category for which to create the fake context.</param>
		/// <param name="locale">The locale for which to create the fake context.</param>
		/// <returns>A new request context.</returns>
		public static RequestContext CreateRequestContext(string category, string locale)
		{
			return CreateRequestContext(CreateHttpContext(string.Format("default.aspx?category={0}&locale={1}", category, locale)));
		}

		/// <summary>
		/// Creates a new request context.
		/// </summary>
		/// <param name="httpContext">The HTTP context to use with the resulting context.</param>
		/// <returns>A new request context.</returns>
		public static RequestContext CreateRequestContext(HttpContextBase httpContext)
		{
			return CreateRequestContext(httpContext, new RouteData());
		}

		/// <summary>
		/// Creates a new request context.
		/// </summary>
		/// <param name="httpContext">The HTTP context to use with the resulting context.</param>
		/// <param name="routeData">The route data to use with the resulting context.</param>
		/// <returns>A new request context.</returns>
		public static RequestContext CreateRequestContext(HttpContextBase httpContext, RouteData routeData)
		{
			RequestContext context = new RequestContext(httpContext, routeData);
			return context;
		}

		/// <summary>
		/// Creates a new fake <see cref="SageContext"/> instance using the specified <paramref name="url"/>.
		/// </summary>
		/// <param name="url">The URL to use when creating the new context.</param>
		/// <returns>A new <see cref="SageContext"/> instance.</returns>
		public static SageContext CreateSageContext(string url)
		{
			return CreateSageContext(url, Mother.MapPath);
		}

		/// <summary>
		/// Creates a Sage context using the specified <paramref name="category"/> and <paramref name="locale"/>.
		/// </summary>
		/// <param name="category">The category to set on the context being created.</param>
		/// <param name="locale">The locale to set on the context being created.</param>
		/// <returns>A new <see cref="SageContext"/> instance.</returns>
		public static SageContext CreateSageContext(string category, string locale)
		{
			return CreateSageContext(string.Format("default.aspx?category={0}&locale={1}", category, locale), Mother.MapPath);
		}

		/// <summary>
		/// Creates a Sage context using the specified <paramref name="category"/>, <paramref name="locale"/> and <paramref name="query"/>.
		/// </summary>
		/// <param name="category">The category to set on the context being created.</param>
		/// <param name="locale">The locale to set on the context being created.</param>
		/// <param name="query">The query string of the fake request to create.</param>
		/// <returns>
		/// A new <see cref="SageContext"/> instance.
		/// </returns>
		public static SageContext CreateSageContext(string category, string locale, string query)
		{
			return CreateSageContext(category, locale, new QueryString(query));
		}

		/// <summary>
		/// Creates a Sage context using the specified <paramref name="category"/>, <paramref name="locale"/> and <paramref name="query"/>.
		/// </summary>
		/// <param name="category">The category to set on the context being created.</param>
		/// <param name="locale">The locale to set on the context being created.</param>
		/// <param name="query">The query string of the fake request to create.</param>
		/// <returns>
		/// A new <see cref="SageContext"/> instance.
		/// </returns>
		public static SageContext CreateSageContext(string category, string locale, QueryString query)
		{
			var result = CreateSageContext(string.Format("default.aspx?category={0}&locale={1}", category, locale), Mother.MapPath);
			result.Query.Merge(query);

			return result;
		}

		/// <summary>
		/// Creates a new fake <see cref="SageContext"/> instance.
		/// </summary>
		/// <param name="url">The URL to use within the created context.</param>
		/// <param name="pathMapper">The function to use for resolving relative paths.</param>
		/// <returns>A new <see cref="SageContext"/> instance.</returns>
		public static SageContext CreateSageContext(string url, Func<string, string> pathMapper)
		{
			HttpContextBase httpContext =
				Mother.CreateHttpContext(url, "default.aspx");

			SageContext context = new SageContext(httpContext, pathMapper);
			return context;
		}

		public static SageController CreateMockController()
		{
			RequestContext context = Mother.CreateRequestContext("basketball", "nl");
			SageControllerFactory factory = new SageControllerFactory();
			factory.InitializeWithControllerTypes(new[] { typeof(MockController) });

			SageController ctrl = factory.CreateController(context, "Mock") as SageController;
			return ctrl;
		}

		/// <summary>
		/// Sets the <see cref="HttpRequest.HttpMethod"/> value to the specified string.
		/// </summary>
		/// <param name="request">The request to modify.</param>
		/// <param name="httpMethod">The HTTP method to set.</param>
		public static void SetHttpMethodResult(this HttpRequestBase request, string httpMethod)
		{
			SetupResult.For(request.HttpMethod).Return(httpMethod);
		}

		/// <summary>
		/// Sets the various URL-related properties of the to parts of the specified URL.
		/// </summary>
		/// <param name="request">The request to modify.</param>
		/// <param name="url">The URL to set.</param>
		/// <exception cref="ArgumentNullException">If the <paramref name="url"/> is <c>null</c>.</exception>
		public static void SetupRequestUrl(this HttpRequestBase request, string url)
		{
			if (url == null)
				throw new ArgumentNullException("url");

			request.Stub(r => r.QueryString).Return(GetQueryStringParameters(url));
			request.Stub(r => r.AppRelativeCurrentExecutionFilePath).Return(GetUrlFileName(url));
			request.Stub(r => r.PathInfo).Return(string.Empty);
		}

		private static string MapPath(string path)
		{
			if (path == "/")
				path = "~/";

			string result = path.Replace(
				"~", WebApplicationPath).Replace(
				"//", "/").Replace(
				"/", "\\");

			return new FileInfo(result).FullName;
		}

		/// <summary>
		/// Given a URL, returns the name without the query string part.
		/// </summary>
		/// <param name="url">The URL to parse.</param>
		/// <returns>The file name part from a URL string.</returns>
		private static string GetUrlFileName(string url)
		{
			if (url.Contains("?"))
				return url.Substring(0, url.IndexOf("?"));

			return url;
		}

		/// <summary>
		/// Given a URL, returns the just the query string part.
		/// </summary>
		/// <param name="url">The URL to parse.</param>
		/// <returns>The query part from a URL string.</returns>
		private static NameValueCollection GetQueryStringParameters(string url)
		{
			if (url.Contains("?"))
			{
				NameValueCollection parameters = new NameValueCollection();

				string[] parts = url.Split("?".ToCharArray());
				string[] keys = parts[1].Split("&".ToCharArray());

				foreach (string key in keys)
				{
					string[] part = key.Split("=".ToCharArray());
					parameters.Add(part[0], part[1]);
				}

				return parameters;
			}

			return null;
		}
	}
}
