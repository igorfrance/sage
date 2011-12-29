namespace Sage
{
	using System;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.Xsl;

	using Kelp.Core.Extensions;
	using log4net;
	using Sage.ResourceManagement;

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

			CacheableXslTransform stylesheet = ResourceManager.LoadXslStylesheet(StylesheetPath);
			XmlWriter xmlwr = XmlWriter.Create(writer, stylesheet.Processor.OutputSettings);

			XsltArgumentList arguments = GetTransformArguments(context);
			stylesheet.Processor.Transform(documentElement, arguments, xmlwr);
		}

		protected virtual XsltArgumentList GetTransformArguments(SageContext context)
		{
			XsltArgumentList arguments = new XsltArgumentList();
			arguments.AddParam("developer", string.Empty, context.IsDeveloperRequest ? 1 : 0);
			return arguments;
		}
	}
}