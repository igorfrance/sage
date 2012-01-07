namespace Kelp.SyntaxHighlighting
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Text.RegularExpressions;

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
		/// <param name="name">The name.</param>
		/// <param name="keywords">The pipe-separated list of language keywords (e.g: 'if|else|function...').</param>
		/// <param name="treatAsWord">if set to <c>true</c> [treat as word].</param>
		/// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
		public ExpressionGroup(string name, string keywords, bool treatAsWord = true, bool caseSensitive = true)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name));
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(keywords));

			RegexOptions options = RegexOptions.None;
			if (!caseSensitive)
				options |= RegexOptions.IgnoreCase;

			string pattern = treatAsWord ? (@"(\b)(" + keywords + @")(\b)") : keywords;

			this.Expression = new Regex(pattern, options);
			this.ClassName = name;
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
