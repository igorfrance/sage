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
	using System.Xml.Schema;

	using log4net;
	using Sage.Extensibility;

	/// <summary>
	/// Handles internal file resource management.
	/// </summary>
	public class ResourceManager
	{
		private static readonly Dictionary<string, ProcessNode> nodeHandlerRegistry = new Dictionary<string, ProcessNode>();
		private static readonly Dictionary<string, ProcessText> textHandlerRegistry = new Dictionary<string, ProcessText>();

		private const string MissingPhrasePlaceholder = "{{{0}}}";
		private const BindingFlags AttributeBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

		private static readonly ILog log = LogManager.GetLogger(typeof(ResourceManager).FullName);
		private static readonly Regex escapeCleanupExpression = new Regex(@"\{({[^}]+})}", RegexOptions.Compiled);
		private readonly SageContext context;
		private static Regex textReplaceExpression;

		static ResourceManager()
		{
			foreach (Assembly a in Application.RelevantAssemblies)
			{
				var types = from t in a.GetTypes()
							where t.IsClass && !t.IsAbstract
							select t;

				foreach (Type type in types)
				{
					foreach (MethodInfo methodInfo in type.GetMethods(AttributeBindingFlags))
					{
						foreach (NodeHandlerAttribute attrib in methodInfo.GetCustomAttributes(typeof(NodeHandlerAttribute), false))
						{
							ProcessNode del = (ProcessNode) Delegate.CreateDelegate(typeof(ProcessNode), methodInfo);
							RegisterNodeHandler(attrib.NodeType, attrib.NodeName, attrib.Namespace, del);
						}

						foreach (TextHandlerAttribute attrib in methodInfo.GetCustomAttributes(typeof(TextHandlerAttribute), false))
						{
							ProcessText del = (ProcessText) Delegate.CreateDelegate(typeof(ProcessText), methodInfo);
							foreach (string variable in attrib.Variables)
							{
								RegisterTextHandler(variable, del);
							}
						}
					}
				}
			}
		}

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
		/// Registers the specified node <paramref name="handler"/>, for the specified <paramref name="nodeType"/>,
		/// <paramref name="nodeName"/> and <paramref name="nodeNamespace"/>.
		/// </summary>
		/// <param name="nodeType">The type of the node for wihch the handler is being registered.</param>
		/// <param name="nodeName">The name of the node for wihch the handler is being registered.</param>
		/// <param name="nodeNamespace">The namespace of the node for wihch the handler is being registered.</param>
		/// <param name="handler">The method that will will handle the node.</param>
		public static void RegisterNodeHandler(XmlNodeType nodeType, string nodeName, string nodeNamespace, ProcessNode handler)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(nodeName));
			Contract.Requires<ArgumentNullException>(handler != null);

			string qualifiedName = QualifyName(nodeType, nodeName, nodeNamespace);
			if (nodeHandlerRegistry.ContainsKey(qualifiedName))
			{
				log.WarnFormat("Replacing existing handler '{0}' for element '{1}' with new handler '{2}'",
					GetDelegateSignature(nodeHandlerRegistry[qualifiedName].Method),
					qualifiedName,
					GetDelegateSignature(handler.Method));

				nodeHandlerRegistry[qualifiedName] = handler;
			}
			else
			{
				nodeHandlerRegistry.Add(qualifiedName, handler);
			}
		}

		/// <summary>
		/// Registers the text handler.
		/// </summary>
		/// <param name="variableName">Name of the variable.</param>
		/// <param name="handler">The handler.</param>
		public static void RegisterTextHandler(string variableName, ProcessText handler)
		{
			if (string.IsNullOrEmpty(variableName))
				throw new ArgumentNullException("variableName");
			if (handler == null)
				throw new ArgumentNullException("handler");

			variableName = variableName.ToLower();
			if (textHandlerRegistry.ContainsKey(variableName))
			{
				log.WarnFormat("Replacing existing handler '{0}' for variable '{1}' with new handler '{2}'",
					GetDelegateSignature(textHandlerRegistry[variableName].Method),
					variableName,
					GetDelegateSignature(handler.Method));

				textHandlerRegistry[variableName] = handler;
			}
			else
			{
				textHandlerRegistry.Add(variableName, handler);
			}

			textReplaceExpression = new Regex("\\$?(?<!{){(" + string.Join("|", textHandlerRegistry.Keys.ToArray()) + ")}",
				RegexOptions.IgnoreCase);
		}

		/// <summary>
		/// Fethes the remote file from the specified <paramref name="url"/> and returns the response text.
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
				catch (GlobalizationError err)
				{
					log.ErrorFormat("Failed to globalize '{0}': {1}", path, err.Message);
					result = LoadSourceDocument(path, context);
				}

				result.ReplaceChild(ResourceManager.CopyTree(result.DocumentElement, context), result.DocumentElement);
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
		/// Creates, validates and returns a new <see cref="CacheableXmlDocument"/>, loaded from the specified <paramref name="path"/>,
		/// using the specified <paramref name="context"/> and <paramref name="schemas"/>.
		/// </summary>
		/// <param name="path">The path to the document to load.</param>
		/// <param name="context">The context to use to resolve the <paramref name="path"/> and load the document.</param>
		/// <param name="schemas">The XML schema set to use to validate the document against.</param>
		/// <returns>A new <see cref="CacheableXmlDocument"/>, loaded from the specified <paramref name="path"/>.</returns>
		public static CacheableXmlDocument LoadXmlDocument(string path, SageContext context, XmlSchemaSet schemas)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));

			CacheableXmlDocument result = LoadXmlDocument(path, context);
			if (schemas != null)
				ValidateDocument(result, schemas);

			return result;
		}

		/// <summary>
		/// Validates the specified document <paramref name="document"/> against the XML schema loaded from the specified
		/// <paramref name="schemaPath"/>.
		/// </summary>
		/// <param name="document">The document to validate.</param>
		/// <param name="schemaPath">The path to the XML schema to use to validate the document against.</param>
		public static void ValidateDocument(XmlDocument document, string schemaPath)
		{
			Contract.Requires<ArgumentNullException>(document != null);
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(schemaPath));

			XmlSchemaSet schemaSet = null;
			if (!string.IsNullOrWhiteSpace(schemaPath))
			{
				UrlResolver resolver = new UrlResolver();
				XmlReaderSettings settings = CacheableXmlDocument.CreateReaderSettings(resolver);
				XmlReader reader = XmlReader.Create(schemaPath, settings);
				XmlSchema schema = XmlSchema.Read(reader, delegate(object sender, ValidationEventArgs args)
				{
					if (args.Severity == XmlSeverityType.Error)
						throw args.Exception;
				});

				schemaSet = new XmlSchemaSet();
				schemaSet.XmlResolver = resolver;
				schemaSet.Add(schema);
				schemaSet.Compile();
			}

			ValidateDocument(document, schemaSet);
		}

		/// <summary>
		/// Validates the specified document <paramref name="document"/> against the specified <paramref name="schemaSet"/>.
		/// </summary>
		/// <param name="document">The document to validate.</param>
		/// <param name="schemaSet">The XML schema set to use to validate the document against.</param>
		public static void ValidateDocument(XmlDocument document, XmlSchemaSet schemaSet)
		{
			Contract.Requires<ArgumentNullException>(document != null);
			Contract.Requires<ArgumentNullException>(schemaSet != null);

			document.Schemas = schemaSet;
			document.Validate(OnSchemaValidatationComplete);
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

		internal static XmlNode CopyTree(XmlNode node, SageContext context)
		{
			XmlNode result;

			switch (node.NodeType)
			{
				case XmlNodeType.Document:
					result = CopyTree(((XmlDocument) node).DocumentElement, context);
					break;

				case XmlNodeType.Element:
					result = node.OwnerDocument.CreateElement(node.Name, node.NamespaceURI);

					XmlNodeList attributes = node.SelectNodes("@*");
					XmlNodeList children = node.SelectNodes("node()");

					foreach (XmlAttribute attribute in attributes)
					{
						XmlNode processed = GetNodeHandler(attribute)(attribute, context);
						if (processed != null)
							result.Attributes.Append((XmlAttribute) processed);
					}

					foreach (XmlNode child in children)
					{
						XmlNode processed = GetNodeHandler(child)(child, context);
						if (processed != null)
							result.AppendChild(processed);
					}

					break;

				case XmlNodeType.Attribute:
				case XmlNodeType.Text:
					result = node.CloneNode(true);
					result.Value = ProcessString(result.Value, context);

					break;

				default:
					result = node.CloneNode(true);
					break;
			}

			return result;
		}

		internal static string ProcessString(string value, SageContext context)
		{
			if (textReplaceExpression != null)
			{
				value = textReplaceExpression.Replace(value, delegate(Match m)
				{
					string varName = m.Groups[1].Value.ToLower();
					return textHandlerRegistry[varName](varName, context);
				});
			}

			value = escapeCleanupExpression.Replace(value, "$1");
			return value;
		}

		internal static ProcessNode GetNodeHandler(XmlNode node)
		{
			string key = ResourceManager.QualifyName(node);
			if (nodeHandlerRegistry.ContainsKey(key))
				return nodeHandlerRegistry[key];

			return ResourceManager.CopyTree;
		}

		internal static string GetDelegateSignature(MethodInfo method)
		{
			return string.Concat(method.DeclaringType.FullName, ".", method.Name);
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

			DictionaryFileCollection dictionaries = Globalizer.GetTranslationDictionaryCollection(context);
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

		private static string QualifyName(XmlNodeType type, string name, string ns)
		{
			if (name.IndexOf(":") >= 0)
				name = name.Substring(name.IndexOf(":") + 1);

			if (string.IsNullOrEmpty(ns))
				return string.Concat((int) type, "_", name);

			return string.Concat((int) type, "_", ns, ":", name);
		}

		private static string QualifyName(XmlNode node)
		{
			if (string.IsNullOrEmpty(node.NamespaceURI))
				return string.Concat((int) node.NodeType, "_", node.LocalName);

			return string.Concat((int) node.NodeType, "_", node.NamespaceURI, ":", node.LocalName);
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
			result.Load(path, resolver);
			result.AddDependencies(path);
			result.AddDependencies(resolver.Dependencies.ToArray());

			return result;
		}

		private static void OnSchemaValidatationComplete(object sender, ValidationEventArgs args)
		{
			if (args.Severity == XmlSeverityType.Error)
				throw args.Exception;
		}
	}
}
