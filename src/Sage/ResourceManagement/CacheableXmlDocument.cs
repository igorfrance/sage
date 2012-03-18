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
		public ReadOnlyCollection<string> Dependencies { get; private set; }

		/// <summary>
		/// Creates <see cref="XmlReaderSettings"/> and assings it the specified <paramref name="resolver"/>.
		/// </summary>
		/// <param name="resolver">The resolver to use with this settings..</param>
		/// <returns>An <see cref="XmlReaderSettings"/> instance, with common options set, and with the specified
		/// <paramref name="resolver"/> assigned to it.</returns>
		public static XmlReaderSettings CreateReaderSettings(XmlUrlResolver resolver)
		{
			XmlReaderSettings settings = readerSettings.Clone();
			settings.XmlResolver = resolver;
			settings.IgnoreComments = false;

			return settings;
		}

		/// <summary>
		/// Adds the specified <paramref name="dependencies"/>.
		/// </summary>
		/// <param name="dependencies">The dependencies to add.</param>
		public void AddDependencies(IEnumerable<string> dependencies)
		{
			Contract.Requires<ArgumentNullException>(dependencies != null);

			this.AddDependencies(dependencies.ToArray());
		}

		/// <summary>
		/// Adds the specified <paramref name="dependencies"/>.
		/// </summary>
		/// <param name="dependencies">The dependencies to add.</param>
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
