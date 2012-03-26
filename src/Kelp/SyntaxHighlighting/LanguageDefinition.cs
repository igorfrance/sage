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

	/// <summary>
	/// Defines the language elements for syntax highlighting.
	/// </summary>
	public class LanguageDefinition
	{
		/// <summary>
		/// The CSS class name that will be used on singleline comments.
		/// </summary>
		public const string ClassNameLineComment = "comment single";

		/// <summary>
		/// The CSS class name that will be used on multiline comments.
		/// </summary>
		public const string ClassNameComment = "comment multi";

		/// <summary>
		/// The CSS class name that will be used on strings.
		/// </summary>
		public const string ClassNameString = "string";

		/// <summary>
		/// Initializes a new instance of the <see cref="LanguageDefinition"/> class.
		/// </summary>
		public LanguageDefinition()
		{
			this.QuoteDelimiters = new List<string>();
			this.LineCommentDelimiters = new List<string>();
			this.Expressions = new List<ExpressionGroup>();
			this.CommentDelimiters = new List<string[]>();
		}

		/// <summary>
		/// Gets or sets the escape character.
		/// </summary>
		public string EscapeChar { get; protected set; }

		/// <summary>
		/// Gets or sets the character that indicates the start of a regular expression.
		/// </summary>
		public string RegexStart { get; protected set; }

		/// <summary>
		/// Gets or sets the character that indicates the end of a regular expression.
		/// </summary>
		public string RegexEnd { get; protected set; }

		/// <summary>
		/// Gets or sets the name of the language. 
		/// </summary>
		/// <remarks>
		/// This value will be includes in the class name of the syntax highlighted block.
		/// </remarks>
		public string Name { get; protected set; }

		/// <summary>
		/// Gets or sets a value indicating whether this language is case sensitive.
		/// </summary>
		/// <value><c>true</c> if this language is case sensitive; otherwise, <c>false</c>.</value>
		public bool CaseSensitive { get; protected set; }

		/// <summary>
		/// Gets or sets the list of language's quote delimiters.
		/// </summary>
		public List<string> QuoteDelimiters { get; protected set; }

		/// <summary>
		/// Gets or sets the list of language's single line comment delimiters.
		/// </summary>
		public List<string> LineCommentDelimiters { get; protected set; }

		/// <summary>
		/// Gets or sets the list of language's comment delimiters.
		/// </summary>
		public List<string[]> CommentDelimiters { get; protected set; }

		/// <summary>
		/// Gets or sets the language expression groups.
		/// </summary>
		/// <value>The groups of expressions to use with this language.</value>
		public List<ExpressionGroup> Expressions { get; protected set; }

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Name;
		}
	}
}
