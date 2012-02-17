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
namespace Sage
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Xml;

	using Kelp.Core.Extensions;
	using Kelp.Extensions;
	using log4net;
	using Sage.Views;

	/// <summary>
	/// Implements an exception that can be XSLT transformed to HTML.
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
		/// Gets the path of the XSLT stylesheet that renderw this exception.
		/// </summary>
		public virtual string StylesheetPath
		{
			get
			{
				return DefaultStylesheet;
			}
		}

		/// <summary>
		/// Gets the actual exception that occured.
		/// </summary>
		public virtual Exception Exception { get; private set; }

		/// <summary>
		/// Renders the exception to the specified <paramref name="writer"/>
		/// </summary>
		/// <param name="context">The context under which this code is executing.</param>
		/// <param name="writer">The writer to render the exception to.</param>
		public virtual void Render(SageContext context, TextWriter writer)
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

			XsltTransform processor = XsltTransform.Create(context, StylesheetPath);
			XmlWriter xmlwr = XmlWriter.Create(writer, processor.OutputSettings);

			processor.Transform(documentElement, xmlwr, context, GetTransformArguments(context));
		}

		protected virtual Dictionary<string, object> GetTransformArguments(SageContext context)
		{
			Dictionary<string, object> arguments = new Dictionary<string, object>();
			arguments.Add("developer", context.IsDeveloperRequest ? 1 : 0);
			return arguments;
		}
	}
}
