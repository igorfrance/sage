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
namespace Kelp.XInclude.Common
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Xml;

	/// <summary>
	/// Custom <see cref="XmlReader"/> supporting <a href="http://www.w3.org/TR/xmlbase/">XML Base</a>.
	/// </summary>
	/// <remarks>
	/// <para>Author: Oleg Tkachenko, <a href="http://www.xmllab.net">http://www.xmllab.net</a>.</para>
	/// </remarks>
	public class XmlBaseAwareXmlReader : XmlWrappingReader
	{
		private XmlBaseState state = new XmlBaseState();
		private Stack<XmlBaseState> states;

		/// <summary>
		/// Creates XmlBaseAwareXmlReader instance for given URI.
		/// </summary>        
		public XmlBaseAwareXmlReader(string uri)
			: base(Create(uri, CreateReaderSettings()))
		{
			this.state.BaseUri = new Uri(base.BaseURI);
		}

		/// <summary>
		/// Creates XmlBaseAwareXmlReader instance for given URI using the given resolver.
		/// </summary>        
		public XmlBaseAwareXmlReader(string uri, XmlResolver resolver)
			: base(Create(uri, CreateReaderSettings(resolver)))
		{
			this.state.BaseUri = new Uri(base.BaseURI);
		}

		/// <summary>
		/// Creates XmlBaseAwareXmlReader instance for given URI and 
		/// name table.
		/// </summary>        
		public XmlBaseAwareXmlReader(string uri, XmlNameTable nt)
			: base(Create(uri, CreateReaderSettings(nt)))
		{
			this.state.BaseUri = new Uri(base.BaseURI);
		}

		/// <summary>
		/// Creates XmlBaseAwareXmlReader instance for given TextReader.
		/// </summary>        
		public XmlBaseAwareXmlReader(TextReader reader)
			: base(Create(reader, CreateReaderSettings()))
		{
		}

		/// <summary>
		/// Creates XmlBaseAwareXmlReader instance for given uri and 
		/// TextReader.
		/// </summary>        
		public XmlBaseAwareXmlReader(string uri, TextReader reader)
			: base(Create(reader, CreateReaderSettings(), uri))
		{
			this.state.BaseUri = new Uri(base.BaseURI);
		}

		/// <summary>
		/// Creates XmlBaseAwareXmlReader instance for given TextReader 
		/// and name table.
		/// </summary>        
		public XmlBaseAwareXmlReader(TextReader reader, XmlNameTable nt)
			: base(Create(reader, CreateReaderSettings(nt)))
		{
		}

		/// <summary>
		/// Creates XmlBaseAwareXmlReader instance for given uri, name table
		/// and TextReader.
		/// </summary>        
		public XmlBaseAwareXmlReader(string uri, TextReader reader, XmlNameTable nt)
			: base(Create(reader, CreateReaderSettings(nt), uri))
		{
			this.state.BaseUri = new Uri(base.BaseURI);
		}

		/// <summary>
		/// Creates XmlBaseAwareXmlReader instance for given stream.
		/// </summary>        
		public XmlBaseAwareXmlReader(Stream stream)
			: base(Create(stream, CreateReaderSettings()))
		{
		}

		/// <summary>
		/// Creates XmlBaseAwareXmlReader instance for given uri and stream.
		/// </summary>        
		public XmlBaseAwareXmlReader(string uri, Stream stream)
			: base(Create(stream, CreateReaderSettings(), uri))
		{
			this.state.BaseUri = new Uri(base.BaseURI);
		}

		/// <summary>
		/// Creates XmlBaseAwareXmlReader instance for given uri and stream.
		/// </summary>        
		public XmlBaseAwareXmlReader(string uri, Stream stream, XmlResolver resolver)
			: base(Create(stream, CreateReaderSettings(resolver), uri))
		{
			this.state.BaseUri = new Uri(base.BaseURI);
		}

		/// <summary>
		/// Creates XmlBaseAwareXmlReader instance for given stream 
		/// and name table.
		/// </summary>        
		public XmlBaseAwareXmlReader(Stream stream, XmlNameTable nt)
			: base(Create(stream, CreateReaderSettings(nt)))
		{
		}

		/// <summary>
		/// Creates XmlBaseAwareXmlReader instance for given stream,
		/// uri and name table.
		/// </summary>        
		public XmlBaseAwareXmlReader(string uri, Stream stream, XmlNameTable nt)
			: base(Create(stream, CreateReaderSettings(nt), uri))
		{
			this.state.BaseUri = new Uri(base.BaseURI);
		}

		/// <summary>
		/// Creates XmlBaseAwareXmlReader instance for given uri and <see cref="XmlReaderSettings"/>.        
		/// </summary>        
		public XmlBaseAwareXmlReader(string uri, XmlReaderSettings settings)
			: base(Create(uri, settings))
		{
		}

		/// <summary>
		/// Creates XmlBaseAwareXmlReader instance for given <see cref="TextReader"/> and <see cref="XmlReaderSettings"/>.        
		/// </summary>        
		public XmlBaseAwareXmlReader(TextReader reader, XmlReaderSettings settings)
			: base(Create(reader, settings))
		{
		}

		/// <summary>
		/// Creates XmlBaseAwareXmlReader instance for given <see cref="Stream"/> and <see cref="XmlReaderSettings"/>.        
		/// </summary>        
		public XmlBaseAwareXmlReader(Stream stream, XmlReaderSettings settings)
			: base(Create(stream, settings))
		{
		}

		/// <summary>
		/// Creates XmlBaseAwareXmlReader instance for given <see cref="XmlReader"/> and <see cref="XmlReaderSettings"/>.        
		/// </summary>        
		public XmlBaseAwareXmlReader(XmlReader reader, XmlReaderSettings settings)
			: base(Create(reader, settings))
		{
		}

		/// <summary>
		/// Creates XmlBaseAwareXmlReader instance for given 
		/// <see cref="TextReader"/>, <see cref="XmlReaderSettings"/>
		/// and base uri.
		/// </summary>        
		public XmlBaseAwareXmlReader(TextReader reader, XmlReaderSettings settings, string baseUri)
			: base(Create(reader, settings, baseUri))
		{
		}

		/// <summary>
		/// Creates XmlBaseAwareXmlReader instance for given 
		/// <see cref="Stream"/>, <see cref="XmlReaderSettings"/>
		/// and base uri.
		/// </summary>        
		public XmlBaseAwareXmlReader(Stream stream, XmlReaderSettings settings, string baseUri)
			: base(Create(stream, settings, baseUri))
		{
		}

		/// <summary>
		/// See <see cref="XmlTextReader.BaseURI"/>.
		/// </summary>
		public override string BaseURI
		{
			get
			{
				return this.state.BaseUri == null ? string.Empty : this.state.BaseUri.AbsoluteUri;
			}
		}

		/// <summary>
		/// See <see cref="XmlTextReader.Read"/>.
		/// </summary>
		public override bool Read()
		{
			bool baseRead = base.Read();
			if (baseRead)
			{
				if (this.NodeType == XmlNodeType.Element && this.HasAttributes)
				{
					string baseAttr = this.GetAttribute("xml:base");
					if (baseAttr == null)
					{
						return true;
					}

					Uri newBaseUri = this.state.BaseUri == null 
						? new Uri(baseAttr) 
						: new Uri(this.state.BaseUri, baseAttr);

					if (this.states == null)
					{
						this.states = new Stack<XmlBaseState>();
					}

					// Push current state and allocate new one
					this.states.Push(this.state);
					this.state = new XmlBaseState(newBaseUri, this.Depth);
				}
				else if (this.NodeType == XmlNodeType.EndElement)
				{
					if (this.Depth == this.state.Depth && this.states != null && this.states.Count > 0)
					{
						// Pop previous state
						this.state = this.states.Pop();
					}
				}
			}

			return baseRead;
		}

		private static XmlReaderSettings CreateReaderSettings()
		{
			var settings = new XmlReaderSettings();
			settings.DtdProcessing = DtdProcessing.Prohibit;
			return settings;
		}

		private static XmlReaderSettings CreateReaderSettings(XmlResolver resolver)
		{
			XmlReaderSettings settings = CreateReaderSettings();
			settings.XmlResolver = resolver;
			return settings;
		}

		private static XmlReaderSettings CreateReaderSettings(XmlNameTable nt)
		{
			XmlReaderSettings settings = CreateReaderSettings();
			settings.NameTable = nt;
			return settings;
		}
	}
}