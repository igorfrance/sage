﻿namespace Kelp.Core
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
