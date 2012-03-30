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
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml;

	using Kelp.Extensions;
	using Kelp.SyntaxHighlighting;
	using log4net;
	using Sage.Modules;
	using Sage.Views;

	public class SyntaxHighlighterModule : IModule
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(SyntaxHighlighterModule).FullName);
		private static readonly Regex indentExpr = new Regex(@"^([\s\t]+)(?=\S)", RegexOptions.Compiled);

		private static readonly XmlNamespaceManager nm = XmlNamespaces.Manager;
		private static bool initialized;
		private static Dictionary<string, LanguageDefinition> languages;

		public ModuleResult ProcessElement(XmlElement moduleElement, ViewConfiguration configuration)
		{
			SageContext context = configuration.Context;
			Initialize(context);

			if (languages.Count == 0)
			{
				log.ErrorFormat("The syntax highligher module isn't configured with any languages. Until this is fixed, the module will not work.");
				return new ModuleResult(ModuleResultStatus.ConfigurationError);
			}

			XmlNode languageNode = moduleElement.SelectSingleNode("mod:config/mod:language", nm);
			XmlElement codeNode = moduleElement.SelectSingleElement("mod:config/mod:code", nm);
			XmlNodeList keywordGroups = moduleElement.SelectNodes("mod:config/mod:keywords/mod:group", nm);

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

			List<ExpressionGroup> additionalGroups = new List<ExpressionGroup>();
			if (keywordGroups.Count != 0)
			{
				additionalGroups = new List<ExpressionGroup>();
				foreach (XmlElement keywordElement in keywordGroups)
				{
					additionalGroups.Add(new ExpressionGroup(keywordElement, languages[language].CaseSensitive));
				}
			}

			SyntaxHighlighter highlighter = new SyntaxHighlighter(languages[language], additionalGroups);
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
			XmlNode definitionRoot = definitionDoc.SelectSingleNode("//mod:definitions", nm);

			languages = new Dictionary<string, LanguageDefinition>();
			foreach (XmlElement languageElement in definitionRoot.SelectNodes("mod:language | mod:xmllanguage", nm))
			{
				LanguageDefinition definition;
				if (languageElement.LocalName == "xmllanguage")
					definition = new ModularXmlLanguageDefinition(languageElement);
				else
					definition = new ModularLanguageDefinition(languageElement);

				languages.Add(definition.Name, definition);
			}

			initialized = true;
		}
	}
}
