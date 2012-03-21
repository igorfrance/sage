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
namespace Kelp.XInclude.XPointer
{
	using System;
	using System.Diagnostics;
	using System.Globalization;
	using System.Xml;
	using System.Xml.XPath;

	using Kelp.Properties;

	/// <summary>
	/// xmlns() scheme based <see cref="XPointer"/> pointer part.
	/// </summary>
	internal class XmlnsSchemaPointerPart : PointerPart
	{
		/// <summary>
		/// Creates xmlns() scheme pointer part with given
		/// namespace prefix and namespace URI. 
		/// </summary>
		/// <param name="prefix">Namespace prefix</param>
		/// <param name="uri">Namespace URI</param>
		public XmlnsSchemaPointerPart(string prefix, string uri)
		{
			this.Prefix = prefix;
			this.Uri = uri;
		}

		public string Prefix { get; set; }

		public string Uri { get; set; }

		public static XmlnsSchemaPointerPart ParseSchemaData(XPointerLexer lexer)
		{
			// [1]   	XmlnsSchemeData	   ::=   	 NCName S? '=' S? EscapedNamespaceName
			// [2]   	EscapedNamespaceName	   ::=   	EscapedData*                      	                    
			// Read prefix as NCName
			lexer.NextLexeme();
			if (lexer.Kind != XPointerLexer.LexKind.NCName)
			{
				Debug.WriteLine(Resources.InvalidTokenInXmlnsSchemeWhileNCNameExpected);
				return null;
			}

			string prefix = lexer.NCName;
			lexer.SkipWhiteSpace();
			lexer.NextLexeme();
			if (lexer.Kind != XPointerLexer.LexKind.Eq)
			{
				Debug.WriteLine(Resources.InvalidTokenInXmlnsSchemeWhileEqualsSignExpected);
				return null;
			}

			lexer.SkipWhiteSpace();
			string namespaceUri;
			try
			{
				namespaceUri = lexer.ParseEscapedData();
			}
			catch (Exception e)
			{
				throw new XPointerSyntaxException(
					string.Format(CultureInfo.CurrentCulture, Resources.SyntaxErrorInXmlnsSchemeData, e.Message));
			}

			return new XmlnsSchemaPointerPart(prefix, namespaceUri);
		}

		/// <summary>
		/// Evaluates <see cref="XPointer"/> pointer part and returns pointed nodes.
		/// </summary>
		/// <param name="doc">Document to evaluate pointer part on</param>
		/// <param name="nm">Namespace manager</param>
		/// <returns>Pointed nodes</returns>
		public override XPathNodeIterator Evaluate(XPathNavigator doc, XmlNamespaceManager nm)
		{
			nm.AddNamespace(this.Prefix, this.Uri);
			return null;
		}
	}
}