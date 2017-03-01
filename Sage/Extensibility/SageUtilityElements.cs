namespace Sage.Extensibility
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Xml;

	using Kelp.Extensions;

	internal class SageUtilityElements
	{
		[NodeHandler(XmlNodeType.Element, "style", XmlNamespaces.SageNamespace)]
		internal static XmlNode SageStyleElement(SageContext context, XmlNode node)
		{
			Contract.Requires<ArgumentNullException>(node != null);
			if (node.SelectSingleElement("ancestor::sage:literal", XmlNamespaces.Manager) != null)
				return node;

			XmlNode thisNode = context.ProcessNode(node);
			XmlComment commentNode = node.OwnerDocument.CreateComment($" \n{thisNode.GetString().Replace("&gt;", ">")}\n");
			XmlElement result = node.OwnerDocument.CreateElement("x:style", XmlNamespaces.XHtmlNamespace);
			result.AppendChild(commentNode);

			return result;
		}
	}
}
