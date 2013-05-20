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

	using Kelp;
	using Kelp.Extensions;

	using log4net;

	using XmlNamespaces = Sage.XmlNamespaces;

	/// <summary>
	/// Provides configuration information about Sage resource libraries.
	/// </summary>
	public class ResourceLibraryInfo : IXmlConvertible
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ResourceLibraryInfo).FullName);
		private List<Regex> includePaths;

		/// <summary>
		/// Initializes a new instance of the <see cref="ResourceLibraryInfo" /> class.
		/// </summary>
		/// <param name="configElem">The configuration element that represents this resource library.</param>
		/// <param name="projectId">The identification string of the project this library belongs to.</param>
		public ResourceLibraryInfo(XmlElement configElem, string projectId)
		{
			Contract.Requires<ArgumentNullException>(configElem != null);

			this.ProjectId = projectId;
			this.Parse(configElem);
		}

		/// <summary>
		/// Gets the name of this resource library.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this library should be included on all views.
		/// </summary>
		public bool IncludeAlways { get; private set; }

		/// <summary>
		/// Gets the identification string of the project this library belongs to.
		/// </summary>
		public string ProjectId { get; private set; }

		/// <summary>
		/// Gets the resource that this library consists of.
		/// </summary>
		public List<Resource> Resources { get; private set; }

		/// <summary>
		/// Gets the list of names of other libraries that this library depends on.
		/// </summary>
		public List<string> LibraryDependencies { get; private set; }

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("{0}{1}", this.Name, this.IncludeAlways ? " (global)" : string.Empty);
		}

		/// <inheritdoc/>
		public void Parse(XmlElement element)
		{
			var nm = XmlNamespaces.Manager;

			this.Name = element.GetAttribute("name");
			this.Resources = new List<Resource>();
			this.LibraryDependencies = new List<string>();
			this.includePaths = new List<Regex>();

			foreach (XmlElement resourceElem in element.SelectNodes("p:resources/p:resource", nm))
				this.Resources.Add(new Resource(resourceElem, this.ProjectId));

			foreach (XmlElement dependencyElement in element.SelectNodes("p:dependencies/p:library", nm))
				this.LibraryDependencies.Add(dependencyElement.GetAttribute("ref"));

			foreach (XmlElement childElem in element.SelectNodes("p:include/*", nm))
			{
				if (childElem.LocalName == "always")
					this.IncludeAlways = true;

				if (childElem.LocalName == "path")
				{
					string matchText = childElem.InnerText.Trim();
					try
					{
						Regex matchExpr = new Regex(matchText);
						this.includePaths.Add(matchExpr);
					}
					catch (Exception ex)
					{
						log.ErrorFormat("Error using '{0}' as library include path regex: {1}", matchText, ex.Message);
						throw;
					}
				}
			}
		}

		/// <inheritdoc/>
		public XmlElement ToXml(XmlDocument document)
		{
			const string Ns = XmlNamespaces.ProjectConfigurationNamespace;
			XmlElement result = document.CreateElement("library", Ns);

			result.SetAttribute("name", this.Name);

			XmlNode dependenciesNode = result.AppendChild(document.CreateElement("dependencies", Ns));
			foreach (string name in this.LibraryDependencies)
				dependenciesNode.AppendElement(document.CreateElement("library", Ns)).SetAttribute("ref", name);

			XmlNode resourcesNode = result.AppendChild(document.CreateElement("resources", Ns));
			foreach (Resource resource in this.Resources)
				resourcesNode.AppendElement(resource.ToXml(document));

			if (this.IncludeAlways || this.includePaths.Count != 0)
			{
				XmlNode includeNode = result.AppendChild(document.CreateElement("include", Ns));
				if (this.IncludeAlways)
					includeNode.AppendChild(document.CreateElement("always", Ns));

				foreach (Regex expression in this.includePaths)
				{
					includeNode.AppendChild(document.CreateElement("path", Ns)).InnerText = expression.ToString();
				}
			}

			return result;
		}

		internal bool MatchesPath(string url)
		{
			if (this.IncludeAlways)
				return true;

			foreach (Regex test in this.includePaths)
			{
				if (test.IsMatch(url))
					return true;
			}

			return false;
		}
	}
}
