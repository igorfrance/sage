namespace Sage.DevTools.Modules.Source
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml;

	using Kelp.Core.Extensions;
	using Kelp.SyntaxHighlighting;

	using Sage.Views;

	using log4net;
	using Sage.Modules;

	public class SyntaxHighlighterModule : IModule
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(SyntaxHighlighterModule).FullName);
		private static readonly Regex indentExpr = new Regex(@"^([\s\t]+)(?=\S)", RegexOptions.Compiled);

		private static readonly XmlNamespaceManager nm = XmlNamespaces.Manager;
		private static bool initialized;
		private static Dictionary<string, LanguageDefinition> languages;

		public ModuleResult ProcessRequest(XmlElement moduleElement, ViewConfiguration configuration)
		{
			Contract.Requires<ArgumentNullException>(moduleElement != null);
			Contract.Requires<ArgumentNullException>(configuration != null);

			SageContext context = configuration.Context;
			Initialize(context);

			if (languages.Count == 0)
			{
				log.ErrorFormat("The syntax highligher module doesn't isn't configured with any languages. Until this is fixed, the module will not work.");
				return new ModuleResult(ModuleResultStatus.ConfigurationError);
			}

			XmlNode languageNode = moduleElement.SelectSingleNode("mod:config/mod:language", nm);
			XmlElement codeNode = moduleElement.SelectSingleElement("mod:config/mod:code", nm);

			if (languageNode == null)
				log.ErrorFormat("The required element mod:language is missing from the module configuration");

			if (codeNode == null)
				log.ErrorFormat("The required element mod:code is missing from the module configuration");

			if (languageNode == null || codeNode == null)
				return new ModuleResult(ModuleResultStatus.MissingParameters);

			string language = languageNode.InnerText.Trim();
			string sourceCode = codeNode.InnerText.Trim();
			string sourcePath = codeNode.GetAttribute("src");

			if (string.IsNullOrWhiteSpace(language))
			{
				log.ErrorFormat("The mod:language is missing the required text value");
				return new ModuleResult(ModuleResultStatus.MissingParameters);
			}

			if (!languages.ContainsKey(language))
			{
				log.ErrorFormat("The specified language '{0}' is not recognized. Valid languages are: '{1}'.",
					language, string.Join(", ", languages.Keys.ToArray()));

				return new ModuleResult(ModuleResultStatus.MissingParameters);
			}

			if (!string.IsNullOrEmpty(sourcePath) && string.IsNullOrWhiteSpace(sourceCode))
			{
				string expanded = context.Path.Resolve(sourcePath);
				if (!File.Exists(expanded))
				{
					log.ErrorFormat("The specified source code location '{0}' ('{1}') doesn't exist.",
						sourcePath, expanded);

					return new ModuleResult(ModuleResultStatus.NoData);
				}

				sourceCode = File.ReadAllText(expanded);
			}

			string indent = null;
			string[] sourceLines = sourceCode.Split('\n');

			foreach (string line in sourceLines)
			{
				Match m;
				if ((m = indentExpr.Match(line)).Success)
				{
					if (indent == null || m.Groups[1].Value.Length < indent.Length)
						indent = m.Groups[1].Value;
				}
			}

			if (!string.IsNullOrEmpty(indent))
			{
				StringBuilder trimmed = new StringBuilder();
				Regex cleanup = new Regex("^" + indent);
				foreach (string line in sourceLines)
				{
					trimmed.AppendLine(cleanup.Replace(line, string.Empty));
				}

				sourceCode = trimmed.ToString();
			}

			SyntaxHighlighter highlighter = new SyntaxHighlighter(languages[language]);
			string highlighted = highlighter.Format(sourceCode);

			ModuleResult result = new ModuleResult(moduleElement);
			XmlElement dataElement = result.AppendDataElement();
			XmlElement sourceElement = dataElement.AppendElement("mod:formatted", XmlNamespaces.ModulesNamespace);
			sourceElement.InnerText = highlighted;

			return result;
		}

		private static void Initialize(SageContext context)
		{
			if (initialized)
				return;

			string definitionPath = context.Path.GetModulePath("SyntaxHighlighter", "SyntaxHighlighter.xml");
			XmlDocument definitionDoc = context.Resources.LoadXml(definitionPath);

			languages = new Dictionary<string, LanguageDefinition>();
			foreach (XmlElement languageElement in definitionDoc.SelectNodes("//mod:definitions/mod:language", nm))
			{
				string name = languageElement.GetAttribute("name");
				bool caseSensivitive = languageElement.GetAttribute("caseSensitive").ContainsAnyOf("false", "no", "0");

				LanguageDefinition definition = new LanguageDefinition(name, caseSensivitive);
				foreach (XmlElement commentNode in languageElement.SelectNodes("mod:comments/mod:comment", nm))
					definition.Comments.Add(commentNode.InnerText.Trim());

				foreach (XmlElement quoteNode in languageElement.SelectNodes("mod:quotes/mod:quote", nm))
					definition.Quotes.Add(quoteNode.InnerText.Trim());

				foreach (XmlElement groupNode in languageElement.SelectNodes("mod:keywords/mod:group", nm))
				{
					bool treatAsWord = !groupNode.GetAttribute("treatAsWord").ContainsAnyOf("false", "no", "0");
					string groupName = groupNode.GetAttribute("name");
					string keywords = groupNode.InnerText
						.ReplaceAll(@"\n", "|")
						.ReplaceAll(@"[\s\t\r]", string.Empty)
						.ReplaceAll(@"\|\|", "|")
						.Trim('|');

					definition.Expressions.Add(new ExpressionGroup(groupName, keywords, treatAsWord, caseSensivitive));
				}

				languages.Add(name, definition);
			}

			initialized = true;
		}
	}
}
