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
namespace Kelp.Extensions
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Implements several extension methods for <see cref="IList{T}"/> and <see cref="IEnumerable{T}"/>.
	/// </summary>
	public static class ListExtensions
	{
		/// <summary>
		/// Sorts the specified <paramref name="list"/> in place, using the specified <paramref name="comparison"/>.
		/// </summary>
		/// <typeparam name="T">The underlying type of <paramref name="list"/>.</typeparam>
		/// <param name="list">The list to sort.</param>
		/// <param name="comparison">The comparison method to use.</param>
		public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
		{
			ArrayList.Adapter((IList) list).Sort(new ComparisonComparer<T>(comparison));
		}

		/// <summary>
		/// Sorts the specified <paramref name="list"/> in place, using the specified <paramref name="comparison"/>.
		/// </summary>
		/// <typeparam name="T">The underlying type of <paramref name="list"/>.</typeparam>
		/// <param name="list">The list to sort.</param>
		/// <param name="comparison">The comparison method to use.</param>
		public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> list, Comparison<T> comparison)
		{
			return list.OrderBy(t => t, new ComparisonComparer<T>(comparison));
		}

		/// <summary>
		/// Executes the specified <paramref name="action"/> on each item in the <paramref name="list"/>.
		/// </summary>
		/// <typeparam name="T">The underlying list type</typeparam>
		/// <param name="list">The list to operate on.</param>
		/// <param name="action">The action to execute.</param>
		/// <returns>A copy of the original list.</returns>
		public static IEnumerable<T> Each<T>(this IEnumerable<T> list, Action<T> action)
		{
			var result = list as T[] ?? list.ToArray();
			var array = list as T[] ?? result.ToArray();
			foreach (T item in array)
				action(item);

			return result;
		}

		/// <summary>
		/// Executes the specified <paramref name="action"/> on each item in the <paramref name="list"/>, passing it the item index.
		/// </summary>
		/// <typeparam name="T">The underlying list type</typeparam>
		/// <param name="list">The list to operate on.</param>
		/// <param name="action">The action to execute.</param>
		/// <returns>A copy of the original list.</returns>
		public static IEnumerable<T> Each<T>(this IEnumerable<T> list, Action<T, int> action)
		{
			var result = list as T[] ?? list.ToArray();
			var array = list as T[] ?? result.ToArray();
			for (int i = 0; i < array.Length; i++)
				action(array[i], i);

			return result;
		}

		/// <summary>
		/// Executes the specified <paramref name="function"/> on each item in the <paramref name="list"/>, and
		/// replaces the original item with the result of that function.
		/// </summary>
		/// <typeparam name="T">The underlying list type</typeparam>
		/// <param name="list">The list to operate on.</param>
		/// <param name="function">The function to execute.</param>
		/// <returns>The modified list.</returns>
		public static IEnumerable<T> Each<T>(this IEnumerable<T> list, Func<T, T> function)
		{
			var array = list as T[] ?? list.ToArray();
			for (int i = 0; i < array.Length; i++)
				array[i] = function(array[i]);

			return array;
		}

		/// <summary>
		/// Executes the specified <paramref name="function"/> on each item in the <paramref name="list"/>, passing it the item index, and
		/// replaces the original item with the result of that function.
		/// </summary>
		/// <typeparam name="T">The underlying list type</typeparam>
		/// <param name="list">The list to operate on.</param>
		/// <param name="function">The function to execute.</param>
		/// <returns>The modified list.</returns>
		public static IEnumerable<T> Each<T>(this IEnumerable<T> list, Func<T, int, T> function)
		{
			var array = list as T[] ?? list.ToArray();
			for (int i = 0; i < array.Length; i++)
				array[i] = function(array[i], i);

			return array;
		}
	}
}
