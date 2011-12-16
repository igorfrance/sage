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
