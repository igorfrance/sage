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

	/// <summary>
	/// Provides a wrapper around <see cref="XslCompiledTransform"/>.
	/// </summary>
	public class MsXsltTransform : XsltTransform
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(MsXsltTransform).FullName);
		private readonly XslCompiledTransform processor;

		/// <summary>
		/// Initializes a new instance of the <see cref="MsXsltTransform"/> class, using the specified 
		/// <paramref name="stylesheetMarkup"/>.
		/// </summary>
		/// <param name="context">The current context.</param>
		/// <param name="stylesheetMarkup">The markup to initialize the transform with.</param>
		public MsXsltTransform(SageContext context, IXPathNavigable stylesheetMarkup)
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(stylesheetMarkup != null);

			UrlResolver resolver = new UrlResolver(context);

			this.processor = new XslCompiledTransform();
			this.processor.Load(stylesheetMarkup, XsltSettings.TrustedXslt, resolver);
			this.dependencies.AddRange(resolver.Dependencies);
		}

		/// <summary>
		/// Gets the XML output settings of this XSLT transform.
		/// </summary>
		public override XmlWriterSettings OutputSettings
		{
			get
			{
				return this.processor.OutputSettings;
			}
		}

		/// <inheritdoc/>
		public override void Transform(XmlNode inputXml, TextWriter outputWriter, SageContext context, Dictionary<string, object> arguments = null)
		{
			Transform(inputXml, XmlWriter.Create(outputWriter, this.OutputSettings), context, arguments);
		}

		/// <inheritdoc/>
		public override void Transform(XmlNode inputXml, XmlWriter outputWriter, SageContext context, Dictionary<string, object> arguments = null)
		{
			Stopwatch sw = new Stopwatch();
			long milliseconds = sw.TimeMilliseconds(delegate
			{
				XmlWriter xmlWriter = XmlWriter.Create(outputWriter, this.OutputSettings);
				XmlWriter output = new XHtmlXmlWriter(xmlWriter);
				XmlNodeReader reader = new XmlNodeReader(inputXml);

				UrlResolver resolver = new UrlResolver(context);
				XsltArgumentList transformArgs = this.GetArguments(arguments);

				try
				{
					processor.Transform(reader, transformArgs, output, resolver);

					reader.Close();
					output.Close();
					xmlWriter.Close();
				}
				catch (XmlException ex)
				{
					ProblemType problemType = DetectProblemType(ex);
					throw new SageHelpException(problemType, ex);
				}
			});

			log.DebugFormat("Transform completed in {0}ms", milliseconds);
		}

		private ProblemType DetectProblemType(Exception ex)
		{
			if (ex.Message.Contains("does not have a root element"))
				return ProblemType.TransformResultMissingRootElement;

			return ProblemType.TransformError;
		}
	}
}
