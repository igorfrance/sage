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
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml;

	using Kelp.Extensions;
	using log4net;
	using Sage.Modules;
	using Sage.ResourceManagement;
	using Sage.Views;

	/// <summary>
	/// Provides a module that reads directories.
	/// </summary>
	/// <remarks>
	/// The configuration element for this module has the following structure:
	/// <para><pre>&lt;mod:config&gt;
	///    &lt;mod:path /&gt;
	///    &lt;mod:recursive /&gt;
	///    &lt;mod:directoriesOnly /&gt;
	///    &lt;mod:filesOnly /&gt;
	///    &lt;mod:pattern /&gt;
	///    &lt;mod:expression /&gt;
	/// &lt;/mod:config&gt;</pre></para>
	/// <list type="table">
	/// <listheader>
	/// <term>name</term>
	/// <term>type</term>
	/// <term>description</term>
	/// </listheader>
	/// <item>
	/// <term>path</term>
	/// <term>string</term>
	/// <definition>specifies the path of the directory to read. Unless absolute, the path will be server-mapped within
	/// the current application. Required.</definition>
	/// </item>
	/// <item>
	/// <term>recursive</term>
	/// <term>boolean</term>
	/// <definition>If <c>true</c>, the path's subdirectories should be scanned recursively as well. Optional. Default is <c>false</c>.</definition>
	/// </item>
	/// <item>
	/// <term>directoriesOnly</term>
	/// <term>boolean</term>
	/// <definition>If <c>true</c>, the only directories will be returned. Optional. Default is <c>false</c>.</definition>
	/// </item>
	/// <item>
	/// <term>filesOnly</term>
	/// <term>boolean</term>
	/// <definition>If <c>true</c>, the only files will be returned. Note that if both <c>directoriesOnly</c> and <c>filesOnly</c>
	/// options are specified, only the files will be returned. Optional. Default is <c>false</c>.</definition>
	/// </item>
	/// <item>
	/// <term>pattern</term>
	/// <term>string</term>
	/// <definition>File pattern to match (e.g. *.txt). Optional. Default is '*'.</definition>
	/// </item>
	/// <item>
	/// <term>expression</term>
	/// <term>string</term>
	/// <definition>The regular expression string that the files and/or folders should match to be returned. Optional.</definition>
	/// </item>
	/// </list>
	/// </remarks>
	public class DirectoryTreeModule : IModule
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(DirectoryTreeModule).FullName);

		private string path;
		private string absolutePath;
		private string pattern = "*";
		private Regex expression;
		private bool recursive;
		private bool filesOnly;
		private bool absolutePaths;
		private bool directoriesOnly;
		private XmlElement moduleElement;

		public ModuleResult ProcessElement(XmlElement moduleElement, ViewConfiguration configuration)
		{
			SageContext context = configuration.Context;
			this.moduleElement = moduleElement;

			XmlNode configNode = moduleElement.SelectSingleNode("mod:config", XmlNamespaces.Manager);
			if (configNode == null)
			{
				log.ErrorFormat("The {0} element doesn't have the mod:config node. Skipping further work", typeof(DirectoryTreeModule).FullName);
				return new ModuleResult(moduleElement, ModuleResultStatus.MissingParameters);
			}

			XmlNode valueNode;
			if ((valueNode = configNode.SelectSingleNode("mod:path", XmlNamespaces.Manager)) == null)
			{
				log.ErrorFormat("The {0} element doesn't have the mod:config/mod:path node. Skipping further work", typeof(DirectoryTreeModule).FullName);
				return new ModuleResult(moduleElement, ModuleResultStatus.MissingParameters);
			}

			this.path = valueNode.InnerText;
			this.absolutePath = this.GetDirectoryPath(moduleElement, context);

			if (configNode.SelectSingleNode("mod:recursive", XmlNamespaces.Manager) != null)
				this.recursive = true;

			if (configNode.SelectSingleNode("mod:filesOnly", XmlNamespaces.Manager) != null)
				this.filesOnly = true;

			if (configNode.SelectSingleNode("mod:directoriesOnly", XmlNamespaces.Manager) != null)
				this.directoriesOnly = true;

			if (configNode.SelectSingleNode("mod:absolutePaths", XmlNamespaces.Manager) != null)
				this.absolutePaths = true;

			if ((valueNode = configNode.SelectSingleNode("mod:pattern", XmlNamespaces.Manager)) != null)
				this.pattern = valueNode.InnerText.Trim();

			if ((valueNode = configNode.SelectSingleNode("mod:expression", XmlNamespaces.Manager)) != null)
			{
				string text = valueNode.InnerText.Trim();
				if (!string.IsNullOrEmpty(text))
				{
					try
					{
						this.expression = new Regex(text);
					}
					catch (Exception ex)
					{
						log.WarnFormat("Invalid regular expression text: '{0}': {1}", text, ex.Message);
					}
				}
			}

			if (!Directory.Exists(this.absolutePath))
			{
				log.WarnFormat("The directory '{0}' (mapped to '{1}') doesn't exist.", this.path, this.absolutePath);
				return new ModuleResult(moduleElement, ModuleResultStatus.NoData);
			}

			ModuleResult result = new ModuleResult(moduleElement);
			result.AppendDataNode(ScanDirectory(absolutePath, context));

			return result;
		}

		private XmlElement ScanDirectory(string path, SageContext context)
		{
			XmlDocument doc = moduleElement.OwnerDocument;
			XmlElement result = doc.CreateElement("mod:directory", XmlNamespaces.ModulesNamespace);
			result.SetAttribute("name", Path.GetFileName(path));
			result.SetAttribute("path", context.Path.GetRelativeWebPath(path, true));
			if (this.absolutePaths)
				result.SetAttribute("absolute", path);

			if (!filesOnly)
			{
				string[] directories = Directory.GetDirectories(path);
				if (expression != null)
					directories = directories.Where(f => expression.IsMatch(f)).ToArray();

				if (this.recursive)
				{
					foreach (string directory in directories)
						result.AppendChild(ScanDirectory(directory, context));
				}
				else
				{
					foreach (string directory in directories)
					{
						XmlElement dirElem = result.AppendElement(doc.CreateElement("mod:directory", XmlNamespaces.ModulesNamespace));
						dirElem.SetAttribute("name", Path.GetFileName(directory));
						dirElem.SetAttribute("path", context.Path.GetRelativeWebPath(directory, true));
						if (this.absolutePaths)
							dirElem.SetAttribute("absolute", directory);
					}
				}
			}

			if ((filesOnly && directoriesOnly) || !directoriesOnly)
			{
				string[] files = Directory.GetFiles(path, this.pattern);
				if (expression != null)
					files = files.Where(f => expression.IsMatch(f)).ToArray();

				foreach (string file in files)
				{
					XmlElement fileElem = result.AppendElement(doc.CreateElement("mod:file", XmlNamespaces.ModulesNamespace));
					fileElem.SetAttribute("name", Path.GetFileName(file));
					fileElem.SetAttribute("path", context.Path.GetRelativeWebPath(file, true));
					if (this.absolutePaths)
						fileElem.SetAttribute("absolute", file);
				}
			}

			return result;
		}

		private string GetDirectoryPath(XmlElement moduleElement, SageContext context)
		{
			XmlNode valueNode = moduleElement.SelectSingleNode("mod:config/mod:path", XmlNamespaces.Manager);
			if (valueNode == null || string.IsNullOrWhiteSpace(valueNode.InnerText))
			{
				return null;
			}

			var path = valueNode.InnerText.Trim();
			if (Path.IsPathRooted(path))
				return path;

			if (path.StartsWith("~/"))
				return context.MapPath(path);

			CacheableXmlDocument document = (CacheableXmlDocument) moduleElement.OwnerDocument;

			var uri = new Uri(document.BaseURI);
			var directory = Path.GetDirectoryName(uri.AbsolutePath);
			return path == "." ? directory : Path.Combine(directory, path);
		}
	}
}
