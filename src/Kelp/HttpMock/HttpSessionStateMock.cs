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
	using System.Collections;
	using System.Collections.Specialized;
	using System.Web;
	using System.Web.SessionState;

	/// <summary>
	/// Mocks an <see cref="HttpSessionState"/>, enabling testing and independent execution of web context dependent code.
	/// </summary>
	public class HttpSessionStateMock : HttpSessionStateBase
	{
		private readonly SessionStateItemCollection sessionItems;

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpSessionStateMock"/> class, using the specified session item collection.
		/// </summary>
		/// <param name="sessionItems">The session items.</param>
		public HttpSessionStateMock(SessionStateItemCollection sessionItems)
		{
			this.sessionItems = sessionItems;
		}

		/// <inheritdoc/>
		public override int Count
		{
			get
			{
				return sessionItems.Count;
			}
		}

		/// <inheritdoc/>
		public override NameObjectCollectionBase.KeysCollection Keys
		{
			get
			{
				return sessionItems.Keys;
			}
		}

		/// <inheritdoc/>
		public override object this[string name]
		{
			get
			{
				return sessionItems[name];
			}

			set
			{
				sessionItems[name] = value;
			}
		}

		/// <inheritdoc/>
		public override object this[int index]
		{
			get
			{
				return sessionItems[index];
			}

			set
			{
				sessionItems[index] = value;
			}
		}

		/// <inheritdoc/>
		public override void Add(string name, object value)
		{
			sessionItems[name] = value;
		}

		/// <inheritdoc/>
		public override IEnumerator GetEnumerator()
		{
			return sessionItems.GetEnumerator();
		}

		/// <inheritdoc/>
		public override void Remove(string name)
		{
			sessionItems.Remove(name);
		}
	}
}