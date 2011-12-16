namespace Kelp.SyntaxHighlighting
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;

	/// <summary>
	/// Formats source code strings with syntax highlighting HTML markup as defined with <see cref="LanguageDefinition"/>.
	/// </summary>
	public class SyntaxHighlighter
	{
		private const string PlaceHolder = "@@%%__%%@@";
		private const string WrapExpression = "<span class='{1}'>{0}</span>";
		private const string WrapBlock = "<span class='syntax {1}'>{0}</span>";

		private readonly string spaceString;
		private readonly string tabString;
		private readonly LanguageDefinition language;
		private readonly List<string> patterns;

		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxHighlighter"/> class.
		/// </summary>
		/// <param name="language">The definition of the language being highlighted.</param>
		public SyntaxHighlighter(LanguageDefinition language)
			: this(language, "&#160;&#160;&#160;&#160;", "&#160;")
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxHighlighter"/> class.
		/// </summary>
		/// <param name="language">The definition of the language being processed.</param>
		/// <param name="tabString">The string to use for replacing TAB characters.</param>
		/// <param name="spaceString">The string to use for replacing SPACE characters.</param>
		public SyntaxHighlighter(LanguageDefinition language, string tabString, string spaceString)
		{
			this.language = language;
			this.tabString = tabString;
			this.spaceString = spaceString;
			this.patterns = new List<string>();
		}

		/// <summary>
		/// Formats the specified source code string.
		/// </summary>
		/// <remarks>The resulting value will be the HTML string that represents the syntax highlighted source code.</remarks>
		/// <param name="sourceCode">The source code to process.</param>
		/// <returns>The syntax highlighted version of the specified source code</returns>
		public string Format(string sourceCode)
		{
			string formattedSource =
				ParseWhitespace(
				ParseKeywords(
				ParseStrings(
				ParseComments(
				ParseHtml(sourceCode)))));

			while (patterns.Count != 0)
			{
				int lastIndex = patterns.Count - 1;
				formattedSource = Regex.Replace(formattedSource, GetPlaceHolderPattern(patterns.Count), patterns[lastIndex]);
				patterns.RemoveAt(lastIndex);
			}
			return string.Format(WrapBlock, formattedSource, language.ID);

		}

		private string ParseKeywords(string sourceCode)
		{
			foreach (ExpressionGroup e in language.Expressions)
			{
				sourceCode = e.Expression.Replace(sourceCode, delegate(Match m)
				{
					patterns.Add(WrapWord(m.Groups[0].Value, e.ClassName));
					return GetPlaceHolderPattern(patterns.Count);
				});
			}
			return sourceCode;
		}

		private string ParseStrings(string sourceCode)
		{
			foreach (string pattern in language.Quotes)
				sourceCode = AddPlaceHolder(sourceCode, pattern, LanguageDefinition.ClassNameString);

			return sourceCode;
		}

		private string ParseWhitespace(string sourceCode)
		{
			sourceCode = Regex.Replace(sourceCode, "\r", string.Empty);
			sourceCode = Regex.Replace(sourceCode, "\n", "<br/>");
			sourceCode = Regex.Replace(sourceCode, "\t", this.tabString);
			sourceCode = Regex.Replace(sourceCode, " ", this.spaceString);
			return sourceCode;
		}

		private string ParseComments(string sourceCode)
		{
			foreach (string pattern in language.Comments)
				sourceCode = AddPlaceHolder(sourceCode, pattern, LanguageDefinition.ClassNameComment);

			return sourceCode;
		}

		private string AddPlaceHolder(string sourceCode, string pattern, string color)
		{
			return Regex.Replace(sourceCode, pattern, delegate(Match m)
			{
				patterns.Add(WrapWord(m.Groups[0].Value, color));
				return GetPlaceHolderPattern(this.patterns.Count);
			});
		}

		private static string ParseHtml(string sourceCode)
		{
			sourceCode = Regex.Replace(sourceCode, "<", "&lt;");
			sourceCode = Regex.Replace(sourceCode, "/>", "&gt;");
			return sourceCode;
		}

		private static string GetPlaceHolderPattern(int index)
		{
			return (PlaceHolder + index + PlaceHolder);
		}

		private static string WrapWord(string word, string className)
		{
			return string.Format(WrapExpression, word, className);
		}
	}
}
