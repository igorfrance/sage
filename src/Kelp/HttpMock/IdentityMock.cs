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
	using System;
	using System.Security.Principal;

	/// <summary>
	/// Provides a class for mocking the <see cref="IIdentity"/>. 
	/// </summary>
	public class IdentityMock : IIdentity
	{
		private readonly string name;

		/// <summary>
		/// Initializes a new instance of the <see cref="IdentityMock"/> class using the specified 
		/// <paramref name="userName"/>.
		/// </summary>
		/// <param name="userName">The user name associated with the identity being mocked.</param>
		public IdentityMock(string userName)
		{
			name = userName;
		}

		/// <summary>
		/// Gets the type of authentication used.
		/// </summary>
		public string AuthenticationType
		{
			get
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the user has been authenticated.
		/// </summary>
		/// <returns>true if the user was authenticated; otherwise, false.</returns>
		public bool IsAuthenticated
		{
			get { return !string.IsNullOrEmpty(name); }
		}

		/// <summary>
		/// Gets the name of the current user.
		/// </summary>
		/// <returns>The name of the user on whose behalf the code is running.</returns>
		public string Name
		{
			get { return name; }
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Name;
		}
	}
}
