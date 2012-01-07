namespace Sage.Modules
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Xml;

	using Kelp.Core.Extensions;

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
		/// <param name="status">The status of this result.</param>
		/// <param name="resultElement">The content of this result.</param>
		public ModuleResult(XmlElement resultElement, ModuleResultStatus status = ModuleResultStatus.Ok)
			: this(status)
		{
			this.ResultElement = resultElement;
		}

		/// <summary>
		/// Gets or sets the result status.
		/// </summary>
		public ModuleResultStatus Status
		{
			get;
			set;
		}

		/// <summary>
		/// The result element contain this module's data. This value can be a <c>null</c> if no data is returned or
		/// is not applicable for the module.
		/// </summary>
		public XmlElement ResultElement
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the dictionary of arbitrary module result data, specific to each module.
		/// </summary>
		public Dictionary<string, object> ModuleData
		{
			get;
			private set;
		}

		public XmlDocument OwnerDocument
		{
			get
			{
				if (ResultElement != null)
					return ResultElement.OwnerDocument;

				return null;
			}
		}

		public XmlElement PrependDataElement(XmlNode dataElement)
		{
			return (XmlElement) PrependDataNode(dataElement);
		}

		public XmlNode PrependDataNode(XmlNode dataNode)
		{
			if (dataNode == null)
				throw new ArgumentNullException("dataNode");
			if (this.ResultElement == null)
				throw new InvalidOperationException("The ResultElement property is null");

			return AppendOrPrependDataNode(dataNode, true);
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
