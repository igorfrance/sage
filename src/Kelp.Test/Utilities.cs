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
namespace Kelp.Test
{
	using System;
	using System.IO;
	using System.Reflection;

	using Kelp.ResourceHandling;

	internal class Utilities
	{
		internal const string ScriptPath = "TestData\\Scripts\\";
		internal const string StylePath = "TestData\\Styles\\";
		internal const string ImagePath = "TestData\\Images\\";
		internal const string XmlPath = "TestData\\Xml\\";
		internal const string TempPath = "TestData\\Temp\\";
		internal const string MockDataPath = "TestData\\MockData\\";

		internal static string ApplicationPath
		{
			get
			{
				return Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
			}
		}

		/// <summary>
		/// Gets the script contents.
		/// </summary>
		/// <param name="fileName">Name of the script.</param>
		/// <returns></returns>
		internal static string GetScriptContents(string fileName)
		{
			return GetFileContents(ScriptPath, fileName);
		}

		/// <summary>
		/// Gets the expanded version of the relative script path.
		/// </summary>
		/// <param name="fileName">The name (relative path) of the script path to expand.</param>
		/// <returns>The expanded version of the relative script path</returns>
		internal static string GetScriptPath(string fileName)
		{
			return GetFilePath(ScriptPath, fileName);
		}

		/// <summary>
		/// Gets the expanded version of the relative image path.
		/// </summary>
		/// <param name="fileName">The name (relative path) of the script path to expand.</param>
		/// <returns>The expanded version of the relative script path</returns>
		internal static string GetImagePath(string fileName)
		{
			return GetFilePath(ImagePath, fileName);
		}

		/// <summary>
		/// Gets the CSS style contents.
		/// </summary>
		/// <param name="fileName">Name of the CSS style file.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">If <paramref name="fileName"/> is <c>null</c>.</exception>
		/// <exception cref="FileNotFoundException">If <paramref name="fileName"/> doesn't exist in the designated test data 
		/// directory.</exception>
		internal static string GetStyleContents(string fileName)
		{
			return GetFileContents(StylePath, fileName);
		}

		/// <summary>
		/// Gets the expanded version of the relative CSS style path.
		/// </summary>
		/// <param name="fileName">The name (relative path) of the CSS style path to expand.</param>
		/// <returns>The expanded version of the relative CSS style path</returns>
		internal static string GetStylePath(string fileName)
		{
			return GetFilePath(StylePath, fileName);
		}

		/// <summary>
		/// Gets the expanded version of the relative xml path.
		/// </summary>
		/// <param name="fileName">The name (relative path) of the xml path to expand.</param>
		/// <returns>The expanded version of the relative xml path</returns>
		internal static string GetXmlPath(string fileName)
		{
			if (Path.IsPathRooted(fileName))
				return fileName;

			if (fileName.StartsWith(XmlPath, StringComparison.InvariantCultureIgnoreCase))
				return ExpandPath(fileName);

			return ExpandPath(string.Concat(XmlPath, fileName));
		}

		/// <summary>
		/// Gets the expanded version of the relative mock data path.
		/// </summary>
		/// <param name="fileName">The name (relative path) of the xml path to expand.</param>
		/// <returns>The expanded version of the relative xml path</returns>
		internal static string GetMockDataPath(string fileName)
		{
			if (Path.IsPathRooted(fileName))
				return fileName;

			if (fileName.StartsWith(MockDataPath, StringComparison.InvariantCultureIgnoreCase))
				return ExpandPath(fileName);

			return ExpandPath(string.Concat(MockDataPath, fileName));
		}

		/// <summary>
		/// Expands the specified relative path to the full physical version of it 
		/// (resolved from the location of the currently executing assembly).
		/// </summary>
		/// <param name="relativePath">The relative path to resolve.</param>
		/// <returns>The expanded version of the specified relative path</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="relativePath"/> is <c>null</c> or empty.</exception>
		internal static string ExpandPath(string relativePath)
		{
			if (string.IsNullOrEmpty(relativePath))
				throw new ArgumentNullException("relativePath");

			if (Path.IsPathRooted(relativePath))
				return relativePath;

			string expanded = Path.Combine(ApplicationPath, relativePath);

			if (File.Exists(expanded))
				return new FileInfo(expanded).FullName;

			if (Directory.Exists(expanded))
				return new DirectoryInfo(expanded).FullName;

			return expanded;
		}

		internal static string MapPath(string relativePath)
		{
			return relativePath
				.Replace("~", ApplicationPath)
				.Replace("/", "\\");
		}

		internal static void ClearTemporaryDirectory()
		{
			var tempDirectory = Utilities.MapPath(Configuration.Current.TemporaryDirectory);
			if (Directory.Exists(tempDirectory))
				Directory.Delete(tempDirectory, true);
		}

		private static string GetFileContents(string parentPath, string scriptName)
		{
			if (string.IsNullOrEmpty(parentPath))
				throw new ArgumentNullException("parentPath");
			if (string.IsNullOrEmpty(scriptName))
				throw new ArgumentNullException("scriptName");

			string path = GetFilePath(parentPath, scriptName);
			if (!File.Exists(path))
				throw new FileNotFoundException(String.Format("The specified file '{0}' doesn't exist in {1}", scriptName, parentPath));

			return File.ReadAllText(path);
		}

		private static string GetFilePath(string parentPath, string fileName)
		{
			if (Path.IsPathRooted(fileName))
				return fileName;

			if (fileName.StartsWith(parentPath, StringComparison.InvariantCultureIgnoreCase))
				return ExpandPath(fileName);

			return ExpandPath(string.Concat(parentPath, fileName));
		}
	}
}