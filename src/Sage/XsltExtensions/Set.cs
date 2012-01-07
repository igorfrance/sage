namespace Sage.XsltExtensions
{
	using System;
	using System.Collections;
	using System.Diagnostics.CodeAnalysis;
	using System.Xml;
	using System.Xml.XPath;

	using Sage.Extensibility;

	[XsltExtensionObject(XmlNamespaces.Extensions.Set)]
	[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter")]
	public class Set
	{
		private static readonly XmlDocument document = new XmlDocument();

		public XPathNavigator distinct(XPathNodeIterator nodeset)
		{
			Hashtable nodelist = new Hashtable();
			while (nodeset.MoveNext())
			{
				if (!nodelist.Contains(nodeset.Current.Value))
					nodelist.Add(nodeset.Current.Value, nodeset.Current);
			}

			var fragment = document.CreateDocumentFragment();
			foreach (object node in nodelist)
			{
				fragment.AppendChild((XmlNode) node);
			}

			return fragment.CreateNavigator();
		}
	}
}
