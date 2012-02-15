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
namespace Sage.Modules
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Xml;

	using Kelp.Core.Extensions;
	using Kelp.Extensions;

	/// <summary>
	/// Represents the result returned by a module after processing a request.
	/// </summary>
	public class ModuleResult
	{
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
		public ModuleResultStatus Status
		 { get; set; }

		/// <summary>
		/// Gets or sets the result element that contains this module's data. 
		/// </summary>
		/// <remarks>
		/// This value can be a <c>null</c> if no data is returned or
		/// is not applicable for the module.
		/// </remarks>
		public XmlElement ResultElement
		 { get; set; }

		/// <summary>
		/// Gets the dictionary of arbitrary module result data, specific to each module.
		/// </summary>
		public Dictionary<string, object> ModuleData { get;  set; }

		public XmlDocument OwnerDocument
		{
			get
			{
				if (ResultElement != null)
					return ResultElement.OwnerDocument;

				return null;
			}
		}

		public XmlElement AppendDataElement(XmlNode dataElement = null)
		{
			return (XmlElement) AppendDataNode(dataElement);
		}

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
