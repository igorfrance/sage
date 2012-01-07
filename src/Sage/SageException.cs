namespace Sage
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Xml;

	using Kelp.Core.Extensions;

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
