namespace Kelp.HttpMock
{
	using System;
	using System.Security.Principal;
	using System.Web;
	using System.Web.Caching;
	using System.Web.SessionState;

	/// <summary>
	/// Provides a class for mocking an <code>HttpContextBase</code>. 
	/// </summary>
	public class HttpContextMock : HttpContextBase
	{
		/// <summary>
		/// Gets the username associated with this instance's <see cref="PrincipalMock"/>.
		/// </summary>
		public static string DefaultIdentityName = "IUSR_BILL";

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpContextMock"/> class.
		/// </summary>
		/// <param name="url">The URL of the mocked context's request.</param>
		public HttpContextMock(string url)
			: this(url, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpContextMock"/> class.
		/// </summary>
		/// <param name="url">The URL of the mocked context's request.</param>
		/// <param name="requestType">The request type of the mocked context's request.</param>
		public HttpContextMock(string url, string requestType)
			: this(url, requestType, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpContextMock"/> class.
		/// </summary>
		/// <param name="url">The URL of the mocked context's request.</param>
		/// <param name="requestType">The request type of the mocked context's request.</param>
		/// <param name="mapPath">The function to use for mapping server paths.</param>
		public HttpContextMock(string url, string requestType, Func<string, string> mapPath)
			: this(url, null, null, null, mapPath)
		{
			this.RequestMock.requestType = requestType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpContextMock"/> class.
		/// </summary>
		/// <param name="url">The URL of the mocked context's request.</param>
		/// <param name="physicalPath">The physical path of the mocked context's request.</param>
		/// <param name="appPath">The application path of the mocked context.</param>
		/// <param name="physicalAppPath">The physical application path of the mocked context.</param>
		/// <param name="mapPath">The function to use for mapping server paths.</param>
		public HttpContextMock(string url, string physicalPath, string appPath, string physicalAppPath, 
			Func<string, string> mapPath)
		{
			this.RequestMock = new HttpRequestMock(url, physicalPath, appPath, physicalAppPath);
			this.ResponseMock = new HttpResponseMock();
			this.ApplicationMock = new HttpApplicationStateMock();
			this.ServerMock = new HttpServerUtilitityMock(mapPath ?? delegate(string input) { return input; });
			this.SessionMock = new HttpSessionStateMock(new SessionStateItemCollection());
			this.UserMock = new PrincipalMock(new IdentityMock(DefaultIdentityName), new string[] { });
			this.CacheMock = new Cache();
		}

		/// <summary>
		/// Gets the <see cref="HttpApplicationState"/> object for the current HTTP request.
		/// </summary>
		/// <returns>The application state object for the current HTTP request.</returns>
		public override HttpApplicationStateBase Application
		{
			get
			{
				return this.ApplicationMock;
			}
		}

		/// <summary>
		/// Gets the <see cref="HttpRequest"/> object for the current HTTP request.
		/// </summary>
		/// <returns>The current HTTP request.</returns>
		public override HttpRequestBase Request
		{
			get
			{
				return this.RequestMock;
			}
		}

		/// <summary>
		/// Gets the <see cref="Cache"/> object for the current application domain.
		/// </summary>
		/// <returns>The cache for the current application domain.</returns>
		public override Cache Cache
		{
			get
			{
				return this.CacheMock;
			}
		}

		/// <summary>
		/// Gets the <see cref="HttpServerUtility"/> object that provides methods that are used when Web requests are being processed.
		/// </summary>
		/// <returns>The server utility object for the current HTTP request.</returns>
		public override HttpServerUtilityBase Server
		{
			get
			{
				return this.ServerMock;
			}
		}

		/// <summary>
		/// Gets the <see cref="HttpResponse"/> object for the current HTTP response.
		/// </summary>
		/// <returns>The current HTTP response.</returns>
		public override HttpResponseBase Response
		{
			get
			{
				return this.ResponseMock;
			}
		}

		/// <summary>
		/// Gets the <see cref="HttpSessionState"/> object for the current HTTP request.
		/// </summary>
		/// <returns>The session-state object for the current HTTP request.</returns>
		public override HttpSessionStateBase Session
		{
			get
			{
				return this.SessionMock;
			}
		}

		/// <summary>
		/// Gets or sets security information for the current HTTP request.
		/// </summary>
		/// <returns>An object that contains security information for the current HTTP request.</returns>
		public override IPrincipal User
		{
			get
			{
				return this.UserMock;
			}
		}

		internal HttpRequestMock RequestMock { get; set; }

		internal HttpResponseMock ResponseMock { get; set; }

		internal HttpSessionStateMock SessionMock { get; set; }

		internal HttpServerUtilitityMock ServerMock { get; set; }

		internal HttpApplicationStateMock ApplicationMock { get; set; }

		internal PrincipalMock UserMock { get; set; }

		internal Cache CacheMock { get; set; }
	}
}