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
namespace Kelp.Http
{
	using System;
	using System.Diagnostics.Contracts;
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
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(url));

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
