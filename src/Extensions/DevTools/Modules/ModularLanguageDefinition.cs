namespace Sage.DevTools.Modules
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Web;
	using System.Xml;

	using Kelp;
	using Kelp.Core.Extensions;
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
			this.CaseSensitive = configurationElement.GetAttribute("caseSensitive").ContainsAnyOf("false", "no", "0");

			XmlElement selection = configurationElement.SelectSingleElement("mod:escape", nm);
			if (selection != null)
				this.EscapeChar = selection.InnerText.Trim().Substring(0, 1);

			selection = configurationElement.SelectSingleElement("mod:regexp", nm);
			if (selection != null)
			{
				var pair = selection.InnerText.Trim().Split(' ');
				if (pair.Length == 2)
					this.Regexp = pair;
			}

			foreach (XmlElement commentNode in configurationElement.SelectNodes("mod:comments/mod:linecomment", nm))
				this.LineCommentDelimiters.Add(commentNode.InnerText.Trim());

			foreach (XmlElement commentNode in configurationElement.SelectNodes("mod:comments/mod:comment", nm))
			{
				var pair = commentNode.InnerText.Trim().Split(' ');
				if (pair.Length == 2)
					this.CommentDelimiters.Add(pair);
			}

			foreach (XmlElement quoteNode in configurationElement.SelectNodes("mod:quotes/mod:quote", nm))
				this.QuoteDelimiters.Add(quoteNode.InnerText.Trim());

			foreach (XmlElement groupNode in configurationElement.SelectNodes("mod:keywords/mod:group", nm))
				this.Expressions.Add(new ExpressionGroup(groupNode, this.CaseSensitive));
		}
	}
}