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
		private const string WrapBlock = "<div class='syntax {1} linecols-{2}'>{0}</div>";
		private const string LineBlock = "<div class='line {2}'><span class='num'>{1}</span><span class='content'>{0}</span></div>";

		private readonly string tabString;
		private readonly LanguageDefinition language;
		private readonly List<string> patterns;
		private readonly List<ExpressionGroup> additionalGroups;

		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxHighlighter"/> class.
		/// </summary>
		/// <param name="language">The definition of the language being highlighted.</param>
		/// <param name="additionalGroups">Possible additional expression groups to use.</param>
		public SyntaxHighlighter(LanguageDefinition language, List<ExpressionGroup> additionalGroups = null)
			: this(language, "<span class='tab'></span>")
		{
			this.additionalGroups = additionalGroups;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxHighlighter"/> class.
		/// </summary>
		/// <param name="language">The definition of the language being processed.</param>
		/// <param name="tabString">The string to use for replacing TAB characters.</param>
		public SyntaxHighlighter(LanguageDefinition language, string tabString)
		{
			this.language = language;
			this.tabString = tabString;
			this.patterns = new List<string>();
		}

		private enum ParseMode
		{
			Code,
			RegularExpression,
			LineComment,
			Comment,
			String,
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
				this.ParseKeywords(
					this.PrepareSource(
						ParseHtml(sourceCode.Trim())));

			formattedSource = Regex.Replace(formattedSource, "\r", string.Empty);
			formattedSource = Regex.Replace(formattedSource, "\t", this.tabString);

			StringBuilder result = new StringBuilder();

			string[] lines = formattedSource.Split('\n');
			for (int i = 0; i < lines.Length; i++)
			{
				result.AppendFormat(LineBlock, lines[i], i + 1, i % 2 == 1 ? "odd" : "even");
			}

			formattedSource = result.ToString();

			while (this.patterns.Count != 0)
			{
				int lastIndex = this.patterns.Count - 1;
				formattedSource = Regex.Replace(formattedSource, GetPlaceHolderPattern(this.patterns.Count), this.patterns[lastIndex]);
				this.patterns.RemoveAt(lastIndex);
			}

			return string.Format(WrapBlock, formattedSource, this.language.Name, lines.Length.ToString().Length);
		}

		private static string ParseHtml(string sourceCode)
		{
			sourceCode = Regex.Replace(sourceCode, "<", "&lt;");
			sourceCode = Regex.Replace(sourceCode, "/>", "&gt;");
			return sourceCode;
		}

		private static string GetPlaceHolderPattern(int index)
		{
			return PlaceHolder + index + PlaceHolder;
		}

		private static string WrapWord(string word, string className)
		{
			return string.Format(WrapExpression, word, className);
		}

		private string ParseKeywords(string sourceCode)
		{
			List<ExpressionGroup> groups = this.language.Expressions;
			if (this.additionalGroups != null)
			{
				groups = groups.ToList();
				groups.AddRange(this.additionalGroups);
			}

			foreach (ExpressionGroup group in groups)
			{
				sourceCode = group.Expression.Replace(sourceCode, delegate(Match m)
				{
					this.patterns.Add(WrapWord(m.Groups[0].Value, group.ClassName));
					return GetPlaceHolderPattern(this.patterns.Count);
				});
			}

			return sourceCode;
		}

		private string PrepareSource(string sourceCode)
		{
			string text = sourceCode;

			string stringEnd = null;
			string commentEnd = null;
			string regexEnd = null;

			ParseMode mode = ParseMode.Code;

			StringBuilder currentComment = null;
			StringBuilder currentString = null;

			StringBuilder resultValue = new StringBuilder();
			for (int i = 0; i < text.Length; i++)
			{
				string curr = text.Substring(i, 1);

				if (mode == ParseMode.RegularExpression)
				{
					string end = text.Substring(i, regexEnd.Length);
					string prev = text.Substring(i - regexEnd.Length + 1, 1);
					if (end == language.Regexp[1] && prev != language.EscapeChar)
						mode = ParseMode.Code;

					continue;
				}

				if (mode == ParseMode.String)
				{
					currentString.Append(curr);
					string prev = text.Substring(i - 1, 1);
					if (curr == stringEnd && prev != language.EscapeChar)
					{
						this.patterns.Add(WrapWord(currentString.ToString(), LanguageDefinition.ClassNameString));
						resultValue.Append(GetPlaceHolderPattern(this.patterns.Count));
						mode = ParseMode.Code;
					}

					continue;
				}

				if (mode == ParseMode.Comment)
				{
					currentComment.Append(curr);
					string end = text.Substring(i - commentEnd.Length, commentEnd.Length);
					string prev = text.Substring(i - commentEnd.Length + 1, 1);
					if (end == commentEnd && prev != language.EscapeChar)
					{
						this.patterns.Add(WrapWord(currentComment.ToString(), LanguageDefinition.ClassNameComment));
						resultValue.Append(GetPlaceHolderPattern(this.patterns.Count));
						mode = ParseMode.Code;
					}

					continue;
				}

				if (mode == ParseMode.LineComment)
				{
					currentComment.Append(curr);
					if (curr == "\r" || curr == "\n")
					{
						this.patterns.Add(WrapWord(currentComment.ToString(), LanguageDefinition.ClassNameComment));
						resultValue.Append(GetPlaceHolderPattern(this.patterns.Count));
						mode = ParseMode.Code;
					}

					continue;
				}

				foreach (string delimiter in language.QuoteDelimiters)
				{
					if (curr == delimiter)
					{
						stringEnd = delimiter;
						mode = ParseMode.String;
						currentString = new StringBuilder(delimiter);
						break;
					}
				}

				if (mode == ParseMode.Code)
				{
					foreach (string[] pair in language.CommentDelimiters)
					{
						if (text.Length >= i + pair[0].Length)
						{
							string test = text.Substring(i, pair[0].Length);
							if (test == pair[0])
							{
								commentEnd = pair[1];
								mode = ParseMode.Comment;
								currentComment = new StringBuilder(test);
								i += test.Length - 1;
								break;
							}
						}
					}

					foreach (string delimiter in language.LineCommentDelimiters)
					{
						if (text.Length >= i + delimiter.Length)
						{
							string test = text.Substring(i, delimiter.Length);
							if (test == delimiter)
							{
								mode = ParseMode.LineComment;
								currentComment = new StringBuilder(delimiter);
								i += test.Length - 1;
								break;
							}
						}
					}
				}

				if (mode == ParseMode.Code && language.Regexp != null)
				{
					if (text.Length >= i + language.Regexp[0].Length)
					{
						string test = text.Substring(i, language.Regexp[0].Length);
						if (test == language.Regexp[0])
						{
							regexEnd = language.Regexp[1];
							i += language.Regexp[0].Length - 1;
							curr = test;
							mode = ParseMode.RegularExpression;
						}
					}
				}

				if (mode < ParseMode.LineComment)
				{
					resultValue.Append(curr);
				}
			}

			return resultValue.ToString();
		}
	}
}
