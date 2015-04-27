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
	public class MetaViewInfo : IXmlConvertible
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="MetaViewInfo"/> class.
		/// </summary>
		/// <param name="infoElement">The info element.</param>
		public MetaViewInfo(XmlElement infoElement)
		{
			Contract.Requires<ArgumentNullException>(infoElement != null);

			this.Parse(infoElement);
		}

		/// <summary>
		/// Gets the name of the meta view.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Optional name of extension that defines this meta view.
		/// </summary>
		public string Extension { get; internal set; }

		/// <summary>
		/// Gets the description of the meta view.
		/// </summary>
		public string Description { get; private set; }

		/// <summary>
		/// Gets the path to the meta view file corresponding to this object.
		/// </summary>
		public string ViewPath { get; private set; }

		/// <summary>
		/// Gets the content type associated with the meta view this object represents.
		/// </summary>
		public string ContentType { get; private set; }

		/// <summary>
		/// Gets the name of the type that implements this meta view.
		/// </summary>
		public string TypeName { get; private set; }

		/// <summary>
		/// Gets the view info context.
		/// </summary>

		/// <summary>
		/// Gets the XSLT transform associated with the meta view this object represents.
		/// </summary>
		public XsltTransform Processor { get; private set; }


		/// <inheritdoc/>
		public void Parse(XmlElement element)
		{
			this.Name = element.GetAttribute("name");
			this.Description = element.GetAttribute("description");
			this.ViewPath = element.GetAttribute("path");

			if (!string.IsNullOrWhiteSpace(element.GetAttribute("contentType")))
				this.ContentType = element.GetAttribute("contentType");
			else
				this.ContentType = "text/html";

			this.TypeName = element.GetAttribute("type");
		}

		/// <inheritdoc/>
		public XmlElement ToXml(XmlDocument document)
		{
			XmlElement result = document.CreateElement("metaView", Sage.XmlNamespaces.ProjectConfigurationNamespace);
			result.SetAttribute("name", this.Name);
			if (!string.IsNullOrWhiteSpace(this.ViewPath))
				result.SetAttribute("path", this.ViewPath);

			result.SetAttribute("contentType", this.ContentType);
			result.SetAttribute("type", this.TypeName);

			if (!string.IsNullOrWhiteSpace(this.Extension))
				result.SetAttribute("extension", this.Extension);

			if (!string.IsNullOrWhiteSpace(this.Description))
				result.SetAttribute("description", this.Description);

			return result;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("{0} ({1}) ({2}) ({3})", this.Name, this.ContentType, this.ViewPath, this.TypeName);
		}
		internal MetaViewInfo Load(SageContext context)
		{
			if (!string.IsNullOrWhiteSpace(this.ViewPath))
				this.Processor = XsltTransform.Create(context, this.ViewPath);
			return this;
		}
	}
}