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
namespace Sage.Views
{
	using System;
	using System.IO;
	using System.Xml;

	/// <summary>
	/// Ensures that all HTML tags that need to have the matching closing tag are not collapsed.
	/// </summary>
	public class XHtmlXmlWriter : XmlWrappingWriter
	{
		/// <summary>
		/// Array of elements that don't require a closing tag
		/// </summary>
		private readonly string[] collapsibleElements = new[] { "base", "meta", "link", "br", "input", "area", "frame", "param", "img" };
		private string lastStartElement;

		/// <summary>
		/// Initializes a new instance of the <see cref="XHtmlXmlWriter"/> class.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		public XHtmlXmlWriter(XmlWriter writer)
			: base(writer)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XHtmlXmlWriter"/> class.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		public XHtmlXmlWriter(Stream stream)
			: base(XmlWriter.Create(stream))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XHtmlXmlWriter"/> class.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		/// <param name="settings">The <c>XmlWriterSettings</c> to use.</param>
		public XHtmlXmlWriter(Stream stream, XmlWriterSettings settings)
			: base(XmlWriter.Create(stream, settings))
		{
		}

		/// <summary>
		/// Writes the specified start tag and associates it with the given namespace and prefix.
		/// </summary>
		/// <param name="prefix">The namespace prefix of the element.</param>
		/// <param name="localName">The local name of the element.</param>
		/// <param name="ns">The namespace URI to associate with the element. If this namespace is
		/// already in scope and has an associated prefix then the writer automatically writes
		/// that prefix also.</param>
		/// <exception cref="InvalidOperationException">The writer is closed. </exception>
		public override void WriteStartElement(string prefix, string localName, string ns)
		{
			lastStartElement = localName;
			base.WriteStartElement(prefix, localName, ns);
		}

		/// <summary>
		/// Closes one element and pops the corresponding namespace scope.
		/// </summary>
		public override void WriteEndElement()
		{
			if (Array.IndexOf(collapsibleElements, lastStartElement) > -1)
				base.WriteEndElement();
			else
				WriteFullEndElement();
		}
	}
}

