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
namespace Sage
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.Xsl;

	using Kelp.Extensions;

	using Sage.ResourceManagement;
	using Sage.Views;

	/// <summary>
	/// Implements an exception that can be converted to XML and through XSLT transformed to HTML.
	/// </summary>
	public class SageException : Exception
	{
		private const string DefaultStylesheet = "sageresx://sage/resources/xslt/error.xslt";
		private Exception actual;

		/// <summary>
		/// Initializes a new instance of the <see cref="SageException"/> class.
		/// </summary>
		public SageException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SageException"/> class, using the specified <paramref name="exception"/>
		/// </summary>
		/// <param name="exception">The actual exception that was thrown.</param>
		public SageException(Exception exception)
			: base(exception.Message, exception)
		{
			this.actual = exception;
		}

		/// <summary>
		/// Gets the path of the XSLT style sheet that renders this exception.
		/// </summary>
		public virtual string StylesheetPath
		{
			get
			{
				return DefaultStylesheet;
			}
		}

		/// <summary>
		/// Gets or sets the actual exception that occurred.
		/// </summary>
		public virtual Exception Exception
		{
			get
			{
				return actual ?? this;
			}

			protected set
			{
				this.actual = value;
			}
		}

		/// <summary>
		/// Renders the exception to the specified <paramref name="writer"/>
		/// </summary>
		/// <param name="writer">The writer to render the exception to.</param>
		/// <param name="context">The context under which this code is executing.</param>
		public virtual void Render(TextWriter writer, SageContext context)
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(writer != null);

			XmlDocument document = new XmlDocument();
			XmlElement documentElement = document.AppendElement(this.ConvertToXml(this.Exception, document));
			Exception inner = this.Exception.InnerException;

			while (inner != null)
			{
				documentElement.AppendChild(this.ConvertToXml(inner, document));
				inner = inner.InnerException;
			}

			documentElement.AppendChild(context.ToXml(document));
			documentElement.SetAttribute("date", DateTime.Now.ToString("dd-MM-yyyy"));
			documentElement.SetAttribute("time", DateTime.Now.ToString("hh:mm:ss"));

			XsltTransform processor = XsltTransform.Create(context, this.StylesheetPath);
			XmlWriter xmlwr = XmlWriter.Create(writer, processor.OutputSettings);

			processor.Transform(documentElement, xmlwr, context, this.GetTransformArguments(context));
		}

		/// <summary>
		/// Renders the exception to the specified <paramref name="writer"/>
		/// </summary>
		/// <param name="writer">The writer to render the exception to.</param>
		public virtual void RenderWithoutContext(TextWriter writer)
		{
			Contract.Requires<ArgumentNullException>(writer != null);

			XmlDocument document = new XmlDocument();
			XmlElement documentElement = document.AppendElement(this.ConvertToXml(this.Exception, document));
			Exception inner = this.Exception.InnerException;

			while (inner != null)
			{
				documentElement.AppendChild(this.ConvertToXml(inner, document));
				inner = inner.InnerException;
			}

			documentElement.SetAttribute("date", DateTime.Now.ToString("dd-MM-yyyy"));
			documentElement.SetAttribute("time", DateTime.Now.ToString("hh:mm:ss"));

			XmlReader xslReader = this.StylesheetPath.StartsWith(EmbeddedResourceResolver.Scheme)
				? XmlReader.Create(EmbeddedResourceResolver.GetStream(this.StylesheetPath), null, this.StylesheetPath)
				: XmlReader.Create(this.StylesheetPath);

			XslCompiledTransform xslTransform = new XslCompiledTransform();
			xslTransform.Load(xslReader, new XsltSettings(true, true), new UrlResolver());

			XmlWriter xmlwr = XmlWriter.Create(writer, xslTransform.OutputSettings);
			xslTransform.Transform(document, xmlwr);
		}

		/// <summary>
		/// Saves the current <paramref name="instance" /> as an <see cref="XmlElement" />, using the specified
		/// <paramref name="ownerDocument" /> to create the element.
		/// </summary>
		/// <param name="instance">The exception instance.</param>
		/// <param name="ownerDocument">The document to use to create the element.</param>
		/// <param name="problemInfo">The problem info associated with the exception.</param>
		/// <returns>An XML representation of the current exception.</returns>
		protected internal virtual XmlElement ConvertToXml(Exception instance, XmlDocument ownerDocument, ProblemInfo problemInfo = null)
		{
			XmlElement exceptionElement = ownerDocument.CreateElement("exception");
			exceptionElement.SetAttribute("type", instance.GetType().ToString());
			exceptionElement.SetAttribute("message", instance.Message);
			exceptionElement.SetAttribute("htmlDescription", instance.Message
				.Replace("<", "&lt;")
				.Replace(">", "&gt;")
				.Replace("\t", "&#160;&#160;&#160;&#160;")
				.Replace("\n", "<br/>"));

			XmlElement straceNode = (XmlElement) exceptionElement.AppendChild(ownerDocument.CreateElement("stacktrace"));
			string[] stackTrace = instance.StackTrace != null
				? instance.StackTrace.Split(new[] { '\n' })
				: new[] { string.Empty };

			if (instance.GetType() == typeof(XmlException))
			{
				exceptionElement.SetAttribute("sourceuri", ((XmlException) instance).SourceUri);
				exceptionElement.SetAttribute("linenumber", ((XmlException) instance).LineNumber.ToString());
				exceptionElement.SetAttribute("lineposition", ((XmlException) instance).LinePosition.ToString());
			}

			foreach (string t in stackTrace)
			{
				XmlElement frameNode = (XmlElement) straceNode.AppendChild(ownerDocument.CreateElement("frame"));
				Match match;
				if ((match = Regex.Match(t, "^\\s*at (.*) in (.*):line (\\d+)[\\s\\r]*$")).Success)
				{
					frameNode.SetAttribute("text", match.Groups[1].Value);
					frameNode.SetAttribute("file", match.Groups[2].Value);
					frameNode.SetAttribute("line", match.Groups[3].Value);
				}
				else
				{
					frameNode.SetAttribute("text", Regex.Replace(t, "^\\s*at ", string.Empty));
				}
			}

			if (problemInfo != null && problemInfo.InfoBlocks.Count != 0)
			{
				foreach (KeyValuePair<string, IDictionary<string, string>> infoBlock in problemInfo.InfoBlocks)
				{
					XmlElement blockElement = exceptionElement.AppendElement("infoblock");
					blockElement.SetAttribute("name", infoBlock.Key);

					foreach (string key in infoBlock.Value.Keys)
					{
						string value = infoBlock.Value[key];
						XmlElement lineElement = blockElement.AppendElement("line");
						if (key != value)
							lineElement.SetAttribute("name", key);

						lineElement.InnerText = value;
					}
				}
			}

			return exceptionElement;
		}

		/// <summary>
		/// Gets the XSLT arguments to use with the transform.
		/// </summary>
		/// <param name="context">The current context.</param>
		/// <returns>The XSLT arguments to use with this transform</returns>
		protected virtual Dictionary<string, object> GetTransformArguments(SageContext context)
		{
			Dictionary<string, object> arguments = new Dictionary<string, object>();
			arguments.Add("developer", context.IsDeveloperRequest ? 1 : 0);
			return arguments;
		}
	}
}
