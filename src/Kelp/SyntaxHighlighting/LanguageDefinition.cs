namespace Kelp.SyntaxHighlighting
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
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
		/// Initializes a new instance of the <see cref="LanguageDefinition"/> class, using the specified
		/// <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The name of the language this definition represents (e.g. 'javascript').</param>
		/// <param name="caseSensitive">If set to <c>true</c>, indicates that this language is case sensitive.</param>
		public LanguageDefinition(string name, bool caseSensitive = true)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name));

			this.Name = name;
			this.CaseSensitive = caseSensitive;

			this.Quotes = new List<string>();
			this.Comments = new List<string>();
			this.Expressions = new List<ExpressionGroup>();
		}

		/// <summary>
		/// Gets the ID of the language. 
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
		/// Gets or sets the language's quote expressions.
		/// </summary>
		/// <value>The quote expressions for this language.</value>
		public IList<string> Quotes { get; protected set; }

		/// <summary>
		/// Gets or sets the language's comment expressions.
		/// </summary>
		/// <value>The comment expressions for this language.</value>
		public IList<string> Comments { get; protected set; }

		/// <summary>
		/// Gets or sets the language expression groups.
		/// </summary>
		/// <value>The groups of expressions to use with this language.</value>
		public IList<ExpressionGroup> Expressions { get; protected set; }
	}
}
