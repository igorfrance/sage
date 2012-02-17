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
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Text;
	using System.Web.Mvc;
	using System.Xml;

	using Sage.Controllers;

	/// <summary>
	/// Implements an XSLT based <see cref="IView"/>.
	/// </summary>
	public class XsltView : IView
	{
		private readonly XsltTransform processor;

        /// <summary>
        /// Initializes a new instance of the <see cref="XsltView"/> class.
        /// </summary>
        /// <param name="processor">The processor.</param>
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

        /// <summary>
        /// Transforms the specified view context.
        /// </summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="textWriter">The text writer.</param>
		public virtual void Transform(ViewContext viewContext, TextWriter textWriter)
		{
			Contract.Requires<ArgumentNullException>(viewContext != null);
			Contract.Requires<ArgumentException>(viewContext.Controller is SageController);
			Contract.Requires<ArgumentNullException>(textWriter != null);

			Transform(viewContext, textWriter, this.processor);
		}

        /// <summary>
        /// Transforms the specified view context.
        /// </summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="textWriter">The text writer.</param>
        /// <param name="template">The template.</param>
		public virtual void Transform(ViewContext viewContext, TextWriter textWriter, XsltTransform template)
		{
			Contract.Requires<ArgumentNullException>(viewContext != null);
			Contract.Requires<ArgumentException>(viewContext.Controller is SageController);
			Contract.Requires<ArgumentNullException>(textWriter != null);
			Contract.Requires<ArgumentNullException>(template != null);
			
			SageController controller = (SageController) viewContext.Controller;
			XmlDocument requestXml = controller.PrepareViewXml(viewContext);

			template.Transform(requestXml, textWriter, controller.Context);
		}
	}
}
