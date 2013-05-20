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
