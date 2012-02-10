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
	using System.Web;

	/// <summary>
	/// Provides a class for mocking an <code>HttpCachePolicyBase</code>. 
	/// </summary>
	public class HttpCachePolicyMock : HttpCachePolicyBase
	{
		private HttpCacheability cacheability;
		private DateTime lastModified;
		private string etag;

		/// <summary>
		/// Sets the Cache-Control header to the specified <see cref="HttpCacheability"/> value.
		/// </summary>
		/// <param name="cacheability">The <see cref="HttpCacheability"/> enumeration value to set the 
		/// header to.</param>
		public override void SetCacheability(HttpCacheability cacheability)
		{
			this.cacheability = cacheability;
		}

		/// <summary>
		/// Sets the last modified date.
		/// </summary>
		/// <param name="lastModified">The last modification date.</param>
		public override void SetLastModified(DateTime lastModified)
		{
			this.lastModified = lastModified;
		}

		/// <summary>
		/// Sets the ETag HTTP header to the specified string.
		/// </summary>
		/// <param name="etag">The text to use for the ETag header.</param>
		public override void SetETag(string etag)
		{
			this.etag = etag;
		}
	}
}
