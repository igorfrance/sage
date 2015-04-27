/**
 * Copyright 2012 Igor France
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not useinternal  this file except in compliance with the License.
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
namespace Sage.Extensibility
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Reflection;
	using System.Xml;

	using Kelp;

	using log4net;

	internal class NodeEvaluator
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(NodeEvaluator).FullName);
		private static readonly Dictionary<string, NodeHandler> nodeHandlers = new Dictionary<string, NodeHandler>();

		private readonly SageContext context;

		static NodeEvaluator()
		{
			NodeEvaluator.DiscoverNodeHandlers();
			Project.AssembliesUpdated += NodeEvaluator.OnAssembliesUpdated;
		}

		public NodeEvaluator(SageContext context)
		{
			this.context = context;
		}

		/// <summary>
		/// Registers the specified node <paramref name="handler"/>, for the specified <paramref name="nodeType"/>,
		/// <paramref name="nodeName"/> and <paramref name="nodeNamespace"/>.
		/// </summary>
		/// <param name="nodeType">The type of the node for which the handler is being registered.</param>
		/// <param name="nodeName">The name of the node for which the handler is being registered.</param>
		/// <param name="nodeNamespace">The namespace of the node for which the handler is being registered.</param>
		/// <param name="handler">The method that will will handle the node.</param>
		public static void RegisterNodeHandler(XmlNodeType nodeType, string nodeName, string nodeNamespace, NodeHandler handler)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(nodeName));
			Contract.Requires<ArgumentNullException>(handler != null);

			string qualifiedName = NodeEvaluator.QualifyName(nodeType, nodeName, nodeNamespace);
			if (nodeHandlers.ContainsKey(qualifiedName))
			{
				if (nodeHandlers[qualifiedName] == handler)
					return;

				log.WarnFormat("Replacing existing handler '{0}' for element '{1}' with new handler '{2}'",
					Util.GetMethodSignature(nodeHandlers[qualifiedName].Method),
					qualifiedName,
					Util.GetMethodSignature(handler.Method));
			}

			nodeHandlers[qualifiedName] = handler;
		}

		public static XmlNode Process(SageContext context, XmlNode node)
		{
			Contract.Requires<ArgumentNullException>(node != null);
			Contract.Requires<ArgumentNullException>(context != null);

			XmlNode result;

			switch (node.NodeType)
			{
				case XmlNodeType.Document:
					result = NodeEvaluator.Process(context, ((XmlDocument) node).DocumentElement);
					break;

				case XmlNodeType.Element:
					result = node.OwnerDocument.CreateElement(node.Name, node.NamespaceURI);

					XmlNodeList attributes = node.SelectNodes("@*");
					XmlNodeList children = node.SelectNodes("node()");

					foreach (XmlAttribute attribute in attributes)
					{
						XmlNode processed = NodeEvaluator.GetNodeHandler(attribute)(context, attribute);
						if (processed != null)
							result.Attributes.Append((XmlAttribute) processed);
					}

					foreach (XmlNode child in children)
					{
						XmlNode processed = NodeEvaluator.GetNodeHandler(child)(context, child);
						if (processed != null)
							result.AppendChild(processed);
					}

					break;

				case XmlNodeType.Attribute:
				case XmlNodeType.Text:
					result = node.CloneNode(true);
					if (node.SelectSingleNode("ancestor::sage:literal", Sage.XmlNamespaces.Manager) == null)
					{
						result.Value = context.ProcessText(result.Value);
					}

					break;

				default:
					result = node.CloneNode(true);
					break;
			}

			return result;
		}

		public XmlNode Process(XmlNode node)
		{
			return NodeEvaluator.Process(context, node);
		}

		/// <summary>
		/// Gets the node handler for the specified node.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <returns>A <see cref="NodeHandler"/> for the specified node. If no handler is registered for this node's
		/// local name and namespace, the default handler is returned; this handler simply continues processing the node
		/// and returns its copy.</returns>
		internal static NodeHandler GetNodeHandler(XmlNode node)
		{
			string key = NodeEvaluator.QualifyName(node);
			if (nodeHandlers.ContainsKey(key))
				return nodeHandlers[key];

			return NodeEvaluator.Process;
		}

		private static string QualifyName(XmlNodeType type, string name, string ns)
		{
			var colonIndex = name.IndexOf(":", StringComparison.Ordinal);
			if (colonIndex >= 0)
				name = name.Substring(colonIndex + 1);

			if (string.IsNullOrEmpty(ns))
				return string.Concat((int) type, "_", name);

			return string.Concat((int) type, "_", ns, ":", name);
		}

		private static string QualifyName(XmlNode node)
		{
			if (string.IsNullOrEmpty(node.NamespaceURI))
				return string.Concat((int) node.NodeType, "_", node.LocalName);

			return string.Concat((int) node.NodeType, "_", node.NamespaceURI, ":", node.LocalName);
		}

		private static void OnAssembliesUpdated(object sender, EventArgs arg)
		{
			NodeEvaluator.DiscoverNodeHandlers();
		}

		private static void DiscoverNodeHandlers()
		{
			const BindingFlags BindingFlags =
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.IgnoreReturn;

			foreach (Assembly a in Project.RelevantAssemblies.ToList())
			{
				try
				{
					var types = from t in a.GetTypes()
								where t.IsClass && !t.IsAbstract
								select t;

					foreach (Type type in types)
					{
						foreach (MethodInfo methodInfo in type.GetMethods(BindingFlags))
						{
							foreach (NodeHandlerAttribute attrib in methodInfo.GetCustomAttributes(typeof(NodeHandlerAttribute), false))
							{
								NodeHandler handler = (NodeHandler) Delegate.CreateDelegate(typeof(NodeHandler), methodInfo);
								NodeEvaluator.RegisterNodeHandler(attrib.NodeType, attrib.NodeName, attrib.Namespace, handler);
							}
						}
					}
				}
				catch (ReflectionTypeLoadException ex)
				{
					log.ErrorFormat("Error loading types from {0}: {1}", a.FullName, ex.Message);
					throw;
				}
			}
		}
	}
}
