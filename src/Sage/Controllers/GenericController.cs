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
			Context.Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
			Context.Response.Cache.SetNoStore();
			return new ContentResult { Content = "File not found (A.K.A. Error 404)", ContentType = "text/html" };
		}

        /// <summary>
        /// Gets the last modification date for the specified <paramref name="viewName"/>.
        /// </summary>
        /// <param name="viewName">The name of the action for which to retrieve the last modification date.</param>
        /// <returns>
        /// The last modified date for the specified <paramref name="viewName"/>.
        /// </returns>
		public override DateTime? GetLastModificationDate(string viewName)
		{
			return genericViewInfo.LastModified;
		}

        /// <summary>
        /// Initializes the controller.
        /// </summary>
        /// <param name="requestContext">The request context.</param>
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
