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
	using Kelp.Extensions;

	using XmlNamespaces = Sage.XmlNamespaces;

	/// <summary>
	/// Provides access to linking configuration.
	/// </summary>
	public class LinkingConfiguration : IXmlConvertible
	{
		//// TODO: Remove the HostingEnvironment.ApplicationVirtualPath property
		//// TODO: Add option to fully qualify URL's or not
		private readonly Dictionary<string, ExtensionString> links;
		private readonly Dictionary<string, ExtensionString> formats;
		private string applicationPath = HostingEnvironment.ApplicationVirtualPath ?? "/";

		/// <summary>
		/// Initializes a new instance of the <see cref="LinkingConfiguration"/> class.
		/// </summary>
		public LinkingConfiguration()
		{
			this.links = new Dictionary<string, ExtensionString>();
			this.formats = new Dictionary<string, ExtensionString>();
			this.Links = new ReadOnlyDictionary<string, ExtensionString>(this.links);
			this.Formats = new ReadOnlyDictionary<string, ExtensionString>(this.formats);
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

			this.Parse(configElement);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LinkingConfiguration"/> class, using the specified 
		/// instance to initialize the content of this instance.
		/// </summary>
		/// <param name="init">The object to copy the contents from.</param>
		internal LinkingConfiguration(LinkingConfiguration init)
		{
			this.links = new Dictionary<string, ExtensionString>(init.links);
			this.formats = new Dictionary<string, ExtensionString>(init.formats);
			this.Links = new ReadOnlyDictionary<string, ExtensionString>(this.links);
			this.Formats = new ReadOnlyDictionary<string, ExtensionString>(this.formats);
		}

		/// <summary>
		/// Gets the dictionary of links.
		/// </summary>
		public ReadOnlyDictionary<string, ExtensionString> Links
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the dictionary of format strings.
		/// </summary>
		public ReadOnlyDictionary<string, ExtensionString> Formats
		{
			get;
			private set;
		}

		/// <summary>
		/// Merges the links and formats from the specified <paramref name="configElement"/> into the
		/// current instance of <see cref="LinkingConfiguration"/>.
		/// </summary>
		/// <param name="configElement">The XML element that contains the data to parse.</param>
		public void Parse(XmlElement configElement)
		{
			XmlNamespaceManager nm = XmlNamespaces.Manager;
			foreach (XmlElement elem in configElement.SelectNodes("p:links/p:link", nm))
			{
				string linkName = elem.GetAttribute("name");
				string linkUrl = elem.InnerText.Trim();

				this.AddLink(linkName, linkUrl);
			}

			foreach (XmlElement elem in configElement.SelectNodes("p:formats/p:format", nm))
			{
				string formatName = elem.GetAttribute("name");
				string formatPattern = elem.InnerText.Trim();

				this.AddFormat(formatName, formatPattern);
			}
		}

		/// <inheritdoc/>
		public XmlElement ToXml(XmlDocument document)
		{
			const string Ns = XmlNamespaces.ProjectConfigurationNamespace;
			XmlElement result = document.CreateElement("linking", Ns);

			if (this.formats.Count != 0)
			{
				XmlElement linksElement = result.AppendElement(document.CreateElement("formats", Ns));
				foreach (KeyValuePair<string, ExtensionString> format in this.formats)
				{
					XmlElement element = linksElement.AppendElement(document.CreateElement("format", Ns));
					element.SetAttribute("name", format.Key);
					if (!string.IsNullOrEmpty(format.Value.Extension))
						element.SetAttribute("extension", format.Value.Extension);

					element.InnerText = format.Value.Value;
				}
			}

			if (this.links.Count != 0)
			{
				XmlElement linksElement = result.AppendElement(document.CreateElement("links", Ns));
				foreach (KeyValuePair<string, ExtensionString> link in this.links)
				{
					XmlElement element = linksElement.AppendElement(document.CreateElement("link", Ns));
					element.SetAttribute("name", link.Key);
					if (!string.IsNullOrEmpty(link.Value.Extension))
						element.SetAttribute("extension", link.Value.Extension);

					element.InnerText = link.Value.Value;
				}
			}

			return result;
		}

		/// <summary>
		/// Adds the link with the specified name and URL (pattern).
		/// </summary>
		/// <param name="linkName">The name of the link.</param>
		/// <param name="linkUrl">The URL (pattern) of the link.</param>
		internal ExtensionString AddLink(string linkName, string linkUrl)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(linkName));
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(linkUrl));

			if (linkUrl.StartsWith("~/"))
				linkUrl = linkUrl.Replace("~/", applicationPath.TrimEnd('/') + "/");

			if (!linkUrl.StartsWith("/") && !linkUrl.Contains("://"))
				linkUrl = string.Concat(applicationPath.TrimEnd('/'), "/", linkUrl);

			ExtensionString result = new ExtensionString(linkUrl);
			this.links[linkName] = result;
			return result;
		}

		/// <summary>
		/// Adds the specified key and value to the dictionary.
		/// </summary>
		/// <param name="formatName">The name of the format pattern.</param>
		/// <param name="formatValue">The format pattern.</param>
		internal ExtensionString AddFormat(string formatName, string formatValue)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(formatName));
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(formatValue));

			ExtensionString result = new ExtensionString(formatValue);
			this.formats[formatName] = result;
			return result;
		}

		internal void SetApplicationPath(string applicationPath)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(applicationPath));

			this.applicationPath = applicationPath;
		}
	}
}
