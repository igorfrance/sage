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
