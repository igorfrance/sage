namespace Sage.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml;

	public class PackageGroup
	{
		//// {assetpath}/somewhere/*
		private static readonly Regex wildcardFolder = new Regex(@"(.*)(?:/\*)$");
		//// {assetpath}/somewhere/*.css
		private static readonly Regex wildcardFileType = new Regex(@"(.*)(\*\.\w+)$");
		//// {assetpath}/somewhere*
		private static readonly Regex wildcardFilePath = new Regex(@"(.*(?<!/))\*$");

		public PackageGroup(XmlNode groupElement)
		{
			this.Includes = new List<string>();
			this.Excludes = new List<string>();

			if (groupElement != null)
			{
				foreach (XmlNode includeNode in groupElement.SelectNodes("p:include", XmlNamespaces.Manager))
					this.Includes.Add(includeNode.InnerText.Trim());

				foreach (XmlNode excludeNode in groupElement.SelectNodes("p:exclude", XmlNamespaces.Manager))
					this.Excludes.Add(excludeNode.InnerText.Trim());
			}
		}

		public List<string> GetFiles(SageContext context)
		{
			List<string> result = new List<string>();

			Match match;

			foreach (string include in this.Includes)
			{
				if ((match = wildcardFolder.Match(include)).Success)
					result.AddRange(GetAllFilesInDirectory(context, match.Groups[1].Value));

				else if ((match = wildcardFileType.Match(include)).Success)
					result.AddRange(GetAllFilesOfTypeInDirectory(context, match.Groups[1].Value, match.Groups[2].Value));

				else if ((match = wildcardFilePath.Match(include)).Success)
					result.AddRange(GetAllFilesStartingWithInDirectory(context, match.Groups[1].Value));

				else
					result.AddRange(GetAllFilesFrom(context, include));
			}

			foreach (string exclude in this.Excludes)
			{
				if ((match = wildcardFolder.Match(exclude)).Success)
					result = ExcludeAllFilesInDirectory(result, context, match.Groups[1].Value);

				else if ((match = wildcardFileType.Match(exclude)).Success)
					result = ExcludeAllFilesOfTypeInDirectory(result, context, match.Groups[1].Value, match.Groups[2].Value);

				else if ((match = wildcardFilePath.Match(exclude)).Success)
					result = ExcludeAllFilesStartingWithInDirectory(result, context, match.Groups[1].Value);

				else
					result = ExcludeAllFilesFrom(result, context, exclude);
			}


			return result;
		}

		public List<string> FilterNames(List<string> names)
		{
			List<string> includes = new List<string>();
			foreach (string include in this.Includes)
			{
				if (include == "*")
					includes.AddRange(names);

				else if (include.StartsWith("*") && include.EndsWith("*"))
					includes.AddRange(names.Where(n => n.Contains(include.Replace("*", string.Empty))));

				else if (include.EndsWith("*"))
					includes.AddRange(names.Where(n => n.StartsWith(include.Replace("*", string.Empty))));

				else if (include.StartsWith("*"))
					includes.AddRange(names.Where(n => n.EndsWith(include.Replace("*", string.Empty))));

				else
					includes.AddRange(names.Where(n => n == include));
			}

			List<string> excludes = new List<string>();
			foreach (string exclude in this.Excludes)
			{
				if (exclude == "*")
					throw new InvalidOperationException("Exclude group is not allowed to be single wildcard '*' (meaning 'all').");

				if (exclude.StartsWith("*") && exclude.EndsWith("*"))
					excludes.AddRange(names.Where(n => !n.Contains(exclude.Replace("*", string.Empty))));

				else if (exclude.EndsWith("*"))
					excludes.AddRange(names.Where(n => !n.StartsWith(exclude.Replace("*", string.Empty))));

				else if (exclude.StartsWith("*"))
					excludes.AddRange(names.Where(n => !n.EndsWith(exclude.Replace("*", string.Empty))));

				else
					excludes.AddRange(names.Where(n => n != exclude));
			}

			includes = includes.Distinct().ToList();
			excludes = excludes.Distinct().ToList();

			return includes.Where(include => !excludes.Contains(include)).ToList();
		}

		public List<string> Includes { get; private set; }

		public List<string> Excludes { get; private set; }

		private static List<string> GetAllFilesInDirectory(SageContext context, string directory)
		{
			string resolved = context.Path.Resolve(directory);
			if (!Directory.Exists(resolved))
				return new List<string>();

			return Directory.GetFiles(resolved, "*.*", SearchOption.AllDirectories).ToList();
		}

		private static List<string> GetAllFilesOfTypeInDirectory(SageContext context, string directory, string filter)
		{
			string resolved = context.Path.Resolve(directory);
			if (!Directory.Exists(resolved))
				return new List<string>();

			return new List<string>(Directory.GetFiles(resolved, filter, SearchOption.AllDirectories));
		}

		private static List<string> GetAllFilesStartingWithInDirectory(SageContext context, string startsWith)
		{
			string directory = Path.GetDirectoryName(startsWith);
			string filter = Path.GetFileName(startsWith).ToLower();

			string resolved = context.Path.Resolve(directory);
			if (!Directory.Exists(resolved))
				return new List<string>();

			return new List<string>(
				Directory.GetFiles(resolved, "*.*", SearchOption.AllDirectories)
					.Where(f => f.ToLower().StartsWith(filter)));
		}

		private static List<string> GetAllFilesFrom(SageContext context, string filepath)
		{
			List<string> result = new List<string>();

			string resolved = context.Path.Resolve(filepath);
			if (Directory.Exists(resolved))
			{
				result.AddRange(Directory.GetFiles(resolved, "*.*", SearchOption.AllDirectories));
			}
			else if (File.Exists(resolved))
			{
				result.Add(resolved);
			}

			return result;
		}

		private List<string> ExcludeAllFilesInDirectory(List<string> files, SageContext context, string directory)
		{
			string resolved = context.Path.Resolve(directory).ToLower();
			return files.Where(f => !f.ToLower().StartsWith(resolved)).ToList();
		}

		private List<string> ExcludeAllFilesOfTypeInDirectory(List<string> files, SageContext context, string directory, string filter)
		{
			string resolved = context.Path.Resolve(directory).ToLower();
			string extension = filter.Replace("*", string.Empty).ToLower();

			return files.Where(f => !(f.ToLower().StartsWith(resolved) && f.ToLower().EndsWith(extension))).ToList();
		}

		private List<string> ExcludeAllFilesStartingWithInDirectory(List<string> files, SageContext context, string startsWith)
		{
			string directory = Path.GetDirectoryName(startsWith);
			string filter = Path.GetFileName(startsWith).ToLower();

			string resolved = context.Path.Resolve(directory).ToLower();

			return files.Where(f => !(f.ToLower().StartsWith(resolved) && Path.GetFileName(f).ToLower().StartsWith(filter))).ToList();
		}

		private List<string> ExcludeAllFilesFrom(List<string> files, SageContext context, string filepath)
		{
			List<string> excludeFiles = GetAllFilesFrom(context, filepath);

			return files.Where(f => excludeFiles.Where(e => e.ToLower() == f.ToLower()).Count() == 0).ToList();
		}
	}
}
