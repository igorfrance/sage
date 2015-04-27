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
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Xml;
	using System.Xml.XPath;
	using System.Xml.Xsl;

	using Kelp.Extensions;
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
		public MsXsltTransform(SageContext context, XmlDocument stylesheetMarkup)
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(stylesheetMarkup != null);

			UrlResolver resolver = new UrlResolver(context);

			processor = new XslCompiledTransform();

			try
			{
				processor.Load(stylesheetMarkup, XsltSettings.TrustedXslt, resolver);
				dependencies.AddRange(resolver.Dependencies);
			}
			catch (Exception ex)
			{
				ProblemInfo problem = this.DetectProblemType(ex);
				throw new SageHelpException(problem, ex);
			}
		}

		/// <summary>
		/// Gets the XML output settings of this XSLT transform.
		/// </summary>
		public override XmlWriterSettings OutputSettings
		{
			get
			{
				return processor.OutputSettings;
			}
		}

		/// <inheritdoc/>
		public override void Transform(XmlNode inputXml, TextWriter outputWriter, SageContext context, Dictionary<string, object> arguments = null)
		{
			this.Transform(inputXml, XmlWriter.Create(outputWriter, this.OutputSettings), context, arguments);
		}

		/// <inheritdoc/>
		public override void Transform(XmlNode inputXml, XmlWriter outputWriter, SageContext context, Dictionary<string, object> arguments = null)
		{
			var startTime = DateTime.Now.Ticks;

			XmlWriter xmlWriter = XmlWriter.Create(outputWriter, this.OutputSettings);
			XmlWriter output = new XHtmlXmlWriter(xmlWriter);
			XmlNodeReader reader = new XmlNodeReader(inputXml);

			UrlResolver resolver = new UrlResolver(context);
			XsltArgumentList transformArgs = this.GetArguments(arguments);

			try
			{
				processor.Transform(reader, transformArgs, output, resolver);
			}
			catch (Exception ex)
			{
				ProblemInfo problem = this.DetectProblemType(ex);
				throw new SageHelpException(problem, ex);
			}
			finally
			{
				reader.Close();
				output.Close();
				xmlWriter.Close();
			}

			var ellapsed = new TimeSpan(DateTime.Now.Ticks - startTime);
			log.DebugFormat("XSLT transform completed in {0}ms", ellapsed.Milliseconds);
		}

		private ProblemInfo DetectProblemType(Exception ex)
		{
			if (ex is XmlException)
			{
				if (ex.Message.Contains("does not have a root element"))
					return new ProblemInfo(ProblemType.TransformResultMissingRootElement, this.Dependencies[0]);
			}

			if (ex.GetType().Name == "XslTransformException")
			{
				if (ex.Message.Contains("Prefix") && ex.Message.Contains("is not defined"))
					return new ProblemInfo(ProblemType.MissingNamespaceDeclaration, this.Dependencies[0]);
			}

			return new ProblemInfo(ProblemType.TransformError);
		}
	}
}
