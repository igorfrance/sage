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
