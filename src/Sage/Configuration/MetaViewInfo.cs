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
	using System.Xml;

	using Sage.Views;

	/// <summary>
	/// Contains information about a meta view.
	/// </summary>
	public class MetaViewInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MetaViewInfo"/> class.
		/// </summary>
		/// <param name="infoElement">The info element.</param>
		public MetaViewInfo(XmlElement infoElement)
		{
			if (infoElement == null)
				throw new ArgumentNullException("infoElement");

			this.Name = infoElement.GetAttribute("name");
			this.Description = infoElement.GetAttribute("description");
			this.ViewPath = infoElement.GetAttribute("path");

			if (!string.IsNullOrWhiteSpace(infoElement.GetAttribute("contentType")))
				this.ContentType = infoElement.GetAttribute("contentType");
			else
				this.ContentType = "text/html";

			this.TypeName = infoElement.GetAttribute("type");
		}

		/// <summary>
		/// Gets the name of the meta view.
		/// </summary>
		public string Name { get; private set; }

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
		/// Gets the XSLT transform associated with the meta view this object represents.
		/// </summary>
		public XsltTransform Processor { get; private set; }

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