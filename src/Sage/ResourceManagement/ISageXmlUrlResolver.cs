namespace Sage.ResourceManagement
{
	using System;
	using System.Xml;

	/// <summary>
	/// Defines an interface similar to <see cref="XmlUrlResolver"/>, with knowledge of Sage classes.
	/// </summary>
	public interface ISageXmlUrlResolver
	{
		/// <summary>
		/// Gets an <see cref="EntityResult"/> that represents the actual resource mapped from the specified <paramref name="uri"/>.
		/// </summary>
		/// <param name="parent">The <see cref="UrlResolver"/> that owns this resolved and calls this method.</param>
		/// <param name="context">The current <see cref="SageContext"/> under which this code is executing.</param>
		/// <param name="uri">The uri to resolve.</param>
		/// <returns>An object that represents the resource mapped from the specified <paramref name="uri"/>.</returns>
		EntityResult GetEntity(UrlResolver parent, SageContext context, string uri);
	}
}
