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
	using System.Diagnostics;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Text;
	using System.Xml;

	using Kelp.Extensions;

	using log4net;

	using Sage.Configuration;
	using Sage.Views;

	public class Globalizer
	{
		private const string XsltPath = "sageresx://sage/resources/xslt/internationalization.xslt";

		private static readonly ILog log = LogManager.GetLogger(typeof(Globalizer).FullName);

		private readonly SageContext context;
		private static readonly Dictionary<string, DictionaryFileCollection> dictionaries = new Dictionary<string, DictionaryFileCollection>();
		private readonly XsltTransform processor;

		/// <summary>
		/// Initializes a new instance of the <see cref="Globalizer"/> class, using the specified <see cref="SageContext"/>.
		/// </summary>
		/// <param name="context">The context under which the globalizer is being executed.</param>
		public Globalizer(SageContext context)
		{
			this.processor = XsltTransform.Create(context, Globalizer.XsltPath);
			this.context = new SageContext(context);
		}

		/// <summary>
		/// Globalizes the specified resource.
		/// </summary>
		/// <param name="resource">The resource.</param>
		/// <returns>
		/// The summary information about the globalization of the specied <paramref name="resource"/>.
		/// </returns>
		/// <exception cref="ApplicationException">
		/// One of the locales configured for the category being globalized doesn't have any dictionary files.
		/// </exception>
		public GlobalizationSummary Globalize(XmlResource resource)
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
			GlobalizationSummary summary = new GlobalizationSummary(resource, categoryInfo);

			Stopwatch sw = new Stopwatch();
			sw.Start();

			foreach (string locale in coll.Locales)
			{
				if (coll.Dictionaries[locale].Document == null)
					throw new GlobalizationError(string.Format(
						"The locale '{0}' in category '{1}' doesn't have any dictionary files", locale, coll.Category));

				CacheableXmlDocument input;
				try
				{
					input = resource.LoadLocalizedSourceDocument(locale);
				}
				catch (FileNotFoundException ex)
				{
					log.ErrorFormat("Could not globalize resource '{0}' to locale '{1}': {2}", resource.Name.Signature, locale, ex.Message);
					continue;
				}


				XmlWriterSettings settings = new XmlWriterSettings { Indent = true };

				string outputPath = resource.GetGlobalizedName(locale, true);
				XmlWriter translateWriter = XmlWriter.Create(outputPath, settings);
				
				StringBuilder builder = new StringBuilder();
				XmlWriter diagnoseWriter = XmlWriter.Create(builder, settings);

				try
				{
					this.Transform(input, context.CategoryConfiguration, coll, locale, translateWriter, GlobalizeType.Translate);
					this.Transform(input, context.CategoryConfiguration, coll, locale, diagnoseWriter, GlobalizeType.Diagnose);
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

			log.InfoFormat("Globalized xml resource '{0}' into {1} locales in {2}ms", 
				resource.Name.Signature, coll.Locales.Count, sw.ElapsedMilliseconds);

			return summary;
		}

		public void Transform(XmlDocument input, CategoryConfiguration categoryConfig, DictionaryFileCollection collection, string locale, XmlWriter output, GlobalizeType type)
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
			if (categoryConfig != null && categoryConfig.VariablesElement != null)
				arguments.Add("globalvariables", categoryConfig.VariablesElement);

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
