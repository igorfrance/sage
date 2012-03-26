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
namespace Kelp.Extensions
{
	using System;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Xml;

	using Kelp.XInclude;

	/// <summary>
	/// Provides extensions for <see cref="System.Xml.XmlDocument"/>.
	/// </summary>
	public static class XmlDocumentExtensions
	{
		private static readonly XmlReaderSettings settings = new XmlReaderSettings 
		{ 
			IgnoreComments = true, 
			CloseInput = true, 
			DtdProcessing = DtdProcessing.Ignore 
		};

		/// <summary>
		/// Loads the XML document from the specified URL, with support for <c>XIncludes</c>.
		/// </summary>
		/// <param name="document">The document being extended.</param>
		/// <param name="filename">URL for the file containing the XML document to load. The URL can be either a local file or an HTTP URL (a Web address).</param>
		public static void LoadX(this XmlDocument document, string filename)
		{
			Contract.Requires<ArgumentNullException>(document != null);
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(filename));

			LoadX(document, filename, null);
		}

		/// <summary>
		/// Loads the XML document from the specified URL, with support for <c>XIncludes</c>.
		/// </summary>
		/// <param name="document">The document being extended.</param>
		/// <param name="filename">URL for the file containing the XML document to load. The URL can be either a local file or an HTTP URL (a Web address).</param>
		/// <param name="resolver">The resolver to use to resolve external references.</param>
		public static void LoadX(this XmlDocument document, string filename, XmlUrlResolver resolver)
		{
			Contract.Requires<ArgumentNullException>(document != null);
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(filename));

			if (resolver != null)
			{
				Uri uri = new Uri(filename, UriKind.RelativeOrAbsolute);
				object reader = resolver.GetEntity(uri, null, null);
				if (reader != null)
				{
					if (reader is Stream)
					{
						LoadX(document, (Stream) reader, resolver);
						return;
					}

					if (reader is TextReader)
					{
						LoadX(document, (TextReader) reader, resolver);
						return;
					}

					if (reader is XmlReader)
					{
						LoadX(document, (XmlReader) reader, resolver);
						return;
					}
				}
			}

			LoadX(document, XmlReader.Create(filename, settings), resolver);
		}

		/// <summary>
		/// Loads the XML document from the specified <see cref="TextReader"/>, with support for <c>XIncludes</c>.
		/// </summary>
		/// <param name="document">The document being extended.</param>
		/// <param name="reader">The TextReader used to feed the XML data into the document.</param>
		public static void LoadX(this XmlDocument document, TextReader reader)
		{
			Contract.Requires<ArgumentNullException>(document != null);
			Contract.Requires<ArgumentNullException>(reader != null);

			LoadX(document, reader, null);
		}

		/// <summary>
		/// Loads the XML document from the specified <see cref="TextReader"/>, with support for <c>XIncludes</c>.
		/// </summary>
		/// <param name="document">The document being extended.</param>
		/// <param name="reader">The TextReader used to feed the XML data into the document.</param>
		/// <param name="resolver">The resolver to use to resolve external references.</param>
		public static void LoadX(this XmlDocument document, TextReader reader, XmlUrlResolver resolver)
		{
			Contract.Requires<ArgumentNullException>(document != null);
			Contract.Requires<ArgumentNullException>(reader != null);

			LoadX(document, XmlReader.Create(reader, settings), resolver);
		}

		/// <summary>
		/// Loads the XML document from the specified stream, with support for <c>XIncludes</c>.
		/// </summary>
		/// <param name="document">The document being extended.</param>
		/// <param name="stream">The stream containing the XML document to load.</param>
		public static void LoadX(this XmlDocument document, Stream stream)
		{
			Contract.Requires<ArgumentNullException>(document != null);
			Contract.Requires<ArgumentNullException>(stream != null);

			LoadX(document, stream, null);
		}

		/// <summary>
		/// Loads the XML document from the specified stream, with support for <c>XIncludes</c>.
		/// </summary>
		/// <param name="document">The document being extended.</param>
		/// <param name="stream">The stream containing the XML document to load.</param>
		/// <param name="resolver">The resolver to use to resolve external references.</param>
		public static void LoadX(this XmlDocument document, Stream stream, XmlUrlResolver resolver)
		{
			Contract.Requires<ArgumentNullException>(document != null);
			Contract.Requires<ArgumentNullException>(stream != null);

			LoadX(document, XmlReader.Create(stream, settings), resolver);
		}

		/// <summary>
		/// Loads the XML document from the specified <see cref="XmlReader"/>, with support for <c>XIncludes</c>.
		/// </summary>
		/// <param name="document">The document being extended.</param>
		/// <param name="reader">The XmlReader used to feed the XML data into the document.</param>
		public static void LoadX(this XmlDocument document, XmlReader reader)
		{
			Contract.Requires<ArgumentNullException>(document != null);
			Contract.Requires<ArgumentNullException>(reader != null);

			LoadX(document, reader, null);
		}

		/// <summary>
		/// Loads the XML document from the specified <see cref="XmlReader"/>, with support for <c>XIncludes</c>.
		/// </summary>
		/// <param name="document">The document being extended.</param>
		/// <param name="reader">The XmlReader used to feed the XML data into the document.</param>
		/// <param name="resolver">The resolver to use to resolve external references.</param>
		public static void LoadX(this XmlDocument document, XmlReader reader, XmlUrlResolver resolver)
		{
			Contract.Requires<ArgumentNullException>(document != null);
			Contract.Requires<ArgumentNullException>(reader != null);

			XIncludingReader xreader = new XIncludingReader(XmlReader.Create(reader, settings));

			if (resolver != null)
				xreader.XmlResolver = resolver;

			try
			{
				document.Load(xreader);
			}
			finally
			{
				if (xreader.ReadState != ReadState.Closed)
					xreader.Close();
			}
		}
	}
}
