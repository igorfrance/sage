namespace Sage
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.Xsl;

	using Kelp.Core.Extensions;
	using log4net;
	using Mvp.Xml.Common.Xsl;
	using Sage.ResourceManagement;

	/// <summary>
	/// Handles exceptions by displaying information about it, and optionally sending a notification e-mail.
	/// </summary>
	/// <remarks>
	/// Depending on wether the user under which the exception occurred, different levels of information will be shown.
	/// The information shown is controlled by the XSLT that will render HTML for the xml document created for the exception.
	/// </remarks>
	public class ApplicationExceptionHandler
	{
		private const string XsltPath = "sageresx://sage/resources/xslt/error.xslt";
		private static readonly ILog log = LogManager.GetLogger(typeof(ApplicationExceptionHandler).FullName);

		private readonly SageContext context;
		private readonly Exception exception;

		/// <summary>
		/// Initializes a new instance of the <see cref="ApplicationExceptionHandler"/> class, using the specified
		/// <paramref name="context"/> and <paramref name="exception"/>.
		/// </summary>
		/// <param name="context">The current context.</param>
		/// <param name="exception">The exception that occurred.</param>
		public ApplicationExceptionHandler(SageContext context, Exception exception)
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(exception != null);

			this.context = context;
			this.exception = exception;
		}

		/// <summary>
		/// Handles the application exception.
		/// </summary>
		/// <param name="writeToResponse">If set to <c>true</c>, this method will write the applicable html to the response.</param>
		/// <returns>An <see cref="XmlDocument"/> that contains the information about the exception that happened.</returns>
		public XmlDocument HandleException(bool writeToResponse)
		{
			// A Response.Redirect(...) call generates a ThreadAbortException, we ignore them.
			if (exception is System.Threading.ThreadAbortException)
				return null;

			log.Fatal(exception.Message, exception);

			XmlDocument exceptionInfo = GenerateErrorDocument();
			string publicHtml = GenerateErrorHtml(exceptionInfo, true);
			string developerHtml = GenerateErrorHtml(exceptionInfo, true);

			if (writeToResponse)
			{
				string errorHtml = context.IsDeveloperRequest ? developerHtml : publicHtml;
				context.Response.Write(errorHtml);
				context.Response.StatusCode = 500;
			}

			return exceptionInfo;
		}

		/// <summary>
		/// Generates the error HTML.
		/// </summary>
		/// <param name="exceptionInfo">The <see cref="XmlDocument"/> that contains the information about the exception.</param>
		/// <param name="isDeveloper">This value is sent to XSLT as an argument and controls the level of detail about the error
		/// that the transformation will emit.</param>
		/// <returns>An HTML string that contains information about the exception</returns>
		private static string GenerateErrorHtml(XmlDocument exceptionInfo, bool isDeveloper)
		{
			CacheableXslTransform stylesheet = ResourceManager.LoadXslStylesheet(XsltPath);
			StringBuilder result = new StringBuilder();
			XmlWriter writer = XmlWriter.Create(result, stylesheet.Processor.OutputSettings);

			XsltArgumentList arguments = new XsltArgumentList();
			arguments.AddParam("developer", string.Empty, isDeveloper ? 1 : 0);
			stylesheet.Processor.Transform(exceptionInfo, arguments, writer);

			return result.ToString();
		}

		private XmlDocument GenerateErrorDocument()
		{
			XmlDocument document = new XmlDocument();
			XmlElement exceptionElement = document.AppendElement(GenerateErrorElement(document, exception));
			Exception inner = exception.InnerException;
			while (inner != null)
			{
				exceptionElement.AppendChild(GenerateErrorElement(document, inner));
				inner = inner.InnerException;
			}

			exceptionElement.AppendChild(context.CreateRequestElement(document));
			exceptionElement.SetAttribute("date", DateTime.Now.ToString("dd-MM-yyyy"));
			exceptionElement.SetAttribute("time", DateTime.Now.ToString("hh:mm:ss"));

			return document;
		}

		private XmlElement GenerateErrorElement(XmlDocument document, Exception error)
		{
			XmlElement exceptionElement = document.CreateElement("exception");
			exceptionElement.SetAttribute("type", error.GetType().ToString());
			exceptionElement.SetAttribute("description", error.Message);
			exceptionElement.SetAttribute("htmlDescription", error.Message
				.Replace("<", "&lt;")
				.Replace(">", "&gt;")
				.Replace("\t", "&#160;&#160;&#160;&#160;")
				.Replace("\n", "<br/>"));

			XmlElement straceNode = (XmlElement)exceptionElement.AppendChild(document.CreateElement("stacktrace"));
			string[] stackTrace = error.StackTrace != null
			                      	? error.StackTrace.Split(new[] { '\n' })
				: new[] { string.Empty };

			if (error.GetType() == typeof(XmlException))
			{
				exceptionElement.SetAttribute("sourceuri", ((XmlException)error).SourceUri);
				exceptionElement.SetAttribute("linenumber", ((XmlException)error).LineNumber.ToString());
				exceptionElement.SetAttribute("lineposition", ((XmlException)error).LinePosition.ToString());
			}

			XmlElement frameNode;
			Match match;
			for (int i = 0; i < stackTrace.Length; i++)
			{
				frameNode = (XmlElement)straceNode.AppendChild(document.CreateElement("frame"));
				if ((match = Regex.Match(stackTrace[i], "^\\s*at (.*) in (.*):line (\\d+)[\\s\\r]*$")).Success)
				{
					frameNode.SetAttribute("text", match.Groups[1].Value);
					frameNode.SetAttribute("file", match.Groups[2].Value);
					frameNode.SetAttribute("line", match.Groups[3].Value);
				}
				else
				{
					frameNode.SetAttribute("text", Regex.Replace(stackTrace[i], "^\\s*at ", string.Empty));
				}
			}

			return exceptionElement;
		}
	}
}