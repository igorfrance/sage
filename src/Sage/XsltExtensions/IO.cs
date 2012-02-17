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
		Justification = "This is an XSLT extension class, these methods will not be used from C#.")]
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
		/// <returns>TODO: Add documentation for readDir.</returns>
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