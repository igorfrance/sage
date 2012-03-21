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
	using System.Globalization;
	using System.Xml;
	using System.Xml.XPath;

	using Kelp.Properties;
	using Kelp.XInclude.XPath;

	/// <summary>
	/// xpointer() scheme based XPointer pointer part.
	/// </summary>
	internal class XPointerSchemaPointerPart : PointerPart
	{
		private readonly string xpath;

		public XPointerSchemaPointerPart(string xpath)
		{
			this.xpath = xpath;
		}

		public static XPointerSchemaPointerPart ParseSchemaData(XPointerLexer lexer)
		{
			try
			{
				return new XPointerSchemaPointerPart(lexer.ParseEscapedData());
			}
			catch (Exception e)
			{
				throw new XPointerSyntaxException(
					string.Format(CultureInfo.CurrentCulture, Resources.SyntaxErrorInXPointerSchemeData, e.Message));
			}
		}

		/// <summary>
		/// Evaluates <see cref="XPointer"/> pointer part and returns pointed nodes.
		/// </summary>
		/// <param name="doc">Document to evaluate pointer part on</param>
		/// <param name="nm">Namespace manager</param>
		/// <returns>Pointed nodes</returns>
		public override XPathNodeIterator Evaluate(XPathNavigator doc, XmlNamespaceManager nm)
		{
			try
			{
				return XPathCache.Select(this.xpath, doc, nm);
			}
			catch
			{
				return null;
			}
		}
	}
}