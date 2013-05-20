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