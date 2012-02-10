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
	using System.Text;

	using log4net;
	using Microsoft.Ajax.Utilities;

	/// <summary>
	/// Implements a JS file merger/processor, optionally minifying and obfuscating them.
	/// </summary>
	public class ScriptFile : CodeFile
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ScriptFile));

		/// <summary>
		/// Initializes a new instance of the <see cref="ScriptFile"/> class, using the specified absolute and relative paths.
		/// </summary>
		/// <param name="absolutePath">The path of the file to load.</param>
		/// <param name="relativePath">The relative path of the file to load.</param>
		public ScriptFile(string absolutePath, string relativePath)
			: this(absolutePath, relativePath, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ScriptFile"/> class, using the specified absolute and relative paths, and the path mapper function.
		/// </summary>
		/// <param name="absolutePath">The path of the file to load.</param>
		/// <param name="relativePath">The relative path of the file to load.</param>
		/// <param name="mappingFunction">The function to use to map relative and virtual paths to absolute.</param>
		public ScriptFile(string absolutePath, string relativePath, Func<string, string> mappingFunction)
			: base(absolutePath, relativePath, mappingFunction)
		{
			this.ContentType = "text/javascript";
		}


		internal override string ConfigurationSettings
		{
			get 
			{
				return Configuration.Current.Script.ToString(); 
			}
		}

		/// <inheritdoc/>
		protected override bool MinificationEnabled
		{
			get
			{
				return Configuration.Current.Script.Enabled;
			}
		}

		/// <inheritdoc/>
		public override string Minify(string sourceCode)
		{
			return Minify(sourceCode, Configuration.Current.Script.Settings);
		}

		/// <summary>
		/// Minifies the specified <paramref name="sourceCode"/>, according to the specified minification <paramref name="settings"/>.
		/// </summary>
		/// <param name="sourceCode">The source code string to minify.</param>
		/// <param name="settings">The object that specifis the minification settings for this file.</param>
		/// <returns>
		/// The minified version of this file's content.
		/// </returns>
		public string Minify(string sourceCode, CodeSettings settings)
		{
			Minifier min = new Minifier();
			string minified = min.MinifyJavaScript(sourceCode, settings);

			if (min.ErrorList.Count == 0)
			{
				if (DebugModeOn)
					log.Debug("Minified code: " + minified);

				return minified;
			}

			// error handling:
			StringBuilder messages = new StringBuilder();
			foreach (var msg in min.ErrorList)
			{
				messages.AppendFormat("Line {0}, Col {1}: {2}\n", msg.StartLine, 
					msg.StartColumn, msg.Message);
			}

			log.ErrorFormat("Minifying javascript file '{0}' resulted in errors:\n{1}", this.AbsolutePath, messages);
			return sourceCode;
		}
	}
}
