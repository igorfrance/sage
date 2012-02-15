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
namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Xml;

	using Kelp.Http;

	/// <summary>
	/// Extends the <see cref="XmlDocument"/> with an advanced loading facilities and additional properties 
	/// that provide the last modification date and a list of files that the document depends on.
	/// </summary>
	public class CacheableXmlDocument : XmlDocument
	{
		private static readonly XmlReaderSettings readerSettings = new XmlReaderSettings
		{
			IgnoreComments = true,
			CloseInput = true,
			DtdProcessing = DtdProcessing.Ignore
		};

		private readonly List<string> dependencies;
		private DateTime? lastModified;
		private string baseUri;

		/// <summary>
		/// Initializes a new instance of the <see cref="CacheableXmlDocument"/> class.
		/// </summary>
		public CacheableXmlDocument()
		{
			this.dependencies = new List<string>();
			this.Dependencies = this.dependencies.AsReadOnly();
		}

		/// <summary>
		/// Gets the last modification date associated with this document.
		/// </summary>
		/// <remarks>
		/// The last modification date will be the latests modification date of all constituent files referenced with
		/// <see cref="Dependencies"/>.
		/// </remarks>
		public DateTime LastModified
		{
			get
			{
				if (lastModified == null)
				{
					lastModified = Util.GetDateLastModified(this.Dependencies);
				}

				return lastModified.Value;
			}
		}

		/// <summary>
		/// Gets the list of files that this document consists of / depends on.
		/// </summary>
		public ReadOnlyCollection<string> Dependencies { get;  set; }

		public static XmlReaderSettings CreateReaderSettings(XmlUrlResolver resolver)
		{
			XmlReaderSettings settings = readerSettings.Clone();
			settings.XmlResolver = resolver;
			settings.IgnoreComments = false;

			return settings;
		}

		public void AddDependencies(IEnumerable<string> dependencies)
		{
			Contract.Requires<ArgumentNullException>(dependencies != null);

			this.AddDependencies(dependencies.ToArray());
		}

		public void AddDependencies(params string[] dependencies)
		{
			this.dependencies.AddRange(dependencies.Where(d => !this.Dependencies.Contains(d)));
		}

		/// <summary>
		/// Loads the XML document from the specified URL, with support for <c>XIncludes</c>.
		/// </summary>
		/// <param name="filename">URL for the file containing the XML document to load. The URL can be either a local file or an HTTP URL (a Web address).</param>
		public virtual new void Load(string filename)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(filename));

			Load(filename, new UrlResolver());
		}

		/// <summary>
		/// Loads the XML document from the specified URL, with support for <c>XIncludes</c>.
		/// </summary>
		/// <param name="filename">URL for the file containing the XML document to load. The URL can be either a local file or an HTTP URL (a Web address).</param>
		/// <param name="resolver">The resolver to use to resolve external references.</param>
		public virtual void Load(string filename, XmlUrlResolver resolver)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(filename));

			baseUri = filename;
			if (resolver != null)
			{
				Uri uri = new Uri(filename, UriKind.RelativeOrAbsolute);
				object reader = resolver.GetEntity(uri, null, null);
				if (reader != null)
				{
					var sr = reader as Stream;
					if (sr != null)
					{
						this.Load(sr, resolver);

						sr.Close();
						sr.Dispose();
						return;
					}

					var tr = reader as TextReader;
					if (tr != null)
					{
						this.Load(tr, resolver);

						tr.Close();
						tr.Dispose();
						return;
					}

					var xr = reader as XmlReader;
					if (xr != null)
					{
						this.Load(xr, resolver);

						if (xr.ReadState != ReadState.Closed)
							xr.Close();
						return;
					}
				}
			}

			XmlReaderSettings settings = CreateReaderSettings(resolver);
			Load(XmlReader.Create(filename, settings), resolver);
		}

		/// <summary>
		/// Loads the XML document from the specified <see cref="TextReader"/>, with support for <c>XIncludes</c>.
		/// </summary>
		/// <param name="reader">The TextReader used to feed the XML data into the document.</param>
		public virtual new void Load(TextReader reader)
		{
			Contract.Requires<ArgumentNullException>(reader != null);

			Load(reader, new UrlResolver());
		}

		/// <summary>
		/// Loads the XML document from the specified <see cref="TextReader"/>, with support for <c>XIncludes</c>.
		/// </summary>
		/// <param name="reader">The TextReader used to feed the XML data into the document.</param>
		/// <param name="resolver">The resolver to use to resolve external references.</param>
		public virtual void Load(TextReader reader, XmlUrlResolver resolver)
		{
			Contract.Requires<ArgumentNullException>(reader != null);

			XmlReaderSettings settings = CreateReaderSettings(resolver);
			Load(XmlReader.Create(reader, settings), resolver);
		}

		/// <summary>
		/// Loads the XML document from the specified stream, with support for <c>XIncludes</c>.
		/// </summary>
		/// <param name="stream">The stream containing the XML document to load.</param>
		public virtual new void Load(Stream stream)
		{
			Contract.Requires<ArgumentNullException>(stream != null);

			Load(stream, new UrlResolver());
		}

		/// <summary>
		/// Loads the XML document from the specified stream, with support for <c>XIncludes</c>.
		/// </summary>
		/// <param name="stream">The stream containing the XML document to load.</param>
		/// <param name="resolver">The resolver to use to resolve external references.</param>
		public virtual void Load(Stream stream, XmlUrlResolver resolver)
		{
			Contract.Requires<ArgumentNullException>(stream != null);

			XmlReaderSettings settings = CreateReaderSettings(resolver);
			Load(XmlReader.Create(stream, settings, baseUri), resolver);
		}

		/// <summary>
		/// Loads the XML document from the specified <see cref="XmlReader"/>, with support for <c>XIncludes</c>.
		/// </summary>
		/// <param name="reader">The XmlReader used to feed the XML data into the document.</param>
		public virtual new void Load(XmlReader reader)
		{
			Contract.Requires<ArgumentNullException>(reader != null);

			Load(reader, new UrlResolver());
		}

		/// <summary>
		/// Loads the XML document from the specified <see cref="XmlReader"/>, with support for <c>XIncludes</c>.
		/// </summary>
		/// <param name="reader">The XmlReader used to feed the XML data into the document.</param>
		/// <param name="resolver">The resolver to use to resolve external references.</param>
		public virtual void Load(XmlReader reader, XmlUrlResolver resolver)
		{
			Contract.Requires<ArgumentNullException>(reader != null);

			XmlReaderSettings settings = CreateReaderSettings(resolver);
			SageIncludeReader xreader = new SageIncludeReader(XmlReader.Create(reader, settings));
			xreader.XmlResolver = resolver;

			try
			{
				base.Load(xreader);
			}
			finally
			{
				if (xreader.ReadState != ReadState.Closed)
					xreader.Close();
			}
		}
	}
}
