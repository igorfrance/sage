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
	using System.Diagnostics.Contracts;
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
		/// Initializes a new instance of the <see cref="ScriptFile" /> class, using the specified absolute and 
		/// relative paths, and the specified <paramref name="configuration"/>.
		/// </summary>
		/// <param name="absolutePath">The path of the file to load.</param>
		/// <param name="relativePath">The relative path of the file to load.</param>
		/// <param name="configuration">The processing configuration for this file.</param>
		public ScriptFile(string absolutePath, string relativePath, FileTypeConfiguration configuration)
			: base(absolutePath, relativePath, configuration)
		{
			this.ContentType = "text/javascript";
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ScriptFile" /> class, using the specified absolute and relative paths.
		/// </summary>
		/// <param name="absolutePath">The path of the file to load.</param>
		/// <param name="relativePath">The relative path of the file to load.</param>
		public ScriptFile(string absolutePath, string relativePath)
			: this(absolutePath, relativePath, ResourceHandling.Configuration.Current.Script)
		{
		}

		/// <inheritdoc/>
		public override string Minify(string sourceCode)
		{
			Minifier min = new Minifier();
			ScriptFileConfiguration scriptConfiguration = (ScriptFileConfiguration) this.Configuration;
			string minified = min.MinifyJavaScript(sourceCode, scriptConfiguration.Settings);

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
