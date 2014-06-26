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
		/// <returns>System.String.</returns>
		public static string GetConfigText(XmlNode moduleElement, string xpath)
		{
			var node = moduleElement.SelectSingleNode("mod:config/" + xpath, XmlNamespaces.Manager);
			return node == null ? null : node.InnerText.Trim();
		}
	}
}
