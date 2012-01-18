namespace Sage.XsltExtensions
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Xml;
	using System.Xml.XPath;

	using Sage.Extensibility;

	[XsltExtensionObject(XmlNamespaces.Extensions.Set)]
	[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter")]
	public class Set
	{
		private static readonly XmlDocument document = new XmlDocument();
		private static readonly XPathNavigator empty = document.CreateNavigator();

		public XPathNodeIterator distinct(XPathNodeIterator nodeset, string xpath)
		{
			return distinct(nodeset, xpath, false);
		}

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
				if (selected == null && !includeNullEntries)
					continue;

				string selection = selected != null ? selected.Value : string.Empty;
				if (!selections.Contains(selection))
				{
					selections.Add(selection);
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
						string prefix = !string.IsNullOrEmpty(node.Prefix) ? node.Prefix : GeneratePrefix();
						if (!namespaces.ContainsKey(prefix))
							namespaces.Add(prefix, node.NamespaceURI);

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
					foreach (string prefix in info.namespaces.Keys.Where(p => !nm.HasNamespace(p)))
					{
						nm.AddNamespace(prefix, info.namespaces[prefix]);
					}
				}

				return nm;
			}

			private static int GetNodeIndex(XPathNavigator navigator)
			{
				XmlNamespaceManager man = new XmlNamespaceManager(new NameTable());
				man.AddNamespace(navigator.Prefix, navigator.NamespaceURI);

				return 1 + navigator.Select(string.Format("preceding-sibling::{0}", navigator.Name), man).Count;
			}

			private string GeneratePrefix()
			{
				int minAscii = 97;
				int maxAscii = 122;

				Random random = new Random();
				string prefix = string.Empty;
				while (prefix == string.Empty || namespaces.ContainsKey(prefix))
				{
					prefix = string.Concat(
						(char) (minAscii + random.Next(maxAscii - minAscii)),
						(char) (minAscii + random.Next(maxAscii - minAscii)),
						(char) (minAscii + random.Next(maxAscii - minAscii)));
				}

				return prefix;
			}
		}
	}
}
