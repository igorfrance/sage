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
		private static readonly Regex attribSpec = new Regex(@"^(?'AttribName'\w[\w\.:]*):(?'AttribValue'.*)$", RegexOptions.Compiled);
		private static readonly Regex urlFunction = new Regex(@"^url\((?'LinkName'[^,]+)(?:,(?'LinkValues'.*))?\)$", RegexOptions.Compiled);

		private static readonly ILog log = LogManager.GetLogger(typeof(UrlGenerator).FullName);
		private readonly SageContext context;

		private static readonly Regex linkPlaceholder = new Regex(@"(?!>^|[^{])\{([^{}]+)\}(?!\})");

		/// <summary>
		/// Initializes a new instance of the <see cref="UrlGenerator"/> class, using the specified <paramref name="context"/>.
		/// </summary>
		/// <param name="context">The current <see cref="SageContext"/>.</param>
		public UrlGenerator(SageContext context)
		{
			this.context = context;
			this.RewritingOn = !string.IsNullOrEmpty(context.ProjectConfiguration.UrlRewritePrefix);
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
		/// Gets a value indicating whether URL rewriting is on.
		/// </summary>
		/// <value><c>true</c> if URL rewriting is on; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// If no RewritingOn hasn't been set prior to getting the value, The setting is taken from the Project.Config
		/// </remarks>
		public bool RewritingOn { get; private set; }

		/// <summary>
		/// Gets the current rewrite prefix of the url, if any.
		/// </summary>
		/// <value>
		/// The rewrite prefix pattern is configured in the sage project configuration file 
		/// (see <see cref="ProjectConfiguration.UrlRewritePrefix"/>). This property returns that configuration value, with 
		/// the placeholders '{category}' and '{locale}' substituted with the values from the current <see cref="SageContext"/>.
		/// <para>
		/// Typically, this will be the <c>gums-r-us/uk</c> part from the <c>http://www.chew.com/gums-r-us/uk/products/chewymango</c>.
		/// </para>
		/// </value>
		public string RewritePrefix
		{
			get
			{
				if (!this.RewritingOn)
					return context.ProjectConfiguration.UrlRewritePrefix.Replace(
						"{category}", context.Category).Replace("{locale}", context.Locale);

				return string.Empty;
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
		/// Gets the complete prefix of the url, combining <see cref="ServerPrefix"/> and <see cref="RewritePrefix"/>.
		/// </summary>
		/// <value>
		/// This will be the <c>http://www.chew.com/gums-r-us/uk/</c> part from the 
		/// <c>http://www.chew.com/gums-r-us/uk/products/chewymango</c>.
		/// </value>
		public string UrlPrefix
		{
			get
			{
				return string.Concat(ServerPrefix.TrimEnd('/'), "/", RewritePrefix.TrimStart('/'));
			}
		}

		/// <summary>
		/// Gets the full url of the current request (taking any rewriting into account).
		/// </summary>
		/// <returns></returns>
		public string VisibleUrl
		{
			get
			{
				string pathAndQuery = context.Request.ServerVariables["HTTP_X_REWRITE_URL"] ?? context.Request.Url.PathAndQuery;
				return string.Concat(ServerPrefix, pathAndQuery);
			}
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
		/// Returns the specified <paramref name="url"/> with the current <see cref="RewritePrefix"/> (if any) pre-pended to it.
		/// </summary>
		/// <param name="url">The URL to process.</param>
		/// <returns>The specified <paramref name="url"/> with the current <see cref="RewritePrefix"/> (if any) pre-pended to it.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="url"/> is <c>null</c>.
		/// </exception>
		public string PrefixUrl(string url)
		{
			if (string.IsNullOrEmpty(url))
				throw new ArgumentNullException("url");

			if (url.Contains("://"))
				return url;

			return string.Concat(RewritePrefix.TrimEnd('/'), "/", url.TrimStart('/'));
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

		[NodeHandler(XmlNodeType.Attribute, "attrib", XmlNamespaces.SageNamespace)]
		internal static XmlNode ProcessSageAttribute(XmlNode attribNode, SageContext context)
		{
			Contract.Requires<ArgumentNullException>(attribNode != null);
			if (attribNode.SelectSingleElement("ancestor::sage:literal", XmlNamespaces.Manager) != null)
				return attribNode;

			Match match;

			if ((match = attribSpec.Match(attribNode.InnerText)).Success)
			{
				string attribName = match.Groups["AttribName"].Value;
				string attribValue = match.Groups["AttribValue"].Value;

				XmlAttribute result = attribNode.OwnerDocument.CreateAttribute(attribName);
				result.InnerText = GetAttributeValue(attribValue, context);

				return result;
			}

			return attribNode;
		}

		[NodeHandler(XmlNodeType.Attribute, "href", "")]
		internal static XmlNode ProcessHrefAttribute(XmlNode attribNode, SageContext context)
		{
			Contract.Requires<ArgumentNullException>(attribNode != null);
			if (attribNode.SelectSingleElement("ancestor::sage:literal", XmlNamespaces.Manager) != null)
				return attribNode;

			string attribValue = GetAttributeValue(attribNode.InnerText, context);
			attribNode.InnerText = ResourceManager.ProcessString(attribValue, context);
			return attribNode;
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
			if (!context.ProjectConfiguration.Links.ContainsKey(linkName))
			{
				log.ErrorFormat("The url configuration doesn't contain a url with name '{0}'", linkName);
				return string.Format("javascript:alert('Unresolved link: {0}')", linkName);
			}

			LinkInfo link = context.ProjectConfiguration.Links[linkName];
			string linkPattern = link.Url;

			QueryString query = new QueryString(context.Query);
			QueryString formatValues = new QueryString { { "locale", context.Locale }, { "category", context.Category } };
			if (parameters != null)
			{
				formatValues.Merge(parameters);
			}

			string resultUrl = linkPlaceholder.Replace(
				linkPattern,
				delegate(Match m)
				{
					string key = m.Groups[1].Value;
					string value = formatValues[key];
					if (key == "rewriteprefix")
						value = RewritePrefix;

					query.Remove(key);
					formatValues.Remove(key);

					return value ?? string.Empty;
				});

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

		private static string GetAttributeValue(string attribValue, SageContext context)
		{
			Match match;
			string result = attribValue;
			if ((match = urlFunction.Match(attribValue)).Success)
			{
				string[] parameters = match.Groups[1].Value.Split(',');

				string linkId = parameters[0].Trim();
				string linkValues = parameters.Length > 1 ? parameters[1].Trim() : null;
				bool encode = parameters.Length > 2 && parameters[2].Trim().EqualsAnyOf("true", "yes", "1");

				string linkHref = context.Url.GetUrl(linkId, linkValues);
				if (!string.IsNullOrEmpty(linkHref))
					return encode ? HttpUtility.UrlEncode(linkHref) : linkHref;
			}

			return result;
		}
	}
}
