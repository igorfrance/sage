/**
 * Open Source Initiative OSI - The MIT License (MIT):Licensing
 * [OSI Approved License]
 * The MIT License (MIT)
 *
 * Copyright (c) 2011 Igor France
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */
namespace Sage.Configuration
{
	using System.Xml;

	public class ExtensionPackageConfiguration
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
