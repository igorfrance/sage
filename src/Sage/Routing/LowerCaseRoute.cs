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
	using System.Web.Routing;

	/// <summary>
	/// Represents a URL route registered with lower case.
	/// </summary>
	public class LowerCaseRoute : Route
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LowerCaseRoute"/> class, using the specified URL pattern and handler class.
		/// </summary>
		/// <param name="url">The URL pattern for the route.</param>
		/// <param name="routeHandler">The object that processes requests for the route.</param>
		public LowerCaseRoute(string url, IRouteHandler routeHandler)
			: base(url, routeHandler)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LowerCaseRoute"/> class, using the specified URL pattern, handler class, and default parameter values.
		/// </summary>
		/// <param name="url">The URL pattern for the route.</param>
		/// <param name="defaults">The values to use if the URL does not contain all the parameters.</param>
		/// <param name="routeHandler">The object that processes requests for the route.</param>
		public LowerCaseRoute(string url, RouteValueDictionary defaults, IRouteHandler routeHandler)
			: base(url, defaults, routeHandler)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LowerCaseRoute"/> class, using the specified URL pattern, handler class, default parameter values, and constraints.
		/// </summary>
		/// <param name="url">The URL pattern for the route.</param>
		/// <param name="defaults">The values to use if the URL does not contain all the parameters.</param>
		/// <param name="constraints">A regular expression that specifies valid values for a URL parameter.</param>
		/// <param name="routeHandler">The object that processes requests for the route.</param>
		public LowerCaseRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, IRouteHandler routeHandler)
			: base(url, defaults, constraints, routeHandler)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LowerCaseRoute"/> class, using the specified URL pattern, handler class, default parameter values, constraints, and custom values.
		/// </summary>
		/// <param name="url">The URL pattern for the route.</param>
		/// <param name="defaults">The values to use if the URL does not contain all the parameters.</param>
		/// <param name="constraints">A regular expression that specifies valid values for a URL parameter.</param>
		/// <param name="dataTokens">Custom values that are passed to the route handler, but which are not used to determine whether the route matches a specific URL pattern. The route handler might need these values to process the request.</param>
		/// <param name="routeHandler">The object that processes requests for the route.</param>
		public LowerCaseRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens, IRouteHandler routeHandler)
			: base(url, defaults, constraints, dataTokens, routeHandler)
		{
		}

		/// <summary>
		/// Gets or sets the name or this route.
		/// </summary>
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		/// Returns information about the URL that is associated with the route.
		/// </summary>
		/// <param name="requestContext">An object that encapsulates information about the requested route.</param>
		/// <param name="values">An object that contains the parameters for a route.</param>
		/// <returns>
		/// An object that contains information about the URL that is associated with the route.
		/// </returns>
		public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
		{
			VirtualPathData path = base.GetVirtualPath(requestContext, values);

			if (path != null)
				path.VirtualPath = path.VirtualPath.ToLowerInvariant();

			return path;
		}
	}
}
