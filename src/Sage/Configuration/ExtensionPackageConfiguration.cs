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
	using System.Xml;

	internal class ExtensionPackageConfiguration
	{
		private static readonly XmlNamespaceManager Nm = XmlNamespaces.Manager;

		public ExtensionPackageConfiguration(XmlNode configurationElement)
		{
			if (configurationElement != null)
			{
				this.Assets = new PackageGroup(configurationElement.SelectSingleNode("p:assets", Nm));
				this.Binaries = new PackageGroup(configurationElement.SelectSingleNode("p:binaries", Nm));
				this.Modules = new PackageGroup(configurationElement.SelectSingleNode("p:modules", Nm));
				this.Routes = new PackageGroup(configurationElement.SelectSingleNode("p:routes", Nm));
				this.Libraries = new PackageGroup(configurationElement.SelectSingleNode("p:libraries", Nm));
				this.Links = new PackageGroup(configurationElement.SelectSingleNode("p:libraries", Nm));
				this.MetaViews = new PackageGroup(configurationElement.SelectSingleNode("p:libraries", Nm));
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
