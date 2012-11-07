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
	using System.Diagnostics;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Text;
	using System.Xml;

	using Kelp.Extensions;

	using log4net;

	using Sage.Configuration;
	using Sage.Views;

	/// <summary>
	/// Provides facilities for transforming XML resources into separate, localized versions.
	/// </summary>
	public class Internationalizer
	{
		private const string XsltPath = "sageresx://sage/resources/xslt/internationalization.xslt";
		//// private const string XsltPath = @"G:\cycle99\projects\web\sage\src\Sage\Resources\Xslt\Internationalization.xslt";

		private static readonly ILog log = LogManager.GetLogger(typeof(Internationalizer).FullName);

		private static readonly Dictionary<string, DictionaryFileCollection> dictionaries =
			new Dictionary<string, DictionaryFileCollection>();

		private readonly SageContext context;
		private readonly XsltTransform processor;

		/// <summary>
		/// Initializes a new instance of the <see cref="Internationalizer"/> class, using the specified
		/// <paramref name="context"/>.
		/// </summary>
		/// <param name="context">The context under which the code is being executed.</param>
		public Internationalizer(SageContext context)
		{
			this.processor = XsltTransform.Create(context, Internationalizer.XsltPath);
			this.context = new SageContext(context);
		}

		/// <summary>
		/// Internationalizes the specified resource.
		/// </summary>
		/// <param name="resource">The resource.</param>
		/// <returns>
		/// The summary information about the internationalization of the specified <paramref name="resource"/>.
		/// </returns>
		/// <exception cref="ApplicationException">
		/// One of the locales configured for the category being processed doesn't have any dictionary files.
		/// </exception>
		public InternationalizationSummary Internationalize(XmlResource resource)
		{
			if (resource == null)
				throw new ArgumentNullException("resource");

			if (!Directory.Exists(resource.TargetDirectory))
			{
				Directory.CreateDirectory(resource.TargetDirectory);
				File.SetAttributes(resource.TargetDirectory, FileAttributes.Hidden);
			}

			DictionaryFileCollection coll = GetTranslationDictionaryCollection(context);
			CategoryInfo categoryInfo = context.ProjectConfiguration.Categories[context.Category];
			InternationalizationSummary summary = new InternationalizationSummary(resource, categoryInfo);

			Stopwatch sw = new Stopwatch();
			sw.Start();

			foreach (string locale in coll.Locales)
			{
				if (coll.Dictionaries[locale].Document == null)
					throw new InternationalizationError(string.Format(
						"The locale '{0}' in category '{1}' doesn't have any dictionary files", locale, coll.Category));

				CacheableXmlDocument input;
				try
				{
					input = resource.LoadLocalizedSourceDocument(locale);
				}
				catch (FileNotFoundException ex)
				{
					log.ErrorFormat("Could not internationalize resource '{0}' to locale '{1}': {2}", resource.Name.Signature, locale, ex.Message);
					continue;
				}

				XmlWriterSettings settings = new XmlWriterSettings { Indent = true };

				string outputPath = resource.GetInternationalizedName(locale, true);
				XmlWriter translateWriter = XmlWriter.Create(outputPath, settings);

				StringBuilder builder = new StringBuilder();
				XmlWriter diagnoseWriter = XmlWriter.Create(builder, settings);

				try
				{
					this.Transform(input, coll, locale, translateWriter, InternationalizeType.Translate);
					this.Transform(input, coll, locale, diagnoseWriter, InternationalizeType.Diagnose);
				}
				finally
				{
					translateWriter.Close();
					diagnoseWriter.Close();
				}

				XmlDocument diagnostics = new XmlDocument();
				diagnostics.LoadXml(builder.ToString());

				summary.AddPhraseSummary(locale, diagnostics);
				summary.AddDependencies(locale, input.Dependencies);
			}

			sw.Stop();

			summary.Duration = sw.ElapsedMilliseconds;

			log.InfoFormat("Internationalized xml resource '{0}' into {1} locales in {2}ms",
				resource.Name.Signature, coll.Locales.Count, sw.ElapsedMilliseconds);

			return summary;
		}

		/// <summary>
		/// Transforms the specified input.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <param name="collection">The collection.</param>
		/// <param name="locale">The locale.</param>
		/// <param name="output">The output.</param>
		/// <param name="type">The type.</param>
		public void Transform(XmlDocument input, DictionaryFileCollection collection, string locale, XmlWriter output, InternationalizeType type)
		{
			Contract.Requires<ArgumentNullException>(input != null);
			Contract.Requires<ArgumentNullException>(collection != null);
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(locale));
			Contract.Requires<ArgumentNullException>(output != null);

			LocaleInfo localeInfo = context.ProjectConfiguration.Locales[locale];
			XmlElement fallbacks = input.CreateElement("fallbacks");
			foreach (string name in localeInfo.ResourceNames)
				fallbacks.AppendElement(input.CreateElement("locale")).InnerText = name;

			DictionaryFile dictionary = collection.Dictionaries[locale];

			Dictionary<string, object> arguments = new Dictionary<string, object>();
			arguments.Add("dictionary", dictionary.Document.DocumentElement);
			arguments.Add("locale", locale);
			arguments.Add("fallbacks", fallbacks);
			arguments.Add("mode", type.ToString().ToLower());

			var projectConfig = this.context.ProjectConfiguration;
			if (projectConfig.Variables != null)
				arguments.Add("globalvariables", projectConfig.Variables);

			var categoryConfig = this.context.CategoryConfiguration;
			if (categoryConfig != null && categoryConfig.Variables != null)
				arguments.Add("categoryvariables", categoryConfig.Variables);

			try
			{
				processor.Transform(input, output, context, arguments);
			}
			finally
			{
				if (output.WriteState != WriteState.Closed)
					output.Close();
			}
		}

		internal static DictionaryFileCollection GetTranslationDictionaryCollection(SageContext context)
		{
			if (!dictionaries.ContainsKey(context.Category))
			{
				lock (log)
				{
					if (!dictionaries.ContainsKey(context.Category))
						dictionaries[context.Category] = new DictionaryFileCollection(context);
				}
			}

			return dictionaries[context.Category];
		}
	}
}
