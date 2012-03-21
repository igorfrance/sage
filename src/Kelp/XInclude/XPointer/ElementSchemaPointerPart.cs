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
	using System.Diagnostics;
	using System.Text;
	using System.Xml;
	using System.Xml.XPath;

	using Kelp.Properties;
	using Kelp.XInclude.XPath;

	/// <summary>
	/// element() scheme based <see cref="XPointer"/> pointer part.
	/// </summary>
	internal class ElementSchemaPointerPart : PointerPart
	{
		/// <summary>
		/// Equivalent XPath expression.
		/// </summary>
		public string XPath { get; set; }

		/// <summary>
		/// Parses element() based pointer part and builds instance of <c>ElementSchemaPointerPart</c> class.
		/// </summary>
		/// <param name="lexer">Lexical analizer.</param>
		/// <returns>Newly created <c>ElementSchemaPointerPart</c> object.</returns>
		public static ElementSchemaPointerPart ParseSchemaData(XPointerLexer lexer)
		{
			// Productions:
			// [1]   	ElementSchemeData	   ::=   	(NCName ChildSequence?) | ChildSequence
			// [2]   	ChildSequence	   ::=   	('/' [1-9] [0-9]*)+                        
			var xpathBuilder = new StringBuilder();
			var part = new ElementSchemaPointerPart();
			lexer.NextLexeme();
			if (lexer.Kind == XPointerLexer.LexKind.NCName)
			{
				xpathBuilder.Append("id('");
				xpathBuilder.Append(lexer.NCName);
				xpathBuilder.Append("')");
				lexer.NextLexeme();
			}

			int childSequenceLen = 0;
			while (lexer.Kind == XPointerLexer.LexKind.Slash)
			{
				lexer.NextLexeme();
				if (lexer.Kind != XPointerLexer.LexKind.Number)
				{
					Debug.WriteLine(Resources.InvalidTokenInElementSchemeWhileNumberExpected);
					return null;
				}

				if (lexer.Number == 0)
				{
					Debug.WriteLine(Resources.ZeroIndexInElementSchemechildSequence);
					return null;
				}

				childSequenceLen++;
				xpathBuilder.Append("/*[");
				xpathBuilder.Append(lexer.Number);
				xpathBuilder.Append("]");
				lexer.NextLexeme();
			}

			if (lexer.Kind != XPointerLexer.LexKind.RRBracket)
			{
				throw new XPointerSyntaxException(Resources.InvalidTokenInElementSchemeWhileClosingRoundBracketExpected);
			}

			if (xpathBuilder.Length == 0 && childSequenceLen == 0)
			{
				Debug.WriteLine(Resources.EmptyElementSchemeXPointer);
				return null;
			}

			part.XPath = xpathBuilder.ToString();
			return part;
		}

		/// <summary>
		/// Evaluates <see cref="XPointer"/> pointer part and returns pointed nodes.
		/// </summary>
		/// <param name="doc">Document to evaluate pointer part on</param>
		/// <param name="nm">Namespace manager</param>
		/// <returns>Pointed nodes</returns>
		public override XPathNodeIterator Evaluate(XPathNavigator doc, XmlNamespaceManager nm)
		{
			return XPathCache.Select(this.XPath, doc, nm);
		}
	}
}