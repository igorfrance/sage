namespace Sage.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Xml;

	using Kelp;
	using Kelp.Core.Extensions;

	using XmlNamespaces = Sage.XmlNamespaces;

	/// <summary>
	/// Represents a collection of messages that the controller is sending to the view.
	/// </summary>
	public class ControllerMessages : List<ControllerMessage>, IXmlConvertible
	{
		/// <inheritdoc/>
		/// <exception cref="ArgumentNullException">
		/// <c>ownerDoc</c> is null.
		/// </exception>
		public XmlElement ToXml(XmlDocument ownerDoc)
		{
			if (ownerDoc == null)
				throw new ArgumentNullException("ownerDoc");

			if (this.Count == 0)
				return null;

			XmlElement messages = ownerDoc.CreateElement("sage:messages", XmlNamespaces.SageNamespace);
			MessageType type = MessageType.Info;
			foreach (ControllerMessage message in this)
			{
				XmlElement element = messages.AppendElement(ownerDoc.CreateElement("sage:message", XmlNamespaces.SageNamespace));
				element.SetAttribute("type", message.Type.ToString().ToLower());
				element.InnerText = message.Text;
				if (!string.IsNullOrEmpty(message.Name))
					element.SetAttribute("name", message.Name);

				if (message.Type > type)
					type = message.Type;
			}

			messages.SetAttribute("type", type.ToString().ToLower());

			return messages;
		}
	}
}
