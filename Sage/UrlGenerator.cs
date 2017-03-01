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
	using System.Linq;
	using System.Runtime.CompilerServices;
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Xml;

	using Kelp;
	using Kelp.Extensions;

	using log4net;
	using Sage.Configuration;
	using Sage.Extensibility;

	/// <summary>
	/// Implements a class that handles all link generation.
	/// </summary>
	public class UrlGenerator
	{
		private static readonly char[] parameterSeparators = new[] { '&', ';' };
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
				return context;
			}
		}

		/// <summary>
		/// Gets the root physical URL of the application.
		/// </summary>
		public string ApplicationRoot
		{
			get
			{
				return this.ServerPrefix + this.Context.ApplicationPath;
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
					"{0}://{1}{2}", context.Request.Url.Scheme, context.Request.Url.Host, context.Request.Url.Port != 80 ? ":" + context.Request.Url.Port : string.Empty);
			}
		}

		/// <summary>
		/// Gets the full url of the current request (taking any rewriting into account).
		/// </summary>
		public string RawUrl
		{
			get
			{
				return string.Concat(this.ServerPrefix, this.RawPathAndQuery);
			}
		}

		/// <summary>
		/// Gets the path-and-query part of current URL (taking any rewriting into account).
		/// </summary>
		public string RawPathAndQuery
		{
			get
			{
				return context.Request.RawUrl;
			}
		}

		/// <summary>
		/// Gets the dictionary of links.
		/// </summary>
		internal ReadOnlyDictionary<string, ExtensionString> Links
		{
			get { return this.Context.ProjectConfiguration.Linking.Links; }
		}

		/// <summary>
		/// Gets the dictionary of format strings.
		/// </summary>
		internal ReadOnlyDictionary<string, ExtensionString> Formats
		{
			get { return this.Context.ProjectConfiguration.Linking.Formats; }
		}

		/// <summary>
		/// Generates the specified link, using the name/value pairs from the specified <paramref name="query" /> to format it.
		/// </summary>
		/// <param name="linkName">The name of the link to generate.</param>
		/// <param name="query">The query to use for formatting the link.</param>
		/// <param name="hashString">The browser hash string to append to the end of the URL.</param>
		/// <param name="qualify">If set to <c>true</c> the resulting URL will be prefixed with <see cref="ServerPrefix"/>.</param>
		/// <returns>The URL with the specified name, with any placeholders replaced with values from the <paramref name="query" />.</returns>
		public string GetUrl(string linkName, string query, string hashString = null, bool qualify = false)
		{
			return this.GetUrl(linkName, new QueryString(query, parameterSeparators), hashString, qualify);
		}

		/// <summary>
		/// Generates the specified link, using the name/value pairs from the specified <paramref name="query"/> to format it.
		/// </summary>
		/// <param name="linkName">The name of the link to get.</param>
		/// <param name="query">The query to use for formatting the link.</param>
		/// <param name="hashString">The browser hash string to append to the end of the URL.</param>
		/// <param name="qualify">If set to <c>true</c> the resulting URL will be prefixed with <see cref="ServerPrefix"/>.</param>
		/// <returns>The URL with the specified name, with any placeholders replaced with values from the <paramref name="query"/>.</returns>
		public string GetUrl(string linkName, NameValueCollection query = null, string hashString = null, bool qualify = false)
		{
			return this.FormatAndRewriteUrl(linkName, query, hashString, qualify);
		}

		/// <summary>
		/// Resolves a link <c>href</c> based on the attributes of the specified <paramref name="linkElement"/>.
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
			string linkName = this.Context.ProcessText(linkElement.GetAttribute("ref"));
			string linkValues = this.Context.ProcessText(linkElement.GetAttribute("values"));
			string linkHash = this.Context.ProcessText(linkElement.GetAttribute("hash"));
			bool urlEncode = linkElement.GetAttribute("encode").ContainsAnyOf("yes", "true", "1");
			bool qualify = linkElement.GetAttribute("absolute").ContainsAnyOf("yes", "true", "1");

			string linkHref = null;

			if (!string.IsNullOrEmpty(linkName))
			{
				linkHref = this.GetUrl(linkName, linkValues, linkHash, qualify);
				if (!string.IsNullOrWhiteSpace(linkHash))
				{
					int hashIndex = linkHref.IndexOf("#", System.StringComparison.Ordinal);
					if (hashIndex != -1)
						linkHref = linkHref.Substring(0, hashIndex);

					linkHref = string.Concat(linkHref, "#", linkHash.Trim('#'));
				}
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

		[NodeHandler(XmlNodeType.Element, "basehref", XmlNamespaces.SageNamespace)]
		internal static XmlNode ProcessSageBaseHrefElement(SageContext context, XmlNode node)
		{
			Contract.Requires<ArgumentNullException>(node != null);
			if (node.SelectSingleElement("ancestor::sage:literal", XmlNamespaces.Manager) != null)
				return node;

			XmlElement result = node.OwnerDocument.CreateElement("base", XmlNamespaces.XHtmlNamespace);
			result.SetAttribute("href", context.BaseHref);

			return result;
		}

		[NodeHandler(XmlNodeType.Element, "link", XmlNamespaces.SageNamespace)]
		internal static XmlNode ProcessSageLinkElement(SageContext context, XmlNode node)
		{
			Contract.Requires<ArgumentNullException>(node != null);
			if (node.SelectSingleElement("ancestor::sage:literal", XmlNamespaces.Manager) != null)
				return node;

			XmlElement linkElem = (XmlElement) node;
			string linkName = context.ProcessText(linkElem.GetAttribute("ref"));
			bool rawString = linkElem.GetAttribute("raw").EqualsAnyOf("1", "yes", "true");

			if (!string.IsNullOrEmpty(linkName) && rawString)
			{
				if (context.Url.Links.ContainsKey(linkName))
				{
					linkElem.InnerText = context.Url.Links[linkName].Value;
				}

				return linkElem;
			}

			string linkHref = context.Url.GetUrl(linkElem);
			if (!string.IsNullOrEmpty(linkHref))
			{
				linkElem.SetAttribute("href", linkHref);
			}

			foreach (XmlNode child in node.ChildNodes)
			{
				XmlNode processed = NodeEvaluator.GetNodeHandler(child)(context, child);
				if (processed != null)
					node.ReplaceChild(processed, child);
				else
					node.RemoveChild(child);
			}

			return linkElem;
		}

		[NodeHandler(XmlNodeType.Element, "url", XmlNamespaces.SageNamespace)]
		internal static XmlNode ProcessSageUrlElement(SageContext context, XmlNode node)
		{
			Contract.Requires<ArgumentNullException>(node != null);
			if (node.SelectSingleElement("ancestor::sage:literal", XmlNamespaces.Manager) != null)
				return node;

			string linkHref = context.Url.GetUrl((XmlElement) node);

			if (!string.IsNullOrEmpty(linkHref))
			{
				if (node.NodeType == XmlNodeType.Element)
					return node.OwnerDocument.CreateTextNode(linkHref);
			}

			return node;
		}

		[NodeHandler(XmlNodeType.Attribute, "href", "")]
		internal static XmlNode ProcessHrefAttribute(SageContext context, XmlNode node)
		{
			Contract.Requires<ArgumentNullException>(node != null);
			if (node.SelectSingleElement("ancestor::sage:literal", XmlNamespaces.Manager) != null)
				return node;

			node.InnerText = context.ProcessText(node.InnerText);
			return node;
		}

		[ContextFunction(Name = "url:project")]
		[ContextFunction(Name = "url:application")]
		internal static string GetProjectLinkFunction(SageContext context, params string[] arguments)
		{
			var linkArguments = new LinkArguments(arguments, false, "encode", "absolute", "pretty");
			var result = linkArguments.Switches["absolute"]
				? context.Url.ApplicationRoot
				: context.Request.ApplicationPath;

			if (linkArguments.Switches["encode"] && !string.IsNullOrEmpty(result))
				result = HttpUtility.UrlEncode(result);

			return result + (result.EndsWith("/") ? string.Empty : "/");
		}

		[ContextFunction(Name = "url:link")]
		internal static string GetLinkFunction(SageContext context, params string[] arguments)
		{
			var linkArguments = new LinkArguments(arguments, true, "encode", "absolute");
			var qualify = linkArguments.Switches["absolute"];

			var result = context.Url.GetUrl(linkArguments.LinkName, linkArguments.QueryString, linkArguments.HashString, qualify);
			if (linkArguments.Switches["encode"] && !string.IsNullOrEmpty(result))
				result = HttpUtility.UrlEncode(result);

			return result;
		}

		[ContextFunction(Name = "url:self")]
		internal static string GetSelfFunction(SageContext context, params string[] arguments)
		{
			var linkArguments = new LinkArguments(arguments, false, "encode", "absolute");
			var currentUrl = linkArguments.Switches["absolute"] 
				? context.Url.RawUrl
				: context.Url.RawPathAndQuery;

			var paramQuery = new QueryString(parameterSeparators);
			if (currentUrl.Contains("?"))
			{
				var questionIndex = currentUrl.IndexOf("?", StringComparison.Ordinal);
				paramQuery.Parse(currentUrl.Substring(questionIndex + 1));
				currentUrl = currentUrl.Substring(0, questionIndex);
			}

			paramQuery.Merge(linkArguments.QueryString);
			var result = string.Concat(currentUrl, paramQuery.ToString("?"), string.IsNullOrEmpty(linkArguments.HashString)
				? string.Empty
				: string.Concat("#", linkArguments.HashString));

			if (linkArguments.Switches["encode"] && !string.IsNullOrEmpty(result))
				result = HttpUtility.UrlEncode(result);

			return result;
		}

		/// <summary>
		/// Formats and rewrites the link pattern with the specified <paramref name="linkName" />.
		/// </summary>
		/// <param name="linkName">Name of the link.</param>
		/// <param name="parameters">The parameters.</param>
		/// <param name="hashString">The hash string.</param>
		/// <param name="qualify">If set to <c>true</c> the resulting URL will be prefixed with <see cref="ServerPrefix"/>.</param>
		/// <returns>The full rewritten Url, ready to use</returns>
		/// <exception cref="ConfigurationError">Current configuration doesn't contain a link with name specified with
		/// <paramref name="linkName" />.</exception>
		internal string FormatAndRewriteUrl(string linkName, NameValueCollection parameters, string hashString = null, bool qualify = false)
		{
			if (!this.Links.ContainsKey(linkName))
			{
				log.ErrorFormat("The url configuration doesn't contain a url with name '{0}'", linkName);
				return string.Format("javascript:alert('Unresolved link: {0}')", linkName);
			}

			string linkPattern = this.Links[linkName].Value;

			QueryString formatValues = new QueryString { { "locale", context.Locale }, { "category", context.Category } };
			if (parameters != null)
				formatValues.Merge(parameters);

			string resultUrl = this.FormatPattern(linkPattern, formatValues);

			formatValues.Remove("locale");
			formatValues.Remove("category");

			string queryString = formatValues.ToString();
			if (queryString != string.Empty)
			{
				string join = resultUrl.Contains("?") ? "&" : "?";
				resultUrl = string.Concat(resultUrl, join, queryString);
			}

			if (!string.IsNullOrWhiteSpace(hashString))
			{
				string join = resultUrl.Contains("#") ? string.Empty : "#";
				resultUrl = string.Concat(resultUrl, join, hashString);
			}

			if (qualify)
				resultUrl = string.Concat(this.ServerPrefix, resultUrl);

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
					return this.FormatPattern(this.Formats[key].Value, parameters, visited.ToArray());
				}

				if (parameters[key] != null)
				{
					string value = parameters[key];
					parameters.Remove(key);
					return value;
				}

				return string.Empty;
			});
		}

		private class LinkArguments
		{
			//// TODO: Add option to include the current querystring
			//// TODO: Figure out a neat way to provide names arguments
			public LinkArguments(IEnumerable<string> arguments, bool useLinkName, params string[] switches)
				: this()
			{
				foreach (string sw in switches)
					this.Switches.Add(sw, false);

				List<string> parameters = arguments.ToArray().ToList();
				if (useLinkName)
				{
					this.LinkName = parameters.ElementAt(0);
					parameters.RemoveAt(0);
				}

				foreach (string parameter in parameters)
				{
					var temp = new QueryString(parameter);
					if (temp.Count == 1 && switches.Contains(temp.Keys.Get(0)))
					{
						string name = temp.Keys.Get(0);
						this.Switches[name] = temp[name].ToLower().ContainsAnyOf("yes", "true", "1");
					}

					if (this.QueryString.Count != 0 && this.HashString == null)
					{
						this.HashString = parameter.Trim('#');
						break;
					}

					if (parameter.IndexOf("#") == 0)
						this.HashString = parameter.Trim('#');
					else
						this.QueryString.Merge(new QueryString(parameter, parameterSeparators));
				}
			}

			private LinkArguments()
			{
				this.QueryString = new QueryString();
				this.Switches = new Dictionary<string, bool>();
			}

			public string LinkName { get; private set; }

			public QueryString QueryString { get; private set; }

			public string HashString { get; private set; }

			public IDictionary<string, bool> Switches { get; private set; }
		}
	}
}
