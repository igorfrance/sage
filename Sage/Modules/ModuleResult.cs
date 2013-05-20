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
namespace Sage.Modules
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Xml;

	using Kelp.Extensions;

	/// <summary>
	/// Represents the result returned by a module after processing a request.
	/// </summary>
	public class ModuleResult
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ModuleResult"/> class.
		/// </summary>
		/// <param name="status">The status.</param>
		public ModuleResult(ModuleResultStatus status)
		{
			this.ModuleData = new Dictionary<string, object>();
			this.Status = status;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ModuleResult"/> class, using the specified <paramref name="status"/> and
		/// <paramref name="resultElement"/>.
		/// </summary>
		/// <param name="resultElement">The content of this result.</param>
		/// <param name="status">The status of this result.</param>
		public ModuleResult(XmlElement resultElement, ModuleResultStatus status = ModuleResultStatus.Ok)
			: this(status)
		{
			this.ResultElement = resultElement;
		}

		/// <summary>
		/// Gets or sets the result status.
		/// </summary>
		public ModuleResultStatus Status { get; set; }

		/// <summary>
		/// Gets or sets the result element that contains this module's data. 
		/// </summary>
		/// <remarks>
		/// This value can be a <c>null</c> if no data is returned or
		/// is not applicable for the module.
		/// </remarks>
		public XmlElement ResultElement { get; set; }

		/// <summary>
		/// Gets the dictionary of arbitrary module result data, specific to each module.
		/// </summary>
		public Dictionary<string, object> ModuleData { get; private set; }

		/// <summary>
		/// Gets the owner document.
		/// </summary>
		public XmlDocument OwnerDocument
		{
			get
			{
				if (ResultElement != null)
					return ResultElement.OwnerDocument;

				return null;
			}
		}

		/// <summary>
		/// Creates a <c>mod:data</c> element in the current module.
		/// </summary>
		/// <param name="dataElement">Optional data element to add to the current module's element.</param>
		/// <returns>The element that was appended.</returns>
		/// <remarks>
		/// If this method is invoked with no arguments, a <c>mod:data</c> element will be created (if it doesn't exist in
		/// the current module) and the final <c>mod:data</c> element will be returned.
		/// If an element whose name is <c>mod:data</c> is supplied, if will either be appended to the current module 
		/// (if the module doesn't have a <c>mod:data</c> element already) or it will replace the existing <c>mod:data</c>
		/// element if one exists already. The element that will be returned will be the appended copy of the specified 
		/// <paramref name="dataElement"/>.
		/// </remarks>
		public XmlElement AppendDataElement(XmlNode dataElement = null)
		{
			return (XmlElement) AppendDataNode(dataElement);
		}

		/// <summary>
		/// Creates a <c>mod:data</c> node in the current module.
		/// </summary>
		/// <param name="dataNode">Optional data node to add to the current module's element.</param>
		/// <returns>The node that was appended.</returns>
		/// <remarks>
		/// If this method is invoked with no arguments, a <c>mod:data</c> element will be created (if it doesn't exist in
		/// the current module) and the final <c>mod:data</c> element will be returned.
		/// If an element whose name is <c>mod:data</c> is supplied, if will either be appended to the current module 
		/// (if the module doesn't have a <c>mod:data</c> element already) or it will replace the existing <c>mod:data</c>
		/// element if one exists already. The node that will be returned will be the appended copy of the specified 
		/// <paramref name="dataNode"/>.
		/// </remarks>
		public XmlNode AppendDataNode(XmlNode dataNode = null)
		{
			Contract.Requires<InvalidOperationException>(this.ResultElement != null, "The ModuleResult.ResultElement property is null");
			
			if (dataNode == null)
				dataNode = OwnerDocument.CreateElement("mod:data", XmlNamespaces.ModulesNamespace);

			return AppendOrPrependDataNode(dataNode, false);
		}

		private XmlNode AppendOrPrependDataNode(XmlNode dataNode, bool prepend)
		{
			Contract.Requires<ArgumentNullException>(dataNode != null);
			Contract.Requires<InvalidOperationException>(this.ResultElement != null, "The ModuleResult.ResultElement property is null");

			if (dataNode.NodeType == XmlNodeType.Document)
				dataNode = ((XmlDocument) dataNode).DocumentElement;

			XmlDocument ownerDocument = this.ResultElement.OwnerDocument;
			XmlElement dataElement = this.ResultElement.SelectSingleElement("mod:data", XmlNamespaces.Manager);
			XmlNode importedNode = ownerDocument.ImportNode(dataNode, true);

			if (importedNode.Name == "mod:data")
			{
				if (dataElement == null)
					return this.ResultElement.AppendChild(importedNode);

				foreach (XmlNode node in importedNode)
					dataElement.AppendChild(node);

				return dataElement;
			}

			if (dataElement == null)
				 dataElement = this.ResultElement.AppendElement(ownerDocument.CreateElement("mod:data", XmlNamespaces.ModulesNamespace));

			if (prepend)
			{
				return dataElement.InsertBefore(importedNode, dataElement.ChildNodes[0]);
			}

			return dataElement.AppendElement(importedNode);
		}
	}
}
