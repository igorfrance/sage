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
namespace Kelp.ResourceHandling
{
	using System;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;

	using Microsoft.Ajax.Utilities;

	/// <summary>
	/// Implements a CSS file merger/processor.
	/// </summary>
	public class CssFile : CodeFile
	{
		private static readonly Regex rxpReferencePath =
			new Regex(@"(url\s*\((?:""|')?)(.*?)((?:""|')?\))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>
		/// Initializes a new instance of the <see cref="CssFile"/> class, using the specified absolute and relative paths.
		/// </summary>
		/// <param name="absolutePath">The path of the file to load.</param>
		/// <param name="relativePath">The relative path of the file to load.</param>
		public CssFile(string absolutePath, string relativePath)
			: base(absolutePath, relativePath)
		{
			this.ContentType = "text/css";
		}

		internal override string ConfigurationSettings
		{
			get
			{
				return Configuration.Current.Css.ToString();
			}
		}

		/// <inheritdoc/>
		protected override bool MinificationEnabled
		{
			get
			{
				return Configuration.Current.Css.Enabled;
			}
		}

		/// <inheritdoc/>
		public override string Minify(string sourceCode)
		{
			return Minify(sourceCode, Configuration.Current.Css.Settings);
		}

		/// <summary>
		/// Minifies the specified <paramref name="sourceCode"/>, according to the specified minification <paramref name="settings"/>.
		/// </summary>
		/// <param name="sourceCode">The source code string to minify.</param>
		/// <param name="settings">The object that specifies the minification settings for this file.</param>
		/// <returns>
		/// The minified version of this file's content.
		/// </returns>
		public string Minify(string sourceCode, CssSettings settings)
		{
			Minifier min = new Minifier();
			return min.MinifyStyleSheet(sourceCode, settings);
		}

		/// <inheritdoc/>
		protected override void Load()
		{
			base.Load();
			this.FixRelativePaths();
		}

		private static string CombineUrls(string folderUrl, string fileUrl)
		{
			if (folderUrl == string.Empty)
				return fileUrl;

			string filePath = string.Concat(folderUrl, "/", fileUrl).Replace("//", "/");
			while (Regex.Match(filePath, @"[^/]+/\.\./").Success)
			{
				filePath = Regex.Replace(filePath, @"[^/]+/\.\./", string.Empty);
			}

			filePath = Regex.Replace(filePath, "/{2,}", "/");
			return filePath;
		}

		private void FixRelativePaths()
		{
			string fileFolder = this.RelativePath.Contains("/")
				? this.RelativePath.Substring(0, this.RelativePath.LastIndexOf("/") + 1)
				: string.Empty;

			this.rawContent = rxpReferencePath.Replace(this.rawContent, delegate(Match m)
			{
				string fileName = m.Groups[2].Value;
				if (fileName.StartsWith("http") || fileName.StartsWith("/"))
					return m.Groups[0].Value;

				return string.Concat(m.Groups[1].Value, CombineUrls(fileFolder, fileName), m.Groups[3].Value);
			});
		}
	}
}
