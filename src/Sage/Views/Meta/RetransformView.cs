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
namespace Sage.Views.Meta
{
	using System;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Text;
	using System.Web.Mvc;
	using System.Xml;

	using Sage.Configuration;
	using Sage.Controllers;
	using Sage.ResourceManagement;

	/// <summary>
	/// Provides an additional transform of the original view result.
	/// </summary>
	public class RetransformView : MetaView
	{
		private readonly XsltTransform processor;

		/// <summary>
		/// Initializes a new instance of the <see cref="RetransformView"/> class.
		/// </summary>
		/// <param name="viewInfo">The object that contains definition of this meta view.</param>
		/// <param name="wrapped">The wrapped original view.</param>
		public RetransformView(MetaViewInfo viewInfo, IView wrapped)
			: base(viewInfo, wrapped)
		{
			this.processor = viewInfo.Processor;
		}

		/// <summary>
		/// Renders the result of the original view with an additional <see cref="XsltTransform"/>.
		/// </summary>
		/// <param name="viewContext">The view context.</param>
		/// <param name="writer">The writer to write to.</param>
		public override void Render(ViewContext viewContext, TextWriter writer)
		{
			Contract.Requires<ArgumentNullException>(viewContext != null);
			Contract.Requires<ArgumentException>(viewContext.Controller is SageController);
			Contract.Requires<ArgumentNullException>(writer != null);

			if (this.View is XsltView)
			{
				XsltView view = (XsltView) this.View;
				StringBuilder sb = new StringBuilder();

				using (StringWriter sw = new StringWriter(sb))
				{
					view.Transform(viewContext, sw);

					SageController controller = (SageController) viewContext.Controller;
					UrlResolver resolver = new UrlResolver(controller.Context);
					XmlReaderSettings settings = CacheableXmlDocument.CreateReaderSettings(resolver);
					XmlReader reader = XmlReader.Create(new StringReader(sb.ToString()), settings);

					XmlDocument viewXml = new XmlDocument();
					viewXml.Load(reader);

					processor.Transform(viewXml, writer, controller.Context);
				}

				this.DisableCaching(viewContext);
			}
			else
			{
				base.Render(viewContext, writer);
			}
		}
	}
}
