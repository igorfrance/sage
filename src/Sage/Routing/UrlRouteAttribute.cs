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
		/// Gets or sets the name of the route.
		/// </summary>
		/// <remarks>
		/// This value is optional, but route names must be unique.
		/// </remarks>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the path of the URL route.
		/// </summary>
		/// <remarks>
		/// This is relative to the root of the web site.
		/// Do not append a "/" prefix. Specify empty string for the root page.
		/// </remarks>
		public string Path { get; set; }

		/// <summary>
		/// Gets or sets the order in which to add the route (default is 0).
		/// </summary>
		/// <remarks>
		/// Routes with lower order values will be added before those with higher.
		/// Routes that have the same order value will be added in undefined
		/// order with respect to each other.
		/// </remarks>
		public int Order { get; set; }

		public override string ToString()
		{
			return string.Format("{0} ({1})", this.Name, this.Path);
		}
	}
}