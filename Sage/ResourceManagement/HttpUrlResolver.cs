namespace Sage.ResourceManagement
{
	using System;
	using System.IO;
	using System.Net;
	using System.Xml;

	/// <summary>
	/// Implements an <see cref="IUrlResolver"/> that can handles <c>http://</c> scheme.
	/// </summary>
	public class HttpUrlResolver : IUrlResolver
	{
		/// <summary>
		/// Defines the scheme (<c>http://</c>) of this resolver.
		/// </summary>
		public const string Scheme = "http://";

		private readonly string accept;
		private readonly string acceptLanguage;
		private readonly int timeout = 10;

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpUrlResolver"/> class.
		/// </summary>
		/// <param name="accept">The http accept header value</param>
		/// <param name="acceptLanguage">The http accept-language header value</param>
		/// <param name="timeout">The time (in seconds) to wait before cancelling the request (default is 10).</param>
		public HttpUrlResolver(string accept = null, string acceptLanguage = null, int timeout = 0)
		{
			this.accept = accept;
			this.acceptLanguage = acceptLanguage;

			if (timeout > 0)
				this.timeout = timeout;
		}

		/// <inheritdoc/>
		public EntityResult GetEntity(UrlResolver parent, SageContext context, string uri)
		{
			var request = WebRequest.Create(uri);
			request.Timeout = timeout;

			if (!string.IsNullOrWhiteSpace(accept))
				request.Headers.Add(HttpRequestHeader.AcceptEncoding, accept);

			if (!string.IsNullOrWhiteSpace(acceptLanguage))
				request.Headers.Add(HttpRequestHeader.AcceptLanguage, acceptLanguage);

			var result = new EntityResult();
			try
			{
				WebResponse response = request.GetResponse();
				result.Entity = new StreamReader(response.GetResponseStream());
			}
			catch (Exception ex)
			{
				XmlDocument document = new XmlDocument();
				document.InnerXml = string.Format("<error uri=\"{0}\">{1}</error>", uri, ex.Message);
				result.Entity = new XmlNodeReader(document);
			}

			return result;
		}
	}
}
