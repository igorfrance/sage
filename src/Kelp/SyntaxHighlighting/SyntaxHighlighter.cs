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
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;

	using Kelp.Extensions;

	/// <summary>
	/// Formats source code strings with syntax highlighting HTML markup as defined with <see cref="LanguageDefinition"/>.
	/// </summary>
	public class SyntaxHighlighter
	{
		private const string PlaceHolder = "@@%%__%%@@";
		private const string SpaceString = "&#160;";
		private const string WrapExpression = "<span class='{1}'>{0}</span>";
		private const string WrapBlock = "<div class='syntax {1} linecols-{2}'>{0}</div>";
		private const string LineBlock = "<div class='line {2}'><span class='num'>{1}</span><span class='content'>{0}</span></div>";

		private static readonly Regex spaceMatch = new Regex(@"(?:^\s+)|(?:\s{2,})", RegexOptions.Compiled);
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

			while (this.patterns.Count != 0)
			{
				int lastIndex = this.patterns.Count - 1;
				formattedSource = Regex.Replace(formattedSource, GetPlaceHolderPattern(this.patterns.Count), this.patterns[lastIndex]);
				this.patterns.RemoveAt(lastIndex);
			}

			StringBuilder result = new StringBuilder();

			string[] lines = formattedSource.Split('\n');
			for (int i = 0; i < lines.Length; i++)
			{
				result.AppendFormat(LineBlock, lines[i], i + 1, i % 2 == 1 ? "odd" : "even");
			}

			formattedSource = result.ToString();

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
			word = spaceMatch.Replace(word, match => 
				SpaceString.Repeat(match.Groups[0].Value.Length));

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
					if (end == language.RegexEnd && prev != language.EscapeChar)
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
						string[] commentLines = currentComment.ToString().Split('\n');
						for (int j = 0; j < commentLines.Length; j++)
						{
							commentLines[j] = WrapWord(commentLines[j], LanguageDefinition.ClassNameComment);
						}

						this.patterns.Add(WrapWord(string.Join("\n", commentLines), LanguageDefinition.ClassNameComment));
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
						this.patterns.Add(WrapWord(currentComment.ToString(), LanguageDefinition.ClassNameLineComment));
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

				if (mode == ParseMode.Code && language.RegexStart != null)
				{
					if (text.Length >= i + language.RegexStart.Length)
					{
						string test = text.Substring(i, language.RegexStart.Length);
						if (test == language.RegexStart)
						{
							regexEnd = language.RegexEnd;
							i += language.RegexStart.Length - 1;
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
