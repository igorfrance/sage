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
	using System.IO;
	using System.Linq;
	using System.Xml;

	using Kelp;
	using Kelp.Extensions;

	using log4net;
	using Sage.Configuration;

	/// <summary>
	/// Provides a base class for use with dictionary and settings translation.
	/// </summary>
	/// <remarks>
	/// The White Label localization framework uses xml 'dictionaries' that contain locale-specific values which at translation
	/// time get inserted at desired locations in XML configuration files. This class, provided a file path template, opens all
	/// dictionaries applicable for a <c>locale</c>, and combines the localization 'items' into a single document, prioritizing them
	/// as defined in the configuration of that <c>locale</c>.
	/// <para>
	/// A locale (<c>la</c> in this example, which stands for Latin America) is typically defined like this:
	/// </para>
	/// <code>
	/// &lt;locale name="la" dictionaryNames="es-LA,es,en" resourceNames="es-LA,es,default"/&gt;
	/// </code>
	/// This means that, when processing la locale the dictionaries to be used for translation should be used in priority order
	/// as specified with <c>dictionaryNames</c>. The most specific dictionary, with highest priority, is in this case '<c>es-LA</c>',
	/// or Spanish for Latin America. Next one is <c>es</c> or Spanish, and finally <c>en</c> or English.
	/// <para>
	/// This class will attempt to open and combine entries from all three dictionaries. The base dictionary for all locales is the
	/// <c>en</c> dictionary. Entries defined in dictionaries with higher priority will override any previously defined entries.
	/// </para>
	/// <example>
	/// <h5>Example:</h5>
	/// In this example, there are three dictionaries, with some of the entries defined in more than one dictionary. Observe how
	/// the dictionaries are combined in the resulting file:
	/// <h6>en.xml:</h6>
	/// <code>
	/// &lt;intl:dictionary&gt;
	///   &lt;intl:phrase id="training.titles.products"&gt;Products&lt;/intl:phrase&gt;
	///   &lt;intl:phrase id="training.titles.home"&gt;Home&lt;/intl:phrase&gt;
	///   &lt;intl:phrase id="training.titles.athletes"&gt;Athletes&lt;/intl:phrase&gt;
	/// &lt;/intl:dictionary
	/// </code>
	/// <h6>es.xml:</h6>
	/// <code>
	/// &lt;intl:dictionary&gt;
	///   &lt;intl:phrase id="training.titles.products"&gt;Productos&lt;/intl:phrase&gt;
	///   &lt;intl:phrase id="training.titles.athletes"&gt;Los atletas&lt;/intl:phrase&gt;
	/// &lt;/intl:dictionary
	/// </code>
	/// <h6>es-LA.xml:</h6>
	/// <code>
	/// &lt;intl:dictionary&gt;
	///   &lt;intl:phrase id="training.titles.products"&gt;Los deportistas&lt;/intl:phrase&gt;
	/// &lt;/intl:dictionary
	/// </code>
	/// <h6>Resulting document:</h6>
	/// <code>
	/// &lt;intl:dictionary&gt;
	///   &lt;intl:phrase id="training.titles.products" source="es"&gt;Productos&lt;/intl:phrase&gt;
	///   &lt;intl:phrase id="training.titles.home" source="en"&gt;Home&lt;/intl:phrase&gt;
	///   &lt;intl:phrase id="training.titles.athletes" source="es-LA"&gt;Los deportistas&lt;/intl:phrase&gt;
	/// &lt;/intl:dictionary
	/// </code>
	/// </example>
	/// </remarks>
	public class DictionaryFile
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(DictionaryFile).FullName);
		private readonly SageContext context;
		private List<string> constituents;

		/// <summary>
		/// Initializes a new instance of the <see cref="DictionaryFile"/> class, using the specified context and locale.
		/// </summary>
		/// <param name="context">The <see cref="SageContext"/> to use for loading resources and resolving paths.</param>
		/// <param name="locale">The locale within the specified context's category that this <see cref="DictionaryFile"/> is
		/// used for.</param>
		public DictionaryFile(SageContext context, string locale)
		{
			this.context = context;

			this.Locale = locale;
			this.Items = new Dictionary<string, string>();
			this.Dependencies = new List<string>();
			this.Document = this.CombineVariations();
		}

		/// <summary>
		/// Gets the locale of this <see cref="DictionaryFile"/>.
		/// </summary>
		public string Locale { get;  set; }

		/// <summary>
		/// Gets the dictionary of phrases from the final merged dictionary, with all the fallbacks resolved.
		/// </summary>
		public Dictionary<string, string> Items { get;  set; }

		/// <summary>
		/// Gets the <see cref="XmlDocument"/> that contains all translation phrase combined by priority.
		/// </summary>
		public XmlDocument Document { get; private set; }

		/// <summary>
		/// Gets the latest modified date of all constituent file of this <see cref="DictionaryFile"/>.
		/// </summary>
		public DateTime DateLastModified
		{
			get
			{
				DateTime maxDate = DateTime.MinValue;
				foreach (string path in constituents)
				{
					DateTime fileDate = File.GetLastWriteTime(path);
					if (fileDate > maxDate)
						maxDate = fileDate;
				}

				return maxDate;
			}
		}

		/// <summary>
		/// Gets the list of files that this document depends on (such as any documents x-included in this one).
		/// </summary>
		public List<string> Dependencies { get; private set; }

		private XmlDocument CombineVariations()
		{
			this.constituents = new List<string>();

			LocaleInfo localeInfo;
			if (!context.ProjectConfiguration.Locales.TryGetValue(this.Locale, out localeInfo))
				throw new UnconfiguredLocaleException(this.Locale);

			// locales contains the list of locales ordered by priority (high to low)
			List<string> names = new List<string>(localeInfo.DictionaryNames);

			// documents are orderered as defined in the configuration, from high to low priority
			OrderedDictionary<string, List<CacheableXmlDocument>> allDictionaries = new OrderedDictionary<string, List<CacheableXmlDocument>>();
			foreach (string locale in names)
			{
				if (!context.ProjectConfiguration.Locales.ContainsKey(locale))
					throw new ConfigurationError(string.Format("Dictionary name '{0}' is invalid because it doesn't match any of the configured locales. Valid names are: {1}", 
						locale, string.Join(",", context.ProjectConfiguration.Locales.Keys)));

				List<CacheableXmlDocument> langDictionaries = new List<CacheableXmlDocument>();
				string documentPath = context.Path.GetDictionaryPath(locale, context.Category);

				// add extension dictionaries for the current locale
				// langDictionaries.AddRange(Application.Extensions.GetDictionaries(context, locale));
				
				// add the project dictionary for the current locale
				if (File.Exists(documentPath))
				{
					CacheableXmlDocument cacheable = new CacheableXmlDocument();
					cacheable.Load(documentPath);

					Dependencies.AddRange(cacheable.Dependencies);
					langDictionaries.Add(cacheable);
				}

				if (langDictionaries.Count != 0)
					allDictionaries.Add(locale, langDictionaries);
			}

			if (allDictionaries.Count == 0)
			{
				log.Error(
					string.Format("There are no dictionary files for locale '{0}' in category '{1}'.\n", this.Locale, context.Category));

				return null;
			}

			// now create a combined document, adding items from each document starting with high priority
			// and moving through the lower priority ones
			string firstLocale = allDictionaries.Keys.First();

			XmlDocument result = allDictionaries[firstLocale][0]; 

			XmlElement rootNode = result.DocumentElement;
			XmlNodeList dictNodes = rootNode.SelectNodes("*");
			
			foreach (XmlElement phrase in dictNodes)
				phrase.SetAttribute("source", firstLocale);

			foreach (string locale in allDictionaries.Keys)
			{
				for (int i = 0; i < allDictionaries[locale].Count; i++)
				{
					if (locale == firstLocale && i == 0)
						continue;

					dictNodes = allDictionaries[locale][i].DocumentElement.SelectNodes("*");
					foreach (XmlElement node in dictNodes)
					{
						string phraseID = node.GetAttribute("id");
						XmlNode existingNode = result.SelectSingleNode(string.Format("/*/*[@id='{0}']", phraseID.Replace("'", "&apos;")));
						if (existingNode == null)
						{
							XmlElement phrase = rootNode.AppendElement(result.ImportNode(node, true));
							phrase.SetAttribute("source", locale);
						}
					}
				}
			}

			foreach (XmlElement phraseNode in rootNode.SelectNodes("*"))
			{
				string itemId = phraseNode.GetAttribute("id");
				string itemText = phraseNode.InnerText;
				if (this.Items.ContainsKey(itemId))
				{
					this.Items[itemId] = itemText;
				}
				else
					this.Items.Add(itemId, itemText);
			}

			return result;
		}
	}
}
