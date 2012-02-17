/**
 * Open Source Initiative OSI - The MIT License (MIT):Licensing
 * [OSI Approved License]
 * The MIT License (MIT)
 *
 * Copyright (c) 2011 Igor France
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */
namespace Kelp.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Xml;

	using Kelp.Core.Extensions;

	using Newtonsoft.Json;

	using Formatting = Newtonsoft.Json.Formatting;

	/// <summary>
	/// Implements extension methods to the <see cref="XmlNode"/> class.
	/// </summary>
	public static class XmlNodeExtensions
	{
		/// <summary>
		/// Iterates through xml nodes selected from <paramref name="instance"/> using the specified 
		/// <paramref name="xpath"/>, and pushes node text info the list of values that is returned.
		/// </summary>
		/// <param name="instance">The xml node from which to execute the xpath selection.</param>
		/// <param name="xpath">The XPath expression that specifies what to select (e.g. './child/@attrib1').</param>
		/// <returns>
		/// List of node values, selected with the specified <paramref name="xpath"/>.
		/// </returns>
		public static List<string> Aggregate(this XmlNode instance, string xpath)
		{
			return Aggregate(instance, xpath, XmlNamespaces.Manager);
		}

		/// <summary>
		/// Iterates through xml nodes selected from <paramref name="instance"/> using the specified
		/// <paramref name="xpath"/> and <paramref name="manager"/>, and pushes node text info the list of
		/// values that is returned.
		/// </summary>
		/// <param name="instance">The xml node from which to execute the xpath selection.</param>
		/// <param name="xpath">The XPath expression that specifies what to select (e.g. './child/@attrib1').</param>
		/// <param name="manager">The XML namespace manager to use with the specified <paramref name="xpath"/>.</param>
		/// <returns>
		/// List of node values, selected with the specified <paramref name="xpath"/>.
		/// </returns>
		public static List<string> Aggregate(this XmlNode instance, string xpath, XmlNamespaceManager manager)
		{
			Contract.Requires<ArgumentNullException>(instance != null);
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(xpath));

			List<string> result = new List<string>();
			foreach (XmlNode node in instance.SelectNodes(xpath, manager))
			{
				result.Add(node.InnerText);
			}

			return result;
		}

		/// <summary>
		/// Appends the specified element to the current node and returns it.
		/// </summary>
		/// <param name="instance">The target node to append the element to.</param>
		/// <param name="child">The child element to append.</param>
		/// <returns>The <paramref name="child"/> element that was appended.</returns>
		public static XmlElement AppendElement(this XmlNode instance, XmlNode child)
		{
			Contract.Requires<ArgumentNullException>(instance != null);
			Contract.Requires<ArgumentNullException>(child != null);

			return (XmlElement) instance.AppendChild(child);
		}

		/// <summary>
		/// Appends the specified element to the current node and returns it.
		/// </summary>
		/// <param name="instance">The target node to append the element to.</param>
		/// <param name="elementName">The name of the element to create.</param>
		/// <returns>The element that was created and appended to <paramref name="instance"/>.</returns>
		public static XmlElement AppendElement(this XmlNode instance, string elementName)
		{
			Contract.Requires<ArgumentNullException>(instance != null);
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(elementName));

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
		public static XmlElement AppendElement(this XmlNode instance, string qualifiedName, string nodeNamespace)
		{
			Contract.Requires<ArgumentNullException>(instance != null);
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(qualifiedName));
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(nodeNamespace));

			XmlDocument ownerDoc = instance.OwnerDocument ?? (XmlDocument) instance;
			XmlNode childNode = ownerDoc.CreateElement(qualifiedName, nodeNamespace);
			return (XmlElement) instance.AppendChild(childNode);
		}

		/// <summary>
		/// Selects the first XmlNode that matches the XPath expression and returns it an an <see cref="XmlElement"/>
		/// </summary>
		/// <param name="instance">The node being extended</param>
		/// <param name="xpath">The XPath expression.</param>
		/// <param name="manager">An <see cref="XmlNamespaceManager"/> to use for resolving any namespaces prefixes in <paramref name="xpath"/>.</param>
		/// <returns>The first XmlNode that matches the XPath expression and returns it an an <see cref="XmlElement"/></returns>
		public static XmlElement SelectSingleElement(this XmlNode instance, string xpath, XmlNamespaceManager manager = null)
		{
			Contract.Requires<ArgumentNullException>(instance != null);
			Contract.Requires<ArgumentNullException>(xpath != null);

			if (manager != null)
				return instance.SelectSingleNode(xpath, manager) as XmlElement;

			return instance.SelectSingleNode(xpath) as XmlElement;
		}

		/// <summary>
		/// Selects the specified <paramref name="xpath"/> expression and returns the result as boolean.
		/// </summary>
		/// <param name="instance">The instance to select from.</param>
		/// <param name="xpath">The xpath to select.</param>
		/// <param name="manager">An <see cref="XmlNamespaceManager"/> to use for resolving any namespaces prefixes in <paramref name="xpath"/>.</param>
		/// <returns>
		/// The specified <paramref name="xpath"/> as a <see cref="Boolean"/>
		/// </returns>
		public static bool GetBoolean(this XmlNode instance, string xpath, XmlNamespaceManager manager = null)
		{
			string value = instance.GetString(xpath, manager);
			return value != null && value.EqualsAnyOf("yes", "true", "1");
		}

		/// <summary>
		/// Selects the specified <paramref name="xpath"/> expression and returns the result as integer.
		/// </summary>
		/// <param name="instance">The instance to select from.</param>
		/// <param name="xpath">The xpath to select.</param>
		/// <param name="manager">An <see cref="XmlNamespaceManager"/> to use for resolving any namespaces prefixes in <paramref name="xpath"/>.</param>
		/// <returns>
		/// The specified <paramref name="xpath"/> as a <see cref="Boolean"/>
		/// </returns>
		public static int GetInt(this XmlNode instance, string xpath, XmlNamespaceManager manager = null)
		{
			return GetInt(instance, xpath, 0, manager);
		}

		/// <summary>
		/// Selects the specified <paramref name="xpath"/> expression and returns the result as integer,
		/// defaulting to the specified <paramref name="defaultValue"/> in case nothing is selected.
		/// </summary>
		/// <param name="instance">The instance to select from.</param>
		/// <param name="xpath">The xpath to select.</param>
		/// <param name="defaultValue">The default value to return in case <paramref name="xpath"/> expression doesn't 
		/// yield a result, or the result could not be cast to integer.</param>
		/// <param name="manager">An <see cref="XmlNamespaceManager"/> to use for resolving any namespaces prefixes in <paramref name="xpath"/>.</param>
		/// <returns>
		/// The specified <paramref name="xpath"/> as a <see cref="Boolean"/>
		/// </returns>
		public static int GetInt(this XmlNode instance, string xpath, int defaultValue, XmlNamespaceManager manager = null)
		{
			string value = instance.GetString(xpath, manager);

			int result;
			if (value == null || !int.TryParse(value, out result))
				return defaultValue;

			return result;
		}

		/// <summary>
		/// Selects the specified <paramref name="xpath"/> expression and returns the result as integer,
		/// defaulting to the specified <paramref name="defaultValue"/> in case nothing is selected, or the number exceeds
		/// the range specified with <paramref name="minValue"/> and <paramref name="maxValue"/>.
		/// </summary>
		/// <param name="instance">The instance to select from.</param>
		/// <param name="xpath">The xpath to select.</param>
		/// <param name="defaultValue">The default value to return in case <paramref name="xpath"/> expression doesn't
		/// yield a result, or the result could not be cast to integer.</param>
		/// <param name="minValue">The min value.</param>
		/// <param name="maxValue">The max value.</param>
		/// <param name="manager">An <see cref="XmlNamespaceManager"/> to use for resolving any namespaces prefixes in <paramref name="xpath"/>.</param>
		/// <returns>
		/// The specified <paramref name="xpath"/> as a <see cref="Boolean"/>
		/// </returns>
		public static int GetInt(this XmlNode instance, string xpath, int defaultValue, int minValue, int maxValue, XmlNamespaceManager manager = null)
		{
			int result = GetInt(instance, xpath, defaultValue, manager);
			return Math.Min(maxValue, Math.Max(minValue, result));
		}

		/// <summary>
		/// Selects the specified <paramref name="xpath"/> expression and returns the result as long.
		/// </summary>
		/// <param name="instance">The instance to select from.</param>
		/// <param name="xpath">The xpath to select.</param>
		/// <param name="manager">An <see cref="XmlNamespaceManager"/> to use for resolving any namespaces prefixes in <paramref name="xpath"/>.</param>
		/// <returns>
		/// The specified <paramref name="xpath"/> as a <see cref="long"/>
		/// </returns>
		public static long GetLong(this XmlNode instance, string xpath, XmlNamespaceManager manager = null)
		{
			return GetLong(instance, xpath, 0, manager);
		}

		/// <summary>
		/// Selects the specified <paramref name="xpath"/> expression and returns the result as long,
		/// defaulting to the specified <paramref name="defaultValue"/> in case nothing is selected.
		/// </summary>
		/// <param name="instance">The instance to select from.</param>
		/// <param name="xpath">The xpath to select.</param>
		/// <param name="defaultValue">The default value to return in case <paramref name="xpath"/> expression doesn't 
		/// yield a result, or the result could not be cast to long.</param>
		/// <param name="manager">An <see cref="XmlNamespaceManager"/> to use for resolving any namespaces prefixes in <paramref name="xpath"/>.</param>
		/// <returns>
		/// The specified <paramref name="xpath"/> as a <see cref="long"/>
		/// </returns>
		public static long GetLong(this XmlNode instance, string xpath, int defaultValue, XmlNamespaceManager manager = null)
		{
			string value = instance.GetString(xpath, manager);

			long result;
			if (value == null || !long.TryParse(value, out result))
				return defaultValue;

			return result;
		}

		/// <summary>
		/// Selects the specified <paramref name="xpath"/> expression and returns the result as long,
		/// defaulting to the specified <paramref name="defaultValue"/> in case nothing is selected, or the number exceeds
		/// the range specified with <paramref name="minValue"/> and <paramref name="maxValue"/>.
		/// </summary>
		/// <param name="instance">The instance to select from.</param>
		/// <param name="xpath">The xpath to select.</param>
		/// <param name="defaultValue">The default value to return in case <paramref name="xpath"/> expression doesn't
		/// yield a result, or the result could not be cast to long.</param>
		/// <param name="minValue">The min value.</param>
		/// <param name="maxValue">The max value.</param>
		/// <param name="manager">An <see cref="XmlNamespaceManager"/> to use for resolving any namespaces prefixes in <paramref name="xpath"/>.</param>
		/// <returns>
		/// The specified <paramref name="xpath"/> as a <see cref="long"/>
		/// </returns>
		public static long GetLong(this XmlNode instance, string xpath, int defaultValue, int minValue, int maxValue, XmlNamespaceManager manager = null)
		{
			long result = GetLong(instance, xpath, defaultValue, manager);
			return Math.Min(maxValue, Math.Max(minValue, result));
		}

		/// <summary>
		/// Selects the specified <paramref name="xpath"/> expression and returns the result as float.
		/// </summary>
		/// <param name="instance">The instance to select from.</param>
		/// <param name="xpath">The xpath to select.</param>
		/// <param name="manager">An <see cref="XmlNamespaceManager"/> to use for resolving any namespaces prefixes in <paramref name="xpath"/>.</param>
		/// <returns>
		/// The specified <paramref name="xpath"/> as a <see cref="float"/> value.
		/// </returns>
		public static float GetFloat(this XmlNode instance, string xpath, XmlNamespaceManager manager = null)
		{
			string value = instance.GetString(xpath, manager);

			float result = 0;
			if (value != null)
				float.TryParse(value, out result);

			return result;
		}

		/// <summary>
		/// Selects the specified <paramref name="xpath"/> expression and returns the result as string.
		/// </summary>
		/// <param name="instance">The instance to select from.</param>
		/// <param name="xpath">The xpath to select.</param>
		/// <param name="manager">An <see cref="XmlNamespaceManager"/> to use for resolving any namespaces prefixes in <paramref name="xpath"/>.</param>
		/// <returns>
		/// The specified <paramref name="xpath"/> as a <see cref="string"/> value.
		/// </returns>
		public static string GetString(this XmlNode instance, string xpath, XmlNamespaceManager manager = null)
		{
			XmlNode selection = instance.SelectSingleNode(xpath, manager);
			return selection != null ? selection.InnerText.Trim() : null;
		}

		/// <summary>
		/// Selects the specified <paramref name="xpath"/> expression and returns the result as integer,
		/// defaulting to the specified <paramref name="defaultValue"/> in case nothing is selected.
		/// </summary>
		/// <param name="instance">The instance to select from.</param>
		/// <param name="xpath">The xpath to select.</param>
		/// <param name="defaultValue">The default value to return in case <paramref name="xpath"/> expression doesn't 
		/// yield a result.</param>
		/// <param name="manager">An <see cref="XmlNamespaceManager"/> to use for resolving any namespaces prefixes in <paramref name="xpath"/>.</param>
		/// <returns>
		/// The specified <paramref name="xpath"/> as a <see cref="String"/>
		/// </returns>
		public static string GetString(this XmlNode instance, string xpath, string defaultValue, XmlNamespaceManager manager = null)
		{
			string value = GetString(instance, xpath, manager);
			if (value == null)
				return defaultValue;

			return value;
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
			Formatting f = prettyPrint ? Formatting.Indented : Formatting.None;
			return JsonConvert.SerializeXmlNode(element, f, false);
		}
	}
}
