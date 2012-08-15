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

namespace Sage.Views
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
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

	/// <summary>
	/// Provides a base class for different XSLT implementations.
	/// </summary>
	[ContractClass(typeof(XsltTransformContract))]
	public abstract class XsltTransform
	{
		/// <summary>
		/// Gets the list of files that this transform depends on (dependencies).
		/// </summary>
		protected readonly List<string> dependencies = new List<string>();
		
		private const string CacheKeyFormat = "XSLT_{0}";
		private static readonly ILog log = LogManager.GetLogger(typeof(XsltTransform).FullName);
		private static readonly Dictionary<string, object> extensions;

		private readonly Hashtable parameterArguments;
		private readonly Hashtable extensionArguments;

		static XsltTransform()
		{
			extensions = new Dictionary<string, object>();
			foreach (Assembly a in Project.RelevantAssemblies)
			{
				var types = from t in a.GetTypes() where t.IsClass && !t.IsAbstract select t;

				foreach (
					Type type in types.Where(t => t.GetCustomAttributes(typeof(XsltExtensionObjectAttribute), false).Length != 0))
				{
					try
					{
						ConstructorInfo ctor = type.GetConstructor(new Type[] { });
						object instance = ctor.Invoke(new object[] { });

						XsltExtensionObjectAttribute attrib =
							(XsltExtensionObjectAttribute)type.GetCustomAttributes(typeof(XsltExtensionObjectAttribute), false)[0];

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

			parameterArguments = (Hashtable)fi1.GetValue(this.Arguments);
			extensionArguments = (Hashtable)fi2.GetValue(this.Arguments);
		}

		/// <summary>
		/// Gets the XSLT arguments (parameters) of this transform.
		/// </summary>
		public XsltArgumentList Arguments { get; private set; }

		/// <summary>
		/// Gets the last modification date of this transform.
		/// </summary>
		public DateTime? LastModified { get; private set; }

		/// <summary>
		/// Gets the list of files that this transform depends on (dependencies).
		/// </summary>
		public ReadOnlyCollection<string> Dependencies { get; private set; }

		/// <summary>
		/// Gets the XML output method associated with this XSLT transform.
		/// </summary>
		public XmlOutputMethod OutputMethod
		{
			get
			{
				return this.OutputSettings.OutputMethod;
			}
		}

		/// <summary>
		/// Gets the XML output settings associated with this XSLT transform.
		/// </summary>
		public abstract XmlWriterSettings OutputSettings { get; }

		/// <summary>
		/// Creates an <see cref="XsltTransform"/> instance initialized with the document loaded from the 
		/// specified <paramref name="stylesheetPath"/>.
		/// </summary>
		/// <param name="context">The context under which this code is executing.</param>
		/// <param name="stylesheetPath">The path to the stylesheet.</param>
		/// <returns>
		/// An <see cref="XsltTransform"/> instance initialized with the document loaded from the 
		/// specified <paramref name="stylesheetPath"/>.
		/// </returns>
		public static XsltTransform Create(SageContext context, string stylesheetPath)
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(stylesheetPath));

			object cachedItem;
			string key = string.Format(CacheKeyFormat, stylesheetPath);
			if ((cachedItem = context.Cache.Get(key)) != null && cachedItem is XsltTransform)
			{
				return (XsltTransform)cachedItem;
			}

			CacheableXmlDocument stylesheetDocument = ResourceManager.LoadXmlDocument(stylesheetPath, context);
			OmitNamespacePrefixResults(stylesheetDocument);

			XsltTransform result = XsltTransform.Create(context, stylesheetDocument);
			result.dependencies.AddRange(stylesheetDocument.Dependencies);

			IEnumerable<string> fileDependencies = result.Dependencies.Where(d => UrlResolver.GetScheme(d) == "file").ToList();
			result.LastModified = Util.GetDateLastModified(fileDependencies);
			context.Cache.Insert(key, result, new CacheDependency(fileDependencies.ToArray()));

			return result;
		}

		/// <summary>
		/// Creates a new <see cref="XsltTransform"/>, using the specified <paramref name="context"/> and <paramref name="stylesheetMarkup"/>.
		/// </summary>
		/// <param name="context">The context under which this code is executing.</param>
		/// <param name="stylesheetMarkup">The XSLT markup for the transform to create.</param>
		/// <returns>
		/// A new <see cref="XsltTransform"/> initialized with the <paramref name="stylesheetMarkup"/>.
		/// </returns>
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

		/// <summary>
		/// Transforms the specified <paramref name="inputXml"/> into the specified <paramref name="outputWriter"/>.
		/// </summary>
		/// <param name="inputXml">The input XML to transform.</param>
		/// <param name="outputWriter">The output writer to transform to.</param>
		/// <param name="context">The current context.</param>
		/// <param name="arguments">Optional transform arguments.</param>
		public abstract void Transform(
			XmlNode inputXml, TextWriter outputWriter, SageContext context, Dictionary<string, object> arguments = null);

		/// <summary>
		/// Transforms the specified <paramref name="inputXml"/> into the specified <paramref name="outputWriter"/>.
		/// </summary>
		/// <param name="inputXml">The input XML to transform.</param>
		/// <param name="outputWriter">The output writer to transform to.</param>
		/// <param name="context">The current context.</param>
		/// <param name="arguments">Optional transform arguments.</param>
		public abstract void Transform(
			XmlNode inputXml, XmlWriter outputWriter, SageContext context, Dictionary<string, object> arguments = null);

		internal static void OmitNamespacePrefixResults(CacheableXmlDocument document)
		{
			if (document.DocumentElement == null)
			{
				return;
			}

			List<string> prefixes = new List<string>();
			foreach (XmlAttribute attribute in document.DocumentElement.Attributes)
			{
				if (attribute.Name.StartsWith("xmlns:"))
				{
					prefixes.Add(attribute.Name.Substring(6));
				}
			}

			document.DocumentElement.SetAttribute("exclude-result-prefixes", string.Join(" ", prefixes.ToArray()));
		}

		/// <summary>
		/// Converts the specified <paramref name="arguments"/> dictionary into an <see cref="XsltArgumentList"/>.
		/// </summary>
		/// <param name="arguments">The dictionary of arguments to use.</param>
		/// <returns>The XSLT arguments to use with this transform</returns>
		protected XsltArgumentList GetArguments(Dictionary<string, object> arguments)
		{
			if (arguments == null || arguments.Count == 0)
			{
				return this.Arguments;
			}

			XsltArgumentList result = new XsltArgumentList();
			foreach (string key in extensionArguments.Keys)
			{
				result.AddExtensionObject(key, extensionArguments[key]);
			}

			foreach (string key in parameterArguments.Keys)
			{
				if (!arguments.ContainsKey(key))
				{
					result.AddParam(key, string.Empty, parameterArguments[key]);
				}
			}

			foreach (string key in arguments.Keys)
			{
				result.AddParam(key, string.Empty, arguments[key]);
			}

			return result;
		}
	}

	/// <summary>
	/// Provides code contracts for <see cref="XsltTransform"/>.
	/// </summary>
	[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
	[ContractClassFor(typeof(XsltTransform))]
	public abstract class XsltTransformContract : XsltTransform
	{
		/// <inheritdoc/>
		public override void Transform(
			XmlNode inputXml, TextWriter outputWriter, SageContext context, Dictionary<string, object> arguments = null)
		{
			Contract.Requires<ArgumentNullException>(inputXml != null);
			Contract.Requires<ArgumentNullException>(inputXml != null);
			Contract.Requires<ArgumentNullException>(context != null);
		}

		/// <inheritdoc/>
		public override void Transform(
			XmlNode inputXml, XmlWriter outputWriter, SageContext context, Dictionary<string, object> arguments = null)
		{
			Contract.Requires<ArgumentNullException>(inputXml != null);
			Contract.Requires<ArgumentNullException>(inputXml != null);
			Contract.Requires<ArgumentNullException>(context != null);
		}
	}
}