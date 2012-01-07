namespace Sage.ResourceManagement
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Xml;

	public class ScriptLibraryInfo
	{
		public ScriptLibraryInfo(XmlElement configElem)
		{
			Contract.Requires<ArgumentNullException>(configElem != null);

			this.Name = configElem.GetAttribute("name");
			this.Version = configElem.GetAttribute("version");
			this.Path = configElem.GetAttribute("path");
		}

		public string Name
		{
			get;
			private set;
		}

		public string Version
		{
			get;
			private set;
		}

		public string Path
		{
			get;
			private set;
		}

		public XmlElement ToXml(XmlDocument ownerDocument, SageContext context)
		{
			Contract.Requires<ArgumentNullException>(ownerDocument != null);
			Contract.Requires<ArgumentNullException>(context != null);

			string resourcePath = context.Path.Resolve(this.Path);

			XmlElement result = ownerDocument.CreateElement("sage:resource", XmlNamespaces.SageNamespace);
			result.SetAttribute("type", "script");
			result.SetAttribute("location", "head");
			result.SetAttribute("path", context.Path.GetRelativeWebPath(resourcePath, true));

			return result;
		}
	}
}
