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
			get { return !String.IsNullOrEmpty(name); }
		}

		/// <summary>
		/// Gets the name of the current user.
		/// </summary>
		/// <returns>The name of the user on whose behalf the code is running.</returns>
		public string Name
		{
			get { return name; }
		}
	}
}
