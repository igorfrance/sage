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
