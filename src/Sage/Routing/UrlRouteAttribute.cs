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
namespace Sage.Routing
{
	using System;

	/// <summary>
	/// Assigns a URL route to an MVC Controller class method.
	/// </summary>
	/// <remarks>
	/// For example:
	/// <example><code>
	/// [UrlRoute(Name = "ProductCategory", Path = "products/{category}")]
	/// public ActionResult ProductCategory(string category)
	/// {
	///   ...
	/// }
	/// </code>
	/// </example>
	/// </remarks>
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
	public class UrlRouteAttribute : Attribute
	{
		/// <summary>
		/// Optional name of the route.  Route names must be unique per route.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Path of the URL route.  This is relative to the root of the web site.
		/// Do not append a "/" prefix.  Specify empty string for the root page.
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// Optional order in which to add the route (default is 0).  Routes
		/// with lower order values will be added before those with higher.
		/// Routes that have the same order value will be added in undefined
		/// order with respect to each other.
		/// </summary>
		public int Order { get; set; }
	}
}