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

		public MsXsltTransform(SageContext context, IXPathNavigable stylesheetMarkup)
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(stylesheetMarkup != null);

			UrlResolver resolver = new UrlResolver(context);

			this.processor = new XslCompiledTransform();
			this.processor.Load(stylesheetMarkup, XsltSettings.TrustedXslt, resolver);

			this.dependencies.AddRange(resolver.Dependencies);
		}

		public override XmlWriterSettings OutputSettings
		{
			get
			{
				return this.processor.OutputSettings;
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
