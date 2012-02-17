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
namespace Sage.XsltExtensions
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Xml;
	using System.Xml.XPath;

	using Sage.Extensibility;

	/// <summary>
	/// Provides several set-related utility methods for use in XSLT.
	/// </summary>
	[XsltExtensionObject(XmlNamespaces.Extensions.Set)]
	[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter",
		Justification = "This is an XSLT extension class, these methods will not be used from C#.")]
	public class Set
	{
		private static readonly XmlDocument document = new XmlDocument();
		private static readonly XPathNavigator empty = document.CreateNavigator();

		/// <summary>
		/// Distincts the specified nodeset.
		/// </summary>
		/// <param name="nodeset">The nodeset.</param>
		/// <param name="xpath">The xpath.</param>
		/// <returns>TODO: Add documentation for distinct.</returns>
		public XPathNodeIterator distinct(XPathNodeIterator nodeset, string xpath)
		{
			return distinct(nodeset, xpath, false);
		}

		/// <summary>
		/// Distincts the specified nodeset.
		/// </summary>
		/// <param name="nodeset">The nodeset.</param>
		/// <param name="xpath">The xpath.</param>
		/// <param name="includeNullEntries">if set to <c>true</c> [include null entries].</param>
		/// <returns></returns>
		public XPathNodeIterator distinct(XPathNodeIterator nodeset, string xpath, bool includeNullEntries)
		{
			if (nodeset.Count < 2)
				return nodeset;

			List<NodeInfo> result = new List<NodeInfo>();
			List<string> selections = new List<string>();

			XPathNodeIterator clone = nodeset.Clone();

			while (clone.MoveNext())
			{
				XPathNavigator selected = clone.Current.SelectSingleNode(xpath);
				if (selected == null)
				{
					if (includeNullEntries)
						result.Add(new NodeInfo(clone.Current));

					continue;
				}

				if (!selections.Contains(selected.Value))
				{
					selections.Add(selected.Value);
					result.Add(new NodeInfo(clone.Current));
				}
			}

			if (result.Count > 0)
			{
				List<string> distinct = new List<string>();
				for (int i = 0; i < result.Count; i++)
					distinct.Add(result[i].XPath);

				return result[0].Root.Select(string.Join(" | ", distinct), NodeInfo.GetNamespaceManager(result));
			}

			return empty.Select("*");
		}

		private class NodeInfo
		{
			private const string ElementXPath = "{0}[{1}]";
			private const string AttributeXPath = "@{0}";
			private readonly Dictionary<string, string> namespaces = new Dictionary<string, string>();

			public NodeInfo(XPathNavigator current)
			{
				List<string> xpathParts = new List<string>();
				XPathNodeIterator selection = current.SelectAncestors(XPathNodeType.All, true);

				while (selection.MoveNext())
				{
					if (selection.Current.NodeType == XPathNodeType.Root)
						break;

					var node = selection.Current;
					var name = node.Name;

					if (!string.IsNullOrEmpty(node.NamespaceURI))
					{
						string prefix = GetPrefix(node);
						name = string.Concat(prefix, ":", node.LocalName);
					}

					if (node.NodeType == XPathNodeType.Attribute)
						xpathParts.Add(string.Format(AttributeXPath, name));
					else
						xpathParts.Add(string.Format(ElementXPath, name, GetNodeIndex(node)));

					this.Root = node;
				}

				xpathParts.Reverse();

				this.XPath = string.Join("/", xpathParts);
			}

			public XPathNavigator Root { get; private set; }

			public string XPath { get; private set; }

			public static XmlNamespaceManager GetNamespaceManager(List<NodeInfo> nodes)
			{
				XmlNamespaceManager nm = new XmlNamespaceManager(new NameTable());
				foreach (NodeInfo info in nodes)
				{
					foreach (string namespaceUri in info.namespaces.Keys)
					{
						string prefix = info.namespaces[namespaceUri];
						if (!nm.HasNamespace(prefix))
							nm.AddNamespace(prefix, namespaceUri);
					}
				}

				return nm;
			}

			private int GetNodeIndex(XPathNavigator navigator)
			{
				string namespaceUri = navigator.NamespaceURI;
				string prefix = namespaces[namespaceUri];

				XmlNamespaceManager man = new XmlNamespaceManager(new NameTable());
				man.AddNamespace(prefix, namespaceUri);

				return 1 + navigator.Select(string.Format("preceding-sibling::{0}:{1}", prefix, navigator.LocalName), man).Count;
			}

			private string GetPrefix(XPathNavigator node)
			{
				if (namespaces.ContainsKey(node.NamespaceURI))
					return namespaces[node.NamespaceURI];

				string prefix = string.Empty;
				if (!string.IsNullOrEmpty(node.Prefix))
				{
					prefix = node.Prefix;
				}
				else
				{
					int minAscii = 97;  // a
					int maxAscii = 122; // z

					Random random = new Random();
					while (prefix == string.Empty || namespaces.ContainsKey(prefix))
					{
						prefix = string.Concat(
							(char) (minAscii + random.Next(maxAscii - minAscii)),
							(char) (minAscii + random.Next(maxAscii - minAscii)),
							(char) (minAscii + random.Next(maxAscii - minAscii)));
					}
				}

				namespaces.Add(node.NamespaceURI, prefix);
				return prefix;
			}
		}
	}
}
