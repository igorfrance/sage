namespace Kelp.Core
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	/// <summary>
	/// Represents a generic collection of key/value pairs that are ordered independently of the key and value.
	/// </summary>
	/// <typeparam name="TKey">The type of the keys in the dictionary</typeparam>
	/// <typeparam name="TValue">The type of the values in the dictionary</typeparam>
	public class OrderedDictionary<TKey, TValue> : IDictionary, IDictionary<TKey, TValue>
	{
		private const int DefaultInitialCapacity = 0;

		private static readonly string keyTypeName = typeof(TKey).FullName;
		private static readonly string valueTypeName = typeof(TValue).FullName;
		private static readonly bool valueTypeIsReferenceType = !typeof(ValueType).IsAssignableFrom(typeof(TValue));

		private readonly IEqualityComparer<TKey> comparer;
		private readonly int initialCapacity;
		private readonly object syncObj = new object();

		private Dictionary<TKey, TValue> dictionary;
		private List<KeyValuePair<TKey, TValue>> list;

		/// <summary>
		/// Initializes a new instance of the <see cref="OrderedDictionary{TKey,TValue}"/> class. 
		/// Initializes a new instance of the OrderedDictionary class.
		/// </summary>
		public OrderedDictionary()
			: this(DefaultInitialCapacity, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OrderedDictionary{TKey,TValue}"/> class. 
		/// Initializes a new instance of the OrderedDictionary class using the specified initial capacity.
		/// </summary>
		/// <param name="capacity">
		/// The initial number of elements that the 
		/// <see cref="OrderedDictionary{TKey,TValue}"/> can contain.
		/// </param>
		public OrderedDictionary(int capacity)
			: this(capacity, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OrderedDictionary{TKey,TValue}"/> class. 
		/// Initializes a new instance of the OrderedDictionary class using the specified comparer.
		/// </summary>
		/// <param name="comparer">
		/// The <see cref="IEqualityComparer{TKey}"/> to use when 
		/// comparing keys, or <null/> to use the default <see cref="EqualityComparer{TKey}"/> 
		/// for the type of the key.
		/// </param>
		public OrderedDictionary(IEqualityComparer<TKey> comparer)
			: this(DefaultInitialCapacity, comparer)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OrderedDictionary{TKey,TValue}"/> class. 
		/// Initializes a new instance of the OrderedDictionary class using the specified initial capacity and comparer.
		/// </summary>
		/// <param name="capacity">
		/// The initial number of elements that the 
		/// <see cref="OrderedDictionary{TKey,TValue}"/> collection can contain.
		/// </param>
		/// <param name="comparer">
		/// The <see cref="IEqualityComparer{TKey}"/> to use when 
		/// comparing keys, or <null/> to use the default <see cref="EqualityComparer{TKey}"/> 
		/// for the type of the key.
		/// </param>
		public OrderedDictionary(int capacity, IEqualityComparer<TKey> comparer)
		{
			if (0 > capacity)
				throw new ArgumentOutOfRangeException("capacity", "The capacity must be  non-negative");

			initialCapacity = capacity;
			this.comparer = comparer;
		}

		/// <summary>
		/// Gets the number of key/values pairs contained in the 
		/// <see cref="OrderedDictionary{TKey,TValue}"/> collection.
		/// </summary>
		/// <value>The number of key/value pairs contained in the 
		/// <see cref="OrderedDictionary{TKey,TValue}"/> collection.</value>
		public int Count
		{
			get
			{
				return this.List.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the 
		/// <see cref="OrderedDictionary{TKey,TValue}"/> collection is read-only.
		/// </summary>
		/// <value><see langword="true"/> if the 
		/// <see cref="OrderedDictionary{TKey,TValue}"/> is read-only; 
		/// otherwise, <see langword="false"/>. The default is <see langword="false"/>.</value>
		/// <remarks>
		/// A collection that is read-only does not allow the addition, removal, or modification of elements 
		/// after the collection is created.
		/// <para>A collection that is read-only is simply a collection with a wrapper that prevents modification of 
		/// the collection; therefore, if changes are made to the underlying collection, the read-only collection reflects 
		/// those changes.</para>
		/// </remarks>
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets an <see cref="System.Collections.Generic.ICollection{TKey}"/> object 
		/// containing the keys in the <see cref="OrderedDictionary{TKey,TValue}"/>.
		/// </summary>
		/// <value>An <see cref="System.Collections.Generic.ICollection{TKey}"/> object 
		/// containing the keys in the <see cref="OrderedDictionary{TKey,TValue}"/>.</value>
		/// <remarks>The returned 
		/// <see cref="System.Collections.Generic.ICollection{TKey}"/> object is not a 
		/// static copy; instead, the collection refers back to the keys in the original 
		/// <see cref="OrderedDictionary{TKey,TValue}"/>. 
		/// Therefore, changes to the 
		/// <see cref="OrderedDictionary{TKey,TValue}"/> continue to be 
		/// reflected in the key collection.</remarks>
		public ICollection<TKey> Keys
		{
			get
			{
				return this.Dictionary.Keys;
			}
		}

		/// <summary>
		/// Gets an <see cref="ICollection{TValue}"/> object containing the values in 
		/// the <see cref="OrderedDictionary{TKey,TValue}"/>.
		/// </summary>
		/// <value>An <see cref="ICollection{TValue}"/> object containing the values in 
		/// the <see cref="OrderedDictionary{TKey,TValue}"/>.</value>
		/// <remarks>The returned <see cref="ICollection{TValue}"/> object is not a static copy; 
		/// instead, the collection refers back to the values in the original 
		/// <see cref="OrderedDictionary{TKey,TValue}"/>. Therefore, 
		/// changes to the <see cref="OrderedDictionary{TKey,TValue}"/> 
		/// continue to be reflected in the value collection.</remarks>
		public ICollection<TValue> Values
		{
			get
			{
				return this.Dictionary.Values;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.IDictionary"/> object has a fixed size.
		/// </summary>
		/// <value></value>
		/// <returns><c>true</c> if the <see cref="T:System.Collections.IDictionary"/> object has a fixed size; otherwise, <c>false</c>.
		/// </returns>
		public bool IsFixedSize
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether access to the <see cref="ICollection"/> is synchronized (thread safe).
		/// </summary>
		/// <value></value>
		/// <returns><c>true</c> if access to the <see cref="ICollection"/> is synchronized (thread safe); otherwise, <c>false</c>.
		/// </returns>
		public bool IsSynchronized
		{
			get { return false; }
		}

		/// <summary>
		/// Gets an object that can be used to synchronize access to the <see cref="ICollection"/>.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// An object that can be used to synchronize access to the <see cref="ICollection"/>.
		/// </returns>
		public object SyncRoot
		{
			get { return syncObj; }
		}

		/// <summary>
		/// Gets an <see cref="ICollection"/> object containing the keys of the <see cref="T:System.Collections.IDictionary"/> object.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// An <see cref="ICollection"/> object containing the keys of the <see cref="T:System.Collections.IDictionary"/> object.
		/// </returns>
		ICollection IDictionary.Keys
		{
			get
			{
				return (ICollection) this.Keys;
			}
		}

		/// <summary>
		/// Gets an <see cref="ICollection"/> object containing the values in the <see cref="T:System.Collections.IDictionary"/> object.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// An <see cref="ICollection"/> object containing the values in the <see cref="T:System.Collections.IDictionary"/> object.
		/// </returns>
		ICollection IDictionary.Values
		{
			get
			{
				return (ICollection) this.Values;
			}
		}

		/// <summary>
		/// Gets the dictionary object that stores the keys and values
		/// </summary>
		/// <value>The dictionary object that stores the keys and values for the 
		/// <see cref="OrderedDictionary{TKey,TValue}"/></value>
		/// <remarks>Accessing this property will create the dictionary object if necessary</remarks>
		private Dictionary<TKey, TValue> Dictionary
		{
			get
			{
				return this.dictionary ?? (this.dictionary = new Dictionary<TKey, TValue>(this.initialCapacity, this.comparer));
			}
		}

		/// <summary>
		/// Gets the list object that stores the key/value pairs.
		/// </summary>
		/// <value>The list object that stores the key/value pairs for the 
		/// <see cref="OrderedDictionary{TKey,TValue}"/></value>
		/// <remarks>Accessing this property will create the list object if necessary.</remarks>
		private List<KeyValuePair<TKey, TValue>> List
		{
			get { return this.list ?? (this.list = new List<KeyValuePair<TKey, TValue>>(this.initialCapacity)); }
		}

		/// <summary>
		/// Gets or sets the value with the specified key.
		/// </summary>
		/// <param name="key">The key of the value to get or set.</param>
		/// <value>The value associated with the specified key. If the specified key is not found, attempting to get it 
		/// returns <null/>, and attempting to set it creates a new element using the specified key.</value>
		public TValue this[TKey key]
		{
			get
			{
				return this.Dictionary[key];
			}

			set
			{
				if (this.Dictionary.ContainsKey(key))
				{
					this.Dictionary[key] = value;
					this.List[this.IndexOfKey(key)] = new KeyValuePair<TKey, TValue>(key, value);
				}
				else
				{
					this.Add(key, value);
				}
			}
		}

		/// <summary>
		/// Gets or sets the value at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the value to get or set.</param>
		/// <value>The value of the item at the specified index.</value>
		public TValue this[int index]
		{
			get
			{
				return this.List[index].Value;
			}

			set
			{
				if (index >= this.Count || index < 0)
					throw new ArgumentOutOfRangeException("index", "The index must be non-negative");

				TKey key = this.List[index].Key;

				this.List[index] = new KeyValuePair<TKey, TValue>(key, value);
				this.Dictionary[key] = value;
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="System.Object"/> with the specified key.
		/// </summary>
		/// <param name="key">The key of the item to get.</param>
		/// <returns>The element with the specified key.</returns>
		public object this[object key]
		{
			get
			{
				return this[ConvertToKeyType(key)];
			}

			set
			{
				this[ConvertToKeyType(key)] = ConvertToValueType(value);
			}
		}

		/// <summary>
		/// Inserts a new entry into the <c>OrderedDictionary</c> collection with the specified key and value at the specified 
		/// index.
		/// </summary>
		/// <param name="index">The zero-based index at which the element should be inserted.</param>
		/// <param name="key">The key of the entry to add.</param>
		/// <param name="value">The value of the entry to add. The value can be <null/> if the type of the values in the 
		/// dictionary is a reference type.</param>
		public void Insert(int index, TKey key, TValue value)
		{
			if (index > this.Count || index < 0)
				throw new ArgumentOutOfRangeException("index");

			this.Dictionary.Add(key, value);
			this.List.Insert(index, new KeyValuePair<TKey, TValue>(key, value));
		}

		/// <summary>
		/// Returns the zero-based index of the specified key in the <c>OrderedDictionary</c>.
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="OrderedDictionary{TKey,TValue}"/></param>
		/// <returns>The zero-based index of <paramref name="key"/>, if <paramref name="key"/> is found in the 
		/// <see cref="OrderedDictionary{TKey,TValue}"/>; otherwise, -1</returns>
		/// <remarks>This method performs a linear search; therefore it has a cost of O(n) at worst.</remarks>
		public int IndexOfKey(TKey key)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			for (int index = 0; index < this.List.Count; index++)
			{
				KeyValuePair<TKey, TValue> entry = this.List[index];
				TKey next = entry.Key;
				if (null != this.comparer)
				{
					if (this.comparer.Equals(next, key))
					{
						return index;
					}
				}
				else if (next.Equals(key))
				{
					return index;
				}
			}

			return -1;
		}

		/// <summary>
		/// Removes the entry at the specified index from the 
		/// <see cref="OrderedDictionary{TKey,TValue}"/> collection.
		/// </summary>
		/// <param name="index">The zero-based index of the entry to remove.</param>
		public void RemoveAt(int index)
		{
			if (index >= this.Count || index < 0)
				throw new ArgumentOutOfRangeException("index", "The index must be non-negative");

			TKey key = this.List[index].Key;

			this.List.RemoveAt(index);
			this.Dictionary.Remove(key);
		}

		/// <summary>
		/// Adds an entry with the specified key and value into the 
		/// <see cref="OrderedDictionary{TKey,TValue}"/> collection with the lowest 
		/// available index.
		/// </summary>
		/// <param name="key">The key of the entry to add.</param>
		/// <param name="value">The value of the entry to add. This value can be <null/>.</param>
		/// <returns>The index of the newly added entry</returns>
		/// <remarks>A key cannot be <null/>, but a value can be.
		/// <para>You can also use the <c>Item</c> property to add new elements by 
		/// setting the value of a key that does not exist in the 
		/// <see cref="OrderedDictionary{TKey,TValue}"/> collection; however, if the 
		/// specified key already exists in the 
		/// <see cref="OrderedDictionary{TKey,TValue}"/>, setting the 
		/// <c>Item</c> property overwrites the old value. 
		/// In contrast, the <c>Add</c> method does not modify existing elements.</para></remarks>
		public int Add(TKey key, TValue value)
		{
			this.Dictionary.Add(key, value);
			this.List.Add(new KeyValuePair<TKey, TValue>(key, value));
			return this.Count - 1;
		}

		/// <summary>
		/// Adds an item to the <see cref="ICollection"/>.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="ICollection"/>.</param>
		public void Add(KeyValuePair<TKey, TValue> item)
		{
			this.Add(item.Key, item.Value);
		}

		/// <summary>
		/// Removes all elements from the 
		/// <see cref="OrderedDictionary{TKey,TValue}"/> collection.
		/// </summary>
		/// <remarks>The capacity is not changed as a result of calling this method.</remarks>
		public void Clear()
		{
			this.Dictionary.Clear();
			this.List.Clear();
		}

		/// <summary>
		/// Determines whether the <see cref="OrderedDictionary{TKey,TValue}"/> 
		/// collection contains a specific key.
		/// </summary>
		/// <param name="key">The key to locate in the 
		/// <see cref="OrderedDictionary{TKey,TValue}"/> collection.</param>
		/// <returns><see langword="true"/> if the 
		/// <see cref="OrderedDictionary{TKey,TValue}"/> collection contains an 
		/// element with the specified key; otherwise, <see langword="false"/>.</returns>
		public bool ContainsKey(TKey key)
		{
			return this.Dictionary.ContainsKey(key);
		}

		/// <summary>
		/// Determines whether the <see cref="ICollection"/> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="ICollection"/>.</param>
		/// <returns>
		/// <c>true</c> if <paramref name="item"/> is found in the <see cref="ICollection"/>; otherwise, <c>false</c>.
		/// </returns>
		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return ((ICollection<KeyValuePair<TKey, TValue>>) this.Dictionary).Contains(item);
		}

		/// <summary>
		/// Copies the elements of the <see cref="ICollection"/> to an <see cref="Array"/>, 
		/// starting at a particular <see cref="Array"/> index.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied from 
		/// <see cref="ICollection"/>. The <see cref="Array"/> must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<TKey, TValue>>) this.Dictionary).CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Removes the entry with the specified key from the 
		/// <see cref="OrderedDictionary{TKey,TValue}"/> collection.
		/// </summary>
		/// <param name="key">The key of the entry to remove</param>
		/// <returns><see langword="true"/> if the key was found and the corresponding element was removed; 
		/// otherwise, <see langword="false"/></returns>
		/// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
		public bool Remove(TKey key)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			int index = this.IndexOfKey(key);
			if (index >= 0)
			{
				if (this.Dictionary.Remove(key))
				{
					this.List.RemoveAt(index);
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="ICollection"/>.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="ICollection"/>.</param>
		/// <returns>
		/// <c>true</c> if <paramref name="item"/> was successfully removed from the <see cref="ICollection"/>; 
		/// otherwise, <c>false</c>. This method also returns <c>false</c> if <paramref name="item"/> is not found in the original <see cref="ICollection"/>.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="ICollection"/> is read-only.
		/// </exception>
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			return Remove(item.Key);
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key of the value to get.</param>
		/// <param name="value">When this method returns, contains the value associated with the specified key, if the key is 
		/// found; otherwise, the default value for the type of <paramref name="value"/>. This parameter can be passed 
		/// uninitialized.</param>
		/// <returns><see langword="true"/> if the 
		/// <see cref="OrderedDictionary{TKey,TValue}"/> contains an element
		/// with the specified key; otherwise, <see langword="false"/>.</returns>
		public bool TryGetValue(TKey key, out TValue value)
		{
			return this.Dictionary.TryGetValue(key, out value);
		}

		/// <summary>
		/// Adds an element with the provided key and value to the <see cref="IDictionary"/> object.
		/// </summary>
		/// <param name="key">The <see cref="T:System.Object"/> to use as the key of the element to add.</param>
		/// <param name="value">The <see cref="T:System.Object"/> to use as the value of the element to add.</param>
		/// <exception cref="ArgumentNullException">
		/// 	<paramref name="key"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// An element with the same key already exists in the <see cref="IDictionary"/> object.
		/// </exception>
		public void Add(object key, object value)
		{
			this.Add(ConvertToKeyType(key), ConvertToValueType(value));
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.IDictionary"/> object contains an element with the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="T:System.Collections.IDictionary"/> object.</param>
		/// <returns>
		/// <c>true</c> if the <see cref="T:System.Collections.IDictionary"/> contains an element with the key; otherwise, <c>false</c>.
		/// </returns>
		public bool Contains(object key)
		{
			return this.ContainsKey(ConvertToKeyType(key));
		}

		/// <summary>
		/// Removes the element with the specified key from the <see cref="T:System.Collections.IDictionary"/> object.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		public void Remove(object key)
		{
			this.Remove(ConvertToKeyType(key));
		}

		/// <summary>
		/// Copies the elements of the <see cref="ICollection"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="ICollection"/>. The <see cref="Array"/> must have zero-based indexing.</param>
		/// <param name="index">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		public void CopyTo(Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException("array");
			if (array.Length > 0 && array.GetValue(0) is Array)
				throw new ArgumentException("The array must not be multi-dimensional", "array");
			if (index < 0)
				throw new ArgumentOutOfRangeException("index", "The index must be non-negative");
			if (index >= array.Length)
				throw new ArgumentException("The index must be smaller than the length of the array", "index");
			if (dictionary.Count > (array.Length - index))
				throw new ArgumentException("The number of elements in current collection exceeds the available space in the specified array from the specified index to the array's end.");

			for (int i = 0; i < this.list.Count; i++)
			{
				array.SetValue(this.list[i].Value, index + i);
			}
		}

		public override string ToString()
		{
			return "Count = " + this.Count;
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return List.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return List.GetEnumerator();
		}

		/// <summary>
		/// Returns an <see cref="IDictionaryEnumerator"/> object for the <see cref="IDictionary"/> object.
		/// </summary>
		/// <returns>
		/// An <see cref="IDictionaryEnumerator"/> object for the <see cref="IDictionary"/> object.
		/// </returns>
		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return this.Dictionary.GetEnumerator();
		}

		/// <summary>
		/// Adds an element with the provided key and value to the <see cref="IDictionary"/>.
		/// </summary>
		/// <param name="key">The object to use as the key of the element to add.</param>
		/// <param name="value">The object to use as the value of the element to add.</param>
		void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
		{
			Add(key, value);
		}

		/// <summary>
		/// Converts the object passed as a key to the key type of the dictionary
		/// </summary>
		/// <param name="keyObject">The key object to check</param>
		/// <returns>The key object, cast as the key type of the dictionary</returns>
		private static TKey ConvertToKeyType(object keyObject)
		{
			if (null == keyObject)
				throw new ArgumentNullException("keyObject");

			if (keyObject is TKey)
				return (TKey) keyObject;

			throw new ArgumentException(string.Format("'key' must be of type {0}key", keyTypeName));
		}

		/// <summary>
		/// Converts the object passed as a value to the value type of the dictionary
		/// </summary>
		/// <param name="value">The object to convert to the value type of the dictionary</param>
		/// <returns>The value object, converted to the value type of the dictionary</returns>
		private static TValue ConvertToValueType(object value)
		{
			if (null == value)
			{
				if (valueTypeIsReferenceType)
					return default(TValue);

				throw new ArgumentNullException("value");
			}

			if (value is TValue)
				return (TValue) value;

			throw new ArgumentException(string.Format("'value' must be of type {0}value", valueTypeName));
		}
	}
}
