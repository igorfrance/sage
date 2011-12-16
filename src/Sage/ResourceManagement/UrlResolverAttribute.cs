namespace Sage.ResourceManagement
{
	using System;

	/// <summary>
	/// Indicates that the attached class should be automatically registered with <see cref="UrlResolver"/>
	/// as a resolver for the specified <see cref="Scheme"/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class UrlResolverAttribute : Attribute
	{
		/// <summary>
		/// The scheme that the attached class resolves.
		/// </summary>
		public string Scheme { get; set; }
	}
}
