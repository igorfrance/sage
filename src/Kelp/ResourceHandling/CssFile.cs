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
			: this(absolutePath, relativePath, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CssFile"/> class, using the specified absolute and relative paths, 
		/// and the <paramref name="mappingFunction"/>.
		/// </summary>
		/// <param name="absolutePath">The absolute path of the file to load.</param>
		/// <param name="relativePath">The relative path of the file to load.</param>
		/// <param name="mappingFunction">The function to use to map relative and virtual paths to absolute.</param>
		public CssFile(string absolutePath, string relativePath, Func<string, string> mappingFunction)
			: base(absolutePath, relativePath, mappingFunction)
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
		/// <param name="settings">The object that specifis the minification settings for this file.</param>
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
