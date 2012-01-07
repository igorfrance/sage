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

		public override void WriteRaw(string data)
		{
			if (!string.IsNullOrEmpty(data))
				base.WriteRaw(data);
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

