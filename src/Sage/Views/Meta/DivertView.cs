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
