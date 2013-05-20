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
