/**
 * Copyright 2012 Igor France
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Sage.XsltExtensions
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Xml;
	using System.Xml.XPath;

	using Sage.Extensibility;

	/// <summary>
	/// Provides several set-related utility methods for use in XSLT.
	/// </summary>
	[XsltExtensionObject(XmlNamespaces.Extensions.Set)]
	[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter",
		Justification = "This is an XSLT extension class, these methods are not intended for use from C#.")]
	public class Set
	{
		private const int MinAscii = 97; // a
		private const int MaxAscii = 122; // z

		private static readonly XmlDocument document = new XmlDocument();
		private static readonly XPathNavigator empty = document.CreateNavigator();

		/// <summary>
		/// Returns <paramref name="nodeset1"/> if it is not empty, otherwise <paramref name="nodeset2"/>.
		/// </summary>
		/// <param name="nodeset1">The primary selection.</param>
		/// <param name="nodeset2">The secondary selection.</param>
		/// <returns><paramref name="nodeset1"/> if it is not empty, otherwise <paramref name="nodeset2"/></returns>
		public XPathNodeIterator either(XPathNodeIterator nodeset1, XPathNodeIterator nodeset2)
		{
			return nodeset1.Count != 0 ? nodeset1.Clone() : nodeset2.Clone();
		}

		/// <summary>
		/// Selects nodes from the specified <paramref name="nodeset"/> that have unique values selected with <paramref name="xpath"/>.
		/// </summary>
		/// <param name="nodeset">The <c>nodeset</c> that contains the nodes to filter.</param>
		/// <param name="xpath">The xpath to use to select the value that should be unique between nodes.</param>
		/// <returns>
		/// The nodes from the specified <paramref name="nodeset"/> that have unique values selected with <paramref name="xpath"/>.
		/// </returns>
		public XPathNodeIterator distinct(XPathNodeIterator nodeset, string xpath)
		{
			return distinct(nodeset, xpath, false);
		}

		/// <summary>
		/// Selects nodes from the specified <paramref name="nodeset"/> that have unique values selected with <paramref name="xpath"/>.
		/// </summary>
		/// <param name="nodeset">The <c>nodeset</c> that contains the nodes to filter.</param>
		/// <param name="xpath">The xpath to use to select the value that should be unique between nodes.</param>
		/// <param name="includeNullEntries">If set to <c>true</c>, nodes that fail to select the specified <paramref name="xpath"/> will be included too.</param>
		/// <returns>
		/// The nodes from the specified <paramref name="nodeset"/> that have unique values selected with <paramref name="xpath"/>.
		/// </returns>
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
				List<string> distinct = result.Select(t => t.XPath).ToList();
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

					xpathParts.Add(
						node.NodeType == XPathNodeType.Attribute
							? string.Format(AttributeXPath, name)
							: string.Format(ElementXPath, name, this.GetNodeIndex(node)));

					this.Root = node;
				}

				xpathParts.Reverse();

				this.XPath = string.Join("/", xpathParts);
			}

			public XPathNavigator Root { get; private set; }

			public string XPath { get; private set; }

			public static XmlNamespaceManager GetNamespaceManager(IEnumerable<NodeInfo> nodes)
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
					Random random = new Random();
					while (prefix == string.Empty || namespaces.ContainsKey(prefix))
					{
						prefix = string.Concat(
							(char)(MinAscii + random.Next(MaxAscii - MinAscii)),
							(char)(MinAscii + random.Next(MaxAscii - MinAscii)),
							(char)(MinAscii + random.Next(MaxAscii - MinAscii)));
					}
				}

				namespaces.Add(node.NamespaceURI, prefix);
				return prefix;
			}
		}
	}
}
