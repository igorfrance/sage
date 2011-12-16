namespace Kelp.Core.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Xml;

	using Newtonsoft.Json;
	using Format = Newtonsoft.Json.Formatting;

	/// <summary>
	/// Implements extension methods to the <see cref="XmlNode"/> class.
	/// </summary>
	public static class XmlNodeExtensions
	{
		private const string AttribJsonFormat1 = "{0}: ";
		private const string AttribJsonFormat2 = "{0}: {1}";

		/// <summary>
		/// Appends the specified element to the current node and returns it.
		/// </summary>
		/// <param name="instance">The target node to append the element to.</param>
		/// <param name="child">The child element to append.</param>
		/// <returns>The <paramref name="child"/> element that was appended.</returns>
		/// <exception cref="ArgumentNullException">
		/// If the current <paramref name="instance"/> is <c>null</c>, or 
		/// if the specified <paramref name="child"/> is <c>null</c>.
		/// </exception>
		public static XmlElement AppendElement(this XmlNode instance, XmlNode child)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");
			if (child == null)
				throw new ArgumentNullException("child");

			return (XmlElement)instance.AppendChild(child);
		}

		/// <summary>
		/// Appends the specified element to the current node and returns it.
		/// </summary>
		/// <param name="instance">The target node to append the element to.</param>
		/// <param name="elementName">The name of the element to create.</param>
		/// <returns>The element that was created and appended to <paramref name="instance"/>.</returns>
		/// <exception cref="ArgumentNullException">
		/// If the current <paramref name="instance"/> is <c>null</c>, or 
		/// if the specified <paramref name="elementName"/> is <c>null</c> or empty.
		/// </exception>
		public static XmlElement AppendElement(this XmlNode instance, string elementName)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");
			if (string.IsNullOrEmpty(elementName))
				throw new ArgumentNullException("elementName");

			XmlNode childNode = instance.OwnerDocument.CreateElement(elementName);
			return (XmlElement) instance.AppendChild(childNode);
		}

		/// <summary>
		/// Appends the specified element to the current node and returns it.
		/// </summary>
		/// <param name="instance">The target node to append the element to.</param>
		/// <param name="qualifiedName">The qualified name of the element.</param>
		/// <param name="nodeNamespace">The XML namespace of the element to create.</param>
		/// <returns>The element that was created and appended to <paramref name="instance"/>.</returns>
		/// <exception cref="ArgumentNullException">
		/// If the current <paramref name="instance"/> is <c>null</c>, or 
		/// if the specified <paramref name="qualifiedName"/> is <c>null</c> or empty, or.
		/// if the specified <paramref name="nodeNamespace"/> is <c>null</c> or empty.
		/// </exception>
		public static XmlElement AppendElement(this XmlNode instance, string qualifiedName, string nodeNamespace)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");
			if (string.IsNullOrEmpty(qualifiedName))
				throw new ArgumentNullException("qualifiedName");
			if (string.IsNullOrEmpty(nodeNamespace))
				throw new ArgumentNullException("nodeNamespace");

			XmlDocument ownerDoc = instance.OwnerDocument ?? (XmlDocument) instance;
			XmlNode childNode = ownerDoc.CreateElement(qualifiedName, nodeNamespace);
			return (XmlElement) instance.AppendChild(childNode);
		}

		/// <summary>
		/// Selects the first XmlNode that matches the XPath expression and returns it an an <see cref="XmlElement"/>
		/// </summary>
		/// <param name="instance">The node being extended</param>
		/// <param name="xpath">The XPath expression.</param>
		/// <returns>The first XmlNode that matches the XPath expression and returns it an an <see cref="XmlElement"/></returns>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="instance"/> is <c>null</c> or <paramref name="xpath"/> is <c>null</c> or an empty string.
		/// </exception>
		public static XmlElement SelectSingleElement(this XmlNode instance, string xpath)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");
			if (string.IsNullOrEmpty(xpath))
				throw new ArgumentNullException("xpath");

			return instance.SelectSingleNode(xpath) as XmlElement;
		}

		/// <summary>
		/// Selects the first XmlNode that matches the XPath expression and returns it an an <see cref="XmlElement"/>.
		/// Any prefixes found in the XPath expression are resolved using the supplied <see cref="XmlNamespaceManager"/>.
		/// </summary>
		/// <param name="instance">The node being extended</param>
		/// <param name="xpath">The XPath expression.</param>
		/// <param name="manager">An <see cref="XmlNamespaceManager"/> to use for resolving namespaces for prefixes in the XPath expression.</param>
		/// <returns>
		/// The first XmlNode that matches the XPath expression and returns it an an <see cref="XmlElement"/>
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="instance"/> is <c>null</c> or <paramref name="xpath"/> is <c>null</c> or an empty string.
		/// </exception>
		public static XmlElement SelectSingleElement(this XmlNode instance, string xpath, XmlNamespaceManager manager)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");
			if (string.IsNullOrEmpty(xpath))
				throw new ArgumentNullException("xpath");

			return instance.SelectSingleNode(xpath, manager) as XmlElement;
		}

		/// <summary>
		/// Serialized the specified <paramref name="element"/> the a JSON string.
		/// </summary>
		/// <param name="element">The element to convert.</param>
		/// <returns>The JSON version of the specified <paramref name="element"/></returns>
		public static string ToJson(this XmlElement element)
		{
			return ToJson(element, false);
		}

		/// <summary>
		/// Serialized the specified <paramref name="element"/> the a JSON string.
		/// </summary>
		/// <param name="element">The element to convert.</param>
		/// <param name="prettyPrint">If set to <c>true</c>, the resulting string will be formatted for easier reading.</param>
		/// <returns>
		/// The JSON version of the specified <paramref name="element"/>
		/// </returns>
		public static string ToJson(this XmlElement element, bool prettyPrint)
		{
			Format f = prettyPrint ? Format.Indented : Format.None;
			return JsonConvert.SerializeXmlNode(element, f, false);
		}
	}
}
