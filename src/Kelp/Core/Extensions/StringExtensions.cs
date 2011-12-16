namespace Kelp.Core.Extensions
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Text.RegularExpressions;

	/// <summary>
	/// Defines extension methods for strings.
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Returns <c>true</c> if the string contains any one of the supplied values.
		/// </summary>
		/// <param name="subject">The string subject being tested.</param>
		/// <param name="values">The values to test for.</param>
		/// <returns><c>true</c> if the string contains any one of the supplied values; otherwise <c>false</c>.</returns>
		public static bool ContainsAnyOf(this string subject, params string[] values)
		{
			return subject.ContainsAnyOf(false, values);
		}

		/// <summary>
		/// Returns <c>true</c> if the string contains any one of the supplied values.
		/// </summary>
		/// <param name="subject">The string subject being tested.</param>
		/// <param name="caseInSensitive">If this argument is <c>true</c>, the search will ignore case.</param>
		/// <param name="values">The values to test for.</param>
		/// <returns><c>true</c> if the string contains any one of the supplied values; otherwise <c>false</c>.</returns>
		public static bool ContainsAnyOf(this string subject, bool caseInSensitive, params string[] values)
		{
			string compare = !caseInSensitive ? subject : subject.ToLower();
			foreach (string test in values)
			{
				if (compare.Contains(!caseInSensitive ? test : test.ToLower()))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Returns <c>true</c> if the string contains all of the supplied values.
		/// </summary>
		/// <param name="subject">The string subject being tested.</param>
		/// <param name="values">The values to test for.</param>
		/// <returns><c>true</c> if the string contains any one of the supplied values; otherwise <c>false</c>.</returns>
		public static bool ContainsAllOf(this string subject, params string[] values)
		{
			return subject.ContainsAllOf(false, values);
		}

		/// <summary>
		/// Returns <c>true</c> if the string contains all of the supplied values.
		/// </summary>
		/// <param name="subject">The string subject being tested.</param>
		/// <param name="caseInSensitive">If this argument is <c>true</c>, the search will ignore case.</param>
		/// <param name="values">The values to test for.</param>
		/// <returns><c>true</c> if the string contains any one of the supplied values; otherwise <c>false</c>.</returns>
		public static bool ContainsAllOf(this string subject, bool caseInSensitive, params string[] values)
		{
			string compare = !caseInSensitive ? subject : subject.ToLower();
			bool result = true;
			foreach (string test in values)
			{
				if (!compare.Contains(!caseInSensitive ? test : test.ToLower()))
				{
					result = false;
					break;
				}
			}

			return result;
		}

		/// <summary>
		/// Returns the number of occurrences of value <paramref name="text"/> within the current <paramref name="subject"/>.
		/// </summary>
		/// <param name="subject">The string to be searched</param>
		/// <param name="text">The value to search for</param>
		/// <returns>The number of occurrences of value <paramref name="text"/> within the current <paramref name="subject"/>.</returns>
		public static int CountOf(this string subject, string text)
		{
			if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(text))
				return 0;

			int count = 0;
			int index = -1;
			while ((index = subject.IndexOf(text, index + 1)) != -1)
				count++;

			return count;
		}

		/// <summary>
		/// Returns <c>true</c> if the string equals any one of the supplied values.
		/// </summary>
		/// <param name="subject">The string subject being tested.</param>
		/// <param name="values">The values to test for.</param>
		/// <returns><c>true</c> if the string contains any one of the supplied values; otherwise <c>false</c>.</returns>
		public static bool EqualsAnyOf(this string subject, params string[] values)
		{
			return subject.EqualsAnyOf(false, values);
		}

		/// <summary>
		/// Returns <c>true</c> if the string equals any one of the supplied values.
		/// </summary>
		/// <param name="subject">The string subject being tested.</param>
		/// <param name="caseInSensitive">If this argument is <c>true</c>, the search will ignore case.</param>
		/// <param name="values">The values to test for.</param>
		/// <returns><c>true</c> if the string contains any one of the supplied values; otherwise <c>false</c>.</returns>
		public static bool EqualsAnyOf(this string subject, bool caseInSensitive, params string[] values)
		{
			string compare = !caseInSensitive ? subject : subject.ToLower();
			foreach (string test in values)
			{
				if (compare == (!caseInSensitive ? test : test.ToLower()))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Replaces the string mathing the specified regular <paramref name="expression"/> string with the specified 
		/// <paramref name="replacement"/> string.
		/// </summary>
		/// <param name="subject">The string to replace.</param>
		/// <param name="expression">The pattern to initialize the regular expression with.</param>
		/// <param name="replacement">The replacement value string.</param>
		/// <returns>The original string with all substrings matching <paramref name="expression"/> replaced with
		/// the specified <paramref name="replacement"/> string.</returns>
		public static string ReplaceAll(this string subject, string expression, string replacement)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(subject));
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(expression));
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(replacement));

			return ReplaceAll(subject, new Regex(expression), replacement);
		}

		/// <summary>
		/// Replaces the string mathing the specified regular <paramref name="expression"/> with the specified 
		/// <paramref name="replacement"/> string.
		/// </summary>
		/// <param name="subject">The string to replace.</param>
		/// <param name="expression">The regular expression to use.</param>
		/// <param name="replacement">The replacement value string.</param>
		/// <returns>The original string with all substrings matching <paramref name="expression"/> replaced with
		/// the specified <paramref name="replacement"/> string.</returns>
		public static string ReplaceAll(this string subject, Regex expression, string replacement)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(subject));
			Contract.Requires<ArgumentNullException>(expression != null);
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(replacement));

			return expression.Replace(subject, replacement);
		}

		/// <summary>
		/// Returns the specified <paramref name="instance"/> with the first letter converted to upper case.
		/// </summary>
		/// <param name="instance">The value to process.</param>
		public static string ToUpperCaseFirst(this string instance)
		{
			if (string.IsNullOrEmpty(instance))
				return instance;

			string temp = instance.Substring(0, 1);
			return temp.ToUpper() + instance.Substring(1);
		}
	}
}
