namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Xml;

	[AttributeUsage(AttributeTargets.Method)]
	public class NodeHandlerAttribute : Attribute
	{
		public NodeHandlerAttribute(XmlNodeType nodeType, string nodeName, string ns)
		{
			this.NodeType = nodeType;
			this.NodeName = nodeName;
			this.Namespace = ns;
		}

		public XmlNodeType NodeType { get; private set; }

		public string NodeName { get; private set; }

		public string Namespace { get; private set; }
	}
}
