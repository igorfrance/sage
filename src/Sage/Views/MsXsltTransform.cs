﻿/**
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
namespace Sage.Views
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Xml;
	using System.Xml.XPath;
	using System.Xml.Xsl;

	using Kelp.Core.Extensions;
	using log4net;

	using Sage.ResourceManagement;

	public class MsXsltTransform : XsltTransform
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(MsXsltTransform).FullName);
		private readonly XslCompiledTransform processor;
		private readonly XmlWriterSettings settings;

		public MsXsltTransform(SageContext context, IXPathNavigable stylesheetMarkup)
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(stylesheetMarkup != null);

			UrlResolver resolver = new UrlResolver(context);

			this.processor = new XslCompiledTransform();
			this.processor.Load(stylesheetMarkup, XsltSettings.TrustedXslt, resolver);

			this.settings = new XmlWriterSettings
			{
				Encoding = this.processor.OutputSettings.Encoding,
				Indent = this.processor.OutputSettings.Indent,
				CloseOutput = this.processor.OutputSettings.CloseOutput,
				NewLineChars = "\r\n",
				NewLineHandling = NewLineHandling.Replace,
			};

			this.dependencies.AddRange(resolver.Dependencies);
		}

		public override XmlWriterSettings OutputSettings
		{
			get
			{
				return this.settings;
			}
		}

		public override void Transform(XmlNode inputXml, TextWriter outputWriter, SageContext context, Dictionary<string, object> arguments = null)
		{
			Transform(inputXml, XmlWriter.Create(outputWriter, this.OutputSettings), context, arguments);
		}

		public override void Transform(XmlNode inputXml, XmlWriter outputWriter, SageContext context, Dictionary<string, object> arguments = null)
		{
			Contract.Requires<ArgumentNullException>(inputXml != null);
			Contract.Requires<ArgumentNullException>(outputWriter != null);
			Contract.Requires<ArgumentNullException>(context != null);

			Stopwatch sw = new Stopwatch();
			long milliseconds = sw.TimeMilliseconds(delegate
			{
				XmlWriter xmlWriter = XmlWriter.Create(outputWriter, this.OutputSettings);
				XmlWriter output = new XHtmlXmlWriter(xmlWriter);
				XmlNodeReader reader = new XmlNodeReader(inputXml);

				UrlResolver resolver = new UrlResolver(context);
				XsltArgumentList transformArgs = this.GetArguments(arguments);

				processor.Transform(reader, transformArgs, output, resolver);

				reader.Close();
				output.Close();
				xmlWriter.Close();
			});

			log.DebugFormat("Transform completed in {0}ms", milliseconds);
		}
	}
}
