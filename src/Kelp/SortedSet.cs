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
namespace Kelp
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Implements a generic sorted set.
	/// </summary>
	/// <typeparam name="T">The type that this set contains</typeparam>
	public class SortedSet<T> : SortedList<T, byte>, IEnumerable<T>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SortedSet&lt;T&gt;"/> class, using the specified
		/// initial list.
		/// </summary>
		/// <param name="initialList">The initial list to populate this set with.</param>
		public SortedSet(IEnumerable<T> initialList)
		{
			foreach (T item in initialList)
				this.Add(item);
		}
	
		/// <summary>
		/// Gets the first item from the set.
		/// </summary>
		public T Min
		{
			get
			{
				if (Count >= 1)
					return Keys[0];

				return default(T);
			}
		}

		/// <summary>
		/// Gets the last item from the set.
		/// </summary>
		public T Max
		{
			get
			{
				if (Count >= 1)
					return Keys[Keys.Count - 1];

				return default(T);
			}
		}

		/// <summary>
		/// Determines whether this instance contains the specified value.
		/// </summary>
		/// <param name="value">The value to look for.</param>
		/// <returns>
		///   <c>true</c> if this instance contains the specified value; otherwise, <c>false</c>.
		/// </returns>
		public bool Contains(T value)
		{
			return ContainsKey(value);
		}

		/// <summary>
		/// Adds the specified value to this set.
		/// </summary>
		/// <param name="value">The value.</param>
		public void Add(T value)
		{
			base.Add(value, 0);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this.Keys.GetEnumerator();
		}
	}
}
