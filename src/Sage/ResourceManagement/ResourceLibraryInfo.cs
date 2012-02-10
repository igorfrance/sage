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
namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Text.RegularExpressions;
	using System.Xml;

	public class ResourceLibraryInfo
	{
		public ResourceLibraryInfo(XmlElement configElem)
		{
			Contract.Requires<ArgumentNullException>(configElem != null);

			var nm = XmlNamespaces.Manager;

			this.Name = configElem.GetAttribute("name");
			this.Resources = new List<Resource>();
			this.IncludeUrls = new List<Regex>();

			foreach (XmlElement resourceElem in configElem.SelectNodes("p:resources/p:resource", nm))
				this.Resources.Add(new Resource(resourceElem));

			foreach (XmlElement childElem in configElem.SelectNodes("p:includes/*", nm))
			{
				if (childElem.LocalName == "always")
					this.IncludeAlways = true;

				if (childElem.LocalName == "url")
					this.IncludeUrls.Add(new Regex(childElem.InnerText));
			}
		}

		public string Name { get; private set; }

		public bool IncludeAlways { get; private set; }

		public List<Resource> Resources { get; private set; }

		public List<Regex> IncludeUrls { get; private set; }

		internal bool MatchesPath(string url)
		{
			if (this.IncludeAlways)
				return true;

			foreach (Regex regex in IncludeUrls)
			{
				if (regex.Match(url).Success)
					return true;
			}

			return false;
		}
	}
}
