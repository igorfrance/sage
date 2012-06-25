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
	using System.Globalization;
	using System.IO;
	using System.Net;
	using System.Text;
	using System.Xml;

	using Kelp.Properties;

	internal class TextIncludingReader : XmlReader
	{
		private readonly string accept;
		private readonly string acceptLanguage;
		private readonly string encoding;
		private readonly bool exposeCDATA;
		private readonly string href;
		private readonly Uri includeLocation;
		private ReadState state;
		private string value;

		public TextIncludingReader(Uri includeLocation, string encoding, string accept, string acceptLanguage, bool exposeCDATA)
		{
			this.includeLocation = includeLocation;
			this.href = includeLocation.AbsoluteUri;
			this.encoding = encoding;
			this.state = ReadState.Initial;
			this.accept = accept;
			this.acceptLanguage = acceptLanguage;
			this.exposeCDATA = exposeCDATA;
		}

		public TextIncludingReader(string value, bool exposeCDATA)
		{
			this.state = ReadState.Initial;
			this.exposeCDATA = exposeCDATA;
			this.value = value;
		}

		public override int AttributeCount
		{
			get
			{
				return 0;
			}
		}

		public override string BaseURI
		{
			get
			{
				return this.href;
			}
		}

		public override int Depth
		{
			get
			{
				return this.state == ReadState.Interactive ? 1 : 0;
			}
		}

		public override bool EOF
		{
			get
			{
				return this.state == ReadState.EndOfFile;
			}
		}

		public override bool HasValue
		{
			get
			{
				return this.state == ReadState.Interactive;
			}
		}

		public override bool IsDefault
		{
			get
			{
				return false;
			}
		}

		public override bool IsEmptyElement
		{
			get
			{
				return false;
			}
		}

		public override string LocalName
		{
			get
			{
				return string.Empty;
			}
		}

		public override string Name
		{
			get
			{
				return string.Empty;
			}
		}

		public override XmlNameTable NameTable
		{
			get
			{
				return null;
			}
		}

		public override string NamespaceURI
		{
			get
			{
				return string.Empty;
			}
		}

		public override XmlNodeType NodeType
		{
			get
			{
				return this.state == ReadState.Interactive ? this.exposeCDATA ? XmlNodeType.CDATA : XmlNodeType.Text : XmlNodeType.None;
			}
		}

		public override string Prefix
		{
			get
			{
				return string.Empty;
			}
		}

		public override char QuoteChar
		{
			get
			{
				return '"';
			}
		}

		public override ReadState ReadState
		{
			get
			{
				return this.state;
			}
		}

		public override string Value
		{
			get
			{
				return this.state == ReadState.Interactive ? this.value : string.Empty;
			}
		}

		public override string XmlLang
		{
			get
			{
				return string.Empty;
			}
		}

		public override XmlSpace XmlSpace
		{
			get
			{
				return XmlSpace.None;
			}
		}

		public override string this[int index]
		{
			get
			{
				return string.Empty;
			}
		}

		public override string this[string qname]
		{
			get
			{
				return string.Empty;
			}
		}

		public override string this[string localname, string nsuri]
		{
			get
			{
				return string.Empty;
			}
		}

		public override void Close()
		{
			this.state = ReadState.Closed;
		}

		public override string GetAttribute(int index)
		{
			throw new ArgumentOutOfRangeException("index", index, Resources.NoAttributesExposed);
		}

		public override string GetAttribute(string qname)
		{
			return null;
		}

		public override string GetAttribute(string localname, string nsuri)
		{
			return null;
		}

		public override string LookupNamespace(string prefix)
		{
			return null;
		}

		public override void MoveToAttribute(int index)
		{
		}

		public override bool MoveToAttribute(string qname)
		{
			return false;
		}

		public override bool MoveToAttribute(string localname, string nsuri)
		{
			return false;
		}

		public override bool MoveToElement()
		{
			return false;
		}

		public override bool MoveToFirstAttribute()
		{
			return false;
		}

		public override bool MoveToNextAttribute()
		{
			return false;
		}

		public override bool Read()
		{
			switch (this.state)
			{
				case ReadState.Initial:
					if (this.value == null)
					{
						WebResponse response;
						Stream stream = XIncludingReader.GetResource(
							this.includeLocation.AbsoluteUri, this.accept, this.acceptLanguage, out response);

						/* According to the spec, encoding should be determined as follows:
							* external encoding information, if available, otherwise
							* if the media type of the resource is text/xml, application/xml, 
							  or matches the conventions text/*+xml or application/*+xml as 
							  described in XML Media Types [IETF RFC 3023], the encoding is 
							  recognized as specified in XML 1.0, otherwise
							* the value of the encoding attribute if one exists, otherwise  
							* UTF-8.
						*/
						try
						{
							// TODO: try to get "content-encoding" from wRes.Headers collection?
							// If mime type is xml-aware, get resource encoding as per XML 1.0
							string contentType = response.ContentType.ToLower();
							StreamReader reader;
							if (contentType != "text/xml" && contentType != "application/xml"
							    && (!contentType.StartsWith("text/") || !contentType.EndsWith("+xml"))
							    && (!contentType.StartsWith("application/") || !contentType.EndsWith("+xml")))
							{
								if (this.encoding != null)
								{
									// Try to use user-specified encoding
									Encoding enc;
									try
									{
										enc = Encoding.GetEncoding(this.encoding);
									}
									catch (Exception e)
									{
										throw new ResourceException(string.Format(CultureInfo.CurrentCulture, Resources.NotSupportedEncoding, this.encoding), e);
									}

									reader = new StreamReader(stream, enc);
								}
								else
								{
									// Fallback to UTF-8
									reader = new StreamReader(stream, Encoding.UTF8);
								}
							}
							else
							{
								// Yes, that's xml, let's read encoding from the xml declaration                    
								reader = new StreamReader(stream, GetEncodingFromXMLDecl(this.href));
							}

							this.value = reader.ReadToEnd();
							TextUtils.CheckForNonXmlChars(this.value);
						}
						catch (OutOfMemoryException oome)
						{
							// Crazy include - memory is out
							// TODO: what about reading by chunks?
							throw new ResourceException(
								string.Format(CultureInfo.CurrentCulture, Resources.OutOfMemoryWhileFetchingResource, this.href), oome);
						}
						catch (IOException ioe)
						{
							throw new ResourceException(
								string.Format(CultureInfo.CurrentCulture, Resources.IOErrorWhileFetchingResource, this.href), ioe);
						}
					}

					this.state = ReadState.Interactive;
					return true;
				case ReadState.Interactive:

					// No more input
					this.state = ReadState.EndOfFile;
					return false;
				default:
					return false;
			}
		}

		public override bool ReadAttributeValue()
		{
			return false;
		}

		public override string ReadInnerXml()
		{
			return this.state == ReadState.Interactive ? this.value : string.Empty;
		}

		public override string ReadOuterXml()
		{
			return this.state == ReadState.Interactive ? this.value : string.Empty;
		}

		public override string ReadString()
		{
			return this.state == ReadState.Interactive ? this.value : string.Empty;
		}

		public override void ResolveEntity()
		{
		}

		private static Encoding GetEncodingFromXMLDecl(string attrValue)
		{
			var tmpReader = new XmlTextReader(attrValue);
			tmpReader.DtdProcessing = DtdProcessing.Prohibit;
			tmpReader.WhitespaceHandling = WhitespaceHandling.None;
			try
			{
				while (tmpReader.Read() && tmpReader.Encoding == null)
				{
				}

				return tmpReader.Encoding ?? Encoding.UTF8;
			}
			finally
			{
				tmpReader.Close();
			}
		}
	}
}