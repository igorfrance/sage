namespace Sage.Modules
{
	using System;
	using System.Xml;

	/// <summary>
	/// Provides several shared utility methods for use with Sage modules.
	/// </summary>
	public static class Module
	{
		/// <summary>
		/// Gets the configuration text.
		/// </summary>
		/// <param name="moduleElement">The module element.</param>
		/// <param name="xpath">The xpath.</param>
		/// <returns>The text value of the node selected with the specified xpath</returns>
		public static string GetConfigText(XmlNode moduleElement, string xpath)
		{
			var node = moduleElement.SelectSingleNode("mod:config/" + xpath, XmlNamespaces.Manager);
			return node == null ? null : node.InnerText.Trim();
		}

		/// <summary>
		/// Selects the specified nodes within the <c>mod:config</c> section of the specified <paramref name="moduleElement"/>.
		/// </summary>
		/// <param name="moduleElement">The module element.</param>
		/// <param name="xpath">The xpath.</param>
		/// <returns>The selected nodes</returns>
		public static XmlNodeList GetConfigNodes(XmlNode moduleElement, string xpath)
		{
			return moduleElement.SelectNodes("mod:config/" + xpath, XmlNamespaces.Manager);
		}
	}
}
