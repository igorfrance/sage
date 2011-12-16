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
