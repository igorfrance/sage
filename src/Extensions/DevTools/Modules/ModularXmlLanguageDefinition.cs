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
namespace Sage.DevTools.Modules
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Xml;

	using Kelp.SyntaxHighlighting;

	/// <summary>
	/// Provides an additional constructor to base class <see cref="XmlLanguageDefinition"/> that
	/// accepts the module's language definition as <see cref="XmlElement"/>.
	/// </summary>
	public class ModularXmlLanguageDefinition : XmlLanguageDefinition
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ModularXmlLanguageDefinition"/> class.
		/// </summary>
		/// <param name="languageElement">The XML element that defines the language.</param>
		public ModularXmlLanguageDefinition(XmlElement languageElement)
		{
			Contract.Requires<ArgumentNullException>(languageElement != null);

			XmlNamespaceManager nm = Sage.XmlNamespaces.Manager;

			this.Name = languageElement.GetAttribute("name");

			foreach (XmlElement groupNode in languageElement.SelectNodes("mod:elements/mod:group", nm))
				this.Elements.Add(new ExpressionGroup(groupNode, true));

			foreach (XmlElement groupNode in languageElement.SelectNodes("mod:attributes/mod:group", nm))
				this.Attributes.Add(new ExpressionGroup(groupNode, true));
		}
	}
}