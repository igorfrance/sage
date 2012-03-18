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
	using System.Web.Hosting;
	using System.Xml;

	/// <summary>
	/// Represents a Link Pattern, for constructing internal and external Urls.
	/// </summary>
	public struct LinkInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LinkInfo"/> structure.
		/// </summary>
		/// <param name="configNode">The configuration XML element that represents this link.</param>
		public LinkInfo(XmlElement configNode)
			: this()
		{
			this.Name = configNode.GetAttribute("name");
			this.Url = configNode.GetAttribute("url");

			if (!this.Url.StartsWith("/") && !this.Url.Contains("://"))
				this.Url = string.Concat(HostingEnvironment.ApplicationVirtualPath.TrimEnd('/'), "/", this.Url);
		}

		/// <summary>
		/// Gets the name of this link.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the URL of this link (for non-dynamic links).
		/// </summary>
		public string Url { get; private set; }

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("{0} ({1})", this.Name, this.Url);
		}
	}
}
