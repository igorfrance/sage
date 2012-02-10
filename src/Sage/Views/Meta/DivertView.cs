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
	using System.IO;
	using System.Web.Mvc;

	using Sage.Configuration;

	/// <summary>
	/// Causes the original view to use this view's <see cref="XsltTransform"/> instead.
	/// </summary>
	public class DivertView : MetaView
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DivertView"/> class.
		/// </summary>
		/// <param name="viewInfo">The object that contains definition of this meta view.</param>
		/// <param name="wrapped">The wrapped original view.</param>
		public DivertView(MetaViewInfo viewInfo, IView wrapped)
			: base(viewInfo, wrapped)
		{
			this.Processor = viewInfo.Processor;
		}

		/// <summary>
		/// Gets the template associated with this meta view.
		/// </summary>
		public XsltTransform Processor { get; private set; }

		/// <summary>
		/// Renders the specified view context by using the specified the writer object.
		/// </summary>
		/// <param name="viewContext">The view context.</param>
		/// <param name="writer">The writer object.</param>
		public override void Render(ViewContext viewContext, TextWriter writer)
		{
			if (this.View is XsltView)
			{
				XsltView view = (XsltView) this.View;

				viewContext.HttpContext.Response.ContentType = this.Info.ContentType;
				view.Transform(viewContext, writer, this.Processor);

				this.DisableCaching(viewContext);
			}
			else
			{
				base.Render(viewContext, writer);
			}
		}
	}
}
