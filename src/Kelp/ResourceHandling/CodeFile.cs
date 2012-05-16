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
	using System.Diagnostics;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Security.Cryptography;
	using System.Text;
	using System.Text.RegularExpressions;

	using Kelp.Extensions;
	using Kelp.Http;

	using log4net;

	/// <summary>
	/// Represents a code file that may include other code files.
	/// </summary>
	public abstract class CodeFile
	{
		/// <summary>
		/// A reference to the including (parent) <see cref="CodeFile"/> class, if this file 
		/// was included from another.
		/// </summary>
		protected CodeFile parent;
		
		/// <summary>
		/// The file's fully processed source code.
		/// </summary>
		protected string content = string.Empty;

		/// <summary>
		/// The file's raw source code.
		/// </summary>
		protected string rawContent = string.Empty;

		private static readonly ILog log = LogManager.GetLogger(typeof(CodeFile).FullName);
		private static readonly Regex instructionExpression = new Regex(@"^/\*# (?'name'\w+):\s*(?'value'.*?) \*/");
		private readonly OrderedDictionary<string, CodeFile> includes = new OrderedDictionary<string, CodeFile>();
		private readonly OrderedDictionary<string, string> references = new OrderedDictionary<string, string>();
		private readonly Stopwatch sw = new Stopwatch();
		private readonly string relativePath;
		private string absolutePath;
		private bool initialized;
		private bool loaded;
		private bool isFromCache;

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeFile"/> class.
		/// </summary>
		/// <param name="absolutePath">The physical path of this code file.</param>
		/// <param name="relativePath">The relative path of this code file.</param>
		protected CodeFile(string absolutePath, string relativePath)
		{
			this.AbsolutePath = absolutePath;
			this.relativePath = relativePath;
			this.parent = null;
			this.MapPath = input => input;
			this.CachedConfigurationSettings = string.Empty;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeFile"/> class.
		/// </summary>
		/// <param name="absolutePath">The physical path of this code file.</param>
		/// <param name="relativePath">The relative path of this code file.</param>
		/// <param name="mappingFunction">The delegate method that handles resolving (mapping) or relative paths.</param>
		protected CodeFile(string absolutePath, string relativePath, Func<string, string> mappingFunction)
			: this(absolutePath, relativePath)
		{
			this.MapPath = mappingFunction;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeFile"/> class.
		/// </summary>
		/// <param name="absolutePath">The physical path of this code file.</param>
		/// <param name="relativePath">The relative path of this code file.</param>
		/// <param name="parent">The script processor creating this instance (used when processing includes)</param>
		protected CodeFile(string absolutePath, string relativePath, CodeFile parent)
			: this(absolutePath, relativePath)
		{
			this.parent = parent;
			this.MapPath = parent.MapPath;
		}

		/// <summary>
		/// Gets the absolute path of this code file.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the supplied value is <c>null</c> or empty.</exception>
		/// <exception cref="FileNotFoundException">If the specified value could not be resolved to an existing file.</exception>
		public string AbsolutePath
		{
			get
			{
				return absolutePath;
			}

			private set
			{
				if (string.IsNullOrEmpty(value))
					throw new ArgumentNullException("value");

				if (!File.Exists(value))
				{
					log.FatalFormat("The file '{0}' doesn't exist", value);
					throw new FileNotFoundException();
				}

				absolutePath = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance has been loaded and initialized entirely from cache.
		/// </summary>
		public bool IsFromCache
		{
			get
			{
				if (!this.initialized)
					this.Initialize();

				return this.isFromCache;
			}
		}

		/// <summary>
		/// Gets the relative path of this code file.
		/// </summary>
		public string RelativePath
		{
			get
			{
				string resultPath = this.relativePath.Replace("~/", string.Empty);
				if (parent != null)
				{
					resultPath = Path.Combine(Path.GetDirectoryName(parent.RelativePath), this.relativePath);
				}

				return resultPath.Replace("\\", "/");
			}
		}

		/// <summary>
		/// Gets the temporary directory in which to store the processed version of this file.
		/// </summary>
		public string CacheDirectory
		{
			get
			{
				return Configuration.Current.TemporaryDirectory;
			}
		}

		/// <summary>
		/// Gets or sets the delegate method that handles resolving (mapping) or relative paths.
		/// </summary>
		public Func<string, string> MapPath { get; set; }

		/// <summary>
		/// Gets the recursive list of files this file includes either directly or through it's includes.
		/// </summary>
		public OrderedDictionary<string, CodeFile> Includes
		{
			get
			{
				this.Initialize();
				return includes;
			}
		}

		/// <summary>
		/// Gets the list of files that this <see cref="CodeFile"/> is depending on
		/// </summary>
		public List<string> Dependencies
		{
			get
			{
				this.Initialize();

				List<string> dependencies = this.references.Keys.ToList();
				if (!dependencies.Contains(this.AbsolutePath))
					dependencies.Add(this.AbsolutePath);

				return dependencies;
			}
		}

		/// <summary>
		/// Gets the last modified Date and Time of this <see cref="CodeFile"/>
		/// </summary>
		/// <remarks>
		/// This is done by getting the latest last modified datetime from all depend files.
		/// </remarks> 
		public DateTime LastModified
		{
			get
			{
				this.Initialize();
				if (File.Exists(this.CacheName))
					return Util.GetDateLastModified(this.CacheName);

				return Util.GetDateLastModified(this.Dependencies);
			}
		}

		/// <summary>
		/// Gets the fully processed content of this file.
		/// </summary>
		public string Content
		{
			get
			{
				this.Initialize();
				return content;
			}
		}

		/// <summary>
		/// Gets the raw, unprocessed content of this file.
		/// </summary>
		public string RawContent
		{
			get
			{
				this.Load();
				return rawContent;
			}
		}

		/// <summary>
		/// Gets the previously persisted list of <see cref="Includes"/>.
		/// </summary>
		/// <remarks>
		/// This list is used to determine if a previously processed file needs to be refreshed due to changes in
		/// its constituent files.
		/// </remarks>
		public OrderedDictionary<string, string> References
		{
			get
			{
				this.Initialize();
				return references;
			}
		}

		/// <summary>
		/// Gets an E-tag for the file represented with this instance.
		/// </summary>
		public string ETag
		{
			get
			{
				return GetETag(this.RelativePath, this.LastModified);
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to output additional debug information to the output stream.
		/// </summary>
		internal bool DebugModeOn { get; set; }

		/// <summary>
		/// Gets the full name of the temporary file where the processed version of this file will be saved.
		/// </summary>
		/// <remarks>
		/// This is an absolute path.
		/// </remarks>
		internal string CacheName
		{
			get
			{
				if (this.CacheDirectory == null)
					return null;

				string fileName = AbsolutePath.Replace('/', '_').Replace('\\', '_').Replace(':', '_');
				return MapPath(Path.Combine(CacheDirectory, fileName));
			}
		}

		/// <summary>
		/// Gets or sets the content-type of this code file.
		/// </summary>
		internal string ContentType { get; set; }

		internal abstract string ConfigurationSettings { get; }

		internal string CachedConfigurationSettings { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the cached <see cref="CodeFile"/> needs to be refreshed.
		/// </summary>
		/// <value><c>true</c> if the cached file is out of date; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// The cached file is considered out-of-date when any of the included files has a last-modified datetime greater than the cached file.
		/// </remarks>
		protected virtual bool NeedsRefresh
		{
			get
			{
				if (this.CachedConfigurationSettings != this.ConfigurationSettings)
					return true;

				if (!File.Exists(this.CacheName))
					return true;

				DateTime lastModified = Util.GetDateLastModified(this.CacheName);
				foreach (string file in this.references.Keys)
				{
					if (!File.Exists(file))
					{
						log.DebugFormat("Refresh needed for '{0}' because referenced file '{1}' doesn't exist.", AbsolutePath, file);
						return true;
					}

					var fileModified = Util.GetDateLastModified(file);
					if (fileModified > lastModified)
					{
						log.DebugFormat("Refresh needed for '{0}' (modified: {2}) because referenced file '{1}' (modified: {3}) is newer.", AbsolutePath, file, lastModified, fileModified);
						return true;
					}
				}

				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether minification is enabled for this code file.
		/// </summary>
		protected virtual bool MinificationEnabled
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Creates a <see cref="CodeFile"/> for the specified <paramref name="absolutePath"/> and <paramref name="relativePath"/>
		/// </summary>
		/// <param name="absolutePath">The physical path of this code file.</param>
		/// <param name="relativePath">The relative path of this code file.</param>
		/// <returns>A code file appropriate for the specified <paramref name="absolutePath"/> and <paramref name="relativePath"/>
		/// If the extension of the specified <paramref name="absolutePath"/> is <c>css</c>, the resulting value will be a 
		/// new <see cref="CssFile"/>. In all other cases the resulting value is a <see cref="ScriptFile"/>.</returns>
		public static CodeFile Create(string absolutePath, string relativePath)
		{
			return Create(absolutePath, relativePath, (CodeFile) null);
		}

		/// <summary>
		/// Creates a code file for the specified <paramref name="absolutePath"/>.
		/// </summary>
		/// <param name="absolutePath">The physical path of this code file.</param>
		/// <param name="relativePath">The relative path of this code file.</param>
		/// <param name="mapPath">The function to use to map relative paths to absolute.</param>
		/// <returns>A code file appropriate for the specified <paramref name="absolutePath"/>.
		/// If the extension of the specified <paramref name="absolutePath"/> is <c>css</c>, the resulting value will be a 
		/// new <see cref="CssFile"/>. In all other cases the resulting value is a <see cref="ScriptFile"/>.</returns>
		public static CodeFile Create(string absolutePath, string relativePath, Func<string, string> mapPath)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(absolutePath));
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(relativePath));
			Contract.Requires<ArgumentNullException>(mapPath != null);

			CodeFile result = Create(absolutePath, relativePath);
			result.MapPath = mapPath;

			return result;
		}

		/// <summary>
		/// Creates a code file for the specified <paramref name="absolutePath"/>.
		/// </summary>
		/// <param name="absolutePath">The physical path of this code file.</param>
		/// <param name="relativePath">The relative path of this code file.</param>
		/// <param name="parent">The parent <see cref="CodeFile"/> that is including this <see cref="CodeFile"/></param>
		/// <returns>A code file appropriate for the specified <paramref name="absolutePath"/>.
		/// If the extension of the specified <paramref name="absolutePath"/> is <c>css</c>, the resulting value will be a 
		/// new <see cref="CssFile"/>. In all other cases the resulting value is a <see cref="ScriptFile"/>.</returns>
		public static CodeFile Create(string absolutePath, string relativePath, CodeFile parent)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(absolutePath));
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(relativePath));

			CodeFile instance;
			if (absolutePath.ToLower().EndsWith("css"))
				instance = new CssFile(absolutePath, relativePath);
			else
				instance = new ScriptFile(absolutePath, relativePath);

			instance.parent = parent;
			if (parent != null)
				instance.MapPath = parent.MapPath;

			return instance;
		}

		/// <summary>
		/// Gets an E-tag for the specified <paramref name="fileName"/> and <paramref name="lastModified"/> date.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="lastModified">The last modified date of the file.</param>
		/// <returns>The E-Tag that matches the specified <paramref name="fileName"/> and <paramref name="lastModified"/> date.</returns>
		public static string GetETag(string fileName, DateTime lastModified)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(fileName));

			Encoder stringEncoder = Encoding.UTF8.GetEncoder();
			MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

			string fileString = fileName + lastModified +
				Assembly.GetExecutingAssembly().GetName().Version;

			// get string bytes
			byte[] bytes = new byte[stringEncoder.GetByteCount(fileString.ToCharArray(), 0, fileString.Length, true)];
			stringEncoder.GetBytes(fileString.ToCharArray(), 0, fileString.Length, bytes, 0, true);

			return BitConverter.ToString(md5.ComputeHash(bytes)).Replace("-", string.Empty).ToLower();
		}

		/// <summary>
		/// Minifies the specified <paramref name="sourceCode"/>.
		/// </summary>
		/// <param name="sourceCode">The source code string to minify.</param>
		/// <returns>
		/// The minified version of this file's content.
		/// </returns>
		public abstract string Minify(string sourceCode);

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.RelativePath;
		}

		internal static bool IsFileExtensionSupported(string extension)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(extension));
			return extension.Replace(".", string.Empty).ToLower().ContainsAnyOf("js", "css");
		}

		/// <summary>
		/// Returns the expanded version of the specified path.
		/// </summary>
		/// <param name="path">The path to expand.</param>
		/// <returns>The expanded version of the specified path</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="path"/> is <c>null</c>.</exception>
		internal string ExpandPath(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			if (Path.IsPathRooted(path) || path.StartsWith("~"))
			{
				if ((path.StartsWith("/") || path.StartsWith("~")) && MapPath != null)
					return MapPath(path);

				return path;
			}

			return Path.Combine(Path.GetDirectoryName(AbsolutePath), path);
		}

		/// <summary>
		/// Determines whether the specified path exists in the list of files already included.
		/// </summary>
		/// <param name="path">The path to check.</param>
		/// <returns>
		/// <c>true</c> if the specified path exists in the list of files already included; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">If the specified <c>path</c> is <c>null</c>.</exception>
		internal bool IsPathInIncludeChain(string path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			if (parent == null)
				return false;

			bool contains = false;
			CodeFile ancestor = this;
			while (ancestor != null)
			{
				contains = ancestor.includes.Keys.Any(value => 
					value.Equals(path, StringComparison.InvariantCultureIgnoreCase));

				ancestor = ancestor.parent;
			}

			return contains;
		}

		/// <summary>
		/// Initialized this <see cref="CodeFile"/>.
		/// </summary>
		protected void Initialize()
		{
			if (this.initialized)
				return;

			this.Load();

			if (File.Exists(this.CacheName))
			{
				log.DebugFormat("Parsing cached file for '{0}'", this.AbsolutePath);
				this.Parse(File.ReadAllText(this.CacheName), false);
			}

			if (!this.NeedsRefresh)
			{
				log.DebugFormat("No refresh needed for '{0}', returning.", this.AbsolutePath);
				this.initialized = true;
				this.isFromCache = true;
				return;
			}

			this.references.Clear();

			log.DebugFormat("Parsing raw content for '{0}'.", this.AbsolutePath);

			this.Parse(this.rawContent, true);

			if (!string.IsNullOrEmpty(this.CacheName))
			{
				string tempName = this.CacheName;
				string tempDirectory = this.MapPath(this.CacheDirectory);
				if (!Directory.Exists(tempDirectory))
				{
					try
					{
						Directory.CreateDirectory(tempDirectory);
					}
					catch (Exception ex)
					{
						log.ErrorFormat("Could not create temporary directory '{0}': {1}", tempDirectory, ex.Message);
					}
				}

				try
				{
					StringBuilder persistContent = new StringBuilder();
					persistContent.AppendLine(string.Format("/*# Configuration: {0} */", this.ConfigurationSettings));
					foreach (string includePath in this.references.Keys)
						persistContent.AppendLine(string.Format("/*# Reference: {0} | {1} */", includePath, this.references[includePath]));

					persistContent.AppendLine();
					persistContent.AppendLine(this.content);

					File.WriteAllText(this.CacheName, persistContent.ToString(), Encoding.UTF8);
					log.DebugFormat("Saved the temporary contents of '{0}' to '{1}'.", AbsolutePath, tempName);
				}
				catch (Exception ex)
				{
					log.ErrorFormat("Could not save the temporary file '{0}': {1}", tempName, ex.Message);
				}
			}

			this.initialized = true;
		}

		/// <summary>
		/// Loads the file, either from cache or from its <see cref="AbsolutePath"/>.
		/// </summary>
		protected virtual void Load()
		{
			if (this.loaded)
				return;

			if (!File.Exists(this.AbsolutePath))
				return;


			this.rawContent = File.ReadAllText(this.AbsolutePath);
			this.loaded = true;
		}

		/// <summary>
		/// Scans through the specified source code and processes it line by line.
		/// </summary>
		/// <param name="sourceCode">The source code to parse.</param>
		/// <param name="minify">If set to <c>true</c>, and the current settings indicate that the source code will be minified.</param>
		protected virtual void Parse(string sourceCode, bool minify)
		{
			Contract.Requires<ArgumentNullException>(sourceCode != null);

			StringBuilder contents = new StringBuilder();
			string[] lines = sourceCode.Replace("\r", string.Empty).Split(new[] { '\n' });

			Match match;
			foreach (string line in lines)
			{
				if ((match = instructionExpression.Match(line)).Success)
				{
					string name = match.Groups["name"].Value;
					string value = match.Groups["value"].Value;

					switch (name.ToLower())
					{
						case "reference":
							string[] parts = value.Split('|');
							if (parts.Length == 2)
								this.AddReference(parts[0].Trim(), parts[1].Trim());

							break;

						case "configuration":
							this.CachedConfigurationSettings = value;
							break;

						case "include":
							var includePath = ExpandPath(Regex.Replace(value, @"\?.*$", string.Empty));
							if (File.Exists(includePath))
							{
								if (includePath.Equals(this.AbsolutePath, StringComparison.InvariantCultureIgnoreCase))
								{
									log.FatalFormat("The script cannot include itself. The script is: {0}({1})", value, includePath);
									throw new InvalidOperationException("The script cannot include itself.");
								}

								if (IsPathInIncludeChain(includePath))
								{
									log.FatalFormat("Including the referenced path would cause recursion due to it being present at a level higher above. The script is: {0}({1})", value, includePath);
									throw new InvalidOperationException("Including the referenced path would cause recursion due to it being present at a level higher above.");
								}

								var inner = CodeFile.Create(includePath, value, this);
								contents.AppendLine(inner.Content);

								foreach (var key in inner.includes.Keys)
								{
									if (inner.includes[key] == null)
										continue;

									this.AddInclude(key, inner.includes[key]);
								}

								this.AddInclude(includePath, inner);

								foreach (var absPath in inner.References.Keys)
								{
									this.AddReference(absPath, inner.References[absPath]);
								}

							}
							else
							{
								log.ErrorFormat("The include file specified with '{0}' and resolved to '{1}' doesn't exist", value, includePath);
								contents.AppendLine(line + "/* File not found */");
								this.AddInclude(includePath, null);
							}

							break;

						default:
							log.WarnFormat("Unrecognized processing instruction '{0}' encountered.", name);
							break;
					}
				}
				else
					contents.AppendLine(line);
			}

			this.content = contents.ToString();
			this.AddReference(this.AbsolutePath, this.RelativePath);
			if (this.MinificationEnabled && minify)
			{
				log.DebugFormat("Minification of '{0}' took {1}ms", this.AbsolutePath, 
					sw.TimeMilliseconds(() => this.content = this.Minify(this.content)));
			}
		}

		private void AddInclude(string absolutePath, CodeFile include)
		{
			string includeKey = absolutePath.ToLower().Replace("/", "\\").Replace("\\\\", "\\");
			if (this.includes.ContainsKey(includeKey))
				return;

			this.includes.Add(includeKey, include);
		}

		private void AddReference(string absolutePath, string relativePath)
		{
			string referenceKey = absolutePath.ToLower().Replace("/", "\\").Replace("\\\\", "\\");
			if (this.references.ContainsKey(referenceKey))
				return;

			this.references.Add(referenceKey, relativePath);
		}
	}
}
