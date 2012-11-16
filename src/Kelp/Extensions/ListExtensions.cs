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
	}
}
