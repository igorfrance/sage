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
namespace Sage.XsltExtensions
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Text.RegularExpressions;

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
		/// The processed version of speciried <paramref name="value"/>.
		/// </returns>
		public string replace(string value, string expression, string replacement)
		{
			if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(expression))
				return value;

			try
			{
				Regex expr = new Regex(expression);
				return expr.Replace(value, replacement ?? string.Empty);
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
	}
}
