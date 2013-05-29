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
namespace Sage.Extensibility
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml;

	internal class PackageGroup
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

		public List<string> Includes { get; private set; }

		public List<string> Excludes { get; private set; }

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
				string test = include;
				if (test == "*")
					includes.AddRange(names);

				else if (include.StartsWith("*") && include.EndsWith("*"))
				{
					includes.AddRange(names.Where(n => n.Contains(test.Replace("*", string.Empty))));
				}

				else if (include.EndsWith("*"))
					includes.AddRange(names.Where(n => n.StartsWith(test.Replace("*", string.Empty))));

				else if (include.StartsWith("*"))
					includes.AddRange(names.Where(n => n.EndsWith(test.Replace("*", string.Empty))));

				else
					includes.AddRange(names.Where(n => n == test));
			}

			List<string> excludes = new List<string>();
			foreach (string exclude in this.Excludes)
			{
				string test = exclude;
				if (test == "*")
					throw new InvalidOperationException("Exclude group is not allowed to be single wildcard '*' (meaning 'all').");

				if (exclude.StartsWith("*") && exclude.EndsWith("*"))
					excludes.AddRange(names.Where(n => !n.Contains(test.Replace("*", string.Empty))));

				else if (exclude.EndsWith("*"))
					excludes.AddRange(names.Where(n => !n.StartsWith(test.Replace("*", string.Empty))));

				else if (exclude.StartsWith("*"))
					excludes.AddRange(names.Where(n => !n.EndsWith(test.Replace("*", string.Empty))));

				else
					excludes.AddRange(names.Where(n => n != test));
			}

			includes = includes.Distinct().ToList();
			excludes = excludes.Distinct().ToList();

			return includes.Where(include => !excludes.Contains(include)).ToList();
		}

		private static IEnumerable<string> GetAllFilesInDirectory(SageContext context, string directory)
		{
			string resolved = context.Path.Resolve(directory);
			if (!Directory.Exists(resolved))
				return new List<string>();

			return Directory.GetFiles(resolved, "*.*", SearchOption.AllDirectories).ToList();
		}

		private static IEnumerable<string> GetAllFilesOfTypeInDirectory(SageContext context, string directory, string filter)
		{
			string resolved = context.Path.Resolve(directory);
			if (!Directory.Exists(resolved))
				return new List<string>();

			return new List<string>(Directory.GetFiles(resolved, filter, SearchOption.AllDirectories));
		}

		private static IEnumerable<string> GetAllFilesStartingWithInDirectory(SageContext context, string startsWith)
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

		private static IEnumerable<string> GetAllFilesFrom(SageContext context, string filepath)
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

		private static List<string> ExcludeAllFilesInDirectory(IEnumerable<string> files, SageContext context, string directory)
		{
			string resolved = context.Path.Resolve(directory).ToLower();
			return files.Where(f => !f.ToLower().StartsWith(resolved)).ToList();
		}

		private static List<string> ExcludeAllFilesOfTypeInDirectory(IEnumerable<string> files, SageContext context, string directory, string filter)
		{
			string extension = filter.Trim('*');
			if (string.IsNullOrEmpty(directory))
			{
				return files.Where(f => !(f.ToLower().EndsWith(extension))).ToList();
			}

			string resolved = context.Path.Resolve(directory).ToLower();
			return files.Where(f => !(f.ToLower().StartsWith(resolved) && f.ToLower().EndsWith(extension))).ToList();
		}

		private static List<string> ExcludeAllFilesStartingWithInDirectory(IEnumerable<string> files, SageContext context, string startsWith)
		{
			string directory = Path.GetDirectoryName(startsWith);
			string filter = Path.GetFileName(startsWith).ToLower();

			string resolved = context.Path.Resolve(directory).ToLower();

			return files.Where(f => !(f.ToLower().StartsWith(resolved) && Path.GetFileName(f).ToLower().StartsWith(filter))).ToList();
		}

		private static List<string> ExcludeAllFilesFrom(IEnumerable<string> files, SageContext context, string filepath)
		{
			IEnumerable<string> excludeFiles = GetAllFilesFrom(context, filepath);

			return files.Where(f => excludeFiles.Count(e => e.ToLower() == f.ToLower()) == 0).ToList();
		}
	}
}
