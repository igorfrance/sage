namespace Sage.Views
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Text;
	using System.Xml;
	using System.Xml.XPath;
	using System.Xml.Xsl;

	using Sage.ResourceManagement;

	using Saxon.Api;

	/// <summary>
	/// Implements a Saxon XSLT transform.
	/// </summary>
	public class SaxonXsltTransform : XsltTransform
	{
		private Processor processor;

		/// <summary>
		/// Initializes a new instance of the <see cref="SaxonXsltTransform"/> class.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="stylesheetMarkup">The stylesheet markup.</param>
		/// <exception cref="SageHelpException"></exception>
		public SaxonXsltTransform(SageContext context, XmlDocument stylesheetMarkup)
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(stylesheetMarkup != null);

			UrlResolver resolver = new UrlResolver(context);

			processor = new Processor();

			XdmNode input = processor.NewDocumentBuilder().Build(stylesheetMarkup);
			XsltTransformer transformer = processor.NewXsltCompiler().Compile(XmlReader.Create(stylesheetMarkup.OuterXml)).Load();

			try
			{
				//this.processor.Load(stylesheetMarkup, XsltSettings.TrustedXslt, resolver);
				dependencies.AddRange(resolver.Dependencies);
			}
			catch //(Exception ex)
			{
				//ProblemInfo problem = this.DetectProblemType(ex);
				//throw new SageHelpException(problem, ex);
			}
		}

		/// <inheritdoc/>
		public override XmlWriterSettings OutputSettings
		{
			get
			{
				return new XmlWriterSettings { Encoding = Encoding.UTF8 };
			}
		}

		/// <inheritdoc/>
		public override void Transform(XmlNode inputXml, TextWriter outputWriter, SageContext context, Dictionary<string, object> arguments = null)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override void Transform(XmlNode inputXml, XmlWriter outputWriter, SageContext context, Dictionary<string, object> arguments = null)
		{
			throw new NotImplementedException();
		}
	}
}
