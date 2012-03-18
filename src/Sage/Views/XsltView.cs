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
