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
		private const BindingFlags AttributeBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

		private static readonly Dictionary<string, NodeHandler> nodeHandlerRegistry = new Dictionary<string, NodeHandler>();
		private static readonly Dictionary<string, TextHandler> textHandlerRegistry = new Dictionary<string, TextHandler>();

		private static readonly ILog log = LogManager.GetLogger(typeof(ResourceManager).FullName);
		private static Regex textReplaceExpression;
		private readonly SageContext context;

		static ResourceManager()
		{
			foreach (Assembly a in Project.RelevantAssemblies)
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
							NodeHandler del = (NodeHandler) Delegate.CreateDelegate(typeof(NodeHandler), methodInfo);
							RegisterNodeHandler(attrib.NodeType, attrib.NodeName, attrib.Namespace, del);
						}

						foreach (TextHandlerAttribute attrib in methodInfo.GetCustomAttributes(typeof(TextHandlerAttribute), false))
						{
							TextHandler del = (TextHandler) Delegate.CreateDelegate(typeof(TextHandler), methodInfo);
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
		public static void RegisterNodeHandler(XmlNodeType nodeType, string nodeName, string nodeNamespace, NodeHandler handler)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(nodeName));
			Contract.Requires<ArgumentNullException>(handler != null);

			string qualifiedName = QualifyName(nodeType, nodeName, nodeNamespace);
			if (nodeHandlerRegistry.ContainsKey(qualifiedName))
			{
				log.WarnFormat("Replacing existing handler '{0}' for element '{1}' with new handler '{2}'",
					Util.GetMethodSignature(nodeHandlerRegistry[qualifiedName].Method),
					qualifiedName,
					Util.GetMethodSignature(handler.Method));

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
		public static void RegisterTextHandler(string variableName, TextHandler handler)
		{
			if (string.IsNullOrEmpty(variableName))
				throw new ArgumentNullException("variableName");
			if (handler == null)
				throw new ArgumentNullException("handler");

			variableName = variableName.ToLower();
			if (textHandlerRegistry.ContainsKey(variableName))
			{
				log.WarnFormat("Replacing existing handler '{0}' for variable '{1}' with new handler '{2}'",
					Util.GetMethodSignature(textHandlerRegistry[variableName].Method),
					variableName,
					Util.GetMethodSignature(handler.Method));

				textHandlerRegistry[variableName] = handler;
			}
			else
			{
				textHandlerRegistry.Add(variableName, handler);
			}

			textReplaceExpression = new Regex(@"\$?(\{)?{(" + string.Join("|", textHandlerRegistry.Keys.ToArray()) + @")}(\})?",
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
				catch (InternationalizationError err)
				{
					log.ErrorFormat("Failed to internationalize '{0}': {1}", path, err.Message);
					result = LoadSourceDocument(path, context);
				}

				result.ReplaceChild(ResourceManager.ApplyHandlers(result.DocumentElement, context), result.DocumentElement);
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
		/// Recursively processes all nodes in the specified <paramref name="node"/>, applying any registered text and node
		/// handlers on the way, and returns the result.
		/// </summary>
		/// <param name="node">The node to process.</param>
		/// <param name="context">The context to use.</param>
		/// <returns>A copy of the specified <paramref name="node"/>, with it's child nodes processed.</returns>
		public static XmlNode ApplyHandlers(XmlNode node, SageContext context)
		{
			XmlNode result;

			switch (node.NodeType)
			{
				case XmlNodeType.Document:
					result = ApplyHandlers(((XmlDocument) node).DocumentElement, context);
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
					result = node.CloneNode(true);
					if (node.SelectSingleNode("ancestor::sage:literal", Sage.XmlNamespaces.Manager) == null)
					{
						result.Value = ApplyTextHandlers(result.Value, context);
						result.Value = context.ProcessFunctions(result.Value);
					}

					break;

				case XmlNodeType.Text:
					result = node.CloneNode(true);
					if (node.SelectSingleNode("ancestor::sage:literal", Sage.XmlNamespaces.Manager) == null)
					{
						result.Value = ApplyTextHandlers(result.Value, context);
					}

					break;

				default:
					result = node.CloneNode(true);
					break;
			}

			return result;
		}

		/// <summary>
		/// Applies registerd text handlers on the specified input <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The value to process.</param>
		/// <param name="context">The context to use.</param>
		/// <returns>A processed version of the specified <paramref name="value"/>.</returns>
		public static string ApplyTextHandlers(string value, SageContext context)
		{
			if (textReplaceExpression != null)
			{
				value = textReplaceExpression.Replace(value, delegate(Match m)
				{
					bool isEscaped = m.Groups[1].Value == "{";
					string varName = m.Groups[2].Value.ToLower();
					if (isEscaped)
						return string.Concat("{", varName, "}");

					return textHandlerRegistry[varName](varName, context);
				});
			}

			return value;
		}

		/// <summary>
		/// Gets the node handler for the spefied node.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <returns>A <see cref="NodeHandler"/> for the specified node. If no handler is registered for this node's
		/// local name and namespace, the default handler is returned; this handler simply continues processing the node 
		/// and returns its copy.</returns>
		public static NodeHandler GetNodeHandler(XmlNode node)
		{
			string key = ResourceManager.QualifyName(node);
			if (nodeHandlerRegistry.ContainsKey(key))
				return nodeHandlerRegistry[key];

			return ResourceManager.ApplyHandlers;
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
			result.Load(path, context);
			result.AddDependencies(path);
			result.AddDependencies(resolver.Dependencies.ToArray());

			return result;
		}
	}
}
