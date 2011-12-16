namespace Kelp.SyntaxHighlighting
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Defines the language elements for syntax highlighting.
	/// </summary>
	public abstract class LanguageDefinition
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
		/// <param name="id">The id of the language.</param>
		protected LanguageDefinition(string id)
		{
			this.ID = id;
			this.CaseSensitive = true;

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
		public string ID { get; protected set; }

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
