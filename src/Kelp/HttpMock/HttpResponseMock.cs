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
