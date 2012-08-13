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
	using System.Globalization;
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
		private const string WrapTemplate = "<span class='{1}'>{0}</span>";
		private const string WrapBlock = "<div class='syntax {1} cols-{2}'><div class='linebg'></div><ol>{0}</ol></div>";
		private const string LineBlock = "<li><span class='content'>{0}</span></li>";

		private static readonly Regex spaceMatch = new Regex(@"(?:^ +)|(?: \s{2,})", RegexOptions.Compiled | RegexOptions.Multiline);

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
			this.LineCountDigits = -1;
		}

		private enum ParseMode
		{
			Code,
			RegularExpression,
			LineComment,
			Comment,
			String,
		}

		private enum XmlParseMode
		{
			Text,
			Element,
			ElementName,
			ProcessingInstruction,
			AttributeName,
			AttributeValue,
			Entity,
			CData,
			Comment,
		}

		/// <summary>
		/// Gets or sets the line count to use in the final format.
		/// </summary>
		public int LineCountDigits
		{
			get;
			set;
		}

		/// <summary>
		/// Formats the specified source code string.
		/// </summary>
		/// <remarks>The resulting value will be the HTML string that represents the syntax highlighted source code.</remarks>
		/// <param name="sourceCode">The source code to process.</param>
		/// <returns>The syntax highlighted version of the specified source code</returns>
		public string Format(string sourceCode)
		{
			string formattedSource = this.language is XmlLanguageDefinition
				? this.ParseXmlSource(sourceCode)
				: this.ParseSource(sourceCode);

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
				result.AppendFormat(LineBlock, lines[i]);
			}

			formattedSource = result.ToString();

			int lineCountDigits = this.LineCountDigits != -1 
				? this.LineCountDigits
				: lines.Length.ToString(CultureInfo.InvariantCulture).Length;

			return string.Format(WrapBlock,
				formattedSource, this.language.ClassName, lineCountDigits);
		}

		private static string GetPlaceHolderPattern(int index)
		{
			return PlaceHolder + index + PlaceHolder;
		}

		private static string WrapExpression(string expression, params string[] classNames)
		{
			expression = spaceMatch
				.Replace(expression, match => SpaceString.Repeat(match.Groups[0].Value.Length))
				.Replace("<", "&lt;")
				.Replace(">", "&gt;");

			string className = string.Join(" ", classNames.Where(c => c != null).ToArray());
			string[] expressionLines = expression.Split('\n');
			for (int j = 0; j < expressionLines.Length; j++)
				expressionLines[j] = string.Format(WrapTemplate, expressionLines[j], className);

			return string.Join("\n", expressionLines);
		}

		private string ParseSource(string sourceCode)
		{
			sourceCode = sourceCode.Trim()
				.ReplaceAll("<", "&lt;")
				.ReplaceAll(">", "&gt;");

			string stringEnd = null;
			string commentEnd = null;
			string regexEnd = null;

			ParseMode mode = ParseMode.Code;

			StringBuilder currentComment = null;
			StringBuilder currentString = null;

			StringBuilder parsedSourceBuilder = new StringBuilder();
			for (int i = 0; i < sourceCode.Length; i++)
			{
				string curr = sourceCode.Substring(i, 1);

				if (mode == ParseMode.RegularExpression)
				{
					string end = sourceCode.Substring(i, regexEnd.Length);
					string prev = sourceCode.Substring(i - regexEnd.Length + 1, 1);
					if (end == language.RegexEnd && prev != language.EscapeChar)
						mode = ParseMode.Code;

					continue;
				}

				if (mode == ParseMode.String)
				{
					currentString.Append(curr);
					string prev = sourceCode.Substring(i - 1, 1);
					if (curr == stringEnd && prev != language.EscapeChar)
					{
						this.patterns.Add(WrapExpression(currentString.ToString(), LanguageDefinition.ClassNameString));
						parsedSourceBuilder.Append(GetPlaceHolderPattern(this.patterns.Count));
						mode = ParseMode.Code;
					}

					continue;
				}

				if (mode == ParseMode.Comment)
				{
					currentComment.Append(curr);
					string end = sourceCode.Substring(i - commentEnd.Length, commentEnd.Length);
					string prev = sourceCode.Substring(i - commentEnd.Length + 1, 1);
					if (end == commentEnd && prev != language.EscapeChar)
					{
						this.patterns.Add(WrapExpression(currentComment + commentEnd, LanguageDefinition.ClassNameComment));
						currentComment = new StringBuilder();
						parsedSourceBuilder.Append(GetPlaceHolderPattern(this.patterns.Count));
						mode = ParseMode.Code;
					}

					continue;
				}

				if (mode == ParseMode.LineComment)
				{
					currentComment.Append(curr);
					if (curr == "\r" || curr == "\n")
					{
						this.patterns.Add(WrapExpression(currentComment.ToString(), LanguageDefinition.ClassNameLineComment));
						currentComment = new StringBuilder();
						parsedSourceBuilder.Append(GetPlaceHolderPattern(this.patterns.Count));
						mode = ParseMode.Code;
					}

					continue;
				}

				foreach (string delimiter in this.language.QuoteDelimiters)
				{
					if (curr != delimiter)
						continue;

					stringEnd = delimiter;
					mode = ParseMode.String;
					currentString = new StringBuilder(delimiter);
					break;
				}

				if (mode == ParseMode.Code)
				{
					foreach (Delimiters delimiters in this.language.CommentDelimiters)
					{
						if (sourceCode.Length < i + delimiters.Start.Length)
							continue;

						string test = sourceCode.Substring(i, delimiters.Start.Length);
						if (test != delimiters.Start)
							continue;

						mode = ParseMode.Comment;
						commentEnd = delimiters.End;
						currentComment = new StringBuilder(test);
						i += test.Length - 1;
						break;
					}

					foreach (string delimiter in this.language.LineCommentDelimiters)
					{
						if (sourceCode.Length < i + delimiter.Length)
							continue;

						string test = sourceCode.Substring(i, delimiter.Length);
						if (test != delimiter)
							continue;

						mode = ParseMode.LineComment;
						commentEnd = string.Empty;
						currentComment = new StringBuilder(delimiter);
						i += test.Length - 1;
						break;
					}
				}

				if (mode == ParseMode.Code && this.language.RegexStart != null)
				{
					if (sourceCode.Length >= i + this.language.RegexStart.Length)
					{
						string test = sourceCode.Substring(i, this.language.RegexStart.Length);
						if (test == this.language.RegexStart)
						{
							regexEnd = this.language.RegexEnd;
							i += this.language.RegexStart.Length - 1;
							curr = test;
							mode = ParseMode.RegularExpression;
						}
					}
				}

				if (mode < ParseMode.LineComment)
				{
					parsedSourceBuilder.Append(curr);
				}
			}

			if (currentComment != null && currentComment.Length != 0)
			{
				string commentClass = mode == ParseMode.LineComment 
					? LanguageDefinition.ClassNameLineComment 
					: LanguageDefinition.ClassNameComment;

				this.patterns.Add(WrapExpression(currentComment + commentEnd, commentClass));
				parsedSourceBuilder.Append(GetPlaceHolderPattern(this.patterns.Count));
			}

			sourceCode = parsedSourceBuilder.ToString();

			return
				this.ParseKeywords(sourceCode);
		}

		private string ParseXmlSource(string sourceCode)
		{
			XmlParseMode mode = XmlParseMode.Text;
			XmlLanguageDefinition definition = (XmlLanguageDefinition) language;

			string stringEnd = null;
			StringBuilder currentElementName = null;
			StringBuilder currentAttributeName = null;
			StringBuilder currentAttributeValue = null;
			StringBuilder currentEntity = null;
			StringBuilder currentProcessingInstruction = null;
			StringBuilder currentCData = null;
			StringBuilder currentComment = null;

			StringBuilder resultValue = new StringBuilder();

			sourceCode = sourceCode.Trim();

			for (int i = 0; i < sourceCode.Length; i++)
			{
				string curr = sourceCode.Substring(i, 1);

				if (mode == XmlParseMode.CData)
				{
					string end = sourceCode.Substring(i, XmlLanguageDefinition.CData.End.Length);
					if (end == XmlLanguageDefinition.CData.End)
					{
						currentCData.Append(end);
						resultValue.Append(WrapExpression(currentCData.ToString(), "cdata"));
						mode = XmlParseMode.Text;
						i += 2;
					}
					else
					{
						currentCData.Append(curr);
						continue;
					}
				}

				if (mode == XmlParseMode.Comment)
				{
					string end = sourceCode.Substring(i, XmlLanguageDefinition.Comment.End.Length);
					if (end == XmlLanguageDefinition.Comment.End)
					{
						currentComment.Append(end);
						resultValue.Append(WrapExpression(currentComment.ToString(), "comment"));
						mode = XmlParseMode.Text;
						i += 2;
					}
					else
					{
						currentComment.Append(curr);
					}

					continue;
				}

				if (mode == XmlParseMode.ProcessingInstruction)
				{
					string end = sourceCode.Substring(i, XmlLanguageDefinition.ProcessingInstruction.End.Length);
					if (end == XmlLanguageDefinition.ProcessingInstruction.End)
					{
						currentProcessingInstruction.Append(end);
						resultValue.Append(WrapExpression(currentProcessingInstruction.ToString(), "pi"));
						mode = XmlParseMode.Text;
						i += 1;
					}
					else
					{
						currentProcessingInstruction.Append(curr);
					}

					continue;
				}

				if (mode == XmlParseMode.ElementName)
				{
					if (XmlLanguageDefinition.ValidNameChar.IsMatch(curr))
					{
						currentElementName.Append(curr);
						continue;
					}

					string elementName = currentElementName.ToString();
					resultValue.Append(WrapExpression(elementName, "element", definition.GetElementClassName(elementName)));
					mode = XmlParseMode.Element;
				}

				if (mode == XmlParseMode.Element)
				{
					foreach (string delimiter in language.QuoteDelimiters)
					{
						if (curr != delimiter)
							continue;

						stringEnd = delimiter;
						mode = XmlParseMode.AttributeValue;
						currentAttributeValue = new StringBuilder(delimiter);

						break;
					}

					if (mode == XmlParseMode.AttributeValue)
						continue;

					if (XmlLanguageDefinition.ValidNameChar.IsMatch(curr))
					{
						currentAttributeName = new StringBuilder(curr);
						mode = XmlParseMode.AttributeName;
						continue;
					}

					if (curr == XmlLanguageDefinition.TagEnd)
					{
						resultValue.Append(WrapExpression(curr, "markup"));
						mode = XmlParseMode.Text;
						continue;
					}

					string test = sourceCode.Substring(i, XmlLanguageDefinition.SelfClosingTagEnd.Length);
					if (test == XmlLanguageDefinition.SelfClosingTagEnd)
					{
						resultValue.Append(WrapExpression(test, "markup"));
						mode = XmlParseMode.Text;
						i += 1;
						continue;
					}
				}

				if (mode == XmlParseMode.AttributeName)
				{
					if (XmlLanguageDefinition.ValidNameChar.IsMatch(curr))
					{
						currentAttributeName.Append(curr);
						continue;
					}

					string attributeName = currentAttributeName.ToString();
					resultValue.Append(WrapExpression(attributeName, "attrName", definition.GetAttributeClassName(attributeName)));
					mode = XmlParseMode.Element;
				}

				if (mode == XmlParseMode.AttributeValue)
				{
					currentAttributeValue.Append(curr);
					if (curr == stringEnd)
					{
						resultValue.Append(WrapExpression(currentAttributeValue.ToString(), "attrValue"));
						mode = XmlParseMode.Element;
					}

					continue;
				}

				if (mode == XmlParseMode.Entity)
				{
					currentEntity.Append(curr);
					if (curr == XmlLanguageDefinition.Entity.End)
					{
						resultValue.Append(WrapExpression(currentEntity.ToString(), "entity"));
						mode = XmlParseMode.Text;
					}

					continue;
				}

				if (mode == XmlParseMode.Text)
				{
					if (sourceCode.Length >= i + XmlLanguageDefinition.CData.Start.Length)
					{
						string test = sourceCode.Substring(i, XmlLanguageDefinition.CData.Start.Length);
						if (test == XmlLanguageDefinition.CData.Start)
						{
							i += XmlLanguageDefinition.CData.Start.Length - 1;
							currentCData = new StringBuilder(test);
							mode = XmlParseMode.CData;
							continue;
						}
					}

					if (sourceCode.Length >= i + XmlLanguageDefinition.Comment.Start.Length)
					{
						string test = sourceCode.Substring(i, XmlLanguageDefinition.Comment.Start.Length);
						if (test == XmlLanguageDefinition.Comment.Start)
						{
							i += XmlLanguageDefinition.Comment.Start.Length - 1;
							currentComment = new StringBuilder(test);
							mode = XmlParseMode.Comment;
							continue;
						}
					}

					if (sourceCode.Length >= i + XmlLanguageDefinition.ProcessingInstruction.Start.Length)
					{
						string test = sourceCode.Substring(i, XmlLanguageDefinition.ProcessingInstruction.Start.Length);
						if (test == XmlLanguageDefinition.ProcessingInstruction.Start)
						{
							i += XmlLanguageDefinition.ProcessingInstruction.Start.Length - 1;
							currentProcessingInstruction = new StringBuilder(test);
							mode = XmlParseMode.ProcessingInstruction;
							continue;
						}
					}

					if (sourceCode.Length >= i + XmlLanguageDefinition.ClosingTagStart.Length)
					{
						string test = sourceCode.Substring(i, XmlLanguageDefinition.ClosingTagStart.Length);
						if (test == XmlLanguageDefinition.ClosingTagStart)
						{
							i += 1;
							resultValue.Append(WrapExpression(test, "markup"));
							mode = XmlParseMode.ElementName;
							currentElementName = new StringBuilder();
							continue;
						}
					}

					if (curr == XmlLanguageDefinition.Entity.Start)
					{
						currentEntity = new StringBuilder("&amp;");
						mode = XmlParseMode.Entity;
						continue;
					}

					if (curr == XmlLanguageDefinition.TagStart)
					{
						resultValue.Append(WrapExpression(curr, "markup"));
						mode = XmlParseMode.ElementName;
						currentElementName = new StringBuilder();
						continue;
					}
				}

				resultValue.Append(curr);
			}

			return resultValue.ToString();
		}

		private string ParseKeywords(string sourceCode)
		{
			List<ExpressionGroup> groups = this.language.Expressions;
			if (this.additionalGroups != null)
			{
				groups = groups.ToList();
				groups.AddRange(this.additionalGroups);
			}

			foreach (ExpressionGroup g in groups)
			{
				ExpressionGroup group = g;
				sourceCode = group.Expression.Replace(sourceCode, delegate(Match m)
				{
					this.patterns.Add(WrapExpression(m.Groups[0].Value, group.ClassName));
					return GetPlaceHolderPattern(this.patterns.Count);
				});
			}

			return sourceCode;
		}
	}
}
