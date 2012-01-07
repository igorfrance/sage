namespace Sage.Views
{
	using System;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Web.Mvc;
	using System.Xml;

	using Sage.Controllers;

	/// <summary>
	/// Implements an XSLT based <see cref="IView"/>.
	/// </summary>
	public class XsltView : IView
	{
		private readonly XsltTransform processor;

		public XsltView(XsltTransform processor)
		{
			Contract.Requires<ArgumentNullException>(processor != null);

			this.processor = processor;
		}

		/// <summary>
		/// Renders the current view to the specified target <see cref="TextWriter"/>.
		/// </summary>
		/// <param name="viewContext">The <see cref="ViewContext"/> object that contains information required for rendering the 
		/// current view.</param>
		/// <param name="textWriter">The destination <see cref="TextWriter"/> that receives the rendered result.</param>
		public virtual void Render(ViewContext viewContext, TextWriter textWriter)
		{
			Transform(viewContext, textWriter, this.processor);
		}

		public virtual void Transform(ViewContext viewContext, TextWriter textWriter)
		{
			Contract.Requires<ArgumentNullException>(viewContext != null);
			Contract.Requires<ArgumentException>(viewContext.Controller is SageController);
			Contract.Requires<ArgumentNullException>(textWriter != null);

			Transform(viewContext, textWriter, this.processor);
		}

		public virtual void Transform(ViewContext viewContext, TextWriter textWriter, XsltTransform template)
		{
			Contract.Requires<ArgumentNullException>(viewContext != null);
			Contract.Requires<ArgumentException>(viewContext.Controller is SageController);
			Contract.Requires<ArgumentNullException>(textWriter != null);
			Contract.Requires<ArgumentNullException>(template != null);

			SageController controller = (SageController) viewContext.Controller;
			XmlDocument requestXml = controller.GetViewXml(viewContext);
			template.Transform(requestXml, textWriter, controller.Context);
		}
	}
}
