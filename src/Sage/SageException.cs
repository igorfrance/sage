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

	using Sage.Views;

	using log4net;

	public class SageException : Exception
	{
		private static readonly string stylesheetPath = "sageresx://sage/resources/xslt/error.xslt";
		private static readonly ILog log = LogManager.GetLogger(typeof(SageException).FullName);

		public SageException()
		{
		}

		public SageException(Exception exception)
			: base(exception.Message, exception)
		{
			this.Exception = exception;
		}

		public virtual string StylesheetPath
		{
			get
			{
				return stylesheetPath;
			}
		}

		public virtual Exception Exception
		{
			get;
			protected set;
		}

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
