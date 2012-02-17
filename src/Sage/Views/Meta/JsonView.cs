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
	using System.Xml;

	using Kelp;
	using Kelp.Extensions;

	using Sage.Configuration;
	using Sage.Controllers;

	/// <summary>
	/// Provides a JSON meta view of a view's input data.
	/// </summary>
	public class JsonView : MetaView
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="JsonView"/> class.
		/// </summary>
		/// <param name="viewInfo">The meta view associated with this view.</param>
		/// <param name="wrapped">The actual view that this meta view wraps.</param>
		public JsonView(MetaViewInfo viewInfo, IView wrapped)
			: base(viewInfo, wrapped)
		{
		}

		/// <summary>
		/// Renders the meta view of the view associated with the specified <paramref name="viewContext"/> 
		/// and <paramref name="writer"/>.
		/// </summary>
		/// <param name="viewContext">The view context.</param>
		/// <param name="writer">The writer to write to.</param>
		public override void Render(ViewContext viewContext, TextWriter writer)
		{
			SageController controller = (SageController) viewContext.Controller;
			XmlDocument requestXml = controller.PrepareViewXml(viewContext);

			QueryString query = new QueryString(viewContext.HttpContext.Request.QueryString);
			bool prettyPrint = query.HasValid("pretty", "1|true|yes");
			viewContext.HttpContext.Response.ContentType = this.Info.ContentType;
			writer.Write(requestXml.DocumentElement.ToJson(prettyPrint));
		}
	}
}
