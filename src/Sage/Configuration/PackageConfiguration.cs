namespace Sage.Configuration
{
	using System;
	using System.Xml;

	public class PackageConfiguration
	{
		private static readonly XmlNamespaceManager nm = XmlNamespaces.Manager;

		public PackageConfiguration(XmlNode configurationElement)
		{
			if (configurationElement != null)
			{
				this.Assets = new PackageGroup(configurationElement.SelectSingleNode("p:assets", nm));
				this.Binaries = new PackageGroup(configurationElement.SelectSingleNode("p:binaries", nm));
				this.Modules = new PackageGroup(configurationElement.SelectSingleNode("p:modules", nm));
				this.Routes = new PackageGroup(configurationElement.SelectSingleNode("p:routes", nm));
				this.Libraries = new PackageGroup(configurationElement.SelectSingleNode("p:libraries", nm));
				this.Links = new PackageGroup(configurationElement.SelectSingleNode("p:libraries", nm));
				this.MetaViews = new PackageGroup(configurationElement.SelectSingleNode("p:libraries", nm));
			}
			else
			{
				this.Assets = new PackageGroup(null);
				this.Binaries = new PackageGroup(null);
				this.Modules = new PackageGroup(null);
				this.Routes = new PackageGroup(null);
				this.Libraries = new PackageGroup(null);
				this.Links = new PackageGroup(null);
				this.MetaViews = new PackageGroup(null);
			}
		}

		public PackageGroup Assets { get; private set; }

		public PackageGroup Binaries { get; private set; }

		public PackageGroup Modules { get; private set; }

		public PackageGroup Routes { get; private set; }

		public PackageGroup Libraries { get; private set; }

		public PackageGroup Links { get; private set; }

		public PackageGroup MetaViews { get; private set; }
	}
}
