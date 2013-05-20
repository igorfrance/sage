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
namespace Kelp.HttpMock
{
	using System;
	using System.Collections.Specialized;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Web;

	/// <summary>
	/// Mocks an <see cref="HttpRequest"/>, enabling testing and independent execution of web context dependent code.
	/// </summary>
	[SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter",
		Justification = "Reviewed. Suppression is OK here.")]
	[SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter",
		Justification = "Reviewed. Suppression is OK here.")]
	public class HttpRequestMock : HttpRequestBase
	{
		/// <summary>
		/// Defines the default scheme to use for completing urls that don't provide one.
		/// </summary>
		public static string DefaultUrlScheme = "http";

		/// <summary>
		/// Defines the default server name to use for completing urls that don't provide one.
		/// </summary>
		public static string DefaultServerName = "localhost";

		/// <summary>
		/// Defines the default application path to use for constructing instances whose constructor doesn't provide one.
		/// </summary>
		public static string DefaultApplicationPath = "/";

		/// <summary>
		/// Defines the default physical application path to use for constructing instances whose constructor doesn't provide one.
		/// </summary>
		public static string DefaultPhysicalApplicationPath = @"c:\inetpub\wwwroot\myapp";

		internal string applicationPath;
		internal HttpCookieCollection cookies;
		internal NameValueCollection form;
		internal NameValueCollection headers;
		internal NameValueCollection queryString;
		internal NameValueCollection serverVariables;
		internal string httpMethod = "GET";
		internal string physicalApplicationPath;
		internal string requestType;
		internal Uri url;
		internal Uri urlReferrer;
		internal string userAgent =
			"Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US; rv:1.9.2.3) Gecko/20100401 Firefox/3.6.3 (.NET CLR 3.5.30729)";

		internal string userHostAddress = "127.0.0.1";

		private string urlRaw;
		private string physicalPath;

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpRequestMock"/> class.
		/// </summary>
		public HttpRequestMock()
		{
			this.applicationPath = DefaultApplicationPath;
			this.physicalApplicationPath = DefaultPhysicalApplicationPath;

			this.rawUrl = "/";

			this.cookies = new HttpCookieCollection();
			this.form = new NameValueCollection();
			this.headers = new NameValueCollection();
			this.serverVariables = new NameValueCollection();

			this.requestType = "GET";
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpRequestMock"/> class, using the specified <paramref name="url"/>.
		/// </summary>
		/// <param name="url">The URL of the mocked request.</param>
		public HttpRequestMock(string url)
			: this(url, null, null, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpRequestMock"/> class, using the specified request URL, application path
		/// and absolute application path.
		/// </summary>
		/// <param name="url">The URL of the mocked request.</param>
		/// <param name="physicalPath">The physical path of the request.</param>
		/// <param name="appPath">The application path of the mocked request.</param>
		/// <param name="physicalAppPath">The absolute application path of the mocked request.</param>
		/// <exception cref="ArgumentNullException"><paramref name="url"/> is <c>null</c>.</exception>
		public HttpRequestMock(string url, string physicalPath, string appPath, string physicalAppPath)
			: this()
		{
			this.applicationPath = appPath ?? DefaultApplicationPath;
			this.physicalApplicationPath = physicalAppPath ?? DefaultPhysicalApplicationPath;
			this.physicalPath = physicalPath;

			this.rawUrl = url;
		}

		/// <inheritdoc/>
		public override string ApplicationPath
		{
			get
			{
				return this.applicationPath;
			}
		}

		/// <inheritdoc/>
		public override string AppRelativeCurrentExecutionFilePath
		{
			get
			{
				return string.Concat("~", Path);
			}
		}

		/// <inheritdoc/>
		public override HttpCookieCollection Cookies
		{
			get
			{
				return this.cookies;
			}
		}

		/// <inheritdoc/>
		public override NameValueCollection Form
		{
			get
			{
				return this.form;
			}
		}

		/// <inheritdoc/>
		public override NameValueCollection Headers
		{
			get
			{
				return this.headers;
			}
		}

		/// <inheritdoc/>
		public override string HttpMethod
		{
			get
			{
				return this.httpMethod;
			}
		}

		/// <inheritdoc/>
		public override NameValueCollection QueryString
		{
			get
			{
				return this.queryString;
			}
		}

		/// <inheritdoc/>
		public override HttpBrowserCapabilitiesBase Browser
		{
			get
			{
				return new HttpBrowserCapabilitiesMock();
			}
		}

		/// <inheritdoc/>
		public override string Path
		{
			get
			{
				if (this.url.IsAbsoluteUri)
				{
					return this.url.LocalPath;
				}

				return this.url.ToString();
			}
		}

		/// <inheritdoc/>
		public override string PathInfo
		{
			get
			{
				return string.Empty;
			}
		}

		/// <inheritdoc/>
		public override string PhysicalApplicationPath
		{
			get
			{
				return this.physicalApplicationPath;
			}
		}

		/// <inheritdoc/>
		public override string PhysicalPath
		{
			get
			{
				return this.physicalPath;
			}
		}

		/// <inheritdoc/>
		public override string RawUrl
		{
			get
			{
				return this.rawUrl;
			}
		}

		/// <inheritdoc/>
		public override string RequestType
		{
			get
			{
				return this.requestType;
			}
		}

		/// <inheritdoc/>
		public override NameValueCollection ServerVariables
		{
			get
			{
				return this.serverVariables;
			}
		}

		/// <inheritdoc/>
		public override Uri Url
		{
			get
			{
				return this.url;
			}
		}

		/// <inheritdoc/>
		public override Uri UrlReferrer
		{
			get
			{
				return this.urlReferrer;
			}
		}

		/// <inheritdoc/>
		public override string UserHostAddress
		{
			get
			{
				return this.userHostAddress;
			}
		}

		/// <inheritdoc/>
		public override string UserAgent
		{
			get
			{
				return this.userAgent;
			}
		}

		internal string rawUrl
		{
			get
			{
				return urlRaw;
			}

			set
			{
				this.url = SetupUri(value, DefaultUrlScheme, DefaultServerName, this.ApplicationPath);
				this.queryString = ParseString(this.url.Query);
				this.urlRaw = this.url.ToString();
				if (physicalPath == null)
				{
					physicalPath = this.url.LocalPath;
				}
			}
		}

		private static NameValueCollection ParseString(string queryString)
		{
			NameValueCollection result = new NameValueCollection();
			if (string.IsNullOrEmpty(queryString))
			{
				return result;
			}

			if (queryString.IndexOf('?') == 0)
			{
				queryString = queryString.Substring(1);
			}

			string[] values = queryString.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < values.Length; i++)
			{
				if (values[i] == string.Empty)
				{
					continue;
				}

				int index = values[i].IndexOf('=');
				if (index != -1)
				{
					string name = values[i].Substring(0, index);
					string value = values[i].Substring(index + 1);
					if (name != string.Empty)
					{
						result.Add(name, value);
					}
				}
				else
				{
					result.Add(values[i], string.Empty);
				}
			}

			return result;
		}

		private static Uri SetupUri(string path, string baseScheme, string baseServer, string baseAppPath)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(path));
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(baseScheme));
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(baseServer));
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(baseAppPath));

			path = path.Replace("~", string.Empty);

			if (!Uri.IsWellFormedUriString(path, UriKind.RelativeOrAbsolute))
				throw new ArgumentException(string.Format("The value '{0}' is not a valid uri string", path), "path");

			if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
				return new Uri(path);

			if (path.StartsWith(baseAppPath) && path.StartsWith("/"))
				return new Uri(string.Concat(baseScheme, "://", baseServer, path));

			if (path.StartsWith(baseAppPath) && !path.StartsWith("/"))
				return new Uri(string.Concat(baseScheme, "://", baseServer, "/", path));

			if (path.StartsWith("/"))
				return new Uri(string.Concat(baseScheme, "://", baseServer, path));

			return new Uri(string.Concat(baseScheme, "://", baseServer, baseAppPath, "/", path));
		}
	}
}