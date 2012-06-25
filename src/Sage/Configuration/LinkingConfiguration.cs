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
namespace Sage.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Web.Hosting;
	using System.Xml;

	using Kelp;

	using XmlNamespaces = Sage.XmlNamespaces;

	/// <summary>
	/// Provides access to linking configuration.
	/// </summary>
	public class LinkingConfiguration
	{
		private readonly Dictionary<string, string> links = new Dictionary<string, string>();
		private readonly Dictionary<string, string> formats = new Dictionary<string, string>();

		/// <summary>
		/// Initializes a new instance of the <see cref="LinkingConfiguration"/> class.
		/// </summary>
		public LinkingConfiguration()
		{
			this.Links = new ReadOnlyDictionary<string, string>(this.links);
			this.Formats = new ReadOnlyDictionary<string, string>(this.formats);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LinkingConfiguration"/> class, using the 
		/// specified <paramref name="configElement"/>.
		/// </summary>
		/// <param name="configElement">The config element.</param>
		public LinkingConfiguration(XmlElement configElement)
			: this()
		{
			Contract.Requires<ArgumentNullException>(configElement != null);

			XmlNamespaceManager nm = XmlNamespaces.Manager;
			foreach (XmlElement linkElem in configElement.SelectNodes("p:links/p:link", nm))
			{
				string linkName = linkElem.GetAttribute("name");
				string linkUrl = linkElem.GetAttribute("url");

				this.AddLink(linkName, linkUrl);
			}

			foreach (XmlElement linkElem in configElement.SelectNodes("p:formats/p:format", nm))
			{
				string formatName = linkElem.GetAttribute("name");
				string formatPattern = linkElem.GetAttribute("pattern");

				this.AddFormat(formatName, formatPattern);
			}
		}

		/// <summary>
		/// Gets the dictionary of links.
		/// </summary>
		public ReadOnlyDictionary<string, string> Links
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the dictionary of formmat strings.
		/// </summary>
		public ReadOnlyDictionary<string, string> Formats
		{
			get;
			private set;
		}

		/// <summary>
		/// Adds the link with the specified name and URL (pattern).
		/// </summary>
		/// <param name="linkName">The name of the link.</param>
		/// <param name="linkUrl">The URL (pattern) of the link.</param>
		public void AddLink(string linkName, string linkUrl)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(linkName));
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(linkUrl));

			if (linkUrl.StartsWith("~/"))
				linkUrl = linkUrl.Replace("~/", HostingEnvironment.ApplicationVirtualPath.TrimEnd('/') + "/");

			if (!linkUrl.StartsWith("/") && !linkUrl.Contains("://"))
				linkUrl = string.Concat(HostingEnvironment.ApplicationVirtualPath.TrimEnd('/'), "/", linkUrl);

			this.links[linkName] = linkUrl;
		}

		/// <summary>
		/// Adds the specified key and value to the dictionary.
		/// </summary>
		/// <param name="formatName">The name of the format pattern.</param>
		/// <param name="formatValue">The format pattern.</param>
		public void AddFormat(string formatName, string formatValue)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(formatName));
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(formatValue));

			this.formats[formatName] = formatValue;
		}
	}
}
