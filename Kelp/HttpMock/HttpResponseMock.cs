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

		/// <inheritdoc/>
		public override TextWriter Output
		{
			get
			{
				return sw;
			}
		}

		/// <inheritdoc/>
		public override int StatusCode { get; set; }

		/// <inheritdoc/>
		public override string Status { get; set; }

		/// <inheritdoc/>
		public override string StatusDescription { get; set; }

		/// <inheritdoc/>
		public override string ContentType { get; set; }

		/// <inheritdoc/>
		public override Encoding ContentEncoding { get; set; }

		/// <inheritdoc/>
		public override NameValueCollection Headers
		{
			get 
			{
				return headers;
			}
		}

		/// <inheritdoc/>
		public override HttpCachePolicyBase Cache
		{
			get
			{
				return cache;
			}
		}

		/// <inheritdoc/>
		public override void Write(string s)
		{
			sw.Write(s);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return sb.ToString();
		}

		/// <inheritdoc/>
		public override void AddHeader(string name, string value)
		{
			headers.Add(name, value);
		}
	}
}
