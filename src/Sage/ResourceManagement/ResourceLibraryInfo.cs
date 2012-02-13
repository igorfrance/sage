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
			this.IncludePaths = new Dictionary<string, PathComparer>();

			foreach (XmlElement resourceElem in configElem.SelectNodes("p:resources/p:resource", nm))
				this.Resources.Add(new Resource(resourceElem));

			foreach (XmlElement childElem in configElem.SelectNodes("p:include/*", nm))
			{
				if (childElem.LocalName == "always")
					this.IncludeAlways = true;

				if (childElem.LocalName == "path")
				{
					string path = childElem.InnerText.Trim();
					string compare = childElem.GetAttribute("compare");

					CompareType compareType;
					PathComparer comparer = this.UrlContains;
					Enum.TryParse(compare, out compareType);

					if (compareType == CompareType.Wildcard)
					{
						if (path.StartsWith("*") && path.EndsWith("*"))
							comparer = this.UrlContains;
						if (path.StartsWith("*"))
							comparer = this.UrlEndsWith;
						if (path.EndsWith("*"))
							comparer = this.UrlStartsWith;
					}
					else if (compareType == CompareType.Regexp)
					{
						comparer = this.UrlMatches;
					}
					else
					{
						comparer = this.UrlEquals;
					}

					this.IncludePaths.Add(path, comparer);
				}
			}
		}

		public delegate bool PathComparer(string url, string search);

		public string Name { get; private set; }

		public bool IncludeAlways { get; private set; }

		public List<Resource> Resources { get; private set; }

		public Dictionary<string, PathComparer> IncludePaths { get; private set; }

		public override string ToString()
		{
			return string.Format("{0}{1}", this.Name, this.IncludeAlways ? " (global)" : string.Empty);
		}

		internal bool MatchesPath(string url)
		{
			if (this.IncludeAlways)
				return true;

			foreach (string search in this.IncludePaths.Keys)
			{
				PathComparer pathValid = this.IncludePaths[search];
				if (pathValid(url, search))
					return true;
			}

			return false;
		}

		private bool UrlMatches(string url, string search)
		{
			return new Regex(search).Match(url).Success;
		}

		private bool UrlEquals(string url, string search)
		{
			return url == search;
		}

		private bool UrlContains(string url, string search)
		{
			return url.Contains(search);
		}

		private bool UrlStartsWith(string url, string search)
		{
			return url.StartsWith(search);
		}

		private bool UrlEndsWith(string url, string search)
		{
			return url.EndsWith(search);
		}
	}
}
