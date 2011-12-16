namespace Sage.Controllers
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Web.Mvc;
	using System.Web.Routing;

	using log4net;
	using Sage.Views;

	/// <summary>
	/// Implements a generic controller that serves page requests for views that don't have their own controller.
	/// </summary>
	/// <remarks>
	/// Most of the times, a Sage view will not require a specific controller, since all that is needed is for
	/// the framework to transform the view's configuration file with the appropriate XSLT stylesheet. If a view
	/// has it's own configuration file, this controller will process it and transform it with the appropriate 
	/// XSLT stylesheet.
	/// </remarks>
	public class GenericController : SageController
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(GenericController).FullName);
		private ViewInfo genericViewInfo;
		private string controllerName;

		/// <summary>
		/// Gets the name of this controller.
		/// </summary>
		public override string ControllerName
		{
			get
			{
				if (controllerName == null)
					return SageController.DefaultController;

				return controllerName;
			}
		}

		/// <summary>
		/// Processes the index view.
		/// </summary>
		/// <returns>The <see cref="ActionResult"/> of processing this view.</returns>
		[Cacheable]
		public ActionResult Action()
		{
			if (genericViewInfo.Exists)
			{
				ViewInput result = this.ProcessView(genericViewInfo);
				return this.View(genericViewInfo.Action, result);
			}

			Context.Response.StatusCode = 404;
			return new ContentResult { Content = "File not found (A.K.A. Error 404)", ContentType = "text/html" };
		}

		internal override DateTime? GetLastModificationDate(string actionName)
		{
			return genericViewInfo.LastModified;
		}

		protected override void Initialize(RequestContext requestContext)
		{
			base.Initialize(requestContext);

			string action = SageController.DefaultAction;
			string childPath = Regex.Replace(Context.Request.Path, "^" + Context.Request.ApplicationPath, string.Empty, RegexOptions.IgnoreCase);
			string testPath = string.Concat(Context.Path.ViewPath.TrimEnd('/'), '/', childPath.TrimStart('/'));

			var pathParts = childPath.TrimStart('/').Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			if (pathParts.Length >= 1)
			{
				this.controllerName = pathParts[0];
			}

			if (pathParts.Length >= 2)
			{
				action = string.Join("/", pathParts.Skip(1).ToArray());

				if (Directory.Exists(Context.MapPath(testPath)))
					action = Path.Combine(action, SageController.DefaultAction);
			}

			genericViewInfo = this.GetViewInfo(action);
		}
	}
}
