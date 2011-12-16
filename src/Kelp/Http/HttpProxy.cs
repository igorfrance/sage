namespace Kelp.Http
{
	using System;
	using System.Net;
	using System.Web;

	/// <summary>
	/// Provides a class that makes an HTTP request and returns <see cref="HttpResult"/> instances.
	/// </summary>
	public class HttpProxy
	{
		/// <summary>
		/// Fetches the specified URL, and returns a <see cref="HttpResult"/> initialized around the result.
		/// </summary>
		/// <param name="url">The URL to fetch.</param>
		/// <returns>A <see cref="HttpResult"/> initialized around the result of fetching the URL</returns>
		public static HttpResult Fetch(string url)
		{
			return Fetch(url, null);
		}

		/// <summary>
		/// Fetches the specified URL, and returns a <see cref="HttpResult"/> initialized around the result.
		/// </summary>
		/// <param name="url">The URL to fetch.</param>
		/// <param name="proxifyUrl">If not <c>null</c> and the response returned HTML or CSS, the
		/// response text will be further processed to make sure any external references will be converted
		/// to the specified proxify URL.</param>
		/// <returns>
		/// A <see cref="HttpResult"/> initialized around the result of fetching the URL
		/// </returns>
		public static HttpResult Fetch(string url, string proxifyUrl)
		{
			if (string.IsNullOrEmpty(url))
				throw new ArgumentNullException("url");

			WebRequest request = WebRequest.Create(HttpUtility.UrlDecode(url));
			HttpWebResponse response = (HttpWebResponse) request.GetResponse();

			var result = new HttpResult(response);
			if (!string.IsNullOrEmpty(proxifyUrl))
			{
				result.ProxifyResponse(proxifyUrl);
			}

			return result;
		}
	}
}
