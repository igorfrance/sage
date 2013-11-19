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
	/// Provides a dynamic view of the effective configuration - current project's with extensions merged into it.
	/// </summary>
	public class ConfigView : MetaView
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigView"/> class.
		/// </summary>
		/// <param name="viewInfo">The meta view associated with this view.</param>
		/// <param name="wrapped">The actual view that this meta view wraps.</param>
		public ConfigView(MetaViewInfo viewInfo, IView wrapped)
			: base(viewInfo, wrapped)
		{
			this.Processor = viewInfo.Processor;
		}

		/// <summary>
		/// Gets the template associated with this meta view.
		/// </summary>
		public XsltTransform Processor { get; private set; }

		/// <summary>
		/// Renders the meta view of the view associated with the specified <paramref name="viewContext"/> 
		/// and <paramref name="writer"/>.
		/// </summary>
		/// <param name="viewContext">The view context.</param>
		/// <param name="writer">The writer to write to.</param>
		public override void Render(ViewContext viewContext, TextWriter writer)
		{
			if (this.View is XsltView && viewContext.Controller is SageController)
			{
				XsltView view = (XsltView) this.View;
				SageController controller = (SageController) viewContext.Controller;
				SageContext context = controller.Context;
				XmlNode configurationXml = context.ProjectConfiguration.ToXml(new XmlDocument());

				viewContext.HttpContext.Response.ContentType = this.Info.ContentType;
				view.Transform(configurationXml, writer, this.Processor, context);

				this.DisableCaching(viewContext);
			}
			else
			{
				base.Render(viewContext, writer);
			}
		}
	}
}
