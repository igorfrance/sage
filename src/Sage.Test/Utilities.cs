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
