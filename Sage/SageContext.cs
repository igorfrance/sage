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
namespace Sage
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Web;
	using System.Web.Hosting;
	using System.Web.Mvc;
	using System.Web.Routing;
	using System.Xml;

	using Kelp;
	using Kelp.Extensions;
	using log4net;
	using Sage.Configuration;
	using Sage.Extensibility;
	using Sage.ResourceManagement;

	/// <summary>
	/// Provides the working environment for code within Sage.
	/// </summary>
	public class SageContext : IXmlConvertible
	{
		/// <summary>
		/// The name of the query string variable that specifies the current category.
		/// </summary>
		public const string CategoryVariableName = "category";

		/// <summary>
		/// The name of the query string variable that specifies the current locale.
		/// </summary>
		public const string LocaleVariableName = "locale";

		/// <summary>
		/// The name of the query string variable that instructs the system to load the latest resources
		/// without considering the cache.
		/// </summary>
		public const string RefreshVariableName = "refresh";

		private static readonly Dictionary<string, CategoryConfiguration> categoryConfigurations =
			new Dictionary<string, CategoryConfiguration>();

		private static readonly ILog log = LogManager.GetLogger(typeof(SageContext).FullName);
		private readonly NodeEvaluator nodeEvaluator;
		private readonly Func<string, string> pathMapper;
		private readonly TextEvaluator textEvaluator;

		/// <summary>
		/// Initializes a new instance of the <see cref="SageContext"/> class, using an existing context instance.
		/// </summary>
		/// <param name="context">An existing <see cref="SageContext"/> to use to initialize this instance.</param>
		/// <param name="config">The project configuration to use with this context instance.</param>
		public SageContext(SageContext context, ProjectConfiguration config = null)
			: this(context, context.Category, config)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SageContext"/> class, using an existing context instance.
		/// </summary>
		/// <param name="context">An existing <see cref="SageContext"/> to use to initialize this instance.</param>
		/// <param name="categoryName">The name of the category to set on the new context.</param>
		/// <param name="config">The project configuration to use with this context instance.</param>
		public SageContext(SageContext context, string categoryName, ProjectConfiguration config = null)
			: this(context.HttpContext, config)
		{
			this.Category = categoryName;
			this.pathMapper = context.MapPath;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SageContext"/> class, using the specified <see cref="ControllerContext"/>.
		/// </summary>
		/// <param name="controllerContext">The current <see cref="ControllerContext"/> to use to initialize this instance.</param>
		/// <param name="config">The project configuration to use with this context instance.</param>
		public SageContext(ControllerContext controllerContext, ProjectConfiguration config = null)
			: this(controllerContext.HttpContext, config)
		{
			this.Route = controllerContext.RouteData.Route;
			this.RouteValues = new NameValueCollection();
			foreach (string key in controllerContext.RouteData.Values.Keys)
			{
				this.RouteValues[key] = controllerContext.RouteData.Values[key] as string;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SageContext"/> class, using the specified <see cref="HttpContextBase"/>.
		/// </summary>
		/// <param name="httpContext">The current <see cref="HttpContextBase"/> to use to initialize this instance.</param>
		/// <param name="config">The project configuration to use with this context instance.</param>
		public SageContext(HttpContextBase httpContext, ProjectConfiguration config = null)
			: this(httpContext, httpContext.Server.MapPath, config)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SageContext"/> class, using the specified <see cref="HttpContext"/>.
		/// </summary>
		/// <param name="httpContext">The current <see cref="HttpContext"/> to use to initialize this instance.</param>
		/// <param name="config">The project configuration to use with this context instance.</param>
		public SageContext(HttpContext httpContext, ProjectConfiguration config = null)
			: this(new HttpContextWrapper(httpContext), config)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SageContext"/> class, using the specified <paramref name="httpContext"/> and
		/// <paramref name="categoryName"/>.
		/// </summary>
		/// <param name="httpContext">The current HTTP context.</param>
		/// <param name="categoryName">The category to set for this instance.</param>
		/// <param name="config">The project configuration to use with this context instance.</param>
		public SageContext(HttpContextBase httpContext, string categoryName, ProjectConfiguration config = null)
			: this(httpContext, config)
		{
			this.Category = categoryName;
			this.pathMapper = httpContext.Server.MapPath;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SageContext"/> class, using the specified <paramref name="httpContext"/> and
		/// <paramref name="categoryName"/>.
		/// </summary>
		/// <param name="httpContext">The current HTTP context.</param>
		/// <param name="categoryName">The category to set for this instance.</param>
		/// <param name="localeName">The locale to set for this instance.</param>
		/// <param name="config">The project configuration to use with this context instance.</param>
		public SageContext(HttpContextBase httpContext, string categoryName, string localeName, ProjectConfiguration config = null)
			: this(httpContext, config)
		{
			this.Category = categoryName;
			this.Locale = localeName;
			this.pathMapper = httpContext.Server.MapPath;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SageContext"/> class, using the specified <paramref name="httpContext"/>,
		/// <paramref name="categoryName"/> and <paramref name="pathMapper"/>.
		/// </summary>
		/// <param name="httpContext">The current HTTP context.</param>
		/// <param name="categoryName">The category to set for this instance.</param>
		/// <param name="pathMapper">The function to use to map relative paths to absolute.</param>
		/// <param name="config">The project configuration to use with this context instance.</param>
		public SageContext(HttpContextBase httpContext, string categoryName, Func<string, string> pathMapper, ProjectConfiguration config = null)
			: this(httpContext, config)
		{
			this.Category = categoryName;
			this.pathMapper = pathMapper;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SageContext"/> class, using the specified <see cref="HttpContextBase"/> and
		/// <paramref name="pathMapper"/>.
		/// </summary>
		/// <param name="httpContext">The current <see cref="HttpContextBase"/> to use to initialize this instance.</param>
		/// <param name="pathMapper">The function to use for resolving relative paths.</param>
		/// <param name="config">The project configuration to use with this context instance.</param>
		public SageContext(HttpContextBase httpContext, Func<string, string> pathMapper, ProjectConfiguration config = null)
		{
			Contract.Requires<ArgumentNullException>(httpContext != null);
			Contract.Requires<ArgumentNullException>(pathMapper != null);

			this.pathMapper = pathMapper;
			this.textEvaluator = new TextEvaluator(this);
			this.nodeEvaluator = new NodeEvaluator(this);

			//// In certain cases, an instance of Sage context is needed while the request
			//// may not be available (when creating it from Application_Start event for instance)
			//// This variable checks for that scenario, so that we can then initialize this instance
			//// with less features.

			bool requestAvailable = false;
			try
			{
				requestAvailable = httpContext.Request != null;
			}
			catch (HttpException ex)
			{
				// request unavailable
				if (ex.ErrorCode != -2147467259)
					throw;
			}

			this.ProjectConfiguration = config ?? Sage.Project.Configuration;
			this.Query = new QueryString();
			this.Cache = new CacheWrapper(httpContext);
			this.LmCache = new LastModifiedCache(this);

			this.HttpContext = httpContext;

			this.Locale = this.Query.GetString(LocaleVariableName, this.ProjectConfiguration.DefaultLocale);
			this.Category = this.Query.GetString(CategoryVariableName);

			if (string.IsNullOrEmpty(this.Category) || !this.ProjectConfiguration.Categories.ContainsKey(this.Category))
				this.Category = this.ProjectConfiguration.DefaultCategory;

			if (string.IsNullOrEmpty(this.Locale))
				this.Locale = this.ProjectConfiguration.DefaultLocale;

			this.Query.Remove(LocaleVariableName);
			this.Query.Remove(CategoryVariableName);
			this.Query.Remove(RefreshVariableName);

			this.Url = new UrlGenerator(this);
			this.Path = new PathResolver(this);
			this.Resources = new ResourceManager(this);

			if (requestAvailable)
			{
				bool isNoCacheRequest = httpContext.Request.Headers["Cache-Control"] == "no-cache";

				this.Query = new QueryString(httpContext.Request.QueryString);
				this.ForceRefresh = isNoCacheRequest ||
					(this.IsDeveloperRequest &&
					this.Query.GetString(RefreshVariableName).EqualsAnyOf("1", "true", "yes"));

				if (httpContext.Request.UrlReferrer != null)
					this.ReferrerUrl = httpContext.Request.UrlReferrer.ToString();

				this.ApplicationPath = httpContext.Request.ApplicationPath.TrimEnd('/') + "/";
				this.PhysicalApplicationPath = httpContext.Request.PhysicalApplicationPath;
				this.UserAgentID = httpContext.Request.Browser.Type.ToLower();
				this.UserAgentType = httpContext.Request.Browser.Crawler
					? UserAgentType.Crawler
					: UserAgentType.Browser;

				this.SubstituteExtensionPath();
			}
			else
			{
				this.ApplicationPath = (HostingEnvironment.ApplicationVirtualPath ?? "/").TrimEnd('/') + "/";
				this.PhysicalApplicationPath = HostingEnvironment.ApplicationPhysicalPath ?? @"c:\inetpub\wwwroot";
			}
		}

		/// <summary>
		/// Gets the current <see cref="HttpApplicationStateBase"/>.
		/// </summary>
		public HttpApplicationStateBase Application
		{
			get
			{
				return this.HttpContext.Application;
			}
		}

		/// <summary>
		/// Gets the virtual root path of the ASP.NET application on the server.
		/// </summary>
		public string ApplicationPath { get; internal set; }

		/// <summary>
		/// Gets the base <c>href</c> value (e.g. <c>http://server:port/appname/</c>) for requests within current context.
		/// </summary>
		public string BaseHref
		{
			get
			{
				return
					this.Request.Url.GetLeftPart(UriPartial.Authority) +
					this.Request.ApplicationPath + (this.Request.ApplicationPath.EndsWith("/") ? string.Empty : "/");
			}
		}

		/// <summary>
		/// Gets the current <see cref="Cache"/>.
		/// </summary>
		/// <value>The cache.</value>
		public CacheWrapper Cache { get; private set; }

		/// <summary>
		/// Gets the currently applicable category.
		/// </summary>
		public string Category { get; private set; }

		/// <summary>
		/// Gets the category configuration instance for the current category, if the current category uses a category configuration file.
		/// </summary>
		/// <value>If the current category does not define it's configuration, the value will be <c>null</c>.</value>
		public CategoryConfiguration CategoryConfiguration
		{
			get
			{
				if (!categoryConfigurations.ContainsKey(this.Category))
				{
					if (!File.Exists(this.Path.PhysicalCategoryConfigurationPath))
						return null;

					CategoryConfiguration ccfg = CategoryConfiguration.Create(this.Path.PhysicalCategoryConfigurationPath);
					if (ccfg.Name != this.Category)
					{
						throw new ConfigurationError(string.Format(
							"Mismatched category name. Current category is '{0}', while the name in category " +
							"configuration document '{1}' is '{2}'. Please make sure the names match before attempting " +
							"to run this project again.", this.Category, this.Path.CategoryConfigurationPath, ccfg.Name));
					}

					categoryConfigurations[this.Category] = ccfg;
				}

				return categoryConfigurations[this.Category];
			}
		}

		/// <summary>
		/// Gets a value indicating whether the system should load the latest resources without considering the cache.
		/// </summary>
		public bool ForceRefresh { get; private set; }

		/// <summary>
		/// Gets the current <see cref="HttpContextBase"/>.
		/// </summary>
		public HttpContextBase HttpContext { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this context is running within a developer request.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is running within a developer request; otherwise, <c>false</c>.
		/// </value>
		public bool IsDeveloperRequest
		{
			get
			{
				return this.ProjectConfiguration.Environment.IsDeveloperIp(this.Request.UserHostAddress) ||
					(this.Session != null && this.Session["developer"] != null && (bool) this.Session["developer"]);
			}
		}

		/// <summary>
		/// Gets the last modification date cache.
		/// </summary>
		public LastModifiedCache LmCache { get; private set; }

		/// <summary>
		/// Gets the currently applicable locale.
		/// </summary>
		public string Locale { get; private set; }

		/// <summary>
		/// Gets the <see cref="Sage.Configuration.LocaleInfo"/> associated with the <see cref="Locale"/> of the
		/// current context.
		/// </summary>
		public LocaleInfo LocaleInfo
		{
			get
			{
				return ProjectConfiguration.Locales[this.Locale];
			}
		}

		/// <summary>
		/// Gets <see cref="PathResolver"/> to be used with the current context.
		/// </summary>
		public PathResolver Path { get; private set; }

		/// <summary>
		/// Gets the physical file-system path of the current application's root directory.
		/// </summary>
		public string PhysicalApplicationPath { get; private set; }

		/// <summary>
		/// Gets the project configuration.
		/// </summary>
		public ProjectConfiguration ProjectConfiguration { get; internal set; }

		/// <summary>
		/// Gets the query string active within the current context.
		/// </summary>
		public QueryString Query { get; private set; }

		/// <summary>
		/// Gets the URL of the client request that linked to the current URL.
		/// </summary>
		public string ReferrerUrl { get; private set; }

		/// <summary>
		/// Gets the current <see cref="HttpRequestBase"/>.
		/// </summary>
		public HttpRequestBase Request
		{
			get
			{
				return this.HttpContext.Request;
			}
		}

		/// <summary>
		/// Gets <see cref="ResourceManager"/> to be used with the current context.
		/// </summary>
		public ResourceManager Resources { get; private set; }

		/// <summary>
		/// Gets the current <see cref="HttpResponseBase"/>.
		/// </summary>
		public HttpResponseBase Response
		{
			get
			{
				return this.HttpContext.Response;
			}
		}

		/// <summary>
		/// Gets the current route.
		/// </summary>
		public RouteBase Route { get; private set; }

		/// <summary>
		/// Gets the collection of current route's route values.
		/// </summary>
		public NameValueCollection RouteValues { get; private set; }

		/// <summary>
		/// Gets the current <see cref="HttpSessionStateBase"/>.
		/// </summary>
		public HttpSessionStateBase Session
		{
			get
			{
				return this.HttpContext.Session;
			}
		}

		/// <summary>
		/// Gets <see cref="UrlGenerator"/> to be used with the current context.
		/// </summary>
		public UrlGenerator Url { get; private set; }

		/// <summary>
		/// Gets the id of the current user agent.
		/// </summary>
		public string UserAgentID { get; private set; }

		/// <summary>
		/// Gets the type of the current user agent.
		/// </summary>
		public UserAgentType UserAgentType { get; private set; }

		/// <summary>
		/// Returns an absolute URL, fully expanded to a path within the current application.
		/// </summary>
		/// <param name="path">The path to expand</param>
		/// <returns>The specified path, fully expanded to a path within the current application.</returns>
		/// <exception cref="ArgumentNullException">
		/// If the <paramref name="path"/> argument is empty or a <c>null</c>.
		/// </exception>
		public string ExpandUrl(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			if (path.Contains("://"))
				return path;

			return BaseHref.TrimEnd('/') + '/' + path.TrimStart('/');
		}

		/// <summary>
		/// Maps the supplied relative <paramref name="path"/> to the actual, physical location of the file.
		/// </summary>
		/// <param name="path">The path to map</param>
		/// <returns>
		/// The actual, physical location of the file.
		/// </returns>
		/// <remarks>
		/// If the specified path is an absolute path, no translation will be done.
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="path"/> is <c>null</c> or empty.
		/// </exception>
		public string MapPath(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			if (path.ContainsAnyOf(":", "\\\\"))
				return path;

			return this.pathMapper.Invoke(path);
		}

		/// <inheritdoc/>
		public void Parse(XmlElement element)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Recursively processes all nodes in the specified <paramref name="node"/>, applying any registered text and node
		/// handlers on the way, and returns the result.
		/// </summary>
		/// <param name="node">The node to process.</param>
		/// <returns>The processed version of the node.</returns>
		public XmlNode ProcessNode(XmlNode node)
		{
			return this.nodeEvaluator.Process(node);
		}

		/// <summary>
		/// Replaces any function or variable expression with the result of invoking it's corresponding 
		/// handler function.
		/// </summary>
		/// <param name="text">The text to process.</param>
		/// <returns>The processed version of the text.</returns>
		public string ProcessText(string text)
		{
			return this.textEvaluator.Process(text);
		}

		/// <summary>
		/// Gets an <see cref="XmlElement"/> that contains information about this context.
		/// </summary>
		/// <param name="ownerDocument">The <see cref="XmlDocument"/> to use to create the element.</param>
		/// <returns>An <see cref="XmlElement"/> that contains information about the current request</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="ownerDocument"/> is <c>null</c>.
		/// </exception>
		public XmlElement ToXml(XmlDocument ownerDocument)
		{
			if (ownerDocument == null)
				throw new ArgumentNullException("ownerDocument");

			XmlElement resultElement = ownerDocument.CreateElement("sage:request", XmlNamespaces.SageNamespace);
			HttpRequestBase request = this.Request;
			Uri requestUri = new Uri(this.Url.RawUrl);

			resultElement.SetAttribute("method", request.HttpMethod);
			resultElement.SetAttribute("basehref", this.BaseHref);
			resultElement.SetAttribute("localAddress", request.ServerVariables["LOCAL_ADDR"]);
			resultElement.SetAttribute("remoteAddress", request.ServerVariables["REMOTE_ADDR"]);

			resultElement.SetAttribute("category", this.Category);
			resultElement.SetAttribute("locale", this.Locale);
			resultElement.SetAttribute("language", this.LocaleInfo.Language);
			resultElement.SetAttribute("thread", System.Threading.Thread.CurrentThread.Name);
			resultElement.SetAttribute("developer", this.IsDeveloperRequest ? "1" : "0");
			resultElement.SetAttribute("debug", ProjectConfiguration.IsDebugEnabled ? "1" : "0");

			XmlElement addressNode = resultElement.AppendElement("sage:address", XmlNamespaces.SageNamespace);
			addressNode.SetAttribute("basehref", this.BaseHref);
			addressNode.SetAttribute("url", this.Url.RawUrl);
			if (request.UrlReferrer != null)
				addressNode.SetAttribute("referrer", request.UrlReferrer.ToString());

			addressNode.SetAttribute("serverName", requestUri.Host);
			addressNode.SetAttribute("serverNameFull", string.Format("{0}://{1}", requestUri.Scheme, requestUri.Authority));
			addressNode.SetAttribute("scriptName", requestUri.LocalPath.TrimEnd('/'));
			addressNode.SetAttribute("scriptNameFull", requestUri.PathAndQuery);
			addressNode.SetAttribute("queryString", requestUri.Query);

			XmlElement pathNode = resultElement.AppendElement("sage:path", XmlNamespaces.SageNamespace);
			pathNode.SetAttribute("applicationPath", this.ApplicationPath);
			pathNode.SetAttribute("assetPath", this.Path.GetRelativeWebPath(this.Path.AssetPath));
			pathNode.SetAttribute("sharedAssetPath", this.Path.GetRelativeWebPath(this.Path.SharedAssetPath));
			pathNode.SetAttribute("modulePath", this.Path.GetRelativeWebPath(this.Path.ModulePath));

			// browser element
			XmlElement browserNode = resultElement.AppendElement("sage:useragent", XmlNamespaces.SageNamespace);
			if (request.Browser != null)
			{
				browserNode.SetAttribute("id", this.UserAgentID);
				browserNode.SetAttribute("name", request.Browser.Browser.ToLower());
				browserNode.SetAttribute("version", request.Browser.Version);
				browserNode.SetAttribute("version.major", request.Browser.MajorVersion.ToString(CultureInfo.InvariantCulture));
				browserNode.SetAttribute("version.minor", request.Browser.MinorVersion.ToString(CultureInfo.InvariantCulture));
				browserNode.SetAttribute("isCrawler", this.UserAgentType == UserAgentType.Crawler ? "1" : "0");
			}

			browserNode.SetAttribute("value", request.UserAgent);

			XmlElement assemblyNode = resultElement.AppendElement("sage:assembly", XmlNamespaces.SageNamespace);
			var version = Assembly.GetExecutingAssembly().GetName().Version;
			assemblyNode.SetAttribute("version", version.ToString());

			resultElement.AppendChild(new QueryString(requestUri.Query).ToXml(ownerDocument, "sage:querystring", XmlNamespaces.SageNamespace));
			resultElement.AppendChild(new QueryString(request.Cookies).ToXml(ownerDocument, "sage:cookies", XmlNamespaces.SageNamespace));
			if (request.HttpMethod == "POST")
				resultElement.AppendChild(new QueryString(request.Form).ToXml(ownerDocument, "sage:form", XmlNamespaces.SageNamespace));
			
			if (this.Session != null)
			{
				XmlElement sessionElem = resultElement.AppendElement("sage:session", XmlNamespaces.SageNamespace);
				foreach (string key in this.Session.Keys)
				{
					var sessionObject = this.Session[key];

					XmlElement itemElem = sessionElem.AppendElement("sage:item", XmlNamespaces.SageNamespace);
					itemElem.SetAttribute("name", QueryString.ValidName(key));

					if (sessionObject != null)
					{
						itemElem.SetAttribute("type", sessionObject.GetType().FullName);
						if (sessionObject is IXmlConvertible)
							sessionElem.AppendChild(((IXmlConvertible) sessionObject).ToXml(ownerDocument));
						else
							sessionElem.InnerText = sessionObject.ToString();
					}
				}
			}

			XmlElement dateNode = resultElement.AppendElement("sage:dateTime", XmlNamespaces.SageNamespace);
			dateNode.SetAttribute("date", DateTime.Now.ToString("dd-MM-yyyy"));
			dateNode.SetAttribute("time", DateTime.Now.ToString("HH:mm:ss"));

			dateNode.SetAttribute("day", DateTime.Now.ToString("dd"));
			dateNode.SetAttribute("month", DateTime.Now.ToString("MM"));
			dateNode.SetAttribute("year", DateTime.Now.ToString("yyyy"));

			dateNode.SetAttribute("hour", DateTime.Now.ToString("HH"));
			dateNode.SetAttribute("minute", DateTime.Now.ToString("mm"));
			dateNode.SetAttribute("second", DateTime.Now.ToString("ss"));

			return resultElement;
		}

		/// <summary>
		/// Selects the case that matches the condition specified with the <paramref name="switchNode"/> and returns it's contents.
		/// </summary>
		/// <param name="context">The current context with which the method is being invoked.</param>
		/// <param name="switchNode">The node that specifies the condition</param>
		/// <returns>An <see cref="XmlDocumentFragment"/> populated with the content of the selected case node,
		/// or the original <paramref name="switchNode"/> is the condition has been improperly specified.</returns>
		/// <example>
		/// &lt;context:switch property="QueryString" key="Test"&gt;
		///   &lt;context:case test="ABC"&gt;...%lt;/context:case&gt;
		///   &lt;context:case test="DEF"&gt;...%lt;/context:case&gt;
		///   &lt;context:default&gt;...%lt;/context:default&gt;
		/// &lt;/context:switch&gt;
		/// </example>
		[NodeHandler(XmlNodeType.Element, "switch", XmlNamespaces.ContextualizationNamespace)]
		internal static XmlNode ProcessContextSwitchNode(SageContext context, XmlNode switchNode)
		{
			if (switchNode.SelectSingleElement("ancestor::sage:literal", XmlNamespaces.Manager) != null)
				return switchNode;

			XmlElement switchElem = (XmlElement) switchNode;

			string propName = switchElem.GetAttribute("property");
			string key = switchElem.GetAttribute("key");

			if (string.IsNullOrEmpty(propName))
			{
				log.ErrorFormat("The switch node in document '{0}' is missing the required property attribute",
					switchNode.OwnerDocument.BaseURI);

				return switchNode;
			}

			string propValue = GetContextProperty(context, propName, key);

			int caseNum = 0;
			bool caseFound = false;
			XmlDocumentFragment result = switchNode.OwnerDocument.CreateDocumentFragment();
			
			foreach (XmlElement caseNode in switchNode.SelectNodes("context:case", XmlNamespaces.Manager))
			{
				string testValue = caseNode.GetAttribute("test");
				if (string.IsNullOrEmpty(testValue))
				{
					log.WarnFormat(
						"The case node with index {0} of switch node '{1}' in document '{2}' didn't specify a test condition.",
							caseNum, propName, switchNode.OwnerDocument.BaseURI);

					continue;
				}

				if (testValue == propValue)
				{
					foreach (XmlNode node in caseNode.SelectNodes("node()"))
						result.AppendChild(context.ProcessNode(node));

					caseFound = true;
					break;
				}

				caseNum += 1;
			}

			if (!caseFound)
			{
				XmlNode defaultNode = switchNode.SelectSingleNode("context:default", XmlNamespaces.Manager);
				if (defaultNode != null)
				{
					foreach (XmlNode node in defaultNode.SelectNodes("node()"))
						result.AppendChild(context.ProcessNode(node));
				}
			}

			return result;
		}

		/// <summary>
		/// Inserts a <paramref name="context"/> property value specified with <paramref name="node"/>.
		/// </summary>
		/// <param name="context">The current context with which the method is being invoked.</param>
		/// <param name="node">The element that represents the property</param>
		/// <returns>The <paramref name="context"/> property value specified with <paramref name="node"/></returns>
		/// <example>
		/// &lt;context:value property="QueryString" key="page"/&gt;
		/// </example>
		[NodeHandler(XmlNodeType.Element, "value", XmlNamespaces.ContextualizationNamespace)]
		internal static XmlNode ProcessContextValueNode(SageContext context, XmlNode node)
		{
			if (node.SelectSingleElement("ancestor::sage:literal", XmlNamespaces.Manager) != null)
				return node;

			XmlElement valueElem = (XmlElement) node;

			string propName = valueElem.GetAttribute("property");
			string propKey = valueElem.GetAttribute("key");
			string propValue = SageContext.GetContextProperty(context, propName, propKey);

			if (propValue != null)
				return node.OwnerDocument.CreateTextNode(propValue);

			return node;
		}

		[NodeHandler(XmlNodeType.Element, "version", XmlNamespaces.SageNamespace)]
		internal static XmlNode ProcessSageVersionNode(SageContext context, XmlNode node)
		{
			if (node.SelectSingleElement("ancestor::sage:literal", XmlNamespaces.Manager) != null)
				return node;

			string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			return node.OwnerDocument.CreateTextNode(version);
		}

		internal static XmlNode ProcessBaseHrefNode(SageContext context, XmlNode node)
		{
			if (node.SelectSingleElement("ancestor::sage:literal", XmlNamespaces.Manager) != null)
				return node;

			XmlElement result = node.OwnerDocument.CreateElement("base", XmlNamespaces.XHtmlNamespace);
			result.SetAttribute("href", context.BaseHref);
			return result;
		}

		/// <summary>
		/// Inserts a <paramref name="context"/> property value specified with <paramref name="variable"/>. 
		/// </summary>
		/// <param name="context">The current context with which the method is being invoked.</param>
		/// <param name="variable">The name of the property to emit; supported values are <c>apppath</c>, <c>assetpath</c>, 
		/// <c>sharedassetpath</c>, <c>modulepath</c>, <c>locale</c>, <c>category</c>, <c>basehref</c>.</param>
		/// <returns>The <paramref name="context"/> property value specified with <paramref name="variable"/></returns>
		/// <example>
		/// <c>{basehref}</c>
		/// </example>
		[TextVariable("apppath", "assetpath", "sharedassetpath", "modulepath", "locale", "category", "basehref")]
		internal static string ResolvePathVariable(SageContext context, string variable)
		{
			switch (variable.ToLower())
			{
				case "apppath":
					return context.ApplicationPath;

				case "locale":
					return context.Locale;

				case "basehref":
					return context.BaseHref;

				case "category":
					return context.Category;

				case "assetpath":
					return context.Path.GetRelativeWebPath(context.Path.AssetPath);

				case "sharedassetpath":
					return context.Path.GetRelativeWebPath(context.Path.SharedAssetPath);

				case "modulepath":
					return context.Path.GetRelativeWebPath(context.Path.ModulePath);
			}

			return variable;
		}

		/// <summary>
		/// Returns a copy of this instance.
		/// </summary>
		/// <returns>A <see cref="SageContext"/> that is a copy of this instance.</returns>
		internal SageContext Copy()
		{
			return this.Copy(this.Category, this.Locale);
		}

		/// <summary>
		/// Returns a copy of this instance, setting its category to the specified <paramref name="category"/>.
		/// </summary>
		/// <param name="category">The category to set on the resulting instance.</param>
		/// <returns>
		/// A <see cref="SageContext"/> copy of this instance, with its category set to the
		/// specified <paramref name="category"/>.
		/// </returns>
		internal SageContext Copy(string category)
		{
			return this.Copy(category, this.Locale);
		}

		/// <summary>
		/// Returns a copy of this instance, setting its category and locale to the specified values.
		/// </summary>
		/// <param name="category">The category to set on the resulting instance.</param>
		/// <param name="locale">The locale to set on the resulting instance.</param>
		/// <returns>A <see cref="SageContext"/> copy of this instance, with its category and locale set to the
		/// specified <paramref name="category"/> and <paramref name="locale"/>.</returns>
		internal SageContext Copy(string category, string locale)
		{
			SageContext result = new SageContext(this);
			result.Category = category;
			result.Locale = locale;

			return result;
		}

		private bool SubstituteExtensionPath()
		{
			try
			{
				string extensionDirectory = this.Path.GetRelativeWebPath(this.Path.ExtensionPath, true);
				if (File.Exists(this.Request.PhysicalPath) || this.Request.Path.Contains(extensionDirectory, true))
					return false;

				string requestedFile = this.Request.Path.ToLower().Replace(this.Request.ApplicationPath.ToLower(), string.Empty).Trim('/');
				foreach (string extensionId in Project.Extensions.Keys)
				{
					ExtensionInfo info = Project.Extensions[extensionId];
					string rewrittenPath = string.Format("{0}/{1}/{2}", extensionDirectory, info.Name, requestedFile);
					if (File.Exists(this.MapPath(rewrittenPath)))
					{
						this.HttpContext.RewritePath(rewrittenPath);
						if (this.IsDeveloperRequest)
							this.Response.AddHeader("OriginalFilePath", requestedFile);

						return true;
					}
				}
			}
			catch (Exception ex)
			{
				log.ErrorFormat("Failed to rewrite path: {0}", ex.Message);
			}

			return false;
		}

		private static string GetContextProperty(SageContext context, string propName, string propKey)
		{
			const BindingFlags BindingFlags = 
				BindingFlags.IgnoreCase |
				BindingFlags.Public |
				BindingFlags.Instance |
				BindingFlags.Static;

			PropertyInfo property = context.GetType().GetProperty(propName, BindingFlags);
			if (property == null)
			{
				log.ErrorFormat(
					"Property name '{0}' is invalid. Please make sure the name matches a property of SageContext",
					propName);

				return null;
			}

			object value = property.GetValue(context, null);
			if (value != null)
			{
				if (!string.IsNullOrEmpty(propKey))
				{
					if (!(value is NameValueCollection))
					{
						log.ErrorFormat(
							"The GetContextProperty '{0}' used a key attribute ('{1}'), but SageContext.{0} is not a collection.",
							propName, propKey);

						return null;
					}

					return ((NameValueCollection) value)[propKey];
				}

				return value.ToString();
			}

			return null;
		}
	}
}
