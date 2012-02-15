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
	using System;
	using System.Xml;

	using Sage.Views;

	/// <summary>
	/// Contains information about a meta view.
	/// </summary>
	public class MetaViewInfo
	{
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