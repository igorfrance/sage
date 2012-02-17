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
