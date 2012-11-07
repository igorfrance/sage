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

	using Kelp.Extensions;
	using Kelp.SyntaxHighlighting;

	/// <summary>
	/// Provides an additional constructor to base class <see cref="LanguageDefinition"/> that
	/// accepts the module's language definition as <see cref="XmlElement"/>.
	/// </summary>
	public class ModularLanguageDefinition : LanguageDefinition
	{
		public ModularLanguageDefinition(XmlElement configurationElement)
		{
			Contract.Requires<ArgumentNullException>(configurationElement != null);

			XmlNamespaceManager nm = Sage.XmlNamespaces.Manager;

			this.Name = configurationElement.GetAttribute("name");
			this.CaseSensitive = !configurationElement.GetAttribute("caseSensitive").ContainsAnyOf("false", "no", "0");

			XmlElement selection = configurationElement.SelectSingleElement("mod:escape", nm);
			if (selection != null)
				this.EscapeChar = selection.InnerText.Trim().Substring(0, 1);

			selection = configurationElement.SelectSingleElement("mod:regexp", nm);
			if (selection != null)
			{
				var pair = selection.InnerText.Trim().Split(' ');
				if (pair.Length == 2)
				{
					this.RegexStart = pair[0];
					this.RegexEnd = pair[1];
				}
			}

			foreach (XmlElement commentNode in configurationElement.SelectNodes("mod:comments/mod:linecomment", nm))
				this.LineCommentDelimiters.Add(commentNode.InnerText.Trim());

			foreach (XmlElement commentNode in configurationElement.SelectNodes("mod:comments/mod:comment", nm))
			{
				var pair = commentNode.InnerText.Trim().Split(' ');
				if (pair.Length == 2)
					this.CommentDelimiters.Add(new Delimiters(pair[0], pair[1]));
			}

			foreach (XmlElement quoteNode in configurationElement.SelectNodes("mod:quotes/mod:quote", nm))
				this.QuoteDelimiters.Add(quoteNode.InnerText.Trim());

			foreach (XmlElement groupNode in configurationElement.SelectNodes("mod:keywords/mod:group", nm))
				this.Expressions.Add(new ExpressionGroup(groupNode, this.CaseSensitive));
		}
	}
}