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

		public ActionResult Inspect(string path)
		{
			if (!Context.IsDeveloperRequest)
				return PageNotFound();

			return this.SageView("inspect");
		}
	}
}
