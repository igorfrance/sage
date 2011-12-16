namespace Sage.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.Xml;

	/// <summary>
	/// Contains the link configuration information.
	/// </summary>
	public class LinkConfiguration
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LinkConfiguration"/> class.
		/// </summary>
		public LinkConfiguration()
		{
			this.Internal = new Dictionary<string, LinkInfo>();
			this.External = new Dictionary<string, string>();
		}

		/// <summary>
		/// Gets the dictionary of internal link urls.
		/// </summary>
		public Dictionary<string, LinkInfo> Internal
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the dictionary of external link urls.
		/// </summary>
		public Dictionary<string, string> External
		{
			get;
			private set;
		}

		/// <summary>
		/// Parses the links from the specified <paramref name="configurationElement"/>.
		/// </summary>
		/// <param name="configurationElement">The configuration element that contains the link data.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="configurationElement"/> is <see langword="null"/>.
		/// </exception>
		public void ParseConfiguration(XmlElement configurationElement)
		{
			if (configurationElement == null)
				throw new ArgumentNullException("configurationElement");

			XmlNamespaceManager nm = XmlNamespaces.Manager;
			foreach (XmlElement elem in configurationElement.SelectNodes("p:internal/p:link", nm))
			{
				var name = elem.GetAttribute("name");
				var link = new LinkInfo(elem);
				if (this.Internal.ContainsKey(name))
					this.Internal[name] = link;
				else
					this.Internal.Add(name, link);
			}

			foreach (XmlElement elem in configurationElement.SelectNodes("p:external/p:link", nm))
			{
				var name = elem.GetAttribute("name");
				var url = elem.GetAttribute("url");
				if (this.External.ContainsKey(name))
					this.External[name] = url;
				else
					this.External.Add(name, url);
			}
		}
	}
}
