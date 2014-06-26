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
namespace Sage.DevTools.Controllers
{
	using System;
	using System.Web.Mvc;

	using Sage.Controllers;
	using Sage.Views;

	/// <summary>
	/// Implements the controller that server the log file views.
	/// </summary>
	public class DevController : SageController
	{
		/// <summary>
		/// Services requests for the log file view.
		/// </summary>
		/// <param name="threadID">The thread ID.</param>
		/// <returns>
		/// The result of processing this view.
		/// </returns>
		public ActionResult Log(long? threadID)
		{
			if (!Context.IsDeveloperRequest)
				return PageNotFound();

			return this.SageView("log");
		}

		[Cacheable(Seconds = 0)]
		public ActionResult Inspect(string path)
		{
			if (!Context.IsDeveloperRequest)
				return PageNotFound();

			return this.SageView("inspect");
		}
	}
}
