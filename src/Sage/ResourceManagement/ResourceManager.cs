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
		private static readonly Dictionary<string, CopyNodeHandler> nodeHandlerRegistry = new Dictionary<string, CopyNodeHandler>();
		private static readonly Dictionary<string, CopyTextHandler> textHandlerRegistry = new Dictionary<string, CopyTextHandler>();

		private static readonly ILog log = LogManager.GetLogger(typeof(ResourceManager).FullName);
		private readonly SageContext context;
		private const string MissingPhrasePlaceholder = "{{{0}}}";
		private static Regex textReplaceExpression;

		static ResourceManager()
		{
			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

			foreach (Assembly a in Application.RelevantAssemblies)
			{
				var types = from t in a.GetTypes()
							where t.IsClass && !t.IsAbstract
							select t;

				foreach (Type type in types)
				{
					foreach (MethodInfo methodInfo in type.GetMethods(flags))
					{
						foreach (NodeHandlerAttribute attrib in methodInfo.GetCustomAttributes(typeof(NodeHandlerAttribute), false))
						{
							CopyNodeHandler del = (CopyNodeHandler) Delegate.CreateDelegate(typeof(CopyNodeHandler), methodInfo);
							ResourceManager.RegisterNodeHandler(attrib.NodeType, attrib.NodeName, attrib.Namespace, del);
						}

						foreach (TextHandlerAttribute attrib in methodInfo.GetCustomAttributes(typeof(TextHandlerAttribute), false))
						{
							CopyTextHandler del = (CopyTextHandler) Delegate.CreateDelegate(typeof(CopyTextHandler), methodInfo);
							foreach (string variable in attrib.Variables)
							{
								ResourceManager.RegisterTextHandler(variable, del);
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
		public static void RegisterNodeHandler(XmlNodeType nodeType, string nodeName, string nodeNamespace, CopyNodeHandler handler)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(nodeName));
			Contract.Requires<ArgumentNullException>(handler != null);

			string qualifiedName = QualifyName(nodeType, nodeName, nodeNamespace);
			if (nodeHandlerRegistry.ContainsKey(qualifiedName))
			{
				log.WarnFormat("Replacing existing handler '{0}' for element '{1}' with new handler '{2}'",
					GetDelegateSignature(nodeHandlerRegistry[qualifiedName]),
					qualifiedName,
					GetDelegateSignature(handler));

				nodeHandlerRegistry[qualifiedName] = handler;
			}
			else
			{
				nodeHandlerRegistry.Add(qualifiedName, handler);
			}
		}

		public static void RegisterTextHandler(string variableName, CopyTextHandler handler)
		{
			if (string.IsNullOrEmpty(variableName))
				throw new ArgumentNullException("variableName");
			if (handler == null)
				throw new ArgumentNullException("handler");

			variableName = variableName.ToLower();
			if (textHandlerRegistry.ContainsKey(variableName))
			{
				log.WarnFormat("Replacing existing handler '{0}' for variable '{1}' with new handler '{2}'",
					GetDelegateSignature(textHandlerRegistry[variableName]),
					variableName,
					GetDelegateSignature(handler));

				textHandlerRegistry[variableName] = handler;
			}
			else
			{
				textHandlerRegistry.Add(variableName, handler);
			}

			textReplaceExpression = new Regex("\\$?{(" + string.Join("|", textHandlerRegistry.Keys.ToArray()) + ")}",
				RegexOptions.IgnoreCase);
		}

		public static string GetRemoteTextFile(string url)
		{
			WebResponse response = GetHttpResponse(url, null, null);
			StreamReader reader = new StreamReader(response.GetResponseStream());
			string responseText = reader.ReadToEnd();
			reader.Close();
			reader.Dispose();

			return responseText;
		}

		public static string GetDelegateSignature(Delegate d)
		{
			return string.Concat(d.Method.DeclaringType.FullName, ".", d.Method.Name);
		}

		public static WebResponse GetHttpResponse(string url)
		{
			return GetHttpResponse(url, null, null);
		}

		public static WebResponse GetHttpResponse(string url, string username, string password)
		{
			WebRequest request = WebRequest.Create(url);
			if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
				request.Credentials = new NetworkCredential(username, password);

			WebResponse response = request.GetResponse();
			return response;
		}

		public static CacheableXmlDocument LoadXmlDocument(string path)
		{
		    Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));

		    return LoadXmlDocument(path, null);
		}

		public static CacheableXmlDocument LoadXmlDocument(string path, SageContext context, string schemaPath)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));

			CacheableXmlDocument result = LoadXmlDocument(path, context);
			if (schemaPath != null)
				ValidateDocument(result, schemaPath);

			return result;
		}

		public static CacheableXmlDocument LoadXmlDocument(string path, SageContext context, XmlSchemaSet schemas)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));

			CacheableXmlDocument result = LoadXmlDocument(path, context);
			if (schemas != null)
				ValidateDocument(result, schemas);

			return result;
		}

		public static CacheableXmlDocument LoadXmlDocument(string path, SageContext context)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path));

			if (UrlResolver.GetScheme(path) == "file" && context != null && !File.Exists(context.Path.Resolve(path)))
			{
				throw new FileNotFoundException(string.Format("The xml document '{0}' could not be loaded", path));
			}

			CacheableXmlDocument result = null;
			bool isXslt = Path.GetExtension(path).ToLower().Contains("xsl");
			bool tryGlobalize = !isXslt && context != null;
			if (tryGlobalize)
			{
				XmlResource resource = new XmlResource(path, context);
				result = resource.Load(context.Locale);
				result.ReplaceChild(ResourceManager.CopyNode(result.DocumentElement, context), result.DocumentElement);
			}
			else
			{
				UrlResolver resolver = new UrlResolver(context);
				result = new CacheableXmlDocument();
				result.Load(path, resolver);
				result.AddDependencies(path);
				result.AddDependencies(resolver.Dependencies.ToArray());
			}

			return result;
		}

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

		public static void ValidateDocument(XmlDocument document, XmlSchemaSet schemaSet)
		{
			Contract.Requires<ArgumentNullException>(document != null);
			Contract.Requires<ArgumentNullException>(schemaSet != null);

			document.Schemas = schemaSet;
			document.Validate(delegate (object e, ValidationEventArgs args)
			{
				if (args.Severity == XmlSeverityType.Error)
					throw args.Exception;
			});
		}

		public CacheableXmlDocument LoadXml(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			return LoadXmlDocument(path, context);
		}

		internal static XmlNode CopyNode(XmlNode node, SageContext context)
		{
			XmlNode result;

			switch (node.NodeType)
			{
				case XmlNodeType.Document:
					result = CopyNode(((XmlDocument) node).DocumentElement, context);
					break;

				case XmlNodeType.Element:
					result = node.OwnerDocument.CreateElement(node.Name, node.NamespaceURI);

					foreach (XmlAttribute attribute in node.Attributes)
					{
						XmlNode processed = GetNodeHandler(attribute)(attribute, context);
						if (processed != null)
							result.Attributes.Append((XmlAttribute) processed);
					}

					foreach (XmlNode child in node.ChildNodes)
					{
						XmlNode processed = GetNodeHandler(child)(child, context);
						if (processed != null)
							result.AppendChild(processed);
					}

					break;

				case XmlNodeType.Attribute:
				case XmlNodeType.Text:
					result = node.CloneNode(true);
					if (textReplaceExpression != null)
					{
						result.Value = textReplaceExpression.Replace(result.Value, delegate(Match m)
						{
							string varName = m.Groups[1].Value.ToLower();
							return textHandlerRegistry[varName](varName, context);
						});
					}

					break;

				default:
					result = node.CloneNode(true);
					break;
			}

			return result;
		}

		internal static CopyNodeHandler GetNodeHandler(XmlNode node)
		{
			string key = ResourceManager.QualifyName(node);
			if (nodeHandlerRegistry.ContainsKey(key))
				return nodeHandlerRegistry[key];

			return ResourceManager.CopyNode;
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
	}
}
