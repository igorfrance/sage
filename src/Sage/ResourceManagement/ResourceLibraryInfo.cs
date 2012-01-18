namespace Sage.ResourceManagement
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Xml;

	using Kelp.Core.Extensions;

	public class ResourceLibraryInfo
	{
		public ResourceLibraryInfo(XmlElement configElem)
		{
			Contract.Requires<ArgumentNullException>(configElem != null);

			this.Name = configElem.GetAttribute("name");
			this.Version = configElem.GetAttribute("version");
			this.Path = configElem.GetAttribute("path");
			this.IsGlobal = configElem.GetAttribute("global").EqualsAnyOf("yes", "true", "1");
			this.Type = (ResourceType) Enum.Parse(typeof(ResourceType), configElem.GetAttribute("type"), true);
		}

		public bool IsGlobal { get; private set; }

		public string Name { get; private set; }

		public ResourceType Type { get; private set; }

		public string Version { get; private set; }

		public string Path { get; private set; }

		public XmlElement ToXml(XmlDocument ownerDocument, SageContext context)
		{
			Contract.Requires<ArgumentNullException>(ownerDocument != null);
			Contract.Requires<ArgumentNullException>(context != null);

			XmlElement result;
			string resourcePath = context.Path.Resolve(this.Path);
			if (this.Type == ResourceType.Style)
			{
				result = ownerDocument.CreateElement("xhtml:link", XmlNamespaces.XHtmlNamespace);
				result.SetAttribute("type", "text/css");
				result.SetAttribute("rel", "stylesheet");
				result.SetAttribute("href", context.Path.GetRelativeWebPath(resourcePath, true));
			}
			else
			{
				result = ownerDocument.CreateElement("xhtml:script", XmlNamespaces.XHtmlNamespace);
				result.SetAttribute("type", "text/javascript");
				result.SetAttribute("language", "javascript");
				result.SetAttribute("src", context.Path.GetRelativeWebPath(resourcePath, true));
			}

			return result;
		}
	}
}
