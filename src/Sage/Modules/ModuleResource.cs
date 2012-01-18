namespace Sage.Modules
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Xml;

	public class ModuleResource
	{
		public ModuleResource(XmlElement resourceNode, string moduleName)
		{
			Contract.Requires<ArgumentNullException>(resourceNode != null);
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(moduleName));

			this.ModuleName = moduleName;
			this.Path = resourceNode.GetAttribute("path");
			this.Type = (ResourceType) Enum.Parse(typeof(ResourceType), resourceNode.LocalName, true);
			this.Location = (ResourceLocation) Enum.Parse(typeof(ResourceLocation), resourceNode.GetAttribute("location"), true);
		}

		public string ModuleName { get; private set; }

		public string Path { get; private set; }

		public ResourceType Type { get; private set; }

		public ResourceLocation Location { get; private set; }

		public XmlElement ToXml(XmlDocument ownerDoc, SageContext context)
		{
			Contract.Requires<ArgumentNullException>(ownerDoc != null);
			Contract.Requires<ArgumentNullException>(context != null);

			XmlElement result;
			string resourcePath = context.Path.GetModulePath(this.ModuleName, this.Path);
			if (this.Type == ResourceType.Style)
			{
				result = ownerDoc.CreateElement("xhtml:link", XmlNamespaces.XHtmlNamespace);
				result.SetAttribute("type", "text/css");
				result.SetAttribute("rel", "stylesheet");
				result.SetAttribute("href", context.Path.GetRelativeWebPath(resourcePath, true));
			}
			else
			{
				result = ownerDoc.CreateElement("xhtml:script", XmlNamespaces.XHtmlNamespace);
				result.SetAttribute("type", "text/javascript");
				result.SetAttribute("language", "javascript");
				result.SetAttribute("src", context.Path.GetRelativeWebPath(resourcePath, true));
			}
			
			return result;
		}
	}
}
