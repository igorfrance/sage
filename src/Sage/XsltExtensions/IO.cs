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
namespace Sage.XsltExtensions
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.XPath;

	using Sage.Extensibility;

	/// <summary>
	/// Provides several IO-related utility methods for use in XSLT.
	/// </summary>
	[XsltExtensionObject(XmlNamespaces.Extensions.IO)]
	[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", 
		Justification = "This is an XSLT extension class, these methods are not intended for use from C#.")]
	public class IO
	{
		private static readonly XmlDocument document = new XmlDocument();

		/// <summary>
		/// Returns an XML tree with the names of files in the specified directory <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The path to the directory to read.</param>
		/// <param name="filter">The filter (wildcards) to apply when getting files.</param>
		/// <param name="recursive">If set to <c>true</c>, all subdirectories of <paramref name="path"/>
		/// will be scanned too.</param>
		/// <returns>An XML tree with the names of files in the specified directory <paramref name="path"/>.</returns>
		public static XPathNavigator readDir(string path, string filter, bool recursive = false)
		{
			Regex filterExpression = null;
			if (!string.IsNullOrWhiteSpace(filter) && filter != "*.*")
			{
				if (filter.Contains("*."))
				{
					var fs = new List<string>();
					foreach (var f in filter.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
					{
						fs.Add(f.Replace("*.", string.Empty));
					}

					filterExpression = new Regex(string.Format(@"\.(?:{0})$", string.Join("|", fs.ToArray())));
				}
				else
				{
					filterExpression = new Regex(filter);
				}
			}

			XmlElement result = GetDirectory(path, filterExpression, recursive);
			return result.CreateNavigator();
		}

		private static XmlElement GetDirectory(string path, Regex filter, bool recursive)
		{
			XmlElement result = document.CreateElement("directory");
			result.SetAttribute("path", path);
			result.SetAttribute("name", Path.GetFileName(path));

			if (!Directory.Exists(path))
			{
				result.SetAttribute("exists", "false");
				return result;
			}

			string[] entries = Directory.GetFileSystemEntries(path);

			foreach (var entry in entries.Where(e => (File.GetAttributes(e) & FileAttributes.Directory) != 0))
			{
				if (filter != null && !filter.IsMatch(entry))
				{
					continue;
				}

				if (recursive)
				{
					result.AppendChild(GetDirectory(entry, filter, true));
				}
				else
				{
					XmlElement directoryElement = document.CreateElement("directory");
					directoryElement.SetAttribute("path", entry);
					directoryElement.SetAttribute("name", Path.GetFileName(entry));

					result.AppendChild(directoryElement);
				}
			}

			foreach (var entry in entries.Where(e => (File.GetAttributes(e) & FileAttributes.Directory) == 0))
			{
				if (filter != null && !filter.IsMatch(entry))
				{
					continue;
				}

				XmlElement fileElement = document.CreateElement("file");
				fileElement.SetAttribute("path", entry);
				fileElement.SetAttribute("name", Path.GetFileName(entry));

				result.AppendChild(fileElement);
			}

			return result;
		}
	}
}