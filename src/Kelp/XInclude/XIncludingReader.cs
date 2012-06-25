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
 * 
 * Original source for XPointer released under BSD licence, hence the disclaimer:
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
 * FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
 * COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
 * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */
namespace Kelp.XInclude
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using System.Reflection;
	using System.Security;
	using System.Text;
	using System.Xml;
	using System.Xml.Schema;

	using Kelp.Properties;
	using Kelp.XInclude.Common;
	using Kelp.XInclude.XPointer;

	/// <summary>
	/// Implements streamable subset of the XInclude 1.0 in a fast, non-caching, forward-only fashion.
	/// </summary>
	/// <remarks>
	/// <c>XIncludingReader</c> is XML Base and XInclude 1.0 aware <see cref="XmlReader"/>.
	/// </remarks>
	public class XIncludingReader : XmlReader, IXmlLineInfo
	{
		private static IDictionary<string, WeakReference> cache;

		// XInclude keywords
		private readonly XIncludeKeywords keywords;
		private readonly XmlNameTable nameTable;
		private readonly Stack<XmlReader> readers;
		private readonly Uri topBaseUri;

		private bool differentLang;
		private bool gotElement;
		private bool gotTopIncludedElem;
		private bool topLevel;
		private FallbackState fallbackState;
		private FallbackState prevFallbackState;

		private XmlReader reader;
		private XmlResolver xmlResolver;

		private int realXmlBaseIndex = -1;
		private XIncludingReaderState state;

		/// <summary>
		/// Initializes a new instance of the <see cref="XIncludingReader"/> class with
		/// specified underlying <c>XmlReader</c> reader.
		/// </summary>
		/// <param name="reader">Underlying reader to read from</param>        
		public XIncludingReader(XmlReader reader)
		{
			this.MakeRelativeBaseUri = true;
			var xtr = reader as XmlTextReader;
			if (xtr != null)
			{
				var s = new XmlReaderSettings();
				s.DtdProcessing = DtdProcessing.Prohibit;
				s.ValidationType = ValidationType.None;
				s.ValidationEventHandler += ValidationCallback;
				
				if (xtr.WhitespaceHandling == WhitespaceHandling.Significant)
				{
					s.IgnoreWhitespace = true;
				}

				this.reader = Create(reader, s);
			}
			else
			{
				this.reader = reader;
			}

			this.nameTable = reader.NameTable;
			this.keywords = new XIncludeKeywords(this.NameTable);
			if (this.reader.BaseURI != string.Empty)
			{
				this.topBaseUri = new Uri(this.reader.BaseURI);
			}
			else
			{
				this.MakeRelativeBaseUri = false;
				this.topBaseUri = new Uri(Assembly.GetExecutingAssembly().Location);
			}

			this.readers = new Stack<XmlReader>();
			this.state = XIncludingReaderState.Default;
		}

		/// <summary>
		/// Creates new instance of <c>XIncludingReader</c> class with
		/// specified URL.
		/// </summary>
		/// <param name="url">Document location.</param>
		public XIncludingReader(string url)
			: this(new XmlBaseAwareXmlReader(url))
		{
		}

		/// <summary>
		/// Creates new instance of <c>XIncludingReader</c> class with
		/// specified URL and resolver.
		/// </summary>
		/// <param name="url">Document location.</param>
		/// <param name="resolver">Resolver to acquire external resources.</param>
		public XIncludingReader(string url, XmlResolver resolver)
			: this(new XmlBaseAwareXmlReader(url, resolver))
		{
		}

		/// <summary>
		/// Creates new instance of <c>XIncludingReader</c> class with
		/// specified URL and nametable.
		/// </summary>
		/// <param name="url">Document location.</param>
		/// <param name="nt">Name table.</param>
		public XIncludingReader(string url, XmlNameTable nt)
			: this(new XmlBaseAwareXmlReader(url, nt))
		{
		}

		/// <summary>
		/// Creates new instance of <c>XIncludingReader</c> class with
		/// specified <c>TextReader</c> reader.
		/// </summary>
		/// <param name="reader"><c>TextReader</c>.</param>
		public XIncludingReader(TextReader reader)
			: this(new XmlBaseAwareXmlReader(reader))
		{
		}

		/// <summary>
		/// Creates new instance of <c>XIncludingReader</c> class with
		/// specified URL and <c>TextReader</c> reader.
		/// </summary>
		/// <param name="reader"><c>TextReader</c>.</param>
		/// <param name="url">Source document's URL</param>
		public XIncludingReader(string url, TextReader reader)
			: this(new XmlBaseAwareXmlReader(url, reader))
		{
		}

		/// <summary>
		/// Creates new instance of <c>XIncludingReader</c> class with
		/// specified <c>TextReader</c> reader and nametable.
		/// </summary>
		/// <param name="reader"><c>TextReader</c>.</param>
		/// <param name="nt">Nametable.</param>
		public XIncludingReader(TextReader reader, XmlNameTable nt)
			: this(new XmlBaseAwareXmlReader(reader, nt))
		{
		}

		/// <summary>
		/// Creates new instance of <c>XIncludingReader</c> class with
		/// specified URL, <c>TextReader</c> reader and nametable.
		/// </summary>
		/// <param name="reader"><c>TextReader</c>.</param>
		/// <param name="nt">Nametable.</param>
		/// <param name="url">Source document's URI</param>
		public XIncludingReader(string url, TextReader reader, XmlNameTable nt)
			: this(new XmlBaseAwareXmlReader(url, reader, nt))
		{
		}

		/// <summary>
		/// Creates new instance of <c>XIncludingReader</c> class with
		/// specified <c>Stream</c>.
		/// </summary>
		/// <param name="input"><c>Stream</c>.</param>
		public XIncludingReader(Stream input)
			: this(new XmlBaseAwareXmlReader(input))
		{
		}

		/// <summary>
		/// Creates new instance of <c>XIncludingReader</c> class with
		/// specified URL and <c>Stream</c>.
		/// </summary>
		/// <param name="input"><c>Stream</c>.</param>
		/// <param name="url">Source document's URL</param>
		public XIncludingReader(string url, Stream input)
			: this(new XmlBaseAwareXmlReader(url, input))
		{
		}

		/// <summary>
		/// Creates new instance of <c>XIncludingReader</c> class with
		/// specified URL, <c>Stream</c> and resolver.
		/// </summary>
		/// <param name="input"><c>Stream</c>.</param>
		/// <param name="url">Source document's URL</param>
		/// <param name="resolver">Resolver to acquire external resources.</param>
		public XIncludingReader(string url, Stream input, XmlResolver resolver)
			: this(new XmlBaseAwareXmlReader(url, input, resolver))
		{
		}

		/// <summary>
		/// Creates new instance of <c>XIncludingReader</c> class with
		/// specified <c>Stream</c> and nametable.
		/// </summary>
		/// <param name="input"><c>Stream</c>.</param>
		/// <param name="nt">Nametable</param>
		public XIncludingReader(Stream input, XmlNameTable nt)
			: this(new XmlBaseAwareXmlReader(input, nt))
		{
		}

		/// <summary>
		/// Creates new instance of <c>XIncludingReader</c> class with
		/// specified URL, <c>Stream</c> and nametable.
		/// </summary>
		/// <param name="input"><c>Stream</c>.</param>
		/// <param name="nt">Nametable</param>
		/// <param name="url">Source document's URL</param>
		public XIncludingReader(string url, Stream input, XmlNameTable nt)
			: this(new XmlBaseAwareXmlReader(url, input, nt))
		{
		}

		/// <inheritdoc/>
		public override int AttributeCount
		{
			get
			{
				if (this.topLevel)
				{
					int ac = this.reader.AttributeCount;
					if (this.reader.GetAttribute(this.keywords.XmlBase) == null)
					{
						ac++;
					}

					if (this.differentLang)
					{
						ac++;
					}

					return ac;
				}

				return this.reader.AttributeCount;
			}
		}

		/// <inheritdoc/>
		public override string BaseURI
		{
			get
			{
				return this.reader.BaseURI;
			}
		}

		/// <inheritdoc/>
		public override int Depth
		{
			get
			{
				if (this.readers.Count == 0)
				{
					return this.reader.Depth;
				}

				// TODO: that might be way ineffective
				return this.readers.Peek().Depth + this.reader.Depth;
			}
		}

		/// <inheritdoc/>
		public override bool EOF
		{
			get
			{
				return this.reader.EOF;
			}
		}

		/// <summary>
		/// Gets the encoding of the document.
		/// </summary>
		/// <remarks>If underlying XmlReader doesn't support Encoding property, null is returned.</remarks>
		public Encoding Encoding
		{
			get
			{
				var xtr = this.reader as XmlTextReader;
				if (xtr != null)
				{
					return xtr.Encoding;
				}

				var xir = this.reader as XIncludingReader;
				if (xir != null)
				{
					return xir.Encoding;
				}

				return null;
			}
		}

		/// <summary>
		/// Flag indicating whether expose text inclusions
		/// as CDATA or as Text. By default it's Text.
		/// </summary>
		public bool ExposeTextInclusionsAsCDATA { get; set; }

		/// <inheritdoc/>
		public override bool HasValue
		{
			get
			{
				if (this.state == XIncludingReaderState.Default)
				{
					return this.reader.HasValue;
				}

				return true;
			}
		}

		/// <inheritdoc/>
		public override bool IsDefault
		{
			get
			{
				if (this.state == XIncludingReaderState.Default)
				{
					return this.reader.IsDefault;
				}

				return false;
			}
		}

		/// <inheritdoc/>
		public override bool IsEmptyElement
		{
			get
			{
				return this.reader.IsEmptyElement;
			}
		}

		/// <summary>
		/// Gets the current line number.
		/// See <see cref="IXmlLineInfo.LineNumber "/>.
		/// </summary>
		public int LineNumber
		{
			get
			{
				var core = this.reader as IXmlLineInfo;
				if (core != null)
				{
					return core.LineNumber;
				}

				return 0;
			}
		}

		/// <summary>
		///   	Gets the current line position.
		/// See <see cref="IXmlLineInfo.LinePosition "/>.
		/// </summary>
		public int LinePosition
		{
			get
			{
				var core = this.reader as IXmlLineInfo;
				if (core != null)
				{
					return core.LinePosition;
				}

				return 0;
			}
		}

		/// <inheritdoc/>
		public override string LocalName
		{
			get
			{
				switch (this.state)
				{
					case XIncludingReaderState.ExposingXmlBaseAttr:
						return this.keywords.BaseName;
					case XIncludingReaderState.ExposingXmlBaseAttrValue:
					case XIncludingReaderState.ExposingXmlLangAttrValue:
						return string.Empty;
					case XIncludingReaderState.ExposingXmlLangAttr:
						return this.keywords.Lang;
					default:
						return this.reader.LocalName;
				}
			}
		}

		/// <summary>
		/// Flag indicating whether to emit <c>xml:base</c> as relative URI.
		/// True by default.
		/// </summary>
		public bool MakeRelativeBaseUri { get; set; }

		/// <inheritdoc/>
		public override string Name
		{
			get
			{
				switch (this.state)
				{
					case XIncludingReaderState.ExposingXmlBaseAttr:
						return this.keywords.XmlBase;
					case XIncludingReaderState.ExposingXmlBaseAttrValue:
					case XIncludingReaderState.ExposingXmlLangAttrValue:
						return string.Empty;
					case XIncludingReaderState.ExposingXmlLangAttr:
						return this.keywords.XmlLang;
					default:
						return this.reader.Name;
				}
			}
		}

		/// <inheritdoc/>
		public override sealed XmlNameTable NameTable
		{
			get
			{
				return this.nameTable;
			}
		}

		/// <inheritdoc/>
		public override string NamespaceURI
		{
			get
			{
				switch (this.state)
				{
					case XIncludingReaderState.ExposingXmlBaseAttr:
					case XIncludingReaderState.ExposingXmlLangAttr:
						return this.keywords.XmlNamespace;
					case XIncludingReaderState.ExposingXmlBaseAttrValue:
					case XIncludingReaderState.ExposingXmlLangAttrValue:
						return string.Empty;
					default:
						return this.reader.NamespaceURI;
				}
			}
		}

		/// <inheritdoc/>
		public override XmlNodeType NodeType
		{
			get
			{
				switch (this.state)
				{
					case XIncludingReaderState.ExposingXmlBaseAttr:
					case XIncludingReaderState.ExposingXmlLangAttr:
						return XmlNodeType.Attribute;
					case XIncludingReaderState.ExposingXmlBaseAttrValue:
					case XIncludingReaderState.ExposingXmlLangAttrValue:
						return XmlNodeType.Text;
					default:
						return this.reader.NodeType;
				}
			}
		}

		/// <inheritdoc/>
		public override string Prefix
		{
			get
			{
				switch (this.state)
				{
					case XIncludingReaderState.ExposingXmlBaseAttr:
					case XIncludingReaderState.ExposingXmlLangAttr:
						return this.keywords.Xml;
					case XIncludingReaderState.ExposingXmlBaseAttrValue:
					case XIncludingReaderState.ExposingXmlLangAttrValue:
						return string.Empty;
					default:
						return this.reader.Prefix;
				}
			}
		}

		/// <inheritdoc/>
		public override char QuoteChar
		{
			get
			{
				switch (this.state)
				{
					case XIncludingReaderState.ExposingXmlBaseAttr:
					case XIncludingReaderState.ExposingXmlLangAttr:
						return '"';
					default:
						return this.reader.QuoteChar;
				}
			}
		}

		/// <inheritdoc/>
		public override ReadState ReadState
		{
			get
			{
				return this.reader.ReadState;
			}
		}

		/// <inheritdoc/>
		public override string Value
		{
			get
			{
				switch (this.state)
				{
					case XIncludingReaderState.ExposingXmlBaseAttr:
					case XIncludingReaderState.ExposingXmlBaseAttrValue:
						return this.GetBaseUri();
					case XIncludingReaderState.ExposingXmlLangAttr:
					case XIncludingReaderState.ExposingXmlLangAttrValue:
						return this.reader.XmlLang;
					default:
						return this.reader.Value;
				}
			}
		}

		/// <summary>
		/// See <see cref="XmlTextReader.WhitespaceHandling"/>.
		/// </summary>
		public WhitespaceHandling WhitespaceHandling { get; set; }

		/// <inheritdoc/>
		public override string XmlLang
		{
			get
			{
				return this.reader.XmlLang;
			}
		}

		/// <summary>
		/// XmlResolver to resolve external URI references
		/// </summary>
		public XmlResolver XmlResolver
		{
			set
			{
				this.xmlResolver = value;
			}
		}

		/// <inheritdoc/>
		public override XmlSpace XmlSpace
		{
			get
			{
				return this.reader.XmlSpace;
			}
		}

		/// <inheritdoc/>
		public override string this[int i]
		{
			get
			{
				return GetAttribute(i);
			}
		}

		/// <inheritdoc/>
		public override string this[string name]
		{
			get
			{
				return GetAttribute(name);
			}
		}

		/// <inheritdoc/>
		public override string this[string name, string namespaceURI]
		{
			get
			{
				return this.GetAttribute(name, namespaceURI);
			}
		}

		/// <inheritdoc/>
		public override void Close()
		{
			if (this.reader != null)
			{
				this.reader.Close();
			}

			// Close all readers in the stack
			while (this.readers.Count > 0)
			{
				this.reader = this.readers.Pop();
				if (this.reader != null)
				{
					this.reader.Close();
				}
			}
		}

		/// <inheritdoc/>
		public override string GetAttribute(int i)
		{
			if (this.topLevel)
			{
				int ac = this.reader.AttributeCount;
				if (i < ac)
				{
					if (i == this.realXmlBaseIndex)
					{
						// case 1: it's real xml:base
						return this.GetBaseUri();
					}

					// case 2: it's real attribute and it's not xml:base
					return this.reader.GetAttribute(i);
				}

				if (i == ac)
				{
					// case 3: it's virtual xml:base - it comes first
					return this.GetBaseUri();
				}

				// case 4: it's virtual xml:lang - it comes last
				return this.reader.XmlLang;
			}

			return this.reader.GetAttribute(i);
		}

		/// <inheritdoc/>
		public override string GetAttribute(string name)
		{
			if (this.topLevel)
			{
				if (XIncludeKeywords.Equals(name, this.keywords.XmlBase))
					return this.GetBaseUri();

				if (XIncludeKeywords.Equals(name, this.keywords.XmlLang))
					return this.reader.XmlLang;
			}

			return this.reader.GetAttribute(name);
		}

		/// <inheritdoc/>
		public override string GetAttribute(string name, string namespaceURI)
		{
			if (this.topLevel)
			{
				if (XIncludeKeywords.Equals(name, this.keywords.BaseName) && XIncludeKeywords.Equals(namespaceURI, this.keywords.XmlNamespace))
					return this.GetBaseUri();

				if (XIncludeKeywords.Equals(name, this.keywords.Lang) && XIncludeKeywords.Equals(namespaceURI, this.keywords.XmlNamespace))
					return this.reader.XmlLang;
			}

			return this.reader.GetAttribute(name, namespaceURI);
		}

		/// <summary>
		/// Gets a value indicating whether the class can return line information.
		/// See <see cref="IXmlLineInfo.HasLineInfo"/>.
		/// </summary>        
		public bool HasLineInfo()
		{
			var core = this.reader as IXmlLineInfo;
			if (core != null)
			{
				return core.HasLineInfo();
			}

			return false;
		}

		/// <inheritdoc/>
		public override string LookupNamespace(string prefix)
		{
			return this.reader.LookupNamespace(prefix);
		}

		/// <inheritdoc/>
		public override void MoveToAttribute(int i)
		{
			if (this.topLevel)
			{
				if (i >= this.reader.AttributeCount || i == this.realXmlBaseIndex)
				{
					this.state = XIncludingReaderState.ExposingXmlBaseAttr;
				}
				else
				{
					this.reader.MoveToAttribute(i);
				}
			}
			else
			{
				this.reader.MoveToAttribute(i);
			}
		}

		/// <inheritdoc/>
		public override bool MoveToAttribute(string name)
		{
			if (this.topLevel)
			{
				if (XIncludeKeywords.Equals(name, this.keywords.XmlBase))
				{
					this.state = XIncludingReaderState.ExposingXmlBaseAttr;
					return true;
				}

				if (XIncludeKeywords.Equals(name, this.keywords.XmlLang))
				{
					this.state = XIncludingReaderState.ExposingXmlLangAttr;
					return true;
				}
			}

			return this.reader.MoveToAttribute(name);
		}

		/// <inheritdoc/>
		public override bool MoveToAttribute(string name, string ns)
		{
			if (this.topLevel)
			{
				if (XIncludeKeywords.Equals(name, this.keywords.BaseName) && XIncludeKeywords.Equals(ns, this.keywords.XmlNamespace))
				{
					this.state = XIncludingReaderState.ExposingXmlBaseAttr;
					return true;
				}

				if (XIncludeKeywords.Equals(name, this.keywords.Lang) && XIncludeKeywords.Equals(ns, this.keywords.XmlNamespace))
				{
					this.state = XIncludingReaderState.ExposingXmlLangAttr;
					return true;
				}
			}

			return this.reader.MoveToAttribute(name, ns);
		}

		/// <inheritdoc/>
		public override bool MoveToElement()
		{
			return this.reader.MoveToElement();
		}

		/// <inheritdoc/>
		public override bool MoveToFirstAttribute()
		{
			if (this.topLevel)
			{
				bool res = this.reader.MoveToFirstAttribute();
				if (res)
				{
					// it might be xml:base or xml:lang
					if (this.reader.Name == this.keywords.XmlBase || this.reader.Name == this.keywords.XmlLang)
					{
						// omit them - we expose virtual ones at the end
						return this.MoveToNextAttribute();
					}

					return true;
				}

				// No attrs? Expose xml:base
				this.state = XIncludingReaderState.ExposingXmlBaseAttr;
				return true;
			}

			return this.reader.MoveToFirstAttribute();
		}

		/// <inheritdoc/>
		public override bool MoveToNextAttribute()
		{
			if (!this.topLevel)
			{
				return this.reader.MoveToNextAttribute();
			}

			switch (this.state)
			{
				case XIncludingReaderState.ExposingXmlBaseAttr:
				case XIncludingReaderState.ExposingXmlBaseAttrValue:
					if (this.differentLang)
					{
						this.state = XIncludingReaderState.ExposingXmlLangAttr;
						return true;
					}

					this.state = XIncludingReaderState.Default;
					return false;

				case XIncludingReaderState.ExposingXmlLangAttr:
				case XIncludingReaderState.ExposingXmlLangAttrValue:
					this.state = XIncludingReaderState.Default;
					return false;

				default:
					bool res = this.reader.MoveToNextAttribute();
					if (res)
					{
						if (this.reader.Name == this.keywords.XmlBase || this.reader.Name == this.keywords.XmlLang)
							return this.MoveToNextAttribute();

						return true;
					}

					this.state = XIncludingReaderState.ExposingXmlBaseAttr;
					return true;
			}
		}

		/// <inheritdoc/>
		public override bool Read()
		{
			this.state = XIncludingReaderState.Default;

			bool baseRead = this.reader.Read();
			if (baseRead)
			{
				this.topLevel = this.readers.Count > 0 && this.reader.Depth == 0;
				if (this.topLevel && this.reader.NodeType == XmlNodeType.Attribute)
				{
					throw new AttributeOrNamespaceInIncludeLocationError(Resources.AttributeOrNamespaceInIncludeLocationError);
				}

				if (this.topLevel && this.readers.Peek().Depth == 0 && this.reader.NodeType == XmlNodeType.Element)
				{
					if (this.gotTopIncludedElem)
					{
						throw new MalformedXInclusionResultError(Resources.MalformedXInclusionResult);
					}

					this.gotTopIncludedElem = true;
				}

				if (this.topLevel)
				{
					this.differentLang = this.AreDifferentLangs(this.reader.XmlLang, this.readers.Peek().XmlLang);
					if (this.reader.NodeType == XmlNodeType.Element)
					{
						this.realXmlBaseIndex = -1;
						int i = 0;
						while (this.reader.MoveToNextAttribute())
						{
							if (this.reader.Name == this.keywords.XmlBase)
							{
								this.realXmlBaseIndex = i;
								break;
							}

							i++;
						}

						this.reader.MoveToElement();
					}
				}

				switch (this.reader.NodeType)
				{
					case XmlNodeType.XmlDeclaration:
					case XmlNodeType.Document:
					case XmlNodeType.DocumentType:
					case XmlNodeType.DocumentFragment:
						return this.readers.Count <= 0 || this.Read();

					case XmlNodeType.Element:
						if (this.IsIncludeElement())
						{
							XmlReader current = this.reader;
							try
							{
								return this.ProcessIncludeElement();
							}
							catch (Exception e)
							{
								if (!current.Equals(this.reader))
								{
									this.reader.Close();
									this.reader = current;
								}

								this.prevFallbackState = this.fallbackState;
								return this.ProcessFallback(this.reader.Depth, e);
							}
						}

						if (this.IsFallbackElement())
						{
							var li = this.reader as IXmlLineInfo;
							if (li != null && li.HasLineInfo())
							{
								throw new XIncludeSyntaxError(
									string.Format(
										CultureInfo.CurrentCulture, 
										Resources.FallbackNotChildOfIncludeLong, 
										this.reader.BaseURI, 
										li.LineNumber, 
										li.LinePosition));
							}

							throw new XIncludeSyntaxError(
								string.Format(CultureInfo.CurrentCulture, Resources.FallbackNotChildOfInclude, this.reader.BaseURI));
						}

						this.gotElement = true;
						goto default;

					case XmlNodeType.EndElement:
						if (this.fallbackState.Fallbacking && this.reader.Depth == this.fallbackState.FallbackDepth
						    && this.IsFallbackElement())
						{
							this.fallbackState.FallbackProcessed = true;
							return this.ProcessFallback(this.reader.Depth - 1, null);
						}

						goto default;

					default:
						return true;
				}
			}

			if (this.topLevel)
			{
				this.topLevel = false;
			}

			if (this.readers.Count > 0)
			{
				this.reader.Close();
				this.reader = this.readers.Pop();

				if (!this.reader.IsEmptyElement)
					this.CheckAndSkipContent();

				return this.Read();
			}

			if (!this.gotElement)
				throw new MalformedXInclusionResultError(Resources.MalformedXInclusionResult);

			return false;
		}

		/// <inheritdoc/>
		public override bool ReadAttributeValue()
		{
			switch (this.state)
			{
				case XIncludingReaderState.ExposingXmlBaseAttr:
					this.state = XIncludingReaderState.ExposingXmlBaseAttrValue;
					return true;
				case XIncludingReaderState.ExposingXmlBaseAttrValue:
					return false;
				case XIncludingReaderState.ExposingXmlLangAttr:
					this.state = XIncludingReaderState.ExposingXmlLangAttrValue;
					return true;
				case XIncludingReaderState.ExposingXmlLangAttrValue:
					return false;
				default:
					return this.reader.ReadAttributeValue();
			}
		}

		/// <inheritdoc/>
		public override string ReadInnerXml()
		{
			switch (this.state)
			{
				case XIncludingReaderState.ExposingXmlBaseAttr:
					return this.GetBaseUri();

				case XIncludingReaderState.ExposingXmlBaseAttrValue:
					return string.Empty;

				case XIncludingReaderState.ExposingXmlLangAttr:
					return this.reader.XmlLang;

				case XIncludingReaderState.ExposingXmlLangAttrValue:
					return string.Empty;

				default:
					switch (this.NodeType)
					{
						case XmlNodeType.Element:
							{
								int depth = this.Depth;
								if (this.Read())
								{
									var sw = new StringWriter();
									var xw = new XmlTextWriter(sw);
									while (this.Depth > depth)
									{
										xw.WriteNode(this, false);
									}

									xw.Close();
									return sw.ToString();
								}

								return string.Empty;
							}

						case XmlNodeType.Attribute:
							return this.Value;

						default:
							return string.Empty;
					}
			}
		}

		/// <inheritdoc/>
		public override string ReadOuterXml()
		{
			switch (this.state)
			{
				case XIncludingReaderState.ExposingXmlBaseAttr:
					return @"xml:base="" + _reader.BaseURI + @""";

				case XIncludingReaderState.ExposingXmlBaseAttrValue:
					return string.Empty;

				case XIncludingReaderState.ExposingXmlLangAttr:
					return @"xml:lang="" + _reader.XmlLang + @""";

				case XIncludingReaderState.ExposingXmlLangAttrValue:
					return string.Empty;

				default:
					if (this.NodeType == XmlNodeType.Element)
					{
						var sw = new StringWriter();
						var xw = new XmlTextWriter(sw);
						xw.WriteNode(this, false);
						xw.Close();
						return sw.ToString();
					}

					if (this.NodeType == XmlNodeType.Attribute)
						return string.Format("{0}=\"{1}\"", this.Name, this.Value);

					return string.Empty;
			}
		}

		/// <inheritdoc/>
		public override string ReadString()
		{
			switch (this.state)
			{
				case XIncludingReaderState.ExposingXmlBaseAttr:
					return string.Empty;
				case XIncludingReaderState.ExposingXmlBaseAttrValue:
					return this.GetBaseUri();
				case XIncludingReaderState.ExposingXmlLangAttr:
					return string.Empty;
				case XIncludingReaderState.ExposingXmlLangAttrValue:
					return this.reader.XmlLang;
				default:
					return this.reader.ReadString();
			}
		}

		/// <inheritdoc/>
		public override void ResolveEntity()
		{
			this.reader.ResolveEntity();
		}

		/// <summary>
		/// Fetches resource by URI.
		/// </summary>        
		internal static Stream GetResource(string includeLocation, string accept, string acceptLanguage, out WebResponse response)
		{
			WebRequest request;
			try
			{
				request = WebRequest.Create(includeLocation);
			}
			catch (NotSupportedException nse)
			{
				throw new ResourceException(string.Format(CultureInfo.CurrentCulture, Resources.URISchemaNotSupported, includeLocation), nse);
			}
			catch (SecurityException se)
			{
				throw new ResourceException(string.Format(CultureInfo.CurrentCulture, Resources.SecurityException, includeLocation), se);
			}

			// Add accept headers if this is HTTP request
			var httpReq = request as HttpWebRequest;
			if (httpReq != null)
			{
				if (accept != null)
				{
					TextUtils.CheckAcceptValue(accept);
					if (string.IsNullOrEmpty(httpReq.Accept))
					{
						httpReq.Accept = accept;
					}
					else
					{
						httpReq.Accept += "," + accept;
					}
				}

				if (acceptLanguage != null)
				{
					if (httpReq.Headers["Accept-Language"] == null)
					{
						httpReq.Headers.Add("Accept-Language", acceptLanguage);
					}
					else
					{
						httpReq.Headers["Accept-Language"] += "," + acceptLanguage;
					}
				}
			}

			try
			{
				response = request.GetResponse();
			}
			catch (WebException we)
			{
				throw new ResourceException(string.Format(CultureInfo.CurrentCulture, Resources.ResourceError, includeLocation), we);
			}

			return response.GetResponseStream();
		}

		private static void ValidationCallback(object sender, ValidationEventArgs args)
		{
			// do nothing
		}

		/// <summary>
		/// Compares two languages as per IETF RFC 3066.
		/// </summary>        
		private bool AreDifferentLangs(string lang1, string lang2)
		{
			return lang1.ToLower() != lang2.ToLower();
		}

		/// <summary>
		/// Skips xi:include element's content, while checking XInclude syntax (no 
		/// xi:include, no more than one xi:fallback).
		/// </summary>
		private void CheckAndSkipContent()
		{
			int depth = this.reader.Depth;
			bool fallbackElem = false;
			while (this.reader.Read() && depth < this.reader.Depth)
			{
				switch (this.reader.NodeType)
				{
					case XmlNodeType.Element:
						if (this.IsIncludeElement())
						{
							// xi:include child of xi:include - fatal error
							var li = this.reader as IXmlLineInfo;
							if (li != null && li.HasLineInfo())
							{
								throw new XIncludeSyntaxError(
									string.Format(
										CultureInfo.CurrentCulture, Resources.IncludeChildOfIncludeLong, this.reader.BaseURI, li.LineNumber, li.LinePosition));
							}

							throw new XIncludeSyntaxError(
								string.Format(CultureInfo.CurrentCulture, Resources.IncludeChildOfInclude, this.reader.BaseURI));
						}

						if (this.IsFallbackElement())
						{
							// Found xi:fallback
							if (fallbackElem)
							{
								// More than one xi:fallback
								var li = this.reader as IXmlLineInfo;
								if (li != null && li.HasLineInfo())
								{
									throw new XIncludeSyntaxError(
										string.Format(
											CultureInfo.CurrentCulture, Resources.TwoFallbacksLong, this.reader.BaseURI, li.LineNumber, li.LinePosition));
								}

								throw new XIncludeSyntaxError(string.Format(CultureInfo.CurrentCulture, Resources.TwoFallbacks, this.reader.BaseURI));
							}

							fallbackElem = true;
							this.SkipContent();
						}
							
						else if (XIncludeKeywords.Equals(this.reader.NamespaceURI, this.keywords.XincludeNamespace))
						{
							throw new XIncludeSyntaxError(
								string.Format(CultureInfo.CurrentCulture, Resources.UnknownXIncludeElement, this.reader.Name));
						}
						else
						{
							// Ignore everything else
							this.SkipContent();
						}

						break;
				}
			}
		}

		/// <summary>
		/// Checks for inclusion loops.
		/// </summary>        
		private void CheckLoops(Uri url)
		{
			// Check circular inclusion  
			Uri baseUri = this.reader.BaseURI == string.Empty ? this.topBaseUri : new Uri(this.reader.BaseURI);
			if (baseUri.Equals(url))
			{
				this.ThrowCircularInclusionError(this.reader, url);
			}

			foreach (XmlReader r in this.readers)
			{
				baseUri = r.BaseURI == string.Empty ? this.topBaseUri : new Uri(r.BaseURI);
				if (baseUri.Equals(url))
				{
					this.ThrowCircularInclusionError(this.reader, url);
				}
			}
		}

		/// <summary>
		/// Creates acquired infoset.
		/// </summary>        
		private string CreateAcquiredInfoset(Uri includeLocation)
		{
			if (cache == null)
			{
				cache = new Dictionary<string, WeakReference>();
			}

			WeakReference wr;
			if (cache.TryGetValue(includeLocation.AbsoluteUri, out wr) && wr.IsAlive)
			{
				return (string)wr.Target;
			}

			// Not cached or GCollected
			WebResponse response;
			Stream stream = GetResource(
				includeLocation.AbsoluteUri, 
				this.reader.GetAttribute(this.keywords.Accept), 
				this.reader.GetAttribute(this.keywords.AcceptLanguage), 
				out response);

			var xir = new XIncludingReader(response.ResponseUri.AbsoluteUri, stream, this.nameTable);
			xir.WhitespaceHandling = this.WhitespaceHandling;
			var sw = new StringWriter();
			var w = new XmlTextWriter(sw);
			try
			{
				while (xir.Read())
				{
					w.WriteNode(xir, false);
				}
			}
			finally
			{
				xir.Close();
				w.Close();
			}

			string content = sw.ToString();
			lock (cache)
			{
				if (!cache.ContainsKey(includeLocation.AbsoluteUri))
				{
					cache.Add(includeLocation.AbsoluteUri, new WeakReference(content));
				}
			}

			return content;
		}

		/// <summary>
		/// Creates acquired infoset.
		/// </summary>
		/// <param name="reader">Source reader</param>
		/// <param name="includeLocation">Base URI</param>
		private string CreateAcquiredInfoset(Uri includeLocation, TextReader reader)
		{
			return this.CreateAcquiredInfoset(new XmlBaseAwareXmlReader(includeLocation.AbsoluteUri, reader, this.nameTable));
		}

		/// <summary>
		/// Creates acquired infoset.
		/// </summary>
		/// <param name="reader">Source reader</param>
		private string CreateAcquiredInfoset(XmlReader reader)
		{
			// TODO: Try to stream out this stuff                                    
			var xir = new XIncludingReader(reader);
			xir.XmlResolver = this.xmlResolver;
			var sw = new StringWriter();
			var w = new XmlTextWriter(sw);
			try
			{
				while (xir.Read())
				{
					w.WriteNode(xir, false);
				}
			}
			finally
			{
				xir.Close();
				w.Close();
			}

			return sw.ToString();
		}

		private string GetBaseUri()
		{
			if (this.reader.BaseURI == string.Empty)
			{
				return string.Empty;
			}

			if (this.MakeRelativeBaseUri)
			{
				var baseUri = new Uri(this.reader.BaseURI);
				return this.topBaseUri.MakeRelativeUri(baseUri).ToString();
			}

			return this.reader.BaseURI;
		}

		/// <summary>
		/// /// Checks if given reader is positioned on a xi:fallback element.
		/// </summary>
		/// <returns></returns>
		private bool IsFallbackElement()
		{
			if ((XIncludeKeywords.Equals(this.reader.NamespaceURI, this.keywords.XincludeNamespace)
			     || XIncludeKeywords.Equals(this.reader.NamespaceURI, this.keywords.OldXIncludeNamespace))
			    && XIncludeKeywords.Equals(this.reader.LocalName, this.keywords.Fallback))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Checks if given reader is positioned on a xi:include element.
		/// </summary>        
		private bool IsIncludeElement()
		{
			if ((XIncludeKeywords.Equals(this.reader.NamespaceURI, this.keywords.XincludeNamespace)
			     || XIncludeKeywords.Equals(this.reader.NamespaceURI, this.keywords.OldXIncludeNamespace))
			    && XIncludeKeywords.Equals(this.reader.LocalName, this.keywords.Include))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Fallback processing.
		/// </summary>
		/// <param name="depth"><c>xi:include</c> depth level.</param>    
		/// <param name="e">Resource error, which caused this processing.</param>
		/// <remarks>When inluding fails due to any resource error, <c>xi:inlcude</c> 
		/// element content is processed as follows: each child <c>xi:include</c> - 
		/// fatal error, more than one child <c>xi:fallback</c> - fatal error. No 
		/// <c>xi:fallback</c> - the resource error results in a fatal error.
		/// Content of first <c>xi:fallback</c> element is included in a usual way.</remarks>
		private bool ProcessFallback(int depth, Exception e)
		{
			// Read to the xi:include end tag
			while (this.reader.Read() && depth < this.reader.Depth)
			{
				switch (this.reader.NodeType)
				{
					case XmlNodeType.Element:
						if (this.IsIncludeElement())
						{
							// xi:include child of xi:include - fatal error
							var li = this.reader as IXmlLineInfo;
							if (li != null && li.HasLineInfo())
							{
								throw new XIncludeSyntaxError(
									string.Format(
										CultureInfo.CurrentCulture, Resources.IncludeChildOfIncludeLong, this.BaseURI, li.LineNumber, li.LinePosition));
							}

							throw new XIncludeSyntaxError(string.Format(CultureInfo.CurrentCulture, Resources.IncludeChildOfInclude, this.BaseURI));
						}

						if (this.IsFallbackElement())
						{
							// Found xi:fallback
							if (this.fallbackState.FallbackProcessed)
							{
								var li = this.reader as IXmlLineInfo;
								if (li != null && li.HasLineInfo())
								{
									// Two xi:fallback                                 
									throw new XIncludeSyntaxError(
										string.Format(CultureInfo.CurrentCulture, Resources.TwoFallbacksLong, this.BaseURI, li.LineNumber, li.LinePosition));
								}

								throw new XIncludeSyntaxError(string.Format(CultureInfo.CurrentCulture, Resources.TwoFallbacks, this.BaseURI));
							}

							if (this.reader.IsEmptyElement)
							{
								// Empty xi:fallback - nothing to include
								this.fallbackState.FallbackProcessed = true;
								break;
							}

							this.fallbackState.Fallbacking = true;
							this.fallbackState.FallbackDepth = this.reader.Depth;
							return this.Read();
						}

						this.SkipContent();
						break;
				}
			}

			if (!this.fallbackState.FallbackProcessed)
			{
				throw new FatalResourceException(e);
			}

			this.fallbackState = this.prevFallbackState;
			return this.Read();
		}

		private bool ProcessIncludeElement()
		{
			string href = this.reader.GetAttribute(this.keywords.Href);
			string xpointer = this.reader.GetAttribute(this.keywords.Xpointer);
			string parse = this.reader.GetAttribute(this.keywords.Parse);

			if (string.IsNullOrEmpty(href))
			{
				// Intra-document inclusion                                                
				if (parse == null || parse.Equals(this.keywords.Xml))
				{
					if (xpointer == null)
					{
						// Both href and xpointer attributes are absent in xml mode, 
						// => critical error
						var li = this.reader as IXmlLineInfo;
						if (li != null && li.HasLineInfo())
						{
							throw new XIncludeSyntaxError(
								string.Format(
									CultureInfo.CurrentCulture, 
									Resources.MissingHrefAndXpointerExceptionLong, 
									this.reader.BaseURI, 
									li.LineNumber, 
									li.LinePosition));
						}

						throw new XIncludeSyntaxError(
							string.Format(CultureInfo.CurrentCulture, Resources.MissingHrefAndXpointerException, this.reader.BaseURI));
					}

					// No support for intra-document refs                    
					throw new InvalidOperationException(Resources.IntradocumentReferencesNotSupported);
				}

				if (parse.Equals(this.keywords.Text))
					throw new InvalidOperationException(Resources.IntradocumentReferencesNotSupported);
			}
			else
			{
				// Inter-document inclusion
				if (parse == null || parse.Equals(this.keywords.Xml))
					return this.ProcessInterDocXMLInclusion(href, xpointer);

				if (parse.Equals(this.keywords.Text))
					return this.ProcessInterDocTextInclusion(href);
			}

			// Unknown "parse" attribute value, critical error
			var li2 = this.reader as IXmlLineInfo;
			if (li2 != null && li2.HasLineInfo())
			{
				throw new XIncludeSyntaxError(
					string.Format(
						CultureInfo.CurrentCulture, 
						Resources.UnknownParseAttrValueLong, 
						parse, 
						this.reader.BaseURI, 
						li2.LineNumber, 
						li2.LinePosition));
			}

			throw new XIncludeSyntaxError(string.Format(CultureInfo.CurrentCulture, Resources.UnknownParseAttrValue, parse));
		}

		/// <summary>
		/// Process inter-document inclusion as text.
		/// </summary>
		/// <param name="href">'href' attr value</param>        
		private bool ProcessInterDocTextInclusion(string href)
		{
			// Include document as text                            
			string encoding = this.GetAttribute(this.keywords.Encoding);
			Uri includeLocation = this.ResolveHref(href);

			// No need to check loops when including as text
			// Push current reader to the stack
			this.readers.Push(this.reader);
			this.reader = new TextIncludingReader(
				includeLocation, 
				encoding, 
				this.reader.GetAttribute(this.keywords.Accept), 
				this.reader.GetAttribute(this.keywords.AcceptLanguage), 
				this.ExposeTextInclusionsAsCDATA);
			return this.Read();
		}

		/// <summary>
		/// Processes inter-document inclusion (xml mode).
		/// </summary>
		/// <param name="href">'href' attr value</param>
		/// <param name="xpointer">'xpointer attr value'</param>
		private bool ProcessInterDocXMLInclusion(string href, string xpointer)
		{
			// Include document as XML                                
			Uri includeLocation = this.ResolveHref(href);
			if (includeLocation.Fragment != string.Empty)
			{
				throw new XIncludeSyntaxError(Resources.FragmentIDInHref);
			}

			this.CheckLoops(includeLocation);
			if (this.xmlResolver == null)
			{
				// No custom resolver
				if (xpointer != null)
				{
					// Push current reader to the stack
					this.readers.Push(this.reader);

					// XPointers should be resolved against the acquired infoset, 
					// not the source infoset                                                                                          
					this.reader = new XPointerReader(includeLocation.AbsoluteUri, CreateAcquiredInfoset(includeLocation), xpointer);
				}
				else
				{
					WebResponse response;
					Stream stream = GetResource(
						includeLocation.AbsoluteUri, 
						this.reader.GetAttribute(this.keywords.Accept), 
						this.reader.GetAttribute(this.keywords.AcceptLanguage), 
						out response);

					// Push current reader to the stack
					this.readers.Push(this.reader);
					var settings = new XmlReaderSettings();
					settings.XmlResolver = this.xmlResolver;
					settings.IgnoreWhitespace = this.WhitespaceHandling == WhitespaceHandling.None;
					
					XmlReader r = new XmlBaseAwareXmlReader(response.ResponseUri.AbsoluteUri, stream, this.nameTable);
					this.reader = r;
				}

				return this.Read();
			}

			object resource;
			try
			{
				resource = this.xmlResolver.GetEntity(includeLocation, null, null);
			}
			catch (Exception e)
			{
				throw new ResourceException(Resources.CustomXmlResolverError, e);
			}

			if (resource == null)
			{
				throw new ResourceException(Resources.CustomXmlResolverReturnedNull);
			}

			// Push current reader to the stack
			this.readers.Push(this.reader);

			// Ok, we accept Stream, TextReader and XmlReader only                    
			if (resource is Stream)
			{
				resource = new StreamReader((Stream)resource);
			}

			if (xpointer != null)
			{
				if (resource is TextReader)
				{
					// XPointers should be resolved against the acquired infoset, 
					// not the source infoset                                     
					this.reader = new XPointerReader(
						includeLocation.AbsoluteUri, this.CreateAcquiredInfoset(includeLocation, (TextReader)resource), xpointer);
				}
				else if (resource is XmlReader)
				{
					var r = (XmlReader)resource;
					this.reader = new XPointerReader(r.BaseURI, this.CreateAcquiredInfoset(r), xpointer);
				}
				else
				{
					// Unsupported type
					throw new ResourceException(
						string.Format(CultureInfo.CurrentCulture, Resources.CustomXmlResolverReturnedUnsupportedType, resource.GetType()));
				}
			}
			else
			{
				// No XPointer   
				if (resource is TextReader)
				{
					this.reader = new XmlBaseAwareXmlReader(includeLocation.AbsoluteUri, (TextReader)resource, this.nameTable);
				}
				else if (resource is XmlReader)
				{
					this.reader = (XmlReader)resource;
				}
				else
				{
					// Unsupported type
					throw new ResourceException(
						string.Format(CultureInfo.CurrentCulture, Resources.CustomXmlResolverReturnedUnsupportedType, resource.GetType()));
				}
			}

			return this.Read();
		}

		/// <summary>
		/// Resolves include location.
		/// </summary>
		/// <param name="href">href value</param>
		/// <returns>Include location.</returns>
		private Uri ResolveHref(string href)
		{
			Uri includeLocation;
			try
			{
				Uri baseURI = this.reader.BaseURI == string.Empty ? this.topBaseUri : new Uri(this.reader.BaseURI);
				includeLocation = this.xmlResolver == null ? new Uri(baseURI, href) : this.xmlResolver.ResolveUri(baseURI, href);
			}
			catch (UriFormatException ufe)
			{
				throw new ResourceException(string.Format(CultureInfo.CurrentCulture, Resources.InvalidURI, href), ufe);
			}
			catch (Exception e)
			{
				throw new ResourceException(string.Format(CultureInfo.CurrentCulture, Resources.UnresolvableURI, href), e);
			}

			return includeLocation;
		}

		/// <summary>
		/// Skips content of an element using directly current reader's methods.
		/// </summary>
		private void SkipContent()
		{
			if (!this.reader.IsEmptyElement)
			{
				int depth = this.reader.Depth;
				while (this.reader.Read() && depth < this.reader.Depth)
				{
				}
			}
		}

		/// <summary>
		/// Throws CircularInclusionException.
		/// </summary>        
		private void ThrowCircularInclusionError(XmlReader reader, Uri url)
		{
			var li = reader as IXmlLineInfo;
			if (li != null && li.HasLineInfo())
			{
				throw new CircularInclusionException(url, this.BaseURI, li.LineNumber, li.LinePosition);
			}

			throw new CircularInclusionException(url);
		}
	}
}