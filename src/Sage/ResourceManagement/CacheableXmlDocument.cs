﻿/**
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
	using System.Collections.ObjectModel;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Text;
	using System.Xml;

	using Kelp.Extensions;
	using Kelp.Http;
	using log4net;
	using Sage.Properties;

	/// <summary>
	/// Extends the <see cref="XmlDocument"/> with an advanced loading facilities and additional properties 
	/// that provide the last modification date and a list of files that the document depends on.
	/// </summary>
	public class CacheableXmlDocument : XmlDocument
	{
		private const int MaxIncludeDepth = 10;
		private static readonly ILog log = LogManager.GetLogger(typeof(CacheableXmlDocument).FullName);

		private static readonly XmlReaderSettings readerSettings = new XmlReaderSettings
		{
			IgnoreComments = true,
			CloseInput = true,
			DtdProcessing = DtdProcessing.Ignore
		};

		private readonly List<string> dependencies;
		private DateTime? lastModified;

		/// <summary>
		/// Initializes a new instance of the <see cref="CacheableXmlDocument"/> class.
		/// </summary>
		public CacheableXmlDocument()
		{
			this.dependencies = new List<string>();
			this.Dependencies = this.dependencies.AsReadOnly();
		}

		/// <summary>
		/// Gets the last modification date associated with this document.
		/// </summary>
		/// <remarks>
		/// The last modification date will be the latests modification date of all constituent files referenced with
		/// <see cref="Dependencies"/>.
		/// </remarks>
		public DateTime LastModified
		{
			get
			{
				if (this.lastModified == null)
				{
					this.lastModified = Util.GetDateLastModified(this.Dependencies);
				}

				return this.lastModified.Value;
			}
		}

		/// <summary>
		/// Gets the list of files that this document consists of / depends on.
		/// </summary>
		public ReadOnlyCollection<string> Dependencies { get; private set; }

		/// <summary>
		/// Creates <see cref="XmlReaderSettings"/> and assings it the specified <paramref name="resolver"/>.
		/// </summary>
		/// <param name="resolver">The resolver to use with this settings..</param>
		/// <returns>An <see cref="XmlReaderSettings"/> instance, with common options set, and with the specified
		/// <paramref name="resolver"/> assigned to it.</returns>
		public static XmlReaderSettings CreateReaderSettings(XmlUrlResolver resolver)
		{
			XmlReaderSettings settings = readerSettings.Clone();
			settings.XmlResolver = resolver;
			settings.DtdProcessing = DtdProcessing.Ignore;
			settings.IgnoreComments = false;

			return settings;
		}

		/// <summary>
		/// Adds the specified <paramref name="dependencies"/>.
		/// </summary>
		/// <param name="dependencies">The dependencies to add.</param>
		public void AddDependencies(IEnumerable<string> dependencies)
		{
			Contract.Requires<ArgumentNullException>(dependencies != null);

			this.AddDependencies(dependencies.ToArray());
		}

		/// <summary>
		/// Adds the specified <paramref name="dependencies"/>.
		/// </summary>
		/// <param name="dependencies">The dependencies to add.</param>
		public void AddDependencies(params string[] dependencies)
		{
			this.dependencies.AddRange(dependencies.Where(d => 
				!this.Dependencies.Contains(d) && UrlResolver.GetScheme(d) == "file"));
		}

		/// <summary>
		/// Loads the XML document from the specified URL, with support for <c>XIncludes</c>.
		/// </summary>
		/// <param name="filename">URL for the file containing the XML document to load. The URL can be either a local file or an HTTP URL (a Web address).</param>
		/// <param name="context">The context under which the code is being invoked.</param>
		/// <param name="resolver">Optional URL resolver to use instead of the default.</param>
		public void Load(string filename, SageContext context, UrlResolver resolver = null)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(filename));

			this.LoadInternal(filename, context, resolver);
		}

		private void LoadInternal(string filename, SageContext context, UrlResolver resolver = null, bool processIncludes = true)
		{
			if (resolver == null)
				resolver = new UrlResolver(context);

			XmlReader xmlReader;
			XmlReaderSettings settings = CreateReaderSettings(resolver);

			Uri uri = new Uri(filename, UriKind.RelativeOrAbsolute);
			IDisposable reader = resolver.GetEntity(uri, null, null) as IDisposable;
			if (reader is XmlReader)
				xmlReader = (XmlReader) reader;
			else if (reader is Stream)
				xmlReader = XmlReader.Create((Stream) reader, settings, filename);
			else if (reader is TextReader)
				xmlReader = XmlReader.Create((TextReader) reader, settings, filename);
			else
				xmlReader = XmlReader.Create(filename, settings);

			this.AddDependencies(filename);
			using (xmlReader)
			{
				try
				{
					base.Load(xmlReader);
					this.AddDependencies(resolver.Dependencies);
				}
				finally
				{
					if (xmlReader.ReadState != ReadState.Closed)
						xmlReader.Close();
				}
			}

			if (processIncludes)
				this.ProcessIncludes(this.DocumentElement, context);
		}

		private void ProcessIncludes(XmlNode contextElement, SageContext context)
		{
			var state = new IncludeProcessingState(context);
			if (!string.IsNullOrWhiteSpace(this.BaseURI))
			{
				var cacheEntry = state.VisitedNodes[this.BaseURI] = new Dictionary<string, XmlNode>();
				cacheEntry["/"] = this.DocumentElement;
			}

			this.ProcessIncludes(contextElement, state);
		}

		private void ProcessIncludes(XmlNode contentElement, IncludeProcessingState state)
		{
			var nm = XmlNamespaces.Manager.MergeWith(new XmlNamespaceManager(contentElement.OwnerDocument.NameTable));
			var includeElements = contentElement.SelectNodes(".//sage:include[not(ancestor::sage:literal)]", XmlNamespaces.Manager);
			
			bool debugMode = false;
			if (state.Context != null)
				debugMode = state.Context.ProjectConfiguration.IsDebugMode;

			XmlDocument elementDocument = contentElement.OwnerDocument;
			foreach (XmlElement includingElement in includeElements)
			{
				XmlNode includedNode = this.ProcessInclude(nm, includingElement, state);
				XmlNode elementParent = includingElement.ParentNode;

				if (state.IsError)
				{
					if (state.IncludeStack.Count == 0)
					{
						log.ErrorFormat(state.ErrorMessage);
						if (debugMode)
						{
							var debugElement = this.CreateErrorElement(state.ErrorMessage);
							elementParent.ReplaceChild(elementDocument.ImportNode(debugElement, true), includingElement);
						}
					}
					else
						elementParent.RemoveChild(includingElement);
				}
				else
				{
					if (includedNode != null)
						elementParent.ReplaceChild(elementDocument.ImportNode(includedNode, true), includingElement);
					else
						elementParent.RemoveChild(includingElement);
				}
			}
		}

		private XmlNode ProcessInclude(XmlNamespaceManager nm, XmlElement element, IncludeProcessingState state)
		{
			XmlNode includedNode = null;

			string href = element.GetAttribute("href");
			string xpath = Kelp.Util.GetParameterValue(element.GetAttribute("xpath"), "/");

			if (string.IsNullOrWhiteSpace(href) && string.IsNullOrWhiteSpace(xpath))
			{
				state.ErrorMessage = Messages.IncludeElementMissingRequiredAttributes;
				return null;
			}

			var includeId = href;
			if (string.IsNullOrWhiteSpace(href))
				includeId = xpath;
			if (!string.IsNullOrWhiteSpace(href) && !string.IsNullOrWhiteSpace(xpath))
				includeId = string.Format("{0}[{1}]", href, xpath);

			if (state.IncludeStack.Count >= MaxIncludeDepth)
			{
				state.ErrorMessage = string.Format(Messages.MaxIncludeDepthExceeded, includeId, MaxIncludeDepth);
				return null;
			}

			string parse = Kelp.Util.GetParameterValue(element.GetAttribute("parse"), "xml", "^text|xml$");
			string encoding = Kelp.Util.GetParameterValue(element.GetAttribute("encoding"), "ascii", "^utf-8|ascii$");

			var directory = string.Empty;
			var includePath = href;

			if (!string.IsNullOrWhiteSpace(this.BaseURI))
			{
				if (string.IsNullOrWhiteSpace(href))
					includePath = this.BaseURI;
						 
				directory = Path.GetDirectoryName(this.BaseURI);
			}

			if (!Path.IsPathRooted(includePath) && !string.IsNullOrWhiteSpace(directory))
				includePath = Path.Combine(directory, includePath);

			IDictionary<string, XmlNode> cacheEntry;
			if (state.VisitedNodes.ContainsKey(includePath))
			{
				cacheEntry = state.VisitedNodes[includePath];
				if (cacheEntry.ContainsKey(xpath))
				{
					if (state.IncludeStack.Contains(includeId))
					{
						state.ErrorMessage = string.Format(Messages.IncludeRecursionError, includeId);
						return null;
					}
				}
			}
			else
			{
				cacheEntry = state.VisitedNodes[includePath] = new Dictionary<string, XmlNode>();
			}

			if (cacheEntry.ContainsKey(xpath))
			{
				includedNode = cacheEntry[xpath];
			}
			else
			{
				try
				{
					if (string.IsNullOrWhiteSpace(href))
					{
						includedNode = this.ResolveIntraDocumentInclude(nm, parse, element, xpath);
						if (includedNode.Contains(element))
						{
							// intra-document circular-reference inclusion
							state.ErrorMessage = string.Format(Messages.IncludeRecursionError, includeId);
							return null;
						}
					}
					else
					{
						includedNode = this.ResolveExtraDocumentInclude(nm, parse, includePath, xpath, encoding, state.Context);
					}

					if (includedNode != null)
					{
						var xmlElement = includedNode as XmlElement;
						if (xmlElement != null)
						{
							state.IncludeStack.Add(includeId);
							this.ProcessIncludes(xmlElement, state);
							state.IncludeStack.RemoveAt(state.IncludeStack.Count - 1);
						}
					}
				}
				catch (Exception ex)
				{
					string target = string.IsNullOrWhiteSpace(href) ? xpath : href;
					state.ErrorMessage = string.Format("Error fetching '{0}': {1}", target, ex.Message);
				}
			}

			return includedNode;
		}

		private XmlNode ResolveIntraDocumentInclude(XmlNamespaceManager nm, string parse, XmlElement element, string xpath)
		{
			XmlNode result = element.SelectSingleNode(xpath, nm);
			if (result == null)
				return this.CreateErrorElement(string.Format(Messages.XpathTargetNotFound, xpath));

			if (parse == "text")
				result = this.CreateTextNode(result.InnerText);

			if (result.NodeType == XmlNodeType.Document)
				result = ((XmlDocument) result).DocumentElement;

			return result;
		}

		private XmlNode ResolveExtraDocumentInclude(XmlNamespaceManager nm, string parse, string includePath, string xpath, string encoding, SageContext context)
		{
			XmlNode result;
			if (parse == "text")
			{
				WebRequest request = WebRequest.Create(includePath);
				WebResponse response = request.GetResponse();
				using (Stream stream = response.GetResponseStream())
				{
					Encoding enc = Encoding.GetEncoding(encoding);
					string text = new StreamReader(stream, enc).ReadToEnd();
					result = this.CreateTextNode(text);
				}
			}
			else
			{
				UrlResolver resolver = new UrlResolver(context);
				resolver.RegisterResolver("http", new HttpUrlResolver());

				CacheableXmlDocument temp = new CacheableXmlDocument();
				temp.LoadInternal(includePath, context, resolver, false);

				result = temp.DocumentElement;
				if (!string.IsNullOrWhiteSpace(xpath))
				{
					result = temp.SelectSingleNode(xpath, nm);
				}
			}

			if (result != null && result.NodeType == XmlNodeType.Document)
				result = ((XmlDocument) result).DocumentElement;

			return result;
		}

		private XmlElement CreateErrorElement(string errorText)
		{
			XmlElement result = this.CreateElement("sage:error", XmlNamespaces.SageNamespace);
			result.InnerText = errorText;

			return result;
		}

		internal class FatalIncludeException : Exception
		{
			public FatalIncludeException(string message, Exception inner)
				: base(message, inner)
			{
			}
		}
	
		private class IncludeProcessingState
		{
			private string errorMessage;

			public IncludeProcessingState(SageContext context)
			{
				this.Context = context;
				this.VisitedNodes = new Dictionary<string, IDictionary<string, XmlNode>>();
				this.IncludeStack = new List<string>();
			}

			public IDictionary<string, IDictionary<string, XmlNode>> VisitedNodes { get; private set; }

			public bool IsError { get; set; }

			public string ErrorMessage
			{
				get
				{
					return this.errorMessage;
				}

				set
				{
					this.IsError = true;
					this.errorMessage = value;
				}
			}

			public List<string> IncludeStack { get; private set; }

			public SageContext Context { get; private set; }
		}
	}
}
