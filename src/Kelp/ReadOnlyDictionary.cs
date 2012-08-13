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

		/// <summary>
		/// Gets an <see cref="ICollection"/> containing the keys of the <see cref="IDictionary"/>.
		/// </summary>
		/// <returns>
		/// An <see cref="ICollection"/> containing the keys of the object that implements 
		/// <see cref="IDictionary"/>.
		/// </returns>
		public ICollection<TKey> Keys
		{
			get { return this.wrapped.Keys; }
		}

		/// <summary>
		/// Gets an <see cref="ICollection"/> containing the values in the <see cref="IDictionary"/>.
		/// </summary>
		/// <returns>
		/// An <see cref="ICollection"/> containing the values in the object that implements <see cref="IDictionary"/>.
		/// </returns>
		public ICollection<TValue> Values
		{
			get { return this.wrapped.Values; }
		}

		/// <summary>
		/// Gets the number of elements contained in the <see cref="ICollection"/>.
		/// </summary>
		/// <returns>
		/// The number of elements contained in the <see cref="ICollection"/>.
		/// </returns>
		public int Count
		{
			get { return this.wrapped.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="ICollection"/> is read-only.
		/// </summary>
		/// <returns>true if the <see cref="ICollection"/> is read-only; otherwise, false.
		/// </returns>
		public bool IsReadOnly
		{
			get { return true; }
		}

		/// <summary>
		/// Gets or sets the element with the specified key.
		/// </summary>
		/// <returns>
		/// The element with the specified key.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
		/// </exception>
		/// <exception cref="KeyNotFoundException">
		/// The property is retrieved and <paramref name="key"/> is not found.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		/// The property is set and the <see cref="IDictionary"/> is read-only.
		/// </exception>
		/// <param name="key">The key of the item.</param>
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

		/// <summary>
		/// Adds an element with the provided key and value to the <see cref="IDictionary"/>.
		/// </summary>
		/// <param name="key">The object to use as the key of the element to add.</param>
		/// <param name="value">The object to use as the value of the element to add.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
		/// </exception>
		/// <exception cref="T:System.ArgumentException">
		/// An element with the same key already exists in the <see cref="IDictionary"/>.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="IDictionary"/> is read-only.
		/// </exception>
		public void Add(TKey key, TValue value)
		{
			throw new NotSupportedException("This dictionary is read-only");
		}

		/// <summary>
		/// Determines whether the <see cref="IDictionary"/> contains an element with the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="IDictionary"/>.</param>
		/// <returns>
		/// true if the <see cref="IDictionary"/> contains an element with the key; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
		/// </exception>
		public bool ContainsKey(TKey key)
		{
			return this.wrapped.ContainsKey(key);
		}

		/// <summary>
		/// Removes the element with the specified key from the <see cref="IDictionary"/>.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <returns>
		/// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key"/> was not found in the original <see cref="IDictionary"/>.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="IDictionary"/> is read-only.
		/// </exception>
		public bool Remove(TKey key)
		{
			throw new NotSupportedException("This dictionary is read-only");
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key whose value to get.</param>
		/// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param>
		/// <returns>
		/// true if the object that implements <see cref="IDictionary"/> contains an element with the specified key; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
		/// </exception>
		public bool TryGetValue(TKey key, out TValue value)
		{
			return this.wrapped.TryGetValue(key, out value);
		}

		/// <summary>
		/// Adds an item to the <see cref="ICollection"/>.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="ICollection"/>.</param>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="ICollection"/> is read-only.
		/// </exception>
		public void Add(KeyValuePair<TKey, TValue> item)
		{
			throw new NotSupportedException("This dictionary is read-only");
		}

		/// <summary>
		/// Removes all items from the <see cref="ICollection"/>.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="ICollection"/> is read-only.
		/// </exception>
		public void Clear()
		{
			throw new NotSupportedException("This dictionary is read-only");
		}

		/// <summary>
		/// Determines whether the <see cref="ICollection"/> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="ICollection"/>.</param>
		/// <returns>
		/// true if <paramref name="item"/> is found in the <see cref="ICollection"/>; otherwise, false.
		/// </returns>
		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return this.wrapped.Contains(item);
		}

		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			this.wrapped.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="ICollection"/>.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="ICollection"/>.</param>
		/// <returns>
		/// true if <paramref name="item"/> was successfully removed from the <see cref="ICollection"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="ICollection"/>.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="ICollection"/> is read-only.
		/// </exception>
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			throw new NotSupportedException("This dictionary is read-only");
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="IEnumerator"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return this.wrapped.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return (this.wrapped as System.Collections.IEnumerable).GetEnumerator();
		}
	}
}
