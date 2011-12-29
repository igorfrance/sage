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

			string resourcePath = context.Path.GetModulePath(this.ModuleName, this.Path);

			XmlElement result = ownerDoc.CreateElement("sage:resource", XmlNamespaces.SageNamespace);
			result.SetAttribute("type", this.Type.ToString().ToLower());
			result.SetAttribute("location", this.Location.ToString().ToLower());
			result.SetAttribute("path", context.Path.GetRelativeWebPath(resourcePath));

			return result;
		}
	}
}
