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

		/// <summary>
		/// Initializes a new instance of the <see cref="SageException"/> class.
		/// </summary>
		public SageException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SageException"/> class, using the specified <paramref name="exception"/>
		/// </summary>
		/// <param name="exception">The actual exception that was trown.</param>
		public SageException(Exception exception)
			: base(exception.Message, exception)
		{
			this.Exception = exception;
		}

		/// <summary>
		/// Gets the path of the XSLT stylesheet that renders this exception.
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
		public virtual Exception Exception { get; protected set; }

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
			XmlElement documentElement = document.AppendElement(this.Exception.ToXml(document));
			Exception inner = this.Exception.InnerException;

			while (inner != null)
			{
				documentElement.AppendChild(inner.ToXml(document));
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
			XmlElement documentElement = document.AppendElement(this.Exception.ToXml(document));
			Exception inner = this.Exception.InnerException;

			while (inner != null)
			{
				documentElement.AppendChild(inner.ToXml(document));
				inner = inner.InnerException;
			}

			documentElement.SetAttribute("date", DateTime.Now.ToString("dd-MM-yyyy"));
			documentElement.SetAttribute("time", DateTime.Now.ToString("hh:mm:ss"));

			XmlReader xslReader = this.StylesheetPath.StartsWith(EmbeddedResourceResolver.Scheme)
				? XmlReader.Create(EmbeddedResourceResolver.GetStream(this.StylesheetPath))
				: XmlReader.Create(this.StylesheetPath);

			XslCompiledTransform xslTransform = new XslCompiledTransform();
			xslTransform.Load(xslReader);

			XmlWriter xmlwr = XmlWriter.Create(writer, xslTransform.OutputSettings);
			xslTransform.Transform(document, xmlwr);
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
