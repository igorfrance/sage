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
namespace Kelp.HttpMock
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;

	/// <summary>
	/// Provides a class for mocking an <code>HttpApplicationStateBase</code>. 
	/// </summary>
	public class HttpApplicationStateMock : HttpApplicationStateBase
	{
		private readonly Dictionary<string, object> collection = new Dictionary<string, object>();

		/// <summary>
		/// Gets the access keys for the objects in the collection.
		/// </summary>
		/// <returns>An array of state object keys.</returns>
		public override string[] AllKeys
		{
			get
			{
				return collection.Keys.ToArray();
			}
		}

		/// <summary>
		/// Gets the number of objects in the collection.
		/// </summary>
		/// <returns>The number of objects in the collection.</returns>
		public override int Count
		{
			get
			{
				return collection.Count;
			}
		}

		/// <summary>
		/// Gets a state object by index.
		/// </summary>
		/// <param name="index">The index of the object to get</param>
		/// <returns>The object referenced by <paramref name="index"/>.</returns>
		public override object this[int index]
		{
			get
			{
				string key = this.GetKey(index);
				return this[key];
			}
		}

		/// <summary>
		/// Gets a state object by name.
		/// </summary>
		/// <param name="name">The name of the object to get</param>
		/// <returns>The object referenced by <paramref name="name"/>.</returns>
		public override object this[string name]
		{
			get
			{
				if (collection.ContainsKey(name))
					return collection[name];

				return null;
			}

			set
			{
				if (collection.ContainsKey(name))
					collection[name] = value;
				else
					collection.Add(name, value);
			}
		}

		/// <summary>
		/// Adds a new object to the collection.
		/// </summary>
		/// <param name="name">The name of the object to add to the collection.</param>
		/// <param name="value">The value of the object.</param>
		public override void Add(string name, object value)
		{
			collection.Add(name, value);
		}

		/// <summary>
		/// Removes all objects from the collection.
		/// </summary>
		public override void Clear()
		{
			collection.Clear();
		}

		/// <summary>
		/// Gets a state object by index.
		/// </summary>
		/// <param name="index">The index of the application state object to get.</param>
		/// <returns>
		/// The object referenced by <paramref name="index"/>.
		/// </returns>
		public override object Get(int index)
		{
			string key = GetKey(index);
			return this[key];
		}

		/// <summary>
		/// Gets a state object by name.
		/// </summary>
		/// <param name="name">The name of the object to get.</param>
		/// <returns>
		/// The object referenced by <paramref name="name"/>.
		/// </returns>
		public override object Get(string name)
		{
			return this[name];
		}

		/// <summary>
		/// Gets the name of a state object by index.
		/// </summary>
		/// <param name="index">The index of the application state object to get.</param>
		/// <returns>
		/// The name of the application state object.
		/// </returns>
		public override string GetKey(int index)
		{
			string[] keys = collection.Keys.ToArray();
			if (index >= 0 && index < keys.Length)
				return keys[index];

			return null;
		}

		/// <summary>
		/// Removes the named object from the collection.
		/// </summary>
		/// <param name="name">The name of the object to remove from the collection.</param>
		public override void Remove(string name)
		{
			collection.Remove(name);
		}

		/// <summary>
		/// Removes all objects from the collection.
		/// </summary>
		public override void RemoveAll()
		{
			collection.Clear();
		}

		/// <summary>
		/// Removes a state object specified by index from the collection.
		/// </summary>
		/// <param name="index">The position in the collection of the item to remove.</param>
		public override void RemoveAt(int index)
		{
			string key = GetKey(index);
			if (key != null)
				collection.Remove(key);
		}

		/// <summary>
		/// Updates the value of an object in the collection.
		/// </summary>
		/// <param name="name">The name of the object to update.</param>
		/// <param name="value">The updated value of the object.</param>
		public override void Set(string name, object value)
		{
			this[name] = value;
		}
	}
}
