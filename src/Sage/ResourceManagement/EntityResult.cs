namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Xml;

	/// <summary>
	/// Provides the result of calling <see cref="ISageXmlUrlResolver.GetEntity"/> on a give <c>uri</c>.
	/// </summary>
	public class EntityResult
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EntityResult"/> class.
		/// </summary>
		public EntityResult()
		{
			this.Dependencies = new List<string>();
		}

		/// <summary>
		/// Gets or sets the entity that was opened.
		/// </summary>
		/// <value>
		/// This will typically be an <see cref="XmlReader"/> around the resource that was opened.
		/// </value>
		public object Entity { get; set; }

		/// <summary>
		/// Gets or sets the list of files that the resource that was opened depends on.
		/// </summary>
		public IList<string> Dependencies { get; set; }
	}
}
