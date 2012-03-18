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
	using System.IO;
	using System.Text.RegularExpressions;
	using System.Xml;

	using Kelp.Extensions;

	using log4net;
	using Sage.Modules;
	using Sage.Views;

	public class XmlTreeModule : IModule
	{
		private static readonly XmlNamespaceManager nm = XmlNamespaces.Manager;
		private static readonly Regex defaultNamespaceNodes = new Regex(@"(/|^)([\w\.]+)(/|$)", RegexOptions.Compiled);
		private static readonly ILog log = LogManager.GetLogger(typeof(XmlTreeModule).FullName);

		public ModuleResult ProcessElement(XmlElement moduleElement, ViewConfiguration configuration)
		{
			SageContext context = configuration.Context;
			ModuleResult result = new ModuleResult(moduleElement);

			XmlNode pathNode = moduleElement.SelectSingleNode("mod:config/mod:source", nm);
			XmlNode selectionNode = moduleElement.SelectSingleNode("mod:config/mod:xpath", nm);
			XmlNode noincludesNode = moduleElement.SelectSingleNode("mod:config/mod:noIncludes", nm);
			XmlNodeList namespaceNodes = moduleElement.SelectNodes("mod:config/mod:namespaces/mod:namespace", nm);

			string xpath = selectionNode != null ? selectionNode.InnerText : null;
			bool skipIncludes = noincludesNode != null && noincludesNode.InnerText.ContainsAnyOf("yes", "1", "true");

			if (pathNode == null)
			{
				result.Status = ModuleResultStatus.None;
				return result;
			}

			string path = pathNode.InnerText.Trim();
			if (string.IsNullOrEmpty(path))
			{
				log.WarnFormat("The path element shouldn't be empty.");
				result.Status = ModuleResultStatus.ModuleWarning;
				return result;
			}

			path = context.Path.Substitute(path);
			if (!Path.IsPathRooted(path))
			{
				string configDirectory = Path.GetDirectoryName(configuration.Info.ConfigPath);
				path = Path.Combine(configDirectory, path);
			}

			string resolved = context.Path.Resolve(path);
			if (!File.Exists(resolved))
			{
				log.WarnFormat("The specified xml path '{0}' ('{1}') doesn't exist", path, resolved);
				result.Status = ModuleResultStatus.ModuleWarning;
				return result;
			}

			XmlNode selection;
			try
			{
				if (skipIncludes)
				{
					selection = new XmlDocument();
					((XmlDocument) selection).Load(resolved);
				}
				else
				{
					selection = context.Resources.LoadXml(resolved);
				}
			}
			catch (Exception ex)
			{
				log.ErrorFormat("Failed to add specified file '{0}' ('{1}') as XML document: {2}", 
					path, resolved, ex.Message);

				result.Status = ModuleResultStatus.ModuleError;
				return result;
			}

			if (!string.IsNullOrEmpty(xpath))
			{
				try
				{
					XmlNamespaceManager manager = new XmlNamespaceManager(new NameTable());
					manager.AddNamespace("default", XmlNamespaces.XHtmlNamespace);

					xpath = defaultNamespaceNodes.Replace(xpath, "$1default:$2$3");
					foreach (XmlElement node in namespaceNodes)
					{
						string prefix = node.GetAttribute("prefix");
						manager.AddNamespace(prefix, node.InnerText.Trim());
					}

					selection = selection.SelectSingleNode(xpath, manager);
					if (selection == null)
					{
						log.WarnFormat("The xpath expression '{0}' yielded no result.", xpath);
						result.Status = ModuleResultStatus.ModuleWarning;
						return result;
					}
				}
				catch (Exception ex)
				{
					log.ErrorFormat("Attempting to select using xpath expression '{0}' resulted in error: {1}.",
						xpath, ex.Message);

					result.Status = ModuleResultStatus.ModuleError;
					return result;
				}
			}
			else if (selectionNode != null)
			{
				log.WarnFormat("The selection element shouldn't be empty.");
				result.Status = ModuleResultStatus.ModuleWarning;
			}

			result.AppendDataElement(selection);
			return result;
		}
	}
}
