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
namespace Sage.Controllers
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Web.Mvc;
	using System.Web.Routing;

	using Sage.Views;

	/// <summary>
	/// Implements a generic controller that serves page requests for views that don't have their own controller.
	/// </summary>
	/// <remarks>
	/// Most of the times, a Sage view will not require a specific controller, since all that is needed is for
	/// the framework to transform the view's configuration file with the appropriate XSLT style sheet. If a view
	/// has it's own configuration file, this controller will process it and transform it with the appropriate 
	/// XSLT style sheet.
	/// </remarks>
	public class GenericController : SageController
	{
		private ViewInfo genericViewInfo;
		private string name;

		/// <summary>
		/// Gets the name of this controller.
		/// </summary>
		public override string Name
		{
			get
			{
				if (this.name == null)
					return SageController.DefaultController;

				return this.name;
			}
		}

		/// <summary>
		/// Processes the index view.
		/// </summary>
		/// <returns>The <see cref="ActionResult"/> of processing this view.</returns>
		[Cacheable]
		public ActionResult Action()
		{
			if (this.genericViewInfo.Exists)
			{
				ViewInput result = this.ProcessView(this.genericViewInfo);
				return this.View(this.genericViewInfo.Action, result);
			}

			return this.PageNotFound(this.genericViewInfo.Action);
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
			return this.genericViewInfo.LastModified;
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
			string testPath = string.Concat(Context.Path.ViewPath, childPath.TrimStart('/'));

			var pathParts = childPath.TrimStart('/').Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			if (pathParts.Length >= 1)
			{
				this.name = pathParts[0];
			}

			if (pathParts.Length >= 2)
			{
				action = string.Join("/", pathParts.Skip(1).ToArray());

				if (Directory.Exists(Context.MapPath(testPath)))
					action = string.Join("/", action, SageController.DefaultAction);
			}

			this.genericViewInfo = this.GetViewInfo(action);
		}
	}
}
