namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Text;
	using System.Xml;
	using System.Xml.Xsl;

	using Kelp.Core.Extensions;
	using log4net;
	using Sage.Configuration;

	public class Globalizer
	{
		private const string XsltPath = "sageresx://sage/resources/xslt/internationalization.xslt";

		private static readonly ILog log = LogManager.GetLogger(typeof(Globalizer).FullName);

		private readonly SageContext context;
		private static readonly Dictionary<string, DictionaryFileCollection> dictionaries = new Dictionary<string, DictionaryFileCollection>();
		private readonly XslCompiledTransform processor;

		/// <summary>
		/// Initializes a new instance of the <see cref="Globalizer"/> class, using the specified <see cref="SageContext"/>.
		/// </summary>
		/// <param name="context">The context under which the globalizer is being executed.</param>
		public Globalizer(SageContext context)
		{
			CacheableXslTransform transform = ResourceManager.LoadXslStylesheet(XsltPath, context);
			this.processor = transform.Processor;
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
			CategoryInfo categoryInfo = context.Config.Categories[context.Category];
			GlobalizationSummary summary = new GlobalizationSummary(resource, categoryInfo);

			Stopwatch sw = new Stopwatch();
			sw.Start();

			foreach (string locale in coll.Locales)
			{
				if (coll.Dictionaries[locale].Document == null)
					throw new ApplicationException(string.Format(
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

				bool transformSuccess = false;

				try
				{
					this.Transform(input, context.CategoryConfiguration, coll, locale, translateWriter, GlobalizeType.Translate);
					this.Transform(input, context.CategoryConfiguration, coll, locale, diagnoseWriter, GlobalizeType.Diagnose);
					transformSuccess = true;
				}
				finally
				{
					translateWriter.Close();
					diagnoseWriter.Close();

					if (!transformSuccess)
						try
						{
							// if transform failed, it's better to delete the file as it's presence
							// indicates to the auto-globalizer that the localication exists and is 
							// therefore considered valid.
							File.Delete(outputPath);
						}
						catch (IOException)
						{
						}
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

		public void Transform(XmlDocument input, CategoryConfiguration categoryConfig, DictionaryFileCollection collection, string locale, XmlWriter output, GlobalizeType type)
		{
			if (input == null)
				throw new ArgumentNullException("input");
			if (collection == null)
				throw new ArgumentNullException("collection");
			if (locale == null)
				throw new ArgumentNullException("locale");
			if (output == null)
				throw new ArgumentNullException("output");

			LocaleInfo localeInfo = context.Config.Locales[locale];
			XmlElement fallbacks = input.CreateElement("fallbacks");
			foreach (string name in localeInfo.ResourceNames)
				fallbacks.AppendElement(input.CreateElement("locale")).InnerText = name;

			DictionaryFile dictionary = collection.Dictionaries[locale];

			XsltArgumentList args = new XsltArgumentList();
			args.AddParam("dictionary", string.Empty, dictionary.Document.DocumentElement);
			args.AddParam("locale", string.Empty, locale);
			args.AddParam("fallbacks", string.Empty, fallbacks);
			args.AddParam("mode", string.Empty, type.ToString().ToLower());
			if (categoryConfig != null && categoryConfig.VariablesElement != null)
				args.AddParam("globalvariables", string.Empty, categoryConfig.VariablesElement);

			try
			{
				processor.Transform(input, args, output);
			}
			finally
			{
				if (output.WriteState != WriteState.Closed)
					output.Close();
			}
		}
	}
}
