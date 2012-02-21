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
	using System.Diagnostics.Contracts;
	using System.Text.RegularExpressions;
	using System.Xml;

	using Kelp.Extensions;

	/// <summary>
	/// Represents a group of language expressions.
	/// </summary>
	/// <remarks>
	/// A group of language expressions is a group keywords that should be treated equally - all specified keywords will
	/// get the same class name.
	/// </remarks>
	public class ExpressionGroup
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionGroup"/> class.
        /// </summary>
        /// <param name="groupElement">The group element.</param>
        /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
		public ExpressionGroup(XmlElement groupElement, bool caseSensitive)
		{
			Contract.Requires<ArgumentNullException>(groupElement != null);

			bool treatAsWord = !groupElement.GetAttribute("treatAsWord").ContainsAnyOf("false", "no", "0");
			string groupName = groupElement.GetAttribute("name");
			string keywords = groupElement.InnerText
				.ReplaceAll(@"\n", "|")
				.ReplaceAll(@"[\s\t\r]", string.Empty)
				.ReplaceAll(@"\|\|", "|")
				.Trim('|');

			RegexOptions options = RegexOptions.None;
			if (!caseSensitive)
				options |= RegexOptions.IgnoreCase;

			string pattern = treatAsWord ? (@"(\b)(" + keywords + @")(\b)") : keywords;

			this.Expression = new Regex(pattern, options);
			this.ClassName = groupName;
		}

		/// <summary>
		/// Gets the CSS class name associated with this group.
		/// </summary>
		/// <value>The CSS class name associated with this group.</value>
		public string ClassName { get; private set; }

		/// <summary>
		/// Gets the regular expression for this group of words.
		/// </summary>
		/// <value>The regular expression for this group of words.</value>
		public Regex Expression { get; private set; }
	}
}
