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
namespace Kelp
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Provides a generic read-only dictionary.
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	public class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		private readonly IDictionary<TKey, TValue> wrapped;

		/// <summary>
		/// Initializes a new instance of the <see cref="ReadOnlyDictionary&lt;TKey, TValue&gt;"/> class.
		/// </summary>
		public ReadOnlyDictionary()
		{
			this.wrapped = new Dictionary<TKey, TValue>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ReadOnlyDictionary&lt;TKey, TValue&gt;"/> class.
		/// </summary>
		/// <param name="wrapped">The dictionary.</param>
		public ReadOnlyDictionary(IDictionary<TKey, TValue> wrapped)
		{
			this.wrapped = wrapped;
		}

		public ICollection<TKey> Keys
		{
			get { return this.wrapped.Keys; }
		}

		public ICollection<TValue> Values
		{
			get { return this.wrapped.Values; }
		}

		public int Count
		{
			get { return this.wrapped.Count; }
		}

		public bool IsReadOnly
		{
			get { return true; }
		}

		public TValue this[TKey key]
		{
			get
			{
				return this.wrapped[key];
			}

			set
			{
				throw new NotSupportedException("This dictionary is read-only");
			}
		}

		public void Add(TKey key, TValue value)
		{
			throw new NotSupportedException("This dictionary is read-only");
		}

		public bool ContainsKey(TKey key)
		{
			return this.wrapped.ContainsKey(key);
		}

		public bool Remove(TKey key)
		{
			throw new NotSupportedException("This dictionary is read-only");
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return this.wrapped.TryGetValue(key, out value);
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			throw new NotSupportedException("This dictionary is read-only");
		}

		public void Clear()
		{
			throw new NotSupportedException("This dictionary is read-only");
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return this.wrapped.Contains(item);
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			this.wrapped.CopyTo(array, arrayIndex);
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			throw new NotSupportedException("This dictionary is read-only");
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return this.wrapped.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (this.wrapped as System.Collections.IEnumerable).GetEnumerator();
		}
	}
}
