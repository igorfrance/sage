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
namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml;

	using Kelp.Core.Extensions;
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
				this.TargetDirectory = Path.Combine(SourceDirectory, context.ProjectConfiguration.PathTemplates.GlobalizedDirectory);
			}
			else
			{
				string converted = path.ReplaceAll(convertScheme, "$1/$2");
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
		/// The directory in which globalized files will be saved.
		/// </summary>
		public string TargetDirectory { get; private set; }

		/// <summary>
		/// Gets or sets the source directory.
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
		/// Gets a value indicating whether this instance is globalizable.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is globalizable; otherwise, <c>false</c>.
		/// </value>
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
		/// Gets the earliest modified date of all versions generated from this resource file.
		/// </summary>
		public DateTime? GetDateFirstGlobalized(List<string> locales)
		{
			if (!Directory.Exists(TargetDirectory))
				return null;

			DateTime minDate = DateTime.MaxValue;
			foreach (string locale in locales)
			{
				string path = GetGlobalizedName(locale, true);
				if (!File.Exists(path))
					return null;

				DateTime fileDate = File.GetLastWriteTime(path);
				if (fileDate < minDate)
					minDate = fileDate;
			}

			return minDate;
		}

		/// <summary>
		/// Gets the source path.
		/// </summary>
		/// <param name="locale">The locale.</param>
		/// <returns>TODO: Add documentation for GetSourcePath.</returns>
		public string GetSourcePath(string locale)
		{
			string localizedPath = this.Name.ToLocalePath(locale);
			return File.Exists(localizedPath) ? localizedPath : this.Name.NonLocalizedPath;
		}

		/// <summary>
		/// Globalizes this instance.
		/// </summary>
		/// <returns>TODO: Add documentation for Globalize.</returns>
		public GlobalizationSummary Globalize()
		{
			Globalizer globalizer = new Globalizer(context);
			GlobalizationSummary summary = globalizer.Globalize(this);
			summary.Save();

			return summary;
		}

		/// <summary>
		/// Gets the globalized locale name of this resource file.
		/// </summary>
		/// <param name="locale">The locale for which to get the name.</param>
		/// <param name="appendPath">If set to <c>true</c> the resulting value will be the full path to the file.</param>
		/// <returns>The globalized name and optionally, path to this resource file.</returns>
		public string GetGlobalizedName(string locale, bool appendPath)
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
		/// <remarks>
		/// <para>If this xml resource is not globalizable (see <see cref="IsGlobalizable"/>) it will be returned as is, without
		/// localization. If however it <b>is</b> globalizable, several other things are taken into account before deciding to globalize
		/// it:</para>
		/// <list type="table">
		/// <listheader>
		/// <term>Consider</term>
		/// <description>Description</description>
		/// </listheader>
		/// <item>
		/// <term><see cref="ProjectConfiguration.AreResourcesPreGenerated"/>?</term>
		/// <description>If the current <see cref="ProjectConfiguration"/> indicates that the xml resources are pre-generated,
		/// no work will be done on the resource and the resource will be returned as it. In the production environment, xml
		/// resources will typically be pre-generated (by running a globalization utility as a part of the build process) and
		/// hence live, on-the-fly globalization will not be needed.</description>
		/// </item>
		/// <item>
		/// <term><see cref="SageContext.ForceRefresh"/>?</term>
		/// <description>If the current <see cref="SageContext"/> indicates that the current request wants an unconditional
		/// refresh of the resource, the resource will be globalized regardless of other factors.</description>
		/// </item>
		/// <item>
		/// <term>Has the resource ever been globalized?</term>
		/// <description>If this xml resource's has not yet been globalized, it will now be globalized.</description>
		/// </item>
		/// <item>
		/// <term>Has the resource changed?</term>
		/// <description>If this xml resource's last modification date is later than it's translated version's, it will be
		/// globalized again.</description>
		/// </item>
		/// <item>
		/// <term>Has one of the dictionaries changed?</term>
		/// <description>If any one of the translation dictionaries for the current category has been changed after this resource
		/// has last been globalized, it will be globalized again.</description>
		/// </item>
		/// </list>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="locale"/> is <c>null</c> or empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// If the specified <paramref name="locale"/> is not a valid project locale (doesn't exist within current configuration's
		/// list of <see cref="ProjectConfiguration.Locales"/>, or if the specified <paramref name="locale"/> is not a valid
		/// locale for the current category (doesn't exist within current category's list of <see cref="CategoryInfo.Locales"/>).
		/// </exception>
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

			var dependencies = new List<string> { GetGlobalizedName(locale, true) };

			if (!context.ProjectConfiguration.AreResourcesPreGenerated)
			{
				DictionaryFileCollection dictionaries = Globalizer.GetTranslationDictionaryCollection(context);
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
					GlobalizationSummary summary = this.Globalize();
					dependencies.AddRange(summary.GetConstituentFiles(locale));
				}
			}

			CacheableXmlDocument result = this.LoadGlobalizedDocument(locale);
			result.AddDependencies(dependencies);

			return result;
		}

		/// <summary>
		/// Loads the original <see cref="XmlDocument"/> of this resource file.
		/// </summary>
		/// <returns>The loaded document.</returns>
		/// <remarks>
		/// When loading the source document, we need to consider that there may be a completely separate document
		/// for a given locale, and in that case we need to use that one. This method will take that into account
		/// and use the source document most closely matching the specified <paramref name="locale"/>.
		/// </remarks>
		internal CacheableXmlDocument LoadSourceDocument(string locale)
		{
			string fullPath = context.Path.Localize(this.FilePath, locale, true);
			if (new Uri(fullPath).Scheme == "file" && !File.Exists(fullPath))
			{
				throw new FileNotFoundException(string.Format("The resource file '{0}' could not be opened using locale '{1}'",
					this.FilePath, locale));
			}

			CacheableXmlDocument xmlDocument = this.LoadXmlDocument(fullPath);
			return xmlDocument;
		}

		internal CacheableXmlDocument LoadLocalizedSourceDocument(string locale)
		{
			string fullPath = GetSourcePath(locale);
			CacheableXmlDocument xmlDocument = this.LoadXmlDocument(fullPath);
			return xmlDocument;
		}

		/// <summary>
		/// Loads the <see cref="XmlDocument"/> of this resource file as globalized in the specified <paramref name="locale"/>
		/// </summary>
		/// <returns>The globalized <see cref="XmlDocument"/> of this resource file.</returns>
		internal CacheableXmlDocument LoadGlobalizedDocument(string locale)
		{
			string fullPath = GetGlobalizedName(locale, true);
			CacheableXmlDocument document = this.LoadXmlDocument(fullPath);
			return document;
		}

		private CacheableXmlDocument LoadXmlDocument(string path)
		{
			CacheableXmlDocument result = new CacheableXmlDocument();
			UrlResolver resolver = new UrlResolver(context);

			result.Load(path, resolver);
			result.AddDependencies(path);
			result.AddDependencies(resolver.Dependencies.Where(d => !result.Dependencies.Contains(d)));

			return result;
		}
	}
}
