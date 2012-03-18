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
namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Text.RegularExpressions;
	using System.Xml;

	/// <summary>
	/// Provides configuration information about Sage resource libraries.
	/// </summary>
	public class ResourceLibraryInfo
	{
		private Dictionary<string, PathComparer> includePaths = new Dictionary<string, PathComparer>();

		/// <summary>
		/// Initializes a new instance of the <see cref="ResourceLibraryInfo"/> class.
		/// </summary>
		/// <param name="configElem">The configuration element that represents this resource library.</param>
		public ResourceLibraryInfo(XmlElement configElem)
		{
			Contract.Requires<ArgumentNullException>(configElem != null);

			var nm = XmlNamespaces.Manager;

			this.Name = configElem.GetAttribute("name");
			this.Resources = new List<Resource>();
			this.Dependencies = new List<string>();
			this.includePaths = new Dictionary<string, PathComparer>();

			foreach (XmlElement resourceElem in configElem.SelectNodes("p:resources/p:resource", nm))
				this.Resources.Add(new Resource(resourceElem));

			foreach (XmlElement dependencyElement in configElem.SelectNodes("p:dependencies/p:library", nm))
				this.Dependencies.Add(dependencyElement.GetAttribute("ref"));

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

					this.includePaths.Add(path, comparer);
				}
			}
		}

		private delegate bool PathComparer(string url, string search);

		/// <summary>
		/// Gets the name of this resource library.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this library should be included on all views.
		/// </summary>
		public bool IncludeAlways { get; private set; }

		/// <summary>
		/// Gets the resource that this library consists of.
		/// </summary>
		public List<Resource> Resources { get; private set; }

		/// <summary>
		/// Gets the lis of names of other libraries that this library depends on.
		/// </summary>
		public List<string> Dependencies { get; private set; }

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("{0}{1}", this.Name, this.IncludeAlways ? " (global)" : string.Empty);
		}

		internal bool MatchesPath(string url)
		{
			if (this.IncludeAlways)
				return true;

			foreach (string search in this.includePaths.Keys)
			{
				PathComparer pathValid = this.includePaths[search];
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
