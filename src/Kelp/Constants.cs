namespace Kelp
{
	using System.Xml;

	/// <summary>
	/// Defines the namespaces and the namespace manager in use throughout the system.
	/// </summary>
	public static class XmlNamespaces
	{
		/// <summary>
		/// Defines the prefix for the main Kelp namespace.
		/// </summary>
		public const string KelpNsPrefix = "kelp";

		/// <summary>
		/// Defines the main Kelp namespace.
		/// </summary>
		public const string KelpNamespace = "http://www.cycle99.com/projects/kelp";

		private static volatile XmlNamespaceManager nsman;

		/// <summary>
		/// Gets the <see cref="XmlNamespaceManager"/> that can be used everywhere where selecting with namespaces needs to be done.
		/// </summary>
		public static XmlNamespaceManager Manager
		{
			get
			{
				if (nsman == null)
				{
					lock (KelpNsPrefix)
					{
						if (nsman == null)
						{
							var temp = new XmlNamespaceManager(new NameTable());
							temp.AddNamespace(KelpNsPrefix, KelpNamespace);

							nsman = temp;
						}
					}
				}

				return nsman;
			}
		}
	}
}
