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
	using System.Linq;
	using System.Security.Principal;

	/// <summary>
	/// Provides a class for mocking an <code>IPrincipal</code>. 
	/// </summary>
	public class PrincipalMock : IPrincipal
	{
		private readonly IIdentity identity;
		private readonly string[] roles;

		/// <summary>
		/// Initializes a new instance of the <see cref="PrincipalMock"/> class.
		/// </summary>
		/// <param name="identity">The identity of the principal.</param>
		/// <param name="roles">The roles of the principal.</param>
		public PrincipalMock(IIdentity identity, string[] roles)
		{
			this.identity = identity;
			this.roles = roles;
		}

		/// <summary>
		/// Gets the identity of the current principal.
		/// </summary>
		/// <returns>The <see cref="IIdentity"/> object associated with the current principal.</returns>
		public IIdentity Identity
		{
			get { return identity; }
		}

		/// <summary>
		/// Determines whether the current principal belongs to the specified role.
		/// </summary>
		/// <param name="role">The name of the role for which to check membership.</param>
		/// <returns>
		/// true if the current principal is a member of the specified role; otherwise, false.
		/// </returns>
		public bool IsInRole(string role)
		{
			if (roles == null)
				return false;
			return roles.Contains(role);
		}
	}
}
