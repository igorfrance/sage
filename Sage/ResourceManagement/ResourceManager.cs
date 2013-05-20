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
	using System.Net;
	using System.Reflection;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.Linq;
	using System.Xml.Schema;

	using Kelp;

	using log4net;
	using Sage.Configuration;
	using Sage.Extensibility;

	/// <summary>
	/// Handles internal file resource management.
	/// </summary>
	public class ResourceManager
	{
		private const string MissingPhrasePlaceholder = "{{{0}}}";

		private static readonly ILog log = LogManager.GetLogger(typeof(ResourceManager).FullName);
		private readonly SageContext context;

		/// <summary>
		/// Initializes a new instance of the <see cref="ResourceManager"/> class, using the specified <see cref="SageContext"/>.
		/// </summary>
		/// <param name="context">The current context.</param>
		public ResourceManager(SageContext context)
		{
			this.context = context;
		}

		/// <summary>
		/// Gets the context.
		/// </summary>
		public SageContext Context
		{
			get
			{
				return context;
			}
		}

		/// <summary>
		/// Returns the <see cref="WebResponse"/> resulting from issuing a request to <paramref name="url"/>.
		/// </summary>
		/// <param name="url">The URL to which to send the request.</param>
		/// <returns>The <see cref="WebResponse"/> resulting from issuing a request to <paramref name="url"/>.</returns>
		public static WebResponse GetHttpResponse(string url)
		{
			return GetHttpResponse(url, null, null);
		}

		/// <summary>
		/// Returns the <see cref="WebResponse"/> resulting from issuing a request to <paramref name="url"/>, using the
		/// specified <paramref name="username"/> and <paramref name="password"/>.
		/// </summary>
		/// <param name="url">The URL to which to send the request.</param>
		/// <param name="username">The username to use.</param>
		/// <param name="password">The password to use.</param>
		/// <returns>The <see cref="WebResponse"/> resulting from issuing a request to <paramref name="url"/>.</returns>
		public static WebResponse GetHttpResponse(string url, string username, string password)
		{
			WebRequest request = WebRequest.Create(url);
			if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
				request.Credentials = new NetworkCredential(username, password);

			WebResponse response = request.GetResponse();
			return response;
		}

		/// <summary>
		/// Fetches the remote file from the specified <paramref name="url"/> and returns the response text.
		/// </summary>
		/// <param name="url">The URL of the file to fetch.</param>
		/// <returns>The contents of the remote file from the specified <paramref name="url"/></returns>
		public static string GetRemoteTextFile(string url)
		{
			WebResponse response = GetHttpResponse(url, null, null);
			StreamReader reader = new StreamReader(response.GetResponseStream());
			string responseText = reader.ReadToEnd();
			reader.Close();
			reader.Dispose();

			return responseText;
		}

		/// <summary>
		/// Creates and returns a new <see cref="CacheableXmlDocument"/>, loaded from the specified <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The path to the document to load.</param>
		/// <returns>A new <see cref="CacheableXmlDocument"/>, loaded from the specified <paramref name="path"/>.</returns>
		public static CacheableXmlDocument LoadXmlDocument(string path)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));

			return LoadXmlDocument(path, null);
		}

		/// <summary>
		/// Creates and returns a new <see cref="CacheableXmlDocument"/>, loaded from the specified <paramref name="path"/>,
		/// using the specified <paramref name="context"/>.
		/// </summary>
		/// <param name="path">The path to the document to load.</param>
		/// <param name="context">The context to use to resolve the <paramref name="path"/> and load the document.</param>
		/// <returns>A new <see cref="CacheableXmlDocument"/>, loaded from the specified <paramref name="path"/>.</returns>
		public static CacheableXmlDocument LoadXmlDocument(string path, SageContext context)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));

			if (UrlResolver.GetScheme(path) == "file" && context != null && !File.Exists(context.Path.Resolve(path)))
			{
				throw new FileNotFoundException(string.Format("The xml document '{0}' could not be loaded", path));
			}

			bool isXslt = Path.GetExtension(path).ToLower().Contains("xsl");
			bool tryGlobalize = !isXslt && context != null;

			CacheableXmlDocument result;
			if (tryGlobalize)
			{
				try
				{
					result = LoadGlobalizedDocument(path, context);
				}
				catch (InternationalizationError err)
				{
					log.ErrorFormat("Failed to internationalize '{0}': {1}", path, err.Message);
					result = LoadSourceDocument(path, context);
				}

				result.ReplaceChild(context.ProcessNode(result), result.DocumentElement);
			}
			else
			{
				result = LoadSourceDocument(path, context);
			}

			return result;
		}

		/// <summary>
		/// Creates, validates and returns a new <see cref="CacheableXmlDocument"/>, loaded from the specified <paramref name="path"/>,
		/// using the specified <paramref name="context"/> and <paramref name="schemaPath"/>.
		/// </summary>
		/// <param name="path">The path to the document to load.</param>
		/// <param name="context">The context to use to resolve the <paramref name="path"/> and load the document.</param>
		/// <param name="schemaPath">The path to the XML schema to use to validate the document against.</param>
		/// <returns>A new <see cref="CacheableXmlDocument"/>, loaded from the specified <paramref name="path"/>.</returns>
		public static CacheableXmlDocument LoadXmlDocument(string path, SageContext context, string schemaPath)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));

			CacheableXmlDocument result = LoadXmlDocument(path, context);
			if (schemaPath != null)
				ValidateDocument(result, schemaPath);

			return result;
		}

		/// <summary>
		/// Validates the specified document <paramref name="document"/> against the XML schema loaded from the specified
		/// <paramref name="schemaPath"/>, and returns an object that contains the validation information.
		/// </summary>
		/// <param name="document">The document to validate.</param>
		/// <param name="schemaPath">The path to the XML schema to use to validate the document against.</param>
		/// <returns>An object that contains the validation information</returns>
		public static ValidationResult ValidateDocument(XmlDocument document, string schemaPath)
		{
			Contract.Requires<ArgumentNullException>(document != null);
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(schemaPath));

			ValidationResult result = new ValidationResult();
			XmlSchemaSet schemaSet = null;
			if (!string.IsNullOrWhiteSpace(schemaPath))
			{
				UrlResolver resolver = new UrlResolver();
				XmlReaderSettings settings = CacheableXmlDocument.CreateReaderSettings(resolver);
				XmlReader reader = XmlReader.Create(schemaPath, settings);
				XmlSchema schema = XmlSchema.Read(reader, null);

				schemaSet = new XmlSchemaSet { XmlResolver = resolver };
				schemaSet.Add(schema);
				schemaSet.Compile();
			}

			XDocument xdocument = XDocument.Parse(document.OuterXml, LoadOptions.SetLineInfo);
			xdocument.Validate(schemaSet, (sender, args) => 
			{
				if (args.Severity == XmlSeverityType.Error)
				{
					result.Success = false;

					var lineInfo = sender as IXmlLineInfo;
					if (lineInfo != null)
					{
						result.Exception = new XmlException(args.Message, args.Exception, lineInfo.LineNumber, lineInfo.LinePosition);
					}
					else
					{
						result.Exception = args.Exception;
					}
				}
			});

			return result;
		}

		/// <summary>
		/// Validates the specified document <paramref name="element"/> against the XML schema loaded from the specified
		/// <paramref name="schemaPath"/>, and returns an object that contains the validation information.
		/// </summary>
		/// <param name="element">The element to validate.</param>
		/// <param name="schemaPath">The path to the XML schema to use to validate the element against.</param>
		/// <returns>An object that contains the validation information</returns>
		public static ValidationResult ValidateElement(XmlElement element, string schemaPath)
		{
			Contract.Requires<ArgumentNullException>(element != null);
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(schemaPath));

			XmlDocument document = new XmlDocument();
			document.PreserveWhitespace = true;
			document.LoadXml(element.OuterXml);

			return ValidateDocument(document, schemaPath);
		}

		/// <summary>
		/// Creates and returns a new <see cref="CacheableXmlDocument"/>, loaded from the specified <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>A new <see cref="CacheableXmlDocument"/>, loaded from the specified <paramref name="path"/>.</returns>
		public CacheableXmlDocument LoadXml(string path)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));

			return LoadXmlDocument(path, context);
		}

		/// <summary>
		/// Gets the dictionary phrase with the specified id, for the current category, in the current locale.
		/// </summary>
		/// <param name="phraseId">The id of the phrase to get.</param>
		/// <returns>The dictionary phrase with the specified id, for the current category, in the current locale.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="phraseId"/> is <c>null</c>.
		/// </exception>
		internal string GetPhrase(string phraseId)
		{
			if (string.IsNullOrEmpty(phraseId))
				throw new ArgumentNullException("phraseId");

			phraseId = string.Concat(context.Category, ".", phraseId);

			DictionaryFileCollection dictionaries = Internationalizer.GetTranslationDictionaryCollection(context);
			if (!dictionaries.Locales.Contains(context.Locale))
			{
				log.ErrorFormat(
					"The requested phrase '{0}' could not be retrieved because the current category '{1}' has no dictionary in the current locale '{2}'.",
					phraseId,
					context.Category,
					context.Locale);

				return string.Format(MissingPhrasePlaceholder, phraseId);
			}

			var dictionary = dictionaries.Dictionaries[context.Locale];
			var phrases = dictionary.Items;
			if (!phrases.ContainsKey(phraseId))
			{
				log.ErrorFormat(
					"The requested phrase '{0}' could not be retrieved because the dictionary for category '{1}' and locale '{2}' has not such phrase defined.",
					phraseId,
					context.Category,
					context.Locale);

				return string.Format(MissingPhrasePlaceholder, phraseId);
			}

			return phrases[phraseId];
		}

		private static CacheableXmlDocument LoadGlobalizedDocument(string path, SageContext context)
		{
			XmlResource resource = new XmlResource(path, context);
			CacheableXmlDocument result = resource.Load(context.Locale);

			return result;
		}

		private static CacheableXmlDocument LoadSourceDocument(string path, SageContext context)
		{
			UrlResolver resolver = new UrlResolver(context);
			CacheableXmlDocument result = new CacheableXmlDocument();
			result.Load(path, context);
			result.AddDependencies(path);
			result.AddDependencies(resolver.Dependencies.ToArray());

			return result;
		}
	}
}
