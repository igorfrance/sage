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
namespace Kelp.XInclude
{
	using System.Xml;

	internal class XIncludeKeywords
	{
		private const string AcceptName = "accept";
		private const string AcceptLanguageName = "accept-language";
		private const string DefaultBaseName = "base";
		private const string EncodingName = "encoding";
		private const string FallbackName = "fallback";
		private const string HrefName = "href";
		private const string IncludeName = "include";
		private const string LangName = "lang";
		private const string OldXIncludeNamespaceName = "http://www.w3.org/2003/XInclude";
		private const string ParseName = "parse";
		private const string TextName = "text";
		private const string XIncludeNamespaceName = "http://www.w3.org/2001/XInclude";
		private const string XmlName = "xml";
		private const string XmlBaseName = "xml:base";
		private const string XmlLangName = "xml:lang";
		private const string XmlNamespaceName = "http://www.w3.org/XML/1998/namespace";
		private const string XpointerName = "xpointer";

		private readonly string href;
		private readonly string include;
		private readonly string oldXIncludeNamespace;
		private readonly string parse;
		private readonly string xincludeNamespace;
		private readonly XmlNameTable nameTable;

		private string accept;
		private string acceptLanguage;
		private string baseName;
		private string encoding;
		private string fallback;
		private string lang;
		private string text;
		private string xml;
		private string xmlBase;
		private string xmlLang;
		private string xmlNamespace;
		private string xpointer;

		public XIncludeKeywords(XmlNameTable nt)
		{
			this.nameTable = nt;

			// Preload some keywords
			this.xincludeNamespace = this.nameTable.Add(XIncludeNamespaceName);
			this.oldXIncludeNamespace = this.nameTable.Add(OldXIncludeNamespaceName);
			this.include = this.nameTable.Add(IncludeName);
			this.href = this.nameTable.Add(HrefName);
			this.parse = this.nameTable.Add(ParseName);
		}

		public string Accept
		{
			get
			{
				return this.accept ?? (this.accept = this.nameTable.Add(AcceptName));
			}
		}

		public string AcceptLanguage
		{
			get
			{
				return this.acceptLanguage ?? (this.acceptLanguage = this.nameTable.Add(AcceptLanguageName));
			}
		}

		public string BaseName
		{
			get
			{
				return this.baseName ?? (this.baseName = this.nameTable.Add(DefaultBaseName));
			}
		}

		public string Encoding
		{
			get
			{
				return this.encoding ?? (this.encoding = this.nameTable.Add(EncodingName));
			}
		}

		public string Fallback
		{
			get
			{
				return this.fallback ?? (this.fallback = this.nameTable.Add(FallbackName));
			}
		}

		public string Href
		{
			get
			{
				return this.href;
			}
		}

		public string Include
		{
			get
			{
				return this.include;
			}
		}

		public string Lang
		{
			get
			{
				return this.lang ?? (this.lang = this.nameTable.Add(LangName));
			}
		}

		public string OldXIncludeNamespace
		{
			get
			{
				return this.oldXIncludeNamespace;
			}
		}

		public string Parse
		{
			get
			{
				return this.parse;
			}
		}

		public string Text
		{
			get
			{
				return this.text ?? (this.text = this.nameTable.Add(TextName));
			}
		}

		public string XincludeNamespace
		{
			get
			{
				return this.xincludeNamespace;
			}
		}

		public string Xml
		{
			get
			{
				return this.xml ?? (this.xml = this.nameTable.Add(XmlName));
			}
		}

		public string XmlBase
		{
			get
			{
				return this.xmlBase ?? (this.xmlBase = this.nameTable.Add(XmlBaseName));
			}
		}

		public string XmlLang
		{
			get
			{
				return this.xmlLang ?? (this.xmlLang = this.nameTable.Add(XmlLangName));
			}
		}

		public string XmlNamespace
		{
			get
			{
				return this.xmlNamespace ?? (this.xmlNamespace = this.nameTable.Add(XmlNamespaceName));
			}
		}

		public string Xpointer
		{
			get
			{
				return this.xpointer ?? (this.xpointer = this.nameTable.Add(XpointerName));
			}
		}

		public static bool Equals(string keyword1, string keyword2)
		{
			return keyword1 == keyword2;
		}
	}
}