namespace Kelp.SyntaxHighlighting
{
	using System;
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
		/// <param name="className">The css class name to use for the words in this group.</param>
		/// <param name="words">The words that define this group.</param>
		public ExpressionGroup(string className, string words)
			: this(className, words, true)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ExpressionGroup"/> class.
		/// </summary>
		/// <param name="className">The css class name to use for the words in this group.</param>
		/// <param name="words">The words that define this group.</param>
		/// <param name="treatAsWord">If set to <c>true</c> the words will be matched as separate words only.</param>
		public ExpressionGroup(string className, string words, bool treatAsWord)
			: this(className, words, treatAsWord, true)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ExpressionGroup"/> class.
		/// </summary>
		/// <param name="className">The css class name to use for the words in this group.</param>
		/// <param name="words">The words that define this group.</param>
		/// <param name="treatAsWord">if set to <c>true</c> indicates that the group should be treated as a word.</param>
		/// <param name="caseSensitive">if set to <c>true</c> indicates that the group should be case sensitive.</param>
		public ExpressionGroup(string className, string words, bool treatAsWord, bool caseSensitive)
		{
			RegexOptions options = RegexOptions.None;
			if (!caseSensitive)
				options |= RegexOptions.IgnoreCase;

			string pattern = treatAsWord ? (@"(\b)(" + words + @")(\b)") : words;

			this.Expression = new Regex(pattern, options);
			this.ClassName = className;
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
