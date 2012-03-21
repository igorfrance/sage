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
	using System.Collections.Generic;

	using Kelp.Properties;

	/// <summary>
	/// XPointer parser.
	/// </summary>
	internal class XPointerParser
	{
		private static readonly IDictionary<string, XPointerSchema.SchemaType> schemas = XPointerSchema.Schemas;

		public static Pointer ParseXPointer(string xpointer)
		{
			XPointerLexer lexer = new XPointerLexer(xpointer);
			lexer.NextLexeme();
			if (lexer.Kind == XPointerLexer.LexKind.NCName && !lexer.CanBeSchemaName)
			{
				// Shorthand pointer
				Pointer ptr = new ShorthandPointer(lexer.NCName);
				lexer.NextLexeme();
				if (lexer.Kind != XPointerLexer.LexKind.Eof)
				{
					throw new XPointerSyntaxException(Resources.InvalidTokenAfterShorthandPointer);
				}

				return ptr;
			}

			// SchemaBased pointer
			IList<PointerPart> parts = new List<PointerPart>();
			while (lexer.Kind != XPointerLexer.LexKind.Eof)
			{
				if ((lexer.Kind == XPointerLexer.LexKind.NCName || lexer.Kind == XPointerLexer.LexKind.QName) && lexer.CanBeSchemaName)
				{
					XPointerSchema.SchemaType schemaType = GetSchema(lexer, parts);

					// Move to '('
					lexer.NextLexeme();
					switch (schemaType)
					{
						case XPointerSchema.SchemaType.Element:
							ElementSchemaPointerPart elemPart = ElementSchemaPointerPart.ParseSchemaData(lexer);
							if (elemPart != null)
							{
								parts.Add(elemPart);
							}

							break;
						case XPointerSchema.SchemaType.Xmlns:
							XmlnsSchemaPointerPart xmlnsPart = XmlnsSchemaPointerPart.ParseSchemaData(lexer);
							if (xmlnsPart != null)
							{
								parts.Add(xmlnsPart);
							}

							break;
						case XPointerSchema.SchemaType.XPath1:
							XPath1SchemaPointerPart xpath1Part = XPath1SchemaPointerPart.ParseSchemaData(lexer);
							if (xpath1Part != null)
							{
								parts.Add(xpath1Part);
							}

							break;
						case XPointerSchema.SchemaType.XPointer:
							XPointerSchemaPointerPart xpointerPart = XPointerSchemaPointerPart.ParseSchemaData(lexer);
							if (xpointerPart != null)
							{
								parts.Add(xpointerPart);
							}

							break;
						default:

							// Unknown scheme
							lexer.ParseEscapedData();
							break;
					}

					// Skip ')'
					lexer.NextLexeme();

					// Skip possible whitespace
					if (lexer.Kind == XPointerLexer.LexKind.Space)
					{
						lexer.NextLexeme();
					}
				}
				else
				{
					throw new XPointerSyntaxException(Resources.InvalidToken);
				}
			}

			return new SchemaBasedPointer(parts, xpointer);
		}

		private static XPointerSchema.SchemaType GetSchema(XPointerLexer lexer, IList<PointerPart> parts)
		{
			string schemaNSURI;
			if (lexer.Prefix != string.Empty)
			{
				schemaNSURI = null;

				// resolve prefix
				for (int i = parts.Count - 1; i >= 0; i--)
				{
					PointerPart part = parts[i];
					var xmlnsPart = part as XmlnsSchemaPointerPart;
					if (xmlnsPart != null)
					{
						if (xmlnsPart.Prefix == lexer.Prefix)
						{
							schemaNSURI = xmlnsPart.Uri;
							break;
						}
					}
				}

				if (schemaNSURI == null)
				{
					// No binding for the prefix - ignore pointer part
					return XPointerSchema.SchemaType.Unknown;
				}
			}
			else
			{
				schemaNSURI = string.Empty;
			}

			string schemaQName = schemaNSURI + ':' + lexer.NCName;
			return schemas.ContainsKey(schemaQName) ? schemas[schemaQName] : XPointerSchema.SchemaType.Unknown;
		}
	}
}