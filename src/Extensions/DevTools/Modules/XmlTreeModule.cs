/**
 * Open Source Initiative OSI - The MIT License (MIT):Licensing
 * [OSI Approved License]
 * The MIT License (MIT)
 *
 * Copyright (c) 2011 Igor France
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
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
