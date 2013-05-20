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

	/// <summary>
	/// Implements a comparer that uses a delegate method for comparison.
	/// </summary>
	/// <typeparam name="T">The type that is being compared</typeparam>
	public class ComparisonComparer<T> : IComparer<T>, IComparer
	{
		private readonly Comparison<T> comparison;

		/// <summary>
		/// Initializes a new instance of the <see cref="ComparisonComparer{T}" /> class.
		/// </summary>
		/// <param name="comparison">The comparison delegate method to use.</param>
		public ComparisonComparer(Comparison<T> comparison)
		{
			this.comparison = comparison;
		}

		/// <inheritdoc/>
		public int Compare(T x, T y)
		{
			return this.comparison(x, y);
		}

		/// <inheritdoc/>
		public int Compare(object o1, object o2)
		{
			return this.comparison((T) o1, (T) o2);
		}
	}
}
