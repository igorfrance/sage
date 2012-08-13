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
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Xml;

	using Kelp;
	using Kelp.Extensions;

	using log4net;
	using Sage.Configuration;
	using Sage.Extensibility;
	using Sage.ResourceManagement;

	/// <summary>
	/// Implements a class that handles all link generation.
	/// </summary>
	public class UrlGenerator
	{
		private static readonly Regex linkPlaceholder = new Regex(@"(?!>^|[^{])\{([^{}]+)\}(?!\})");

		private static readonly ILog log = LogManager.GetLogger(typeof(UrlGenerator).FullName);
		private readonly SageContext context;

		/// <summary>
		/// Initializes a new instance of the <see cref="UrlGenerator"/> class, using the specified <paramref name="context"/>.
		/// </summary>
		/// <param name="context">The current <see cref="SageContext"/>.</param>
		public UrlGenerator(SageContext context)
		{
			this.context = context;
		}

		/// <summary>
		/// Gets the context.
		/// </summary>
		public SageContext Context
		{
			get
			{
				return this.context;
			}
		}

		/// <summary>
		/// Gets the server prefix part of the url.
		/// </summary>
		/// <value>
		/// This will be the <c>https://chew.local:888/</c> part from the <c>https://chew.local:888/gums-r-us/uk/products/chewymango</c>.
		/// </value>
		public string ServerPrefix
		{
			get
			{
				return string.Format(
					"{0}://{1}{2}",
					context.Request.Url.Scheme,
					context.Request.Url.Host,
					context.Request.Url.Port != 80 ? ":" + context.Request.Url.Port : string.Empty);
			}
		}

		/// <summary>
		/// Gets the full url of the current request (taking any rewriting into account).
		/// </summary>
		/// <returns></returns>
		public string RawUrl
		{
			get
			{
				string pathAndQuery = context.Request.ServerVariables["HTTP_X_REWRITE_URL"] ?? context.Request.RawUrl;
				return string.Concat(ServerPrefix, pathAndQuery);
			}
		}

		/// <summary>
		/// Gets the dictionary of links.
		/// </summary>
		internal ReadOnlyDictionary<string, string> Links
		{
			get { return this.Context.ProjectConfiguration.Linking.Links; }
		}

		/// <summary>
		/// Gets the dictionary of formmat strings.
		/// </summary>
		internal ReadOnlyDictionary<string, string> Formats
		{
			get { return this.Context.ProjectConfiguration.Linking.Formats; }
		}

		/// <summary>
		/// Gets the link with the specified link name.
		/// </summary>
		/// <param name="linkName">The name of the link to get.</param>
		/// <returns>The URL with the specified name.</returns>
		public string GetUrl(string linkName)
		{
			return this.GetUrl(linkName, false);
		}

		/// <summary>
		/// Gets the link with the specified link name.
		/// </summary>
		/// <param name="linkName">The name of the link to get.</param>
		/// <param name="includeActiveQuery">If set to <c>true</c>, the resulting URL will include the query string from
		/// the active <see cref="SageContext"/>.</param>
		/// <returns>The URL with the specified name.</returns>
		public string GetUrl(string linkName, bool includeActiveQuery)
		{
			return this.FormatAndRewriteUrl(linkName, null, includeActiveQuery);
		}

		/// <summary>
		/// Generates the specified link, using the name/value pairs from the specified <paramref name="query"/> to format it.
		/// </summary>
		/// <param name="linkName">The name of the link to get.</param>
		/// <param name="query">The query to use for formatting the link.</param>
		/// <returns>The URL with the specified name, with any placeholders replaced with values from the <paramref name="query"/>.</returns>
		public string GetUrl(string linkName, NameValueCollection query)
		{
			return this.GetUrl(linkName, query, false);
		}

		/// <summary>
		/// Generates the specified link, using the name/value pairs from the specified <paramref name="query"/> to format it.
		/// </summary>
		/// <param name="linkName">The name of the link to get.</param>
		/// <param name="query">The query to use for formatting the link.</param>
		/// <param name="includeActiveQuery">If set to <c>true</c>, the resulting link will include the query string from
		/// the active <see cref="SageContext"/>.</param>
		/// <returns>The URL with the specified name, with any placeholders replaced with values from the <paramref name="query"/>.</returns>
		public string GetUrl(string linkName, NameValueCollection query, bool includeActiveQuery)
		{
			return this.FormatAndRewriteUrl(linkName, query, includeActiveQuery);
		}

		/// <summary>
		/// Generates the specified link, using the name/value pairs from the specified <paramref name="query"/> to format it.
		/// </summary>
		/// <param name="linkName">The name of the link to generate.</param>
		/// <param name="query">The query to use for formatting the link.</param>
		/// <returns>The URL with the specified name, with any placeholders replaced with values from the <paramref name="query"/>.</returns>
		public string GetUrl(string linkName, string query)
		{
			return this.GetUrl(linkName, query, false);
		}

		/// <summary>
		/// Generates the specified link, using the name/value pairs from the specified <paramref name="query"/> to format it.
		/// </summary>
		/// <param name="linkName">The name of the link to get.</param>
		/// <param name="query">The query to use for formatting the link.</param>
		/// <param name="includeActiveQuery">If set to <c>true</c>, the resulting link will include the query string from
		/// the active <see cref="SageContext"/>.</param>
		/// <returns>The URL with the specified name, with any placeholders replaced with values from the <paramref name="query"/>.</returns>
		public string GetUrl(string linkName, string query, bool includeActiveQuery)
		{
			return this.FormatAndRewriteUrl(linkName, new QueryString(query), includeActiveQuery);
		}

		/// <summary>
		/// Generates the specified link, using the specified <paramref name="name"/>/<paramref name="value"/> pair to format it.
		/// </summary>
		/// <param name="linkName">The name of the URL to generate.</param>
		/// <param name="name">The name of a single format parameter from the URL to generate.</param>
		/// <param name="value">The value of the parameter specified with <paramref name="name"/>.</param>
		/// <returns>The URL with the specified name, with placeholder named <paramref name="name"/> replaced with 
		/// <paramref name="value"/>.</returns>
		public string GetUrl(string linkName, string name, string value)
		{
			return this.GetUrl(linkName, name, value, false);
		}

		/// <summary>
		/// Generates the specified link, using the specified <paramref name="name"/>/<paramref name="value"/> pair to format it.
		/// </summary>
		/// <param name="linkName">The name of the link to get.</param>
		/// <param name="name">The name of a single format parameter from the link to generate.</param>
		/// <param name="value">The value of the parameter specified with <paramref name="name"/>.</param>
		/// <param name="includeActiveQuery">If set to <c>true</c>, the resulting link will include the query string from
		/// the active <see cref="SageContext"/>.</param>
		/// <returns>The URL with the specified name, with placeholder named <paramref name="name"/> replaced with 
		/// <paramref name="value"/>.</returns>
		public string GetUrl(string linkName, string name, string value, bool includeActiveQuery)
		{
			return this.FormatAndRewriteUrl(linkName, new QueryString { { name, value } }, includeActiveQuery);
		}

		/// <summary>
		/// Generates the specified link, using the specified 2 name/value pairs to format it.
		/// </summary>
		/// <param name="linkName">The name of the link to get.</param>
		/// <param name="name1">The name of a first format parameter from the link to generate.</param>
		/// <param name="value1">The value of the parameter specified with <paramref name="name1"/>.</param>
		/// <param name="name2">The name of a second format parameter from the link to generate.</param>
		/// <param name="value2">The value of the parameter specified with <paramref name="name2"/>.</param>
		/// <returns>The URL with the specified name, with 2 specified placeholders replaced with 2 specified values.</returns>
		public string GetUrl(string linkName, string name1, string value1, string name2, string value2)
		{
			return GetUrl(linkName, name1, value1, name2, value2, false);
		}

		/// <summary>
		/// Generates the specified link, using the specified 2 name/value pairs to format it.
		/// </summary>
		/// <param name="linkName">The name of the link to get.</param>
		/// <param name="name1">The name of a first format parameter from the link to generate.</param>
		/// <param name="value1">The value of the parameter specified with <paramref name="name1"/>.</param>
		/// <param name="name2">The name of a second format parameter from the link to generate.</param>
		/// <param name="value2">The value of the parameter specified with <paramref name="name2"/>.</param>
		/// <param name="includeActiveQuery">If set to <c>true</c>, the resulting link will include the query string from
		/// the active <see cref="SageContext"/>.</param>
		/// <returns>The URL with the specified name, with 2 specified placeholders replaced with 2 specified values.</returns>
		public string GetUrl(string linkName, string name1, string value1, string name2, string value2, bool includeActiveQuery)
		{
			return this.FormatAndRewriteUrl(linkName, new QueryString { { name1, value1 }, { name2, value2 } }, includeActiveQuery);
		}

		/// <summary>
		/// Resolves a link href based on the attributes of the specified <paramref name="linkElement"/>.
		/// </summary>
		/// <param name="linkElement">The element that contains attributes that define the link to resolve.</param>
		/// <returns>The URL from the configuration file, with values and encoding as specified by the element</returns>
		/// <remarks>
		/// <para>The attributes on the element that will be taken into account:</para>
		/// <list type="table">
		/// <listheader>
		/// <term>Attribute</term>
		/// <description>Description</description>
		/// </listheader>
		/// <item>
		/// <term><c>id</c></term>
		/// <description>
		/// The id of the URL of the link to resolve:
		/// <para><c>&lt;sage:link id="CustomerSuport" /&gt;</c></para>
		/// </description>
		/// </item>
		/// <item>
		/// <term><term><c>values</c></term></term>
		/// <description>
		/// To add formatting values to the resulting URL, such as the ID of the current customer, use the <c>values</c>
		/// attribute:
		/// <para><c>&lt;sage:link id="CustomerSuport" values="id=${CustomerID}"/&gt;</c></para>
		/// </description>
		/// </item>
		/// <item>
		/// <term><term><c>encode</c></term></term>
		/// <description>
		/// To URL-encode the resulting URL, use the <c>encode</c> attribute and set it's value to "1", "true", or "yes".
		/// </description>
		/// </item>
		/// </list>
		/// </remarks>
		public string GetUrl(XmlElement linkElement)
		{
			string linkName = linkElement.GetAttribute("ref");
			string linkValues = linkElement.GetAttribute("values");
			bool urlEncode = linkElement.GetAttribute("encode").ContainsAnyOf("yes", "true", "1");

			string linkHref = null;

			if (!string.IsNullOrEmpty(linkName))
			{
				linkHref = this.GetUrl(linkName, linkValues);
			}

			if (urlEncode && !string.IsNullOrEmpty(linkHref))
				linkHref = HttpUtility.UrlEncode(linkHref);

			return linkHref;
		}

		/// <summary>
		/// Returns the specified <paramref name="url"/> with the current <see cref="ServerPrefix"/> (if any) pre-pended to it.
		/// </summary>
		/// <param name="url">The URL to process.</param>
		/// <returns>The specified <paramref name="url"/> with the current <see cref="ServerPrefix"/> (if any) pre-pended to it.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="url"/> is <c>null</c>.
		/// </exception>
		public string PrefixUrl(string url)
		{
			if (string.IsNullOrEmpty(url))
				throw new ArgumentNullException("url");

			if (url.Contains("://"))
				return url;

			return string.Concat(this.ServerPrefix.TrimEnd('/'), "/", url.TrimStart('/'));
		}

		[NodeHandler(XmlNodeType.Element, "link", XmlNamespaces.SageNamespace)]
		internal static XmlNode ProcessSageLinkElement(XmlNode linkNode, SageContext context)
		{
			Contract.Requires<ArgumentNullException>(linkNode != null);
			if (linkNode.SelectSingleElement("ancestor::sage:literal", XmlNamespaces.Manager) != null)
				return linkNode;

			XmlElement linkElem = (XmlElement) linkNode;
			string linkHref = context.Url.GetUrl(linkElem);

			if (!string.IsNullOrEmpty(linkHref))
			{
				linkElem.SetAttribute("href", linkHref);
			}

			return linkElem;
		}

		[NodeHandler(XmlNodeType.Element, "url", XmlNamespaces.SageNamespace)]
		internal static XmlNode ProcessSageUrlElement(XmlNode linkNode, SageContext context)
		{
			Contract.Requires<ArgumentNullException>(linkNode != null);
			if (linkNode.SelectSingleElement("ancestor::sage:literal", XmlNamespaces.Manager) != null)
				return linkNode;

			string linkHref = context.Url.GetUrl((XmlElement) linkNode);

			if (!string.IsNullOrEmpty(linkHref))
			{
				if (linkNode.NodeType == XmlNodeType.Element)
					return linkNode.OwnerDocument.CreateTextNode(linkHref);
			}

			return linkNode;
		}

		[NodeHandler(XmlNodeType.Attribute, "href", "")]
		internal static XmlNode ProcessHrefAttribute(XmlNode attribNode, SageContext context)
		{
			Contract.Requires<ArgumentNullException>(attribNode != null);
			if (attribNode.SelectSingleElement("ancestor::sage:literal", XmlNamespaces.Manager) != null)
				return attribNode;

			string attribValue = context.ProcessFunctions(attribNode.InnerText);
			attribNode.InnerText = ResourceManager.ApplyTextHandlers(attribValue, context);
			return attribNode;
		}

		[TextFunction(Name = "url:link")]
		internal static string GetLinkFunction(string argumentString, SageContext context)
		{
			string[] parameters = argumentString.Split(',');

			string linkName = parameters[0].Trim();
			QueryString paramQuery = new QueryString();
			QueryString hashQuery = new QueryString();

			for (int i = 1; i < parameters.Length; i++)
			{
				string parameter = parameters[i].Trim();

				QueryString tempQuery;

				if (parameter.IndexOf("#") == 0)
				{
					tempQuery = new QueryString(parameter.Substring(1));
					hashQuery.Merge(tempQuery);
				}
				else 
				{
					tempQuery = new QueryString(parameter.Substring(1));
					paramQuery.Merge(tempQuery);
				}
			}

			string linkHref = context.Url.GetUrl(linkName, paramQuery);
			return string.Concat(linkHref, paramQuery.ToString("?"), hashQuery.ToString("#"));
		}

		[TextFunction(Name = "url:self")]
		internal static string GetSelfFunction(string argumentString, SageContext context)
		{
			string currentUrl = context.Url.RawUrl;
			QueryString paramQuery = new QueryString();
			QueryString hashQuery = new QueryString();

			if (currentUrl.Contains("?"))
			{
				paramQuery.Parse(currentUrl.Substring(currentUrl.IndexOf("?") + 1));
				currentUrl = currentUrl.Substring(0, currentUrl.IndexOf("?"));
			}

			string[] arguments = argumentString.Split(',');
			foreach (string t in arguments)
			{
				string argument = t.Trim();

				QueryString tempQuery;
				if (argument.IndexOf("?") == 0)
				{
					tempQuery = new QueryString(argument.Substring(1));
					paramQuery.Merge(tempQuery);
				}

				if (argument.IndexOf("#") == 0)
				{
					tempQuery = new QueryString(argument.Substring(1));
					hashQuery.Merge(tempQuery);
				}
			}

			return string.Concat(currentUrl, paramQuery.ToString("?"), hashQuery.ToString("#"));
		}

		/// <summary>
		/// Formats and rewrites the link pattern with the specified <paramref name="linkName"/>.
		/// </summary>
		/// <param name="linkName">Name of the link.</param>
		/// <param name="parameters">The parameters.</param>
		/// <param name="includeActiveQuery">if set to <c>true</c> active query parameters are also appended.</param>
		/// <returns>The full rewritten Url, ready to use</returns>
		/// <exception cref="ConfigurationError">Current configuration doesn't contain a link with name specified with
		/// <paramref name="linkName"/>.</exception>
		internal string FormatAndRewriteUrl(string linkName, NameValueCollection parameters, bool includeActiveQuery)
		{
			if (!this.Links.ContainsKey(linkName))
			{
				log.ErrorFormat("The url configuration doesn't contain a url with name '{0}'", linkName);
				return string.Format("javascript:alert('Unresolved link: {0}')", linkName);
			}

			string linkPattern = this.Links[linkName];

			QueryString query = new QueryString(context.Query);
			QueryString formatValues = new QueryString { { "locale", context.Locale }, { "category", context.Category } };
			if (parameters != null)
				formatValues.Merge(parameters);

			string resultUrl = this.FormatPattern(linkPattern, formatValues);

			formatValues.Remove("locale");
			formatValues.Remove("category");

			QueryString resultQuery = new QueryString(formatValues);
			if (includeActiveQuery)
				resultQuery.Merge(query);

			string queryString = resultQuery.ToString();
			if (queryString != string.Empty)
			{
				string join = resultUrl.Contains("?") ? "&" : "?";
				resultUrl = string.Concat(resultUrl, join, queryString);
			}

			if (resultUrl.StartsWith("#"))
				return string.Concat(ServerPrefix, resultUrl);

			return resultUrl;
		}

		private string FormatPattern(string pattern, NameValueCollection parameters, params string[] visitedFormats)
		{
			List<string> visited = new List<string>(visitedFormats);
			return linkPlaceholder.Replace(pattern, delegate(Match match)
			{
				string key = match.Groups[1].Value;
				if (this.Formats.ContainsKey(key))
				{
					if (visited.Contains(key))
					{
						log.ErrorFormat("Skipping processing link format '{0}' the second time because it would cause recursion.", key);
						return string.Empty;
					}

					visited.Add(key);
					return this.FormatPattern(this.Formats[key], parameters, visited.ToArray());
				}

				return parameters[key] ?? string.Empty;
			});
		}
	}
}
