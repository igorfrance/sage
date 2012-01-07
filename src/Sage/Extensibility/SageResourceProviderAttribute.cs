namespace Sage.Extensibility
{
	using System;

	/// <summary>
	/// Indicates that a method can be used to get a specific sage resource.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class SageResourceProviderAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SageResourceProviderAttribute"/> class, using 
		/// </summary>
		/// <param name="resourceName">The name of the resource that the method provides.</param>
		public SageResourceProviderAttribute(string resourceName)
		{
			this.ResourceName = resourceName;
		}

		/// <summary>
		/// Gets the name of the resource that the method provides.
		/// </summary>
		public string ResourceName { get; private set; }
	}
}
