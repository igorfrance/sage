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
namespace Sage.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Xml;

	using Kelp;
	using Kelp.Extensions;

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
