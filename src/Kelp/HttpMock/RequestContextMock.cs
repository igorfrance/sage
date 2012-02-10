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
namespace Kelp.HttpMock
{
	using System;
	using System.Web;
	using System.Web.Routing;

	/// <summary>
	/// Mocks aRequestContext, enabling testing and independent execution of web context dependent code.
	/// </summary>
	public class RequestContextMock : RequestContext
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RequestContextMock"/> class, using the specified HTTP context.
		/// </summary>
		/// <param name="httpContext">An object containing information about the HTTP request.</param>
		public RequestContextMock(HttpContextBase httpContext)
			: this(httpContext, new RouteData())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RequestContextMock"/> class, using the specified HTTP context and route data.
		/// </summary>
		/// <param name="httpContext">An object containing information about the HTTP request.</param>
		/// <param name="routeData">An object containing information about the route that matched the current request.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="httpContext"/> or <paramref name="routeData"/> is null.
		/// </exception>
		public RequestContextMock(HttpContextBase httpContext, RouteData routeData)
			: base(httpContext, routeData)
		{
		}
	}
}
