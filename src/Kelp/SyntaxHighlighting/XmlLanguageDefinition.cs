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
namespace Kelp.SyntaxHighlighting
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;

	/// <summary>
	/// Defines the language elements for syntax highlighting.
	/// </summary>
	public class XmlLanguageDefinition : LanguageDefinition
	{
		/// <summary>
		/// Defines the XML tag start character
		/// </summary>
		public const string TagStart = "<";

		/// <summary>
		/// Defines the XML tag end character
		/// </summary>
		public const string TagEnd = ">";

		/// <summary>
		/// Defines the closing XML tag start string
		/// </summary>
		public const string ClosingTagStart = "</";

		/// <summary>
		/// Defines the self-closing XML tag end string
		/// </summary>
		public const string SelfClosingTagEnd = "/>";

		/// <summary>
		/// Defines the regular expression that select a valid XML name character.
		/// </summary>
		public static readonly Regex ValidNameChar = new Regex(@"[\w\.\-:]", RegexOptions.Compiled);

		/// <summary>
		/// Defines the regular expression that select a valid XML name start character.
		/// </summary>
		public static readonly Regex ValidNameStartChar = new Regex(@"[a-zA-Z_]", RegexOptions.Compiled);

		/// <summary>
		/// Defines the XML comment delimiters.
		/// </summary>
		public static readonly Delimiters Comment = new Delimiters("<!--", "-->");

		/// <summary>
		/// Defines the XML entity delimiters.
		/// </summary>
		public static readonly Delimiters Entity = new Delimiters("&", ";");

		/// <summary>
		/// Defines the XML processing instruction delimiters.
		/// </summary>
		public static readonly Delimiters ProcessingInstruction = new Delimiters("<?", "?>");

		/// <summary>
		/// Defines the XML CData delimiters.
		/// </summary>
		public static readonly Delimiters CData = new Delimiters("<![CDATA[", "]]>");

		/// <summary>
		/// Initializes a new instance of the <see cref="LanguageDefinition"/> class.
		/// </summary>
		public XmlLanguageDefinition()
		{
			this.CaseSensitive = true;

			this.Elements = new List<ExpressionGroup>();
			this.Attributes = new List<ExpressionGroup>();
			this.QuoteDelimiters = new List<string> { "\"", "'" };
		}

		/// <inheritdoc/>
		public override string ClassName
		{
			get
			{
				return "xml " + base.ClassName;
			}
		}

		/// <summary>
		/// Gets the element groups of this XML language.
		/// </summary>
		public List<ExpressionGroup> Elements { get; private set; }

		/// <summary>
		/// Gets the attribute groups of this XML language.
		/// </summary>
		public List<ExpressionGroup> Attributes { get; private set; }

		internal string GetElementClassName(string elementName)
		{
			ExpressionGroup container = this.Elements.FirstOrDefault(g => g.Keywords.Any(n => n == elementName));
			return container == null ? null : container.ClassName;
		}

		internal string GetAttributeClassName(string attributeName)
		{
			ExpressionGroup container = this.Attributes.FirstOrDefault(g => g.Keywords.Any(n => n == attributeName));
			return container == null ? null : container.ClassName;
		}
	}
}
