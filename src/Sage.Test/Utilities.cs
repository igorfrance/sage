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
namespace Sage.Test
{
	using System;
	using System.IO;
	using System.Reflection;

	internal static class Utilities
	{
		internal static string ApplicationPath
		{
			get
			{
				return Path.GetDirectoryName(
					Assembly.GetExecutingAssembly()
						.CodeBase
						.Replace("file:///", string.Empty)
						.Replace("/", "\\"));
			}
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

		internal static string ExpandResourcePath(string resourcePath)
		{
			if (string.IsNullOrEmpty(resourcePath))
				throw new ArgumentNullException("resourcePath");

			if (Path.IsPathRooted(resourcePath))
				return resourcePath;

			return ExpandPath(Path.Combine("Resources", resourcePath));
		}
	}
}
