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
	using System.Collections.Specialized;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Net;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Web;

	/// <summary>
	/// Represents the result from requesting a file over HTTP.
	/// </summary>
	public class HttpResult
	{
		private static readonly Regex correctHtmlPaths =
			new Regex(@"(src|href|action)\s*=\s*(""|')([^""]+)\2", RegexOptions.IgnoreCase);

		private static readonly Regex correctCssPaths =
			new Regex(@"url\s*\(\s*(""|')?(.*?)\1?\)", RegexOptions.IgnoreCase);

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpResult"/> class, using the specified
		/// <paramref name="response"/>.
		/// </summary>
		/// <param name="response">The response recieved from requesting a file over HTTP.</param>
		public HttpResult(HttpWebResponse response)
		{
			Contract.Requires<ArgumentNullException>(response != null);

			this.ContentType = (response.ContentType.IndexOf(";") == -1
				? response.ContentType
				: response.ContentType.Substring(0, response.ContentType.IndexOf(";"))).ToLower();

			this.Charset = response.CharacterSet;
			this.Encoding = response.ContentEncoding;
			this.Length = response.ContentLength;
			this.Uri = response.ResponseUri;
			this.ResponseHeaders = new NameValueCollection(response.Headers);

			this.StatusCode = response.StatusCode;
			this.StatusDescription = response.StatusDescription;

			if (this.ContentType.IndexOf("text") == 0 || this.ContentType.IndexOf("application") == 0)
				IsTextResponse = true;

			if (this.IsTextResponse && this.ContentType.IndexOf("html") != -1)
				IsHtmlResponse = true;

			ReadResponse(response);

			response.Close();
		}

		/// <summary>
		/// Gets a value indicating whether this instance represents a text response.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance represents a text response; otherwise, <c>false</c>.
		/// </value>
		public bool IsTextResponse { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this instance represents an HTML text response.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance represents an HTML text response; otherwise, <c>false</c>.
		/// </value>
		public bool IsHtmlResponse { get; private set; }

		/// <summary>
		/// Gets the content type of this instance.
		/// </summary>
		public string ContentType { get; private set; }

		/// <summary>
		/// Gets the charset of this instance.
		/// </summary>
		public string Charset { get; private set; }

		/// <summary>
		/// Gets the encoding of this instance.
		/// </summary>
		public string Encoding { get; private set; }

		/// <summary>
		/// Gets the length (in bytes) of this instance.
		/// </summary>
		public long Length { get; private set; }

		/// <summary>
		/// Gets the text of this instance (if the result is a text result).
		/// </summary>
		public string Text { get; private set; }

		/// <summary>
		/// Gets the binary data of this instance (if the result is a binary result).
		/// </summary>
		public byte[] Data { get; private set; }

		/// <summary>
		/// Gets the Uri of the Internet resource that responded to this request.
		/// </summary>
		public Uri Uri { get; private set; }

		/// <summary>
		/// Gets the headers that are associated with this response from the server.
		/// </summary>
		public NameValueCollection ResponseHeaders { get; private set; }

		/// <summary>
		/// Gets the response status of this result
		/// </summary>
		public HttpStatusCode StatusCode { get; private set; }

		/// <summary>
		/// Gets the response status description of this result.
		/// </summary>
		public string StatusDescription { get; private set; }

		internal void ProxifyResponse(string proxifyUrl)
		{
			if (this.IsTextResponse)
			{
				var server = this.Uri.GetLeftPart(UriPartial.Authority);
				var baseHref = this.Uri.ToString().Substring(0, this.Uri.ToString().LastIndexOf("/") + 1);

				this.Text = correctHtmlPaths.Replace(this.Text, delegate(Match m)
				{
					string attrib = m.Groups[1].Value;
					string url = m.Groups[3].Value;

					if (url.StartsWith("/"))
						url = server + url;
					else if (!url.Contains("://"))
						url = baseHref + url;

					return string.Format("{0}=\"{1}\"", attrib, string.Format(proxifyUrl, HttpUtility.UrlEncode(url)));
				});

				this.Text = correctCssPaths.Replace(this.Text, delegate(Match m)
				{
					string url = m.Groups[2].Value;

					if (url.StartsWith("/"))
						url = server + url;
					else if (!url.Contains("://"))
						url = baseHref + url;

					return string.Format("url({0})", string.Format(proxifyUrl, HttpUtility.UrlEncode(url)));
				});
			}
		}

		private void ReadResponse(WebResponse response)
		{
			using (Stream ds = response.GetResponseStream())
			{
				MemoryStream memoryStream = new MemoryStream(0x10000);
				byte[] buffer = new byte[0x1000];
				int bytes;

				while ((bytes = ds.Read(buffer, 0, buffer.Length)) > 0)
				{
					memoryStream.Write(buffer, 0, bytes);
				}

				this.Data = memoryStream.ToArray();

				memoryStream.Position = 0;
				using (StreamReader sr = new StreamReader(memoryStream))
				{
					StringBuilder sb = new StringBuilder();
					try
					{
						while (!sr.EndOfStream)
						{
							sb.Append((char) sr.Read());
						}
					}
					catch (IOException)
					{
					}

					this.Text = sb.ToString();
				}

				memoryStream.Close();
				memoryStream.Dispose();
			}
		}
	}
}
