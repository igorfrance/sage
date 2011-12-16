namespace Sage.Xml
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Xml;
	using System.Xml.XPath;
	using System.Xml.Xsl;

	using Kelp.Core.Extensions;
	using log4net;

	using Sage.ResourceManagement;

	/// <summary>
	/// Represents a XSLT template.
	/// </summary>
	public class XsltTemplate
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(XsltTemplate).FullName);

		public XsltTemplate(XslCompiledTransform transform)
		{
			this.Processor = transform;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XsltTemplate"/> class, using the specified <see cref="IXPathNavigable"/> that contains the XSLT markup.
		/// </summary>
		/// <param name="context">The current context under which this transform is being created.</param>
		/// <param name="viewMarkup">The <see cref="IXPathNavigable"/> that contains the XSLT markup.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="viewMarkup"/> is <c>null</c>
		/// </exception>
		public XsltTemplate(SageContext context, IXPathNavigable viewMarkup)
		{
			if (viewMarkup == null)
				throw new ArgumentNullException("viewMarkup");

			this.Processor = new XslCompiledTransform();
			this.Processor.Load(viewMarkup, XsltSettings.TrustedXslt, new UrlResolver(context));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XsltTemplate"/> class, using the specified string to load the XSLT style sheet.
		/// </summary>
		/// <param name="context">The current context under which this transform is being created.</param>
		/// <param name="viewPath">The view path.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="viewPath"/> is <c>null</c>
		/// </exception>
		public XsltTemplate(SageContext context, string viewPath)
		{
			if (string.IsNullOrEmpty(viewPath))
				throw new ArgumentNullException("viewPath");

			this.Processor = XsltRegistry.Load(viewPath, context).Processor;
		}

		/// <summary>
		/// Gets the XML output method of this XSLT template.
		/// </summary>
		internal XmlOutputMethod OutputMethod
		{
			get
			{
				return this.OutputSettings.OutputMethod;
			}
		}

		/// <summary>
		/// Gets the XML outputsettings of this XSLT template
		/// </summary>
		internal XmlWriterSettings OutputSettings
		{
			get
			{
				return this.Processor.OutputSettings;
			}
		}

		private XslCompiledTransform Processor { get; set; }

		/// <summary>
		/// Transforms the specified <see cref="XmlNode"/> to the specified <see cref="TextWriter"/>.
		/// </summary>
		/// <param name="inputXml">The <see cref="XmlNode"/> that contains the XML to transform.</param>
		/// <param name="outputWriter">The <see cref="TextWriter"/> that receives the result of the transformation.</param>
		/// <param name="context">The current context under which this transform is being called.</param>
		public void Transform(XmlNode inputXml, TextWriter outputWriter, SageContext context)
		{
			this.Transform(inputXml, outputWriter, null, context);
		}

		/// <summary>
		/// Transforms the specified <see cref="XmlNode"/> to the specified <see cref="TextWriter"/>.
		/// </summary>
		/// <param name="inputXml">The <see cref="XmlNode"/> that contains the XML to transform.</param>
		/// <param name="outputWriter">The <see cref="TextWriter"/> that receives the result of the transformation.</param>
		/// <param name="arguments">The <see cref="XsltArgumentList"/> to pass on to the transformation.</param>
		/// <param name="context">The current context under which this transform is being called.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="inputXml"/> or <paramref name="outputWriter"/> or <paramref name="context"/> is null.</exception>
		public void Transform(XmlNode inputXml, TextWriter outputWriter, XsltArgumentList arguments, SageContext context)
		{
			if (inputXml == null)
				throw new ArgumentNullException("inputXml");
			if (outputWriter == null)
				throw new ArgumentNullException("outputWriter");
			if (context == null)
				throw new ArgumentNullException("context");

			Stopwatch sw = new Stopwatch();
			long milliseconds = sw.TimeMilliseconds(delegate
			{
				XmlWriter xmlWriter = XmlWriter.Create(outputWriter, this.OutputSettings);
				XmlWriter output = new XHtmlXmlWriter(xmlWriter);
				XmlNodeReader reader = new XmlNodeReader(inputXml);

				UrlResolver resolver = new UrlResolver(context);
				this.Processor.Transform(reader, arguments, output, resolver);

				reader.Close();
				output.Close();
				xmlWriter.Close();
			});

			log.DebugFormat("Transform completed in {0}ms", milliseconds);
		}
	}
}
