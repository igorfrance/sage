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
	using System.Xml.XPath;

	using Sage.Extensibility;

	/// <summary>
	/// Provides several math or logic-related utility methods for use in XSLT.
	/// </summary>
	[XsltExtensionObject(XmlNamespaces.Extensions.Math)]
	[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter",
		Justification = "This is an XSLT extension class, these methods will not be used from C#.")]
	public class Math
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
