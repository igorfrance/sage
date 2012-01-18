namespace Sage.Views
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Web.Caching;
	using System.Xml;
	using System.Xml.XPath;
	using System.Xml.Xsl;

	using Kelp.Http;

	using log4net;

	using Sage.Extensibility;
	using Sage.ResourceManagement;

	public abstract class XsltTransform
	{
		protected readonly List<string> dependencies = new List<string>();
		private const string CacheKeyFormat = "XSLT_{0}";

		private static readonly ILog log = LogManager.GetLogger(typeof(XsltTransform).FullName);
		private static readonly Dictionary<string, object> extensions;

		private readonly Hashtable parameterArguments;
		private readonly Hashtable extensionArguments;

		static XsltTransform()
		{
			extensions = new Dictionary<string, object>();
			foreach (Assembly a in Application.RelevantAssemblies)
			{
				var types = from t in a.GetTypes()
							where t.IsClass && !t.IsAbstract
							select t;

				foreach (Type type in types.Where(t => t.GetCustomAttributes(typeof(XsltExtensionObjectAttribute), false).Length != 0))
				{
					try
					{
						ConstructorInfo ctor = type.GetConstructor(new Type[] { });
						object instance = ctor.Invoke(new object[] { });

						XsltExtensionObjectAttribute attrib = (XsltExtensionObjectAttribute)
							type.GetCustomAttributes(typeof(XsltExtensionObjectAttribute), false)[0];

						extensions.Add(attrib.Namespace, instance);
						log.DebugFormat("Successfully created the XSLT extension object '{0}'.", type.FullName);
					}
					catch (Exception ex)
					{
						log.ErrorFormat("Failed to create the XSLT extension object '{0}': {1}", type.FullName, ex.Message);
					}
				}
			}
		}

		internal XsltTransform()
		{
			this.Dependencies = this.dependencies.AsReadOnly();
			this.Arguments = new XsltArgumentList();
			this.LastModified = null;

			FieldInfo fi1 = typeof(XsltArgumentList).GetField("parameters", BindingFlags.NonPublic | BindingFlags.Instance);
			FieldInfo fi2 = typeof(XsltArgumentList).GetField("extensions", BindingFlags.NonPublic | BindingFlags.Instance);
			
			parameterArguments = (Hashtable) fi1.GetValue(this.Arguments);
			extensionArguments = (Hashtable) fi2.GetValue(this.Arguments);
		}

		public XsltArgumentList Arguments { get; private set; }

		public DateTime? LastModified { get; private set; }

		public ReadOnlyCollection<string> Dependencies { get; private set; }

		/// <summary>
		/// Gets the XML output method of this XSLT transform.
		/// </summary>
		public XmlOutputMethod OutputMethod
		{
			get
			{
				return this.OutputSettings.OutputMethod;
			}
		}

		/// <summary>
		/// Gets the XML output settings of this XSLT transform.
		/// </summary>
		public abstract XmlWriterSettings OutputSettings 
		{
			get;
		}

		public static XsltTransform Create(SageContext context, string stylesheetPath)
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(stylesheetPath));

			object cachedItem;
			string key = string.Format(CacheKeyFormat, stylesheetPath);
			if ((cachedItem = context.Cache.Get(key)) != null && cachedItem is XsltTransform)
				return (XsltTransform) cachedItem;

			CacheableXmlDocument stylesheetDocument = ResourceManager.LoadXmlDocument(stylesheetPath, context);
			ExcludeNamespacesPrefixResults(stylesheetDocument);

			XsltTransform result = XsltTransform.Create(context, stylesheetDocument);
			result.dependencies.AddRange(stylesheetDocument.Dependencies);

			IEnumerable<string> fileDependencies = result.Dependencies.Where(d => new Uri(d).Scheme == "file");
			result.LastModified = Util.GetDateLastModified(fileDependencies);

			context.Cache.Insert(key, result, new CacheDependency(fileDependencies.ToArray()));

			return result;
		}

		public static XsltTransform Create(SageContext context, IXPathNavigable stylesheetMarkup)
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(stylesheetMarkup != null);

			XsltTransform result = new MsXsltTransform(context, stylesheetMarkup);
			foreach (string key in extensions.Keys)
			{
				result.Arguments.AddExtensionObject(key, extensions[key]);
			}

			return result;
		}

		public abstract void Transform(XmlNode inputXml, TextWriter outputWriter, SageContext context, Dictionary<string, object> arguments = null);

		public abstract void Transform(XmlNode inputXml, XmlWriter outputWriter, SageContext context, Dictionary<string, object> arguments = null);

		internal static void ExcludeNamespacesPrefixResults(CacheableXmlDocument document)
		{
			List<string> prefixes = new List<string>();
			foreach (XmlAttribute attribute in document.DocumentElement.Attributes)
			{
				if (attribute.Name.StartsWith("xmlns:"))
					prefixes.Add(attribute.Name.Substring(6));
			}

			document.DocumentElement.SetAttribute("exclude-result-prefixes", string.Join(" ", prefixes.ToArray()));
		}

		protected XsltArgumentList GetArguments(Dictionary<string, object> arguments)
		{
			if (arguments == null || arguments.Count == 0)
				return this.Arguments;

			XsltArgumentList result = new XsltArgumentList();
			foreach (string key in extensionArguments.Keys)
				result.AddExtensionObject(key, extensionArguments[key]);

			foreach (string key in parameterArguments.Keys)
				if (!arguments.ContainsKey(key))
					result.AddParam(key, string.Empty, parameterArguments[key]);

			foreach (string key in arguments.Keys)
				result.AddParam(key, string.Empty, arguments[key]);

			return result;
		}
	}
}
