namespace Sage.Test
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Web.Mvc;

	using Sage.Controllers;

	internal class MockController : SageController
	{
		public ViewResult Index()
		{
			return View("index");
		}

		public ViewResult List()
		{
			return View("list");
		}
	}
}
