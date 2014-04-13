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
namespace Sage.XsltExtensions
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Text.RegularExpressions;

	using Kelp.Extensions;

	using log4net;
	using Sage.Extensibility;

	/// <summary>
	/// Provides several string-related utility methods for use in XSLT.
	/// </summary>
	[XsltExtensionObject(XmlNamespaces.Extensions.String)]
	[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter",
		Justification = "This is an XSLT extension class, these methods will not be used from C#.")]
	public class String
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(Sage.XsltExtensions.String).FullName);

		/// <summary>
		/// Formats the specified string <paramref name="value"/> using the specified formatting parameters.
		/// </summary>
		/// <param name="value">The string to format.</param>
		/// <param name="param1">The first substitution value.</param>
		/// <returns>
		/// The formatted version of the specified <paramref name="value"/>
		/// </returns>
		public string format(string value, string param1)
		{
			return string.Format(value, param1);
		}

		/// <summary>
		/// Formats the specified string <paramref name="value"/> using the specified formatting parameters.
		/// </summary>
		/// <param name="value">The string to format.</param>
		/// <param name="param1">The first substitution value.</param>
		/// <param name="param2">The second substitution value.</param>
		/// <returns>
		/// The formatted version of the specified <paramref name="value"/>
		/// </returns>
		public string format(string value, string param1, string param2)
		{
			return string.Format(value, param1, param2);
		}

		/// <summary>
		/// Formats the specified string <paramref name="value"/> using the specified formatting parameters.
		/// </summary>
		/// <param name="value">The string to format.</param>
		/// <param name="param1">The first substitution value.</param>
		/// <param name="param2">The second substitution value.</param>
		/// <param name="param3">The third substitution value.</param>
		/// <returns>
		/// The formatted version of the specified <paramref name="value"/>
		/// </returns>
		public string format(string value, string param1, string param2, string param3)
		{
			return string.Format(value, param1, param2, param3);
		}

		/// <summary>
		/// Formats the specified string <paramref name="value"/> using the specified formatting parameters.
		/// </summary>
		/// <param name="value">The string to format.</param>
		/// <param name="param1">The first substitution value.</param>
		/// <param name="param2">The second substitution value.</param>
		/// <param name="param3">The third substitution value.</param>
		/// <param name="param4">The fourth substitution value.</param>
		/// <returns>
		/// The formatted version of the specified <paramref name="value"/>
		/// </returns>
		public string format(string value, string param1, string param2, string param3, string param4)
		{
			return string.Format(value, param1, param2, param3, param4);
		}

		/// <summary>
		/// Formats the specified string <paramref name="value"/> using the specified formatting parameters.
		/// </summary>
		/// <param name="value">The string to format.</param>
		/// <param name="param1">The first substitution value.</param>
		/// <param name="param2">The second substitution value.</param>
		/// <param name="param3">The third substitution value.</param>
		/// <param name="param4">The fourth substitution value.</param>
		/// <param name="param5">The fifth substitution value.</param>
		/// <returns>
		/// The formatted version of the specified <paramref name="value"/>
		/// </returns>
		public string format(string value, string param1, string param2, string param3, string param4, string param5)
		{
			return string.Format(value, param1, param2, param3, param4, param5);
		}

		/// <summary>
		/// Searched the specified <paramref name="value"/> for <paramref name="expression"/> and replaces it with
		/// specified <paramref name="replacement"/>.
		/// </summary>
		/// <param name="value">The string to replace.</param>
		/// <param name="expression">The expression to look for.</param>
		/// <param name="replacement">The replacement string to substitute with.</param>
		/// <returns>
		/// The processed version of specified <paramref name="value"/>.
		/// </returns>
		public string replace(string value, string expression, string replacement)
		{
			return replace(value, expression, replacement, (int) RegexOptions.IgnoreCase);
		}

		/// <summary>
		/// Searched the specified <paramref name="value"/> for <paramref name="expression"/> and replaces it with
		/// specified <paramref name="replacement"/>.
		/// </summary>
		/// <param name="value">The string to replace.</param>
		/// <param name="expression">The expression to look for.</param>
		/// <param name="replacement">The replacement string to substitute with.</param>
		/// <param name="regexOptions">The regex options to use.</param>
		/// <returns>
		/// The processed version of specified <paramref name="value"/>.
		/// </returns>
		public string replace(string value, string expression, string replacement, int regexOptions)
		{
			if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(expression))
				return value;

			replacement = unquoteReplacement(replacement);

			RegexOptions options = (RegexOptions) regexOptions;
			try
			{
				Regex expr = new Regex(expression, options);
				return expr.Replace(value, replacement);
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}

		/// <summary>
		/// Returns a value indicating whether the specified <paramref name="value"/> matches the specified regular
		/// <paramref name="expression"/> string.
		/// </summary>
		/// <param name="value">The value to check.</param>
		/// <param name="expression">The expression to look for.</param>
		/// <returns><c>true</c> if the specified <paramref name="value"/> matches the specified <paramref name="expression"/>; otherwise <c>false</c>.</returns>
		public bool matches(string value, string expression)
		{
			if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(expression))
				return false;

			try
			{
				Regex expr = new Regex(expression);
				return expr.Match(value).Success;
			}
			catch (Exception ex)
			{
				log.ErrorFormat("Error evaluating '{0}' as regular expression on '{1}': {2}", expression, value, ex.Message);
				return false;
			}
		}

		/// <summary>
		/// Returns a value indicating whether the specified <paramref name="value"/> exists in the specified
		/// <paramref name="subject"/>.
		/// </summary>
		/// <param name="subject">The string to search.</param>
		/// <param name="value">The value to look for.</param>
		/// <returns>
		/// <c>true</c> if the specified <paramref name="value"/> exists in the specified <paramref name="subject"/>; 
		/// otherwise, <c>false</c>.
		/// </returns>
		public bool contains(string subject, string value)
		{
			if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(value))
				return false;

			return subject.Contains(value);
		}

		/// <summary>
		/// Returns a value indicating whether the specified <paramref name="subject"/> contains any one of the
		/// specified <paramref name="values"/> (split on comma and space).
		/// </summary>
		/// <param name="subject">The string to search.</param>
		/// <param name="values">The list of values (comma or space-separated) to look for.</param>
		/// <returns>
		/// <c>true</c> if any one of the specified <paramref name="values"/> exist in the specified 
		/// <paramref name="subject"/>; otherwise, <c>false</c>.
		/// </returns>
		public bool containsAny(string subject, string values)
		{
			if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(values))
				return false;

			string[] vs = values.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
			return subject.ContainsAnyOf(vs);
		}

		/// <summary>
		/// Trims the specified subject string.
		/// </summary>
		/// <param name="subject">The subject.</param>
		/// <returns>The trimmed subject string</returns>
		public string trim(string subject)
		{
			if (string.IsNullOrEmpty(subject))
				return subject;

			return subject.Trim();
		}

		/// <summary>
		/// Trims the specified <paramref name="chars"/> from the specified <paramref name="subject"/> string.
		/// </summary>
		/// <param name="subject">The subject to trim from.</param>
		/// <param name="chars">The chars to trim.</param>
		/// <returns>
		/// The trimmed subject string
		/// </returns>
		public string trim(string subject, string chars)
		{
			if (string.IsNullOrEmpty(subject) || string.IsNullOrWhiteSpace(chars))
				return subject;

			return subject.Trim(chars.ToCharArray());
		}

		/// <summary>
		/// Trims all specified <paramref name="chars"/> from the specified <paramref name="subject"/> string,
		/// using the <paramref name="separator"/> to split the <paramref name="chars"/>.
		/// </summary>
		/// <param name="subject">The subject to trim from.</param>
		/// <param name="chars">The chars to trim.</param>
		/// <param name="separator">The separator.</param>
		/// <returns>
		/// The trimmed subject string
		/// </returns>
		public string trim(string subject, string chars, string separator)
		{
			if (string.IsNullOrEmpty(subject) || string.IsNullOrWhiteSpace(chars) || string.IsNullOrWhiteSpace(separator))
				return subject;

			foreach (var segment in chars.Split(separator.ToCharArray()))
				subject = subject.Trim(segment.ToCharArray());

			return subject;
		}

		/// <summary>
		/// Trims the source code of initial indent characters.
		/// </summary>
		/// <param name="subject">The subject.</param>
		/// <returns>The trimmed version of <paramref name="subject" />.</returns>
		public string trimSourceCode(string subject)
		{
			return trimSourceCode(subject, "  ");
		}

		/// <summary>
		/// Trims the source code of initial indent characters.
		/// </summary>
		/// <param name="subject">The subject.</param>
		/// <param name="tabChars">The chars to replace the tabs with.</param>
		/// <returns>The trimmed version of <paramref name="subject" />.</returns>
		public string trimSourceCode(string subject, string tabChars)
		{
			return trimSourceCode(subject, "  ", "  ");
		}

		/// <summary>
		/// Trims the source code of initial indent characters.
		/// </summary>
		/// <param name="subject">The subject.</param>
		/// <param name="tabChars">The chars to replace the tabs with.</param>
		/// <param name="lineIndent">The indent chars to use at the start of each line.</param>
		/// <returns>The trimmed version of <paramref name="subject" />.</returns>
		public string trimSourceCode(string subject, string tabChars, string lineIndent)
		{
			if (string.IsNullOrEmpty(subject))
				return subject;

			Match match;
			string initialSpace = string.Empty;

			if ((match = Regex.Match(subject, @"^([\n\r]+)")).Success)
			{
				initialSpace = match.Groups[1].Value;
				subject = Regex.Replace(subject, "^" + initialSpace, string.Empty);
			}

			if ((match = Regex.Match(subject, @"^([\s\t]+)")).Success)
			{
				string indent = match.Groups[1].Value;
				string[] lines = subject.Split('\n');
				for (int i = 0; i < lines.Length; i++)
				{
					lines[i] = Regex.Replace(lines[i], "^" + indent, string.Empty);
					lines[i] = Regex.Replace(lines[i], @"\t", tabChars);
					lines[i] = lineIndent + lines[i];
				}

				subject = string.Join("\n", lines);
			}

			return initialSpace + subject;
		}

		private string unquoteReplacement(string replacement)
		{
			return replacement
				.Replace("\\n", "\n")
				.Replace("\\r", "\r")
				.Replace("\\t", "\t")
				.Replace("\\s", " ");
		}
	}
}
