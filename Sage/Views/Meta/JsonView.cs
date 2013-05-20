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
