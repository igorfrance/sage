/**
 * Open Source Initiative OSI - The MIT License (MIT):Licensing
 * [OSI Approved License]
 * The MIT License (MIT)
 *
 * Copyright (c) 2011 Igor France
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */
namespace Kelp.SyntaxHighlighting
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Xml;

	using Kelp.Core.Extensions;

	/// <summary>
	/// Defines the language elements for syntax highlighting.
	/// </summary>
	public class LanguageDefinition
	{
		/// <summary>
		/// The CSS class name that will be used on comments.
		/// </summary>
		public const string ClassNameComment = "comment";

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
