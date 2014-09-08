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
	using System;
	using System.Diagnostics.Contracts;
	using System.Xml;

	using Kelp;

	using Sage.Views;

	/// <summary>
	/// Contains information about a meta view.
	/// </summary>
	public class ErrorViewInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ErrorViewInfo"/> class.
		/// </summary>
		/// <param name="element">The info element.</param>
		public ErrorViewInfo(XmlElement element)
		{
			Contract.Requires<ArgumentNullException>(element != null);

			this.Error = element.GetAttribute("error");

			var defaultContent = element.SelectSingleNode("p:default", Sage.XmlNamespaces.Manager);
			var developerContent = element.SelectSingleNode("p:developer", Sage.XmlNamespaces.Manager);

			if (defaultContent != null)
				this.DefaultContent = defaultContent.InnerText;
			if (developerContent != null)
				this.DeveloperContent = developerContent.InnerText;
		}

		/// <summary>
		/// Gets the error number associated with this view
		/// </summary>
		public string Error { get; private set; }

		/// <summary>
		/// Optional name of extension that defines this meta view.
		/// </summary>
		public string Extension { get; internal set; }

		/// <summary>
		/// Gets the view content to show by default (for standard users).
		/// </summary>
		public string DefaultContent { get; private set; }

		/// <summary>
		/// Gets the view content to show to developers.
		/// </summary>
		public string DeveloperContent { get; private set; }

		/// <inheritdoc/>
		public XmlElement ToXml(XmlDocument document)
		{
			XmlElement result = document.CreateElement("view", Sage.XmlNamespaces.ProjectConfigurationNamespace);
			result.SetAttribute("error", this.Error);

			result
				.AppendChild(document.CreateElement("developer", Sage.XmlNamespaces.ProjectConfigurationNamespace))
				.InnerText = this.DeveloperContent;

			result
				.AppendChild(document.CreateElement("default", Sage.XmlNamespaces.ProjectConfigurationNamespace))
				.InnerText = this.DefaultContent;

			return result;
		}
	}
}