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
namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml;

	using Kelp.Extensions;
	using Sage.Configuration;

	/// <summary>
	/// Represents an XML resource that may be globalized.
	/// </summary>
	public class XmlResource
	{
		private readonly SageContext context;
		private readonly Regex convertScheme = new Regex(@"(\w+)://(.*)");

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlResource"/> class, using the specified resource path and context.
		/// </summary>
		/// <param name="path">The path to the XML resource.</param>
		/// <param name="context">The current <see cref="SageContext"/> under which this class is being created.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="path"/> is <c>null</c> or <paramref name="context"/> is <c>null</c>.
		/// </exception>
		public XmlResource(string path, SageContext context)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));
			Contract.Requires<ArgumentNullException>(context != null);

			this.context = context;

			this.FilePath = path;
			this.Name = new ResourceName(path, context.ProjectConfiguration.Categories[context.Category]);
			this.SourceDirectory = Path.GetDirectoryName(path);

			if (UrlResolver.GetScheme(path) == "file")
			{
				this.TargetDirectory = Path.Combine(this.SourceDirectory, context.ProjectConfiguration.PathTemplates.GlobalizedDirectory);
			}
			else
			{
				string converted = path.ReplaceAll(this.convertScheme, "$1/$2");
				string expanded = Path.Combine(context.ProjectConfiguration.PathTemplates.GlobalizedDirectoryForNonFileResources, converted);

				this.SourceDirectory =
				this.TargetDirectory = context.Path.Resolve(Path.GetDirectoryName(expanded));
			}
		}

		/// <summary>
		/// Gets the <see cref="ResourceName"/> of this resource.
		/// </summary>
		public ResourceName Name { get; private set; }

		/// <summary>
		/// Gets the full file path of this resource.
		/// </summary>
		public string FilePath { get; private set; }

		/// <summary>
		/// Gets the target directory of this resource.
		/// </summary>
		public string TargetDirectory { get; private set; }

		/// <summary>
		/// Gets the source directory of this resource.
		/// </summary>
		public string SourceDirectory { get; private set; }

		/// <summary>
		/// Gets the last modified date of this resource's main file.
		/// </summary>
		public DateTime DateLastModified
		{
			get
			{
				CategoryInfo category = context.ProjectConfiguration.Categories[context.Category];
				string pattern1 = string.Concat(this.Name.FileName, "*", this.Name.Extension);
				Regex pattern2 = new Regex(
					string.Format(
						@"(?:{0}{1})|(?:{0}-(?:{2}){1})",
						this.Name.FileName,
						this.Name.Extension,
					string.Join("|", category.Locales.ToArray())));

				IEnumerable<string> localizedFiles = Directory.GetFiles(SourceDirectory, pattern1, SearchOption.TopDirectoryOnly)
					.Where(n => pattern2.IsMatch(n));

				DateTime maxDate = DateTime.MinValue;
				foreach (string path in localizedFiles)
				{
					DateTime fileDate = File.GetLastWriteTime(path);
					if (fileDate > maxDate)
						maxDate = fileDate;
				}

				return maxDate;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the specified <paramref name="document"/> is globalizable.
		/// </summary>
		/// <param name="document">The document to check.</param>
		/// <returns>
		/// <c>true</c> if the specified document is globalizable; otherwise, <c>false</c>.
		/// </returns>
		/// <remarks>
		/// In order for an xml resources to be considered globalizable, it needs to reference the
		/// <see cref="XmlNamespaces.InternationalizationNamespace"/>.
		/// </remarks>
		public bool IsGlobalizable(XmlDocument document)
		{
			Contract.Requires<ArgumentNullException>(document != null);

			return document.NameTable.Get(XmlNamespaces.InternationalizationNamespace) != null;
		}

		/// <summary>
		/// Gets the earliest modification date of all versions generated from this resource file.
		/// </summary>
		/// <param name="locales">The locales to check for.</param>
		/// <returns>The earliest modification date of all versions generated from this resource file.</returns>
		public DateTime? GetDateFirstGlobalized(List<string> locales)
		{
			if (!Directory.Exists(TargetDirectory))
				return null;

			DateTime minDate = DateTime.MaxValue;
			foreach (string locale in locales)
			{
				string path = this.GetInternationalizedName(locale, true);
				if (!File.Exists(path))
					return null;

				DateTime fileDate = File.GetLastWriteTime(path);
				if (fileDate < minDate)
					minDate = fileDate;
			}

			return minDate;
		}

		/// <summary>
		/// Gets the path to the <paramref name="locale"/>-specific version of this resource if it exists, or the path to the 
		/// non-specific version if it doesn't.
		/// </summary>
		/// <param name="locale">The locale for which to get the path.</param>
		/// <returns>
		/// The path to the <paramref name="locale"/>-specific version of this resource if it exists, or the path to the 
		/// non-specific version if it doesn't.
		/// </returns>
		public string GetSourcePath(string locale)
		{
			string localizedPath = this.Name.ToLocalePath(locale);
			return File.Exists(localizedPath) ? localizedPath : this.Name.NonLocalizedPath;
		}

		/// <summary>
		/// Globalizes this instance.
		/// </summary>
		/// <returns>
		/// An object that contains information about the globalization of this resource.
		/// </returns>
		public InternationalizationSummary Globalize()
		{
			Internationalizer globalizer = new Internationalizer(context);
			InternationalizationSummary summary = globalizer.Internationalize(this);
			summary.Save();

			return summary;
		}

		/// <summary>
		/// Gets the globalized locale name of this resource file.
		/// </summary>
		/// <param name="locale">The locale for which to get the name.</param>
		/// <param name="appendPath">If set to <c>true</c> the resulting value will be the full path to the file.</param>
		/// <returns>The globalized name and optionally, path to this resource file.</returns>
		public string GetInternationalizedName(string locale, bool appendPath)
		{
			string globName = Name.ToLocale(locale);
			return appendPath ? Path.Combine(this.TargetDirectory, globName) : globName;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Name.Signature;
		}

		/// <summary>
		/// Loads this resource (optionally) localized into the specified <paramref name="locale"/>.
		/// </summary>
		/// <param name="locale">The locale into which to localize this resource.</param>
		/// <returns>
		/// The localized XML document that this xml resource represents.
		/// </returns>
		public CacheableXmlDocument Load(string locale)
		{
			if (string.IsNullOrEmpty(locale))
				throw new ArgumentNullException("locale");

			if (!context.ProjectConfiguration.Locales.ContainsKey(locale))
				throw new ArgumentException(string.Format("The specified value '{0}' is not a valid locale", locale), "locale");

			if (!context.ProjectConfiguration.Categories[context.Category].Locales.Contains(locale))
				throw new ArgumentException(string.Format("Category '{0}' hasn't been configured for locale '{1}'", context.Category, locale), "locale");

			CacheableXmlDocument sourceDoc = this.LoadSourceDocument(locale);
			if (!this.IsGlobalizable(sourceDoc))
				return sourceDoc;

			var dependencies = new List<string> { this.GetInternationalizedName(locale, true) };

			if (context.ProjectConfiguration.AutoInternationalize)
			{
				DictionaryFileCollection dictionaries = Internationalizer.GetTranslationDictionaryCollection(context);
				DateTime? resourceFirstGlobalized = this.GetDateFirstGlobalized(dictionaries.Locales);

				DictionaryFile dictionary = dictionaries.Dictionaries[locale];
				dependencies.Add(this.GetSourcePath(locale));
				dependencies.AddRange(dictionary.Dependencies);

				bool generateNew =
					context.ForceRefresh ||
					(resourceFirstGlobalized == null) ||
					(this.DateLastModified > resourceFirstGlobalized) ||
					(dictionaries.DateLastModified > resourceFirstGlobalized);

				if (dictionaries.DateLastModified > resourceFirstGlobalized)
					dictionaries.Refresh();

				if (generateNew)
				{
					InternationalizationSummary summary = this.Globalize();
					dependencies.AddRange(summary.GetConstituentFiles(locale));
				}
			}

			CacheableXmlDocument result = this.LoadGlobalizedDocument(locale);
			result.AddDependencies(dependencies);

			return result;
		}

		internal CacheableXmlDocument LoadSourceDocument(string locale)
		{
			string fullPath = context.Path.Localize(this.FilePath, locale, true);
			if (new Uri(fullPath).Scheme == "file" && !File.Exists(fullPath))
			{
				throw new FileNotFoundException(string.Format("The resource file '{0}' could not be opened using locale '{1}'",
					this.FilePath, locale));
			}

			CacheableXmlDocument document = new CacheableXmlDocument();
			document.Load(fullPath, this.context);
			return document;
		}

		internal CacheableXmlDocument LoadLocalizedSourceDocument(string locale)
		{
			string fullPath = this.GetSourcePath(locale);
			CacheableXmlDocument document = new CacheableXmlDocument();
			document.Load(fullPath, this.context);
			return document;
		}

		internal CacheableXmlDocument LoadGlobalizedDocument(string locale)
		{
			string fullPath = this.GetInternationalizedName(locale, true);
			CacheableXmlDocument document = new CacheableXmlDocument();
			document.Load(fullPath, this.context);
			return document;
		}
	}
}
