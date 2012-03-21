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
namespace Kelp.XInclude.Common
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Xml;

	/// <summary>
	/// Base <see cref="XmlReader"/> that can be use to create new readers by 
	/// wrapping existing ones.
	/// </summary>
	/// <remarks>
	/// Supports <see cref="IXmlLineInfo"/> if the underlying reader supports it.
	/// <para>Author: Daniel Cazzulino, <a href="http://clariusconsulting.net/kzu">blog</a>.</para>
	/// </remarks>
	public abstract class XmlWrappingReader : XmlReader, IXmlLineInfo
	{
		private XmlReader baseReader;

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlWrappingReader"/>.
		/// </summary>
		/// <param name="baseReader">The underlying reader this instance will wrap.</param>
		protected XmlWrappingReader(XmlReader baseReader)
		{
			Contract.Requires<ArgumentNullException>(baseReader != null);

			this.baseReader = baseReader;
		}

		/// <summary>
		/// See <see cref="XmlReader.AttributeCount"/>.
		/// </summary>
		public override int AttributeCount
		{
			get
			{
				return this.baseReader.AttributeCount;
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.BaseURI"/>.
		/// </summary>
		public override string BaseURI
		{
			get
			{
				return this.baseReader.BaseURI;
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.CanReadBinaryContent"/>.
		/// </summary>
		public override bool CanReadBinaryContent
		{
			get
			{
				return this.baseReader.CanReadBinaryContent;
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.CanReadValueChunk"/>.
		/// </summary>
		public override bool CanReadValueChunk
		{
			get
			{
				return this.baseReader.CanReadValueChunk;
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.CanResolveEntity"/>.
		/// </summary>
		public override bool CanResolveEntity
		{
			get
			{
				return this.baseReader.CanResolveEntity;
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.Depth"/>.
		/// </summary>
		public override int Depth
		{
			get
			{
				return this.baseReader.Depth;
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.EOF"/>.
		/// </summary>
		public override bool EOF
		{
			get
			{
				return this.baseReader.EOF;
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.HasValue"/>.
		/// </summary>
		public override bool HasValue
		{
			get
			{
				return this.baseReader.HasValue;
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.IsDefault"/>.
		/// </summary>
		public override bool IsDefault
		{
			get
			{
				return this.baseReader.IsDefault;
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.IsEmptyElement"/>.
		/// </summary>
		public override bool IsEmptyElement
		{
			get
			{
				return this.baseReader.IsEmptyElement;
			}
		}

		/// <summary>
		/// See <see cref="IXmlLineInfo.LineNumber"/>.
		/// </summary>
		public int LineNumber
		{
			get
			{
				var info = this.baseReader as IXmlLineInfo;
				if (info != null)
				{
					return info.LineNumber;
				}

				return 0;
			}
		}

		/// <summary>
		/// See <see cref="IXmlLineInfo.LinePosition"/>.
		/// </summary>
		public int LinePosition
		{
			get
			{
				var info = this.baseReader as IXmlLineInfo;
				if (info != null)
				{
					return info.LinePosition;
				}

				return 0;
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.LocalName"/>.
		/// </summary>
		public override string LocalName
		{
			get
			{
				return this.baseReader.LocalName;
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.Name"/>.
		/// </summary>
		public override string Name
		{
			get
			{
				return this.baseReader.Name;
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.NameTable"/>.
		/// </summary>
		public override XmlNameTable NameTable
		{
			get
			{
				return this.baseReader.NameTable;
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.NamespaceURI"/>.
		/// </summary>
		public override string NamespaceURI
		{
			get
			{
				return this.baseReader.NamespaceURI;
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.NodeType"/>.
		/// </summary>
		public override XmlNodeType NodeType
		{
			get
			{
				return this.baseReader.NodeType;
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.Prefix"/>.
		/// </summary>
		public override string Prefix
		{
			get
			{
				return this.baseReader.Prefix;
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.QuoteChar"/>.
		/// </summary>
		public override char QuoteChar
		{
			get
			{
				return this.baseReader.QuoteChar;
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.ReadState"/>.
		/// </summary>
		public override ReadState ReadState
		{
			get
			{
				return this.baseReader.ReadState;
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.Value"/>.
		/// </summary>
		public override string Value
		{
			get
			{
				return this.baseReader.Value;
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.XmlLang"/>.
		/// </summary>
		public override string XmlLang
		{
			get
			{
				return this.baseReader.XmlLang;
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.XmlSpace"/>.
		/// </summary>
		public override XmlSpace XmlSpace
		{
			get
			{
				return this.baseReader.XmlSpace;
			}
		}

		/// <summary>
		/// Gets or sets the underlying reader this instance is wrapping.
		/// </summary>
		protected XmlReader BaseReader
		{
			get
			{
				return this.baseReader;
			}

			set
			{
				Contract.Requires<ArgumentNullException>(value != null);
				this.baseReader = value;
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.this[int]"/>.
		/// </summary>
		public override string this[int i]
		{
			get
			{
				return this.baseReader[i];
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.this[string]"/>.
		/// </summary>
		public override string this[string name]
		{
			get
			{
				return this.baseReader[name];
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.this[string, string]"/>.
		/// </summary>
		public override string this[string name, string namespaceURI]
		{
			get
			{
				return this.baseReader[name, namespaceURI];
			}
		}

		/// <summary>
		/// See <see cref="XmlReader.Close"/>.
		/// </summary>
		public override void Close()
		{
			this.baseReader.Close();
		}

		/// <summary>
		/// See <see cref="XmlReader.GetAttribute(int)"/>.
		/// </summary>
		public override string GetAttribute(int i)
		{
			return this.baseReader.GetAttribute(i);
		}

		/// <summary>
		/// See <see cref="XmlReader.GetAttribute(string)"/>.
		/// </summary>
		public override string GetAttribute(string name)
		{
			return this.baseReader.GetAttribute(name);
		}

		/// <summary>
		/// See <see cref="XmlReader.GetAttribute(string, string)"/>.
		/// </summary>
		public override string GetAttribute(string localName, string namespaceURI)
		{
			{
				return this.baseReader.GetAttribute(localName, namespaceURI);
			}
		}

		/// <summary>
		/// See <see cref="IXmlLineInfo.HasLineInfo"/>.
		/// </summary>
		public bool HasLineInfo()
		{
			var info = this.baseReader as IXmlLineInfo;
			if (info != null)
			{
				return info.HasLineInfo();
			}

			return false;
		}

		/// <summary>
		/// See <see cref="XmlReader.LookupNamespace"/>.
		/// </summary>
		public override string LookupNamespace(string prefix)
		{
			return this.baseReader.LookupNamespace(prefix);
		}

		/// <summary>
		/// See <see cref="XmlReader.MoveToAttribute(int)"/>.
		/// </summary>
		public override void MoveToAttribute(int i)
		{
			this.baseReader.MoveToAttribute(i);
		}

		/// <summary>
		/// See <see cref="XmlReader.MoveToAttribute(string)"/>.
		/// </summary>
		public override bool MoveToAttribute(string name)
		{
			return this.baseReader.MoveToAttribute(name);
		}

		/// <summary>
		/// See <see cref="XmlReader.MoveToAttribute(string, string)"/>.
		/// </summary>
		public override bool MoveToAttribute(string localName, string namespaceURI)
		{
			return this.baseReader.MoveToAttribute(localName, namespaceURI);
		}

		/// <summary>
		/// See <see cref="XmlReader.MoveToElement"/>.
		/// </summary>
		public override bool MoveToElement()
		{
			return this.baseReader.MoveToElement();
		}

		/// <summary>
		/// See <see cref="XmlReader.MoveToFirstAttribute"/>.
		/// </summary>
		public override bool MoveToFirstAttribute()
		{
			return this.baseReader.MoveToFirstAttribute();
		}

		/// <summary>
		/// See <see cref="XmlReader.MoveToNextAttribute"/>.
		/// </summary>
		public override bool MoveToNextAttribute()
		{
			return this.baseReader.MoveToNextAttribute();
		}

		/// <summary>
		/// See <see cref="XmlReader.Read"/>.
		/// </summary>
		public override bool Read()
		{
			return this.baseReader.Read();
		}

		/// <summary>
		/// See <see cref="XmlReader.ReadAttributeValue"/>.
		/// </summary>
		public override bool ReadAttributeValue()
		{
			return this.baseReader.ReadAttributeValue();
		}

		/// <summary>
		/// See <see cref="XmlReader.ReadValueChunk"/>.
		/// </summary>
		public override int ReadValueChunk(char[] buffer, int index, int count)
		{
			return this.baseReader.ReadValueChunk(buffer, index, count);
		}

		/// <summary>
		/// See <see cref="XmlReader.ResolveEntity"/>.
		/// </summary>
		public override void ResolveEntity()
		{
			this.baseReader.ResolveEntity();
		}

		/// <summary>
		/// See <see cref="XmlReader.Dispose"/>.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (this.ReadState != ReadState.Closed)
			{
				this.Close();
			}

			((IDisposable)this.baseReader).Dispose();
		}
	}
}