namespace Sage.Configuration
{
	using System.Xml;

	/// <summary>
	/// Represents a Link Pattern, for constructing internal and external Urls.
	/// </summary>
	public struct LinkInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LinkInfo"/> structure.
		/// </summary>
		/// <param name="configNode">The configuration XML element that represents this link.</param>
		public LinkInfo(XmlElement configNode)
			: this()
		{
			this.Name = configNode.GetAttribute("name");
			this.Pattern = configNode.GetAttribute("pattern");
		}

		/// <summary>
		/// Gets the name of this link.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the format pattern of this link, e.g. <c>products/{tab}/{page}/</c>.
		/// </summary>
		public string Pattern { get; private set; }
	}
}
