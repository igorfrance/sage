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
namespace Kelp.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;

	/// <summary>
	/// Implements extensions to <see cref="IEnumerable&lt;T&gt;"/>.
	/// </summary>
	public static class IEnumerableExtensions
	{
		/// <summary>
		/// Creates a string of all values in the collection joined with the specified separator.
		/// </summary>
		/// <typeparam name="T">The type of the collection</typeparam>
		/// <param name="collection">The collection whose values should be joined.</param>
		/// <param name="separator">The separator to use for joining the collection's values.</param>
		/// <returns>The joined collection.</returns>
		/// <example>
		/// var myList = new List&lt;string&gt; { "red", "green", "blue" };
		/// // The following line returns "red,green,blue":
		/// var joined = myList.Join(","); 
		/// </example>
		public static string Join<T>(this IEnumerable<T> collection, string separator)
		{
			Contract.Requires<ArgumentNullException>(collection != null);
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(separator));

			return string.Join(separator, collection.ToArray());
		}

		/// <summary>
		/// Executes the specified <paramref name="action"/> on each of the elements in the <paramref name="collection"/>.
		/// </summary>
		/// <typeparam name="T">The type of the collection</typeparam>
		/// <param name="collection">The collection on which to operate.</param>
		/// <param name="action">The action to apply to each of the elements.</param>
		public static void ForEach<T>(this IEnumerable<T> collection, Action<T, int> action)
		{
			List<T> list = collection.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				action(list[i], i);
			}
		}

		/// <summary>
		/// Returns the index of the specified <paramref name="item"/> in the specified <paramref name="collection"/>.
		/// </summary>
		/// <param name="collection">The collection to search</param>
		/// <param name="item">The item to find</param>
		/// <returns>The index of the specified <paramref name="item"/> in the specified <paramref name="collection"/>,
		/// if found and -1 if the item could not be found</returns>
		public static int IndexOf(this IEnumerable<string> collection, string item)
		{
			List<string> list = collection.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] == item)
					return i;
			}

			return -1;
		}
	}
}
