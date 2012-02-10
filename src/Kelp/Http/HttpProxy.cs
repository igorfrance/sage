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
