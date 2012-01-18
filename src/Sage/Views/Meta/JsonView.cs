namespace Sage.Views.Meta
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Web.Mvc;
	using System.Xml;

	using Kelp.Core;
	using Kelp.Core.Extensions;

	using Sage.Configuration;
	using Sage.Controllers;

	public class JsonView : MetaView
	{
		public JsonView(MetaViewInfo viewInfo, IView wrapped)
			: base(viewInfo, wrapped)
		{
		}

		/// <summary>
		/// Renders the specified view context by using the specified the writer object.
		/// </summary>
		/// <param name="viewContext">The view context.</param>
		/// <param name="writer">The writer object.</param>
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
