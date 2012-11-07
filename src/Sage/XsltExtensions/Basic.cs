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
	using System.Xml.XPath;

	using Sage.Extensibility;

	/// <summary>
	/// Provides several math or logic-related utility methods for use in XSLT.
	/// </summary>
	[XsltExtensionObject(XmlNamespaces.Extensions.Basic)]
	[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter",
		Justification = "This is an XSLT extension class, these methods will not be used from C#.")]
	public class Basic
	{
		/// <summary>
		/// Tests the specified <paramref name="condition"/> and returns <paramref name="result1"/> if
		/// <paramref name="condition"/> is <c>true</c>, or <paramref name="result2"/> if
		/// <paramref name="condition"/> is <c>false</c>
		/// </summary>
		/// <param name="condition">The condition to test</param>
		/// <param name="result1">The value to return if <paramref name="condition"/> is <c>true</c>.</param>
		/// <param name="result2">The value to return if <paramref name="condition"/> is <c>false</c>.</param>
		/// <returns>
		/// <paramref name="result1"/> if <paramref name="condition"/> is <c>true</c>, or
		/// <paramref name="result2"/> if <paramref name="condition"/> is <c>false</c></returns>
		public object iif(bool condition, object result1, object result2)
		{
			return condition ? result1 : result2;
		}

		/// <summary>
		/// Returns either of two specified values that is not <c>null</c>.
		/// </summary>
		/// <param name="result1">The first value to consider.</param>
		/// <param name="result2">The value to return if <paramref name="result1"/> is <c>null</c>.</param>
		/// <returns>
		/// <paramref name="result2"/> if it is not <c>null</c>; otherwise <paramref name="result2"/>.
		/// </returns>
		public object isnull(object result1, object result2)
		{
			switch (result1.GetType().Name)
			{
				case "Boolean":
					return (bool) result1 ? result1 : result2;

				case "String":
					return string.IsNullOrWhiteSpace((string) result1) ? result1 : result2;

				case "Double":
					return !((double) result1).Equals(0) ? result1 : result2;

				default:
					var iterator = result1 as XPathNodeIterator;
					if (iterator == null)
						return result2;

					return iterator.Count != 0 ? result1 : result2;
			}
		}
	}
}
