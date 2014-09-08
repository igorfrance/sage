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
	using System.Diagnostics.Contracts;
	using System.Xml;

	using Kelp;

	using XmlNamespaces = Sage.XmlNamespaces;

	/// <summary>
	/// Represents a single message that a controller is sending to the view.
	/// </summary>
	public class ControllerMessage : IXmlConvertible
	{
		/// <summary>
		/// Gets or sets the type of this message.
		/// </summary>
		public MessageType Type { get; set; }

		/// <summary>
		/// Gets or sets the name of this message.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the text of this message.
		/// </summary>
		public string Text { get; set; }

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("{0} ({1})", this.Name, this.Type);
		}

		/// <summary>
		/// Parses the specified <paramref name="element" /> into the current object.
		/// </summary>
		/// <param name="element">The element to parse.</param>
		public void Parse(XmlElement element)
		{
			MessageType type;
			if (Enum.TryParse(element.GetAttribute("type"), false, out type))
				this.Type = type;

			this.Name = element.GetAttribute("name");
			this.Text = element.InnerText;
		}

		/// <summary>
		/// Generates an <see cref="XmlElement" /> that represents this instance.
		/// </summary>
		/// <param name="document">The document to use to create the element with.</param>
		/// <returns>An <see cref="XmlElement" /> that represents this instance.</returns>
		public XmlElement ToXml(XmlDocument document)
		{
			var result = document.CreateElement("sage:message", XmlNamespaces.SageNamespace);
			result.SetAttribute("type", this.Type.ToString());
			result.SetAttribute("name", this.Name);
			result.InnerText = this.Text;
			return result;
		}
	}
}
