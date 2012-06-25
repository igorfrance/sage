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
namespace Kelp.XInclude.XPointer
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Xml;
	using System.Xml.XPath;

	using Kelp.XInclude.Common;
	using Kelp.XInclude.XPath;

	/// <summary>
	/// Implements XPointer Framework in a fast, non-caching, forward-only way.
	/// </summary>
	/// <remarks>
	/// Supports XPointer Framework, element(), xmlns(), xpath1() and
	/// xpointer() (XPath subset only) XPointer schemes.
	/// </remarks>
	public class XPointerReader : XmlReader, IHasXPathNavigator, IXmlLineInfo
	{
		private static IDictionary<string, WeakReference> cache;
		private XPathNodeIterator pointedNodes;
		private XmlReader reader;

		/// <summary>
		/// Creates <c>XPointerReader</c> instnace with given <see cref="IXPathNavigable"/>
		/// and xpointer.
		/// </summary>        
		public XPointerReader(IXPathNavigable doc, string xpointer)
			: this(doc.CreateNavigator(), xpointer)
		{
		}

		/// <summary>
		/// Creates <c>XPointerReader</c> instnace with given <see cref="XPathNavigator"/>
		/// and xpointer.
		/// </summary>        
		public XPointerReader(XPathNavigator nav, string xpointer)
		{
			this.Init(nav, xpointer);
		}

		/// <summary>
		/// Creates <c>XPointerReader</c> instance with given uri and xpointer.
		/// </summary>	    
		public XPointerReader(string uri, string xpointer)
			: this(new XmlBaseAwareXmlReader(uri), xpointer)
		{
		}

		/// <summary>
		/// Creates <c>XPointerReader</c> instance with given uri, nametable and xpointer.
		/// </summary>	    
		public XPointerReader(string uri, XmlNameTable nt, string xpointer)
			: this(new XmlBaseAwareXmlReader(uri, nt), xpointer)
		{
		}

		/// <summary>
		/// Creates <c>XPointerReader</c> instance with given uri, stream, nametable and xpointer.
		/// </summary>	    
		public XPointerReader(string uri, Stream stream, XmlNameTable nt, string xpointer)
			: this(new XmlBaseAwareXmlReader(uri, stream, nt), xpointer)
		{
		}

		/// <summary>
		/// Creates <c>XPointerReader</c> instance with given uri, stream and xpointer.
		/// </summary>	    
		public XPointerReader(string uri, Stream stream, string xpointer)
			: this(uri, stream, new NameTable(), xpointer)
		{
		}

		/// <summary>
		/// Creates <c>XPointerReader</c> instance with given XmlReader and xpointer.
		/// Additionally sets a flag whether to support schema-determined IDs.
		/// </summary>	    
		public XPointerReader(XmlReader reader, string xpointer)
		{
			XPathDocument doc;
			if (cache == null)
			{
				cache = new Dictionary<string, WeakReference>();
			}

			WeakReference wr;
			if (!string.IsNullOrEmpty(reader.BaseURI) && cache.TryGetValue(reader.BaseURI, out wr) && wr.IsAlive)
			{
				doc = (XPathDocument)wr.Target;
				reader.Close();
			}
			else
			{
				// Not cached or GCollected or no base Uri                
				doc = this.CreateAndCacheDocument(reader);
			}

			this.Init(doc.CreateNavigator(), xpointer);
		}

		/// <summary>
		/// Creates <c>XPointerReader</c> instance with given
		/// document's URI and content.
		/// </summary>
		/// <param name="uri">XML document's base URI</param>
		/// <param name="content">XML document's content</param>
		/// <param name="xpointer">XPointer pointer</param>        
		public XPointerReader(string uri, string content, string xpointer)
		{
			XPathDocument doc;
			if (cache == null)
			{
				cache = new Dictionary<string, WeakReference>();
			}

			WeakReference wr;
			if (cache.TryGetValue(uri, out wr) && wr.IsAlive)
			{
				doc = (XPathDocument)wr.Target;
			}
			else
			{
				// Not cached or GCollected                        
				// XmlReader r = new XmlBaseAwareXmlReader(uri, new StringReader(content));
				var settings = new XmlReaderSettings();
				settings.DtdProcessing = DtdProcessing.Prohibit;
				settings.IgnoreProcessingInstructions = true;
				XmlReader r = Create(new StringReader(content), settings, uri);
				doc = this.CreateAndCacheDocument(r);
			}

			this.Init(doc.CreateNavigator(), xpointer);
		}

		/// <summary>See <see cref="XmlReader.AttributeCount"/>.</summary>
		public override int AttributeCount
		{
			get
			{
				return this.reader.AttributeCount;
			}
		}

		/// <summary>See <see cref="XmlReader.BaseURI"/>.</summary>
		public override string BaseURI
		{
			get
			{
				return this.reader.BaseURI;
			}
		}

		/// <summary>See <see cref="XmlReader.Depth"/>.</summary>
		public override int Depth
		{
			get
			{
				return this.reader.Depth;
			}
		}

		/// <summary>See <see cref="XmlReader.EOF"/>.</summary>
		public override bool EOF
		{
			get
			{
				return this.reader.EOF;
			}
		}

		/// <summary>See <see cref="XmlReader.HasValue"/>.</summary>
		public override bool HasValue
		{
			get
			{
				return this.reader.HasValue;
			}
		}

		/// <summary>See <see cref="XmlReader.IsDefault"/>.</summary>
		public override bool IsDefault
		{
			get
			{
				return this.reader.IsDefault;
			}
		}

		/// <summary>See <see cref="XmlReader.IsEmptyElement"/>.</summary>
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

		/// <summary>See <see cref="XmlReader.LocalName"/>.</summary>
		public override string LocalName
		{
			get
			{
				return this.reader.LocalName;
			}
		}

		/// <summary>See <see cref="XmlReader.Name"/>.</summary>
		public override string Name
		{
			get
			{
				return this.reader.Name;
			}
		}

		/// <summary>See <see cref="XmlReader.NameTable"/>.</summary>
		public override XmlNameTable NameTable
		{
			get
			{
				return this.reader.NameTable;
			}
		}

		/// <summary>See <see cref="XmlReader.NamespaceURI"/>.</summary>
		public override string NamespaceURI
		{
			get
			{
				return this.reader.NamespaceURI;
			}
		}

		/// <summary>See <see cref="XmlReader.NodeType"/>.</summary>
		public override XmlNodeType NodeType
		{
			get
			{
				return this.reader.NodeType;
			}
		}

		/// <summary>See <see cref="XmlReader.Prefix"/>.</summary>
		public override string Prefix
		{
			get
			{
				return this.reader.Prefix;
			}
		}

		/// <summary>See <see cref="XmlReader.QuoteChar"/>.</summary>
		public override char QuoteChar
		{
			get
			{
				return this.reader.QuoteChar;
			}
		}

		/// <summary>See <see cref="XmlReader.ReadState"/>.</summary>            
		public override ReadState ReadState
		{
			get
			{
				return this.reader.ReadState;
			}
		}

		/// <summary>See <see cref="XmlReader.Value"/>.</summary>
		public override string Value
		{
			get
			{
				return this.reader.Value;
			}
		}

		/// <summary>See <see cref="XmlReader.XmlLang"/>.</summary>
		public override string XmlLang
		{
			get
			{
				return this.reader.XmlLang;
			}
		}

		/// <summary>See <see cref="XmlReader.XmlSpace"/>.</summary>
		public override XmlSpace XmlSpace
		{
			get
			{
				return this.reader.XmlSpace;
			}
		}

		/// <summary>See <see cref="XmlReader.this[int]"/>.</summary>
		public override string this[int i]
		{
			get
			{
				return this.reader[i];
			}
		}

		/// <summary>See <see cref="XmlReader.this[string]"/>.</summary>
		public override string this[string name]
		{
			get
			{
				return this.reader[name];
			}
		}

		/// <summary>See <see cref="XmlReader.this[string, string]"/>.</summary>
		public override string this[string name, string namespaceURI]
		{
			get
			{
				return this.reader[name, namespaceURI];
			}
		}

		/// <summary>See <see cref="XmlReader.Close"/>.</summary>
		public override void Close()
		{
			if (this.reader != null)
			{
				this.reader.Close();
			}
		}

		/// <summary>See <see cref="XmlReader.GetAttribute(int)"/>.</summary>
		public override string GetAttribute(int i)
		{
			return this.reader.GetAttribute(i);
		}

		/// <summary>See <see cref="XmlReader.GetAttribute(string)"/>.</summary>
		public override string GetAttribute(string name)
		{
			return this.reader.GetAttribute(name);
		}

		/// <summary>See <see cref="XmlReader.GetAttribute(string, string)"/>.</summary>
		public override string GetAttribute(string name, string namespaceURI)
		{
			return this.reader.GetAttribute(name, namespaceURI);
		}

		/// <summary>
		/// Returns the XPathNavigator for the current context or position.
		/// </summary>
		/// <returns></returns>
		public XPathNavigator GetNavigator()
		{
			return this.pointedNodes.Current.Clone();
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

		/// <summary>See <see cref="XmlReader.LookupNamespace"/>.</summary>
		public override string LookupNamespace(string prefix)
		{
			return this.reader.LookupNamespace(prefix);
		}

		/// <summary>See <see cref="XmlReader.MoveToAttribute(int)"/>.</summary>
		public override void MoveToAttribute(int i)
		{
			this.reader.MoveToAttribute(i);
		}

		/// <summary>See <see cref="XmlReader.MoveToAttribute(string)"/>.</summary>
		public override bool MoveToAttribute(string name)
		{
			return this.reader.MoveToAttribute(name);
		}

		/// <summary>See <see cref="XmlReader.MoveToAttribute(string, string)"/>.</summary>
		public override bool MoveToAttribute(string name, string ns)
		{
			return this.reader.MoveToAttribute(name, ns);
		}

		/// <summary>See <see cref="XmlReader.MoveToElement"/>.</summary>
		public override bool MoveToElement()
		{
			return this.reader.MoveToElement();
		}

		/// <summary>See <see cref="XmlReader.MoveToFirstAttribute"/>.</summary>
		public override bool MoveToFirstAttribute()
		{
			return this.reader.MoveToFirstAttribute();
		}

		/// <summary>See <see cref="XmlReader.MoveToNextAttribute"/>.</summary>
		public override bool MoveToNextAttribute()
		{
			return this.reader.MoveToNextAttribute();
		}

		/// <summary>See <see cref="XmlReader.Read"/>.</summary>
		public override bool Read()
		{
			bool baseRead = this.reader.Read();
			if (baseRead)
			{
				return true;
			}
			
			if (this.pointedNodes != null)
			{
				if (this.pointedNodes.MoveNext())
				{
					this.reader = new SubtreeXPathNavigator(this.pointedNodes.Current).ReadSubtree();
					return this.reader.Read();
				}
			}

			return false;
		}

		/// <summary>See <see cref="XmlReader.ReadAttributeValue"/>.</summary>
		public override bool ReadAttributeValue()
		{
			return this.reader.ReadAttributeValue();
		}

		/// <summary>See <see cref="XmlReader.ReadInnerXml"/>.</summary>
		public override string ReadInnerXml()
		{
			return this.reader.ReadInnerXml();
		}

		/// <summary>See <see cref="XmlReader.ReadOuterXml"/>.</summary>
		public override string ReadOuterXml()
		{
			return this.reader.ReadOuterXml();
		}

		/// <summary>See <see cref="XmlReader.ReadString"/>.</summary>
		public override string ReadString()
		{
			return this.reader.ReadString();
		}

		/// <summary>See <see cref="XmlReader.ResolveEntity"/>.</summary>
		public override void ResolveEntity()
		{
			this.reader.ResolveEntity();
		}

		private XPathDocument CreateAndCacheDocument(XmlReader r)
		{
			string uri = r.BaseURI;
			var doc = new XPathDocument(r, XmlSpace.Preserve);
			r.Close();

			// Can't cache documents with empty base URI
			if (!string.IsNullOrEmpty(uri))
			{
				lock (cache)
				{
					if (!cache.ContainsKey(uri))
					{
						cache.Add(uri, new WeakReference(doc));
					}
				}
			}

			return doc;
		}

		/// <summary>
		/// Initializes the <c>XPointerReader</c>.
		/// </summary>
		private void Init(XPathNavigator nav, string xpointer)
		{
			Pointer pointer = XPointerParser.ParseXPointer(xpointer);
			this.pointedNodes = pointer.Evaluate(nav);

			// There is always at least one identified node
			// XPathNodeIterator is already at the first node
			this.reader = new SubtreeXPathNavigator(this.pointedNodes.Current).ReadSubtree();
		}
	}
}