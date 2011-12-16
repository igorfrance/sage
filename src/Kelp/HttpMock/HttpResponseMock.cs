namespace Kelp.HttpMock
{
	using System;
	using System.Collections.Specialized;
	using System.IO;
	using System.Text;
	using System.Web;

	/// <summary>
	/// Provides a class for mocking an <code>HttpResponseBase</code>. 
	/// </summary>
	public class HttpResponseMock : HttpResponseBase
	{
		private readonly StringBuilder sb;
		private readonly StringWriter sw;

		private readonly NameValueCollection headers;
		private readonly HttpCachePolicyBase cache;

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpResponseMock"/> class.
		/// </summary>
		public HttpResponseMock()
		{
			sb = new StringBuilder();
			sw = new StringWriter(sb);
			headers = new NameValueCollection();
			cache = new HttpCachePolicyMock();
		}

		/// <summary>
		/// Gets the object that enables text output to the HTTP response stream.
		/// </summary>
		/// <returns>An object that enables output to the client.</returns>
		public override TextWriter Output
		{
			get
			{
				return sw;
			}
		}

		/// <summary>
		/// Gets or sets the HTTP status code of the output that is returned to the client.
		/// </summary>
		/// <returns>The status code of the HTTP output that is returned to the client. For information about 
		/// valid status codes, see HTTP Status Codes on the MSDN Web site.</returns>
		public override int StatusCode { get; set; }

		/// <summary>
		/// Gets or sets the Status value that is returned to the client.
		/// </summary>
		/// <returns>The status of the HTTP output. For information about valid status codes, see HTTP Status Codes 
		/// on the MSDN Web site.</returns>
		public override string Status { get; set; }

		/// <summary>
		/// Gets or sets the HTTP status message of the output that is returned to the client.
		/// </summary>
		/// <returns>The status message of the HTTP output that is returned to the client. For information about 
		/// valid status codes, see HTTP Status Codes on the MSDN Web site.</returns>
		public override string StatusDescription { get; set; }

		/// <summary>
		/// Gets or sets the HTTP MIME type of the current response.
		/// </summary>
		/// <returns>The HTTP MIME type of the current response. </returns>
		public override string ContentType { get; set; }

		/// <summary>
		/// Gets or sets the content encoding of the current response.
		/// </summary>
		/// <returns>Information about the content encoding of the current response.</returns>
		public override Encoding ContentEncoding { get; set; }

		/// <summary>
		/// Gets the collection of response headers.
		/// </summary>
		/// <returns>The response headers.</returns>
		public override NameValueCollection Headers
		{
			get 
			{
				return headers;
			}
		}

		/// <summary>
		/// Gets the caching policy (such as expiration time, privacy settings, and vary clauses) of the current 
		/// Web page.
		/// </summary>
		/// <returns>The caching policy of the current response.</returns>
		public override HttpCachePolicyBase Cache
		{
			get
			{
				return cache;
			}
		}

		/// <summary>
		/// Writes the specified string to the HTTP response output stream.
		/// </summary>
		/// <param name="s">The string to write to the HTTP output stream.</param>
		public override void Write(string s)
		{
			sw.Write(s);
		}

		/// <summary>
		/// Returns a <see cref="String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return sb.ToString();
		}

		/// <summary>
		/// Adds an HTTP header to the current response. This method is provided for compatibility with earlier versions of ASP.
		/// </summary>
		/// <param name="name">The name of the HTTP header to add <paramref name="value"/> to.</param>
		/// <param name="value">The string to add to the header.</param>
		public override void AddHeader(string name, string value)
		{
			headers.Add(name, value);
		}
	}
}
