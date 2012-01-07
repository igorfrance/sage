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

	[XsltExtensionObject(XmlNamespaces.Extensions.IO)]
	[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter")]
	public class IO
	{
		private static readonly XmlDocument document = new XmlDocument();

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