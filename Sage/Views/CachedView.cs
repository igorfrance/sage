namespace Sage.Views
{
	using System;
	using System.IO;
	using System.Web.Mvc;

	internal class CachedView : IView
	{
		private readonly string content;

		internal CachedView(string content)
		{
			this.content = content;
		}

		public void Render(ViewContext viewContext, TextWriter writer)
		{
			writer.Write(content);
		}
	}
}
