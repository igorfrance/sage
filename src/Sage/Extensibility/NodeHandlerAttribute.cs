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
namespace Sage.Extensibility
{
	using System;
	using System.Xml;

	using Sage.ResourceManagement;

	/// <summary>
	/// Indicates that the method this attribute decorates should be used as a node handler for 
	/// <see cref="ResourceManager.CopyTree"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class NodeHandlerAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NodeHandlerAttribute"/> class.
		/// </summary>
		/// <param name="nodeType">The type of the node being handled.</param>
		/// <param name="nodeName">The name of the node being handled.</param>
		/// <param name="ns">The namespace of the node being handled.</param>
		public NodeHandlerAttribute(XmlNodeType nodeType, string nodeName, string ns = null)
		{
			this.NodeType = nodeType;
			this.NodeName = nodeName;
			this.Namespace = ns;
		}

		/// <summary>
		/// Gets the type of the handled node.
		/// </summary>
		public XmlNodeType NodeType { get; private set; }

		/// <summary>
		/// Gets the name of the handled node.
		/// </summary>
		public string NodeName { get; private set; }

		/// <summary>
		/// Gets the namespace of the handled node.
		/// </summary>
		public string Namespace { get; private set; }
	}
}
