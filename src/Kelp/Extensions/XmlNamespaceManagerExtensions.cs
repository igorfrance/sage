namespace Kelp.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Xml;

	/// <summary>
	/// Provides extension methods for <see cref="XmlNamespaceManager"/>.
	/// </summary>
	public static class XmlNamespaceManagerExtensions
	{
		/// <summary>
		/// Creates a copy of <paramref name="instance"/> with (local) namespaces from <paramref name="other"/> added
		/// to it.
		/// </summary>
		/// <param name="instance">The instance being operated on.</param>
		/// <param name="other">The <see cref="XmlNamespaceManager"/> to copy the namespaces from</param>
		/// <returns>A copy of <paramref name="instance"/> with (local) namespaces from <paramref name="other"/> added
		/// to it</returns>
		public static XmlNamespaceManager MergeWith(this XmlNamespaceManager instance, XmlNamespaceManager other)
		{
			var result = new XmlNamespaceManager(instance.NameTable);

			var nspairs = instance.GetNamespacesInScope(XmlNamespaceScope.Local);
			foreach (string key in nspairs.Keys)
				result.AddNamespace(key, nspairs[key]);

			nspairs = other.GetNamespacesInScope(XmlNamespaceScope.Local);
			foreach (string key in nspairs.Keys)
				result.AddNamespace(key, nspairs[key]);

			return result;
		}
	}
}
