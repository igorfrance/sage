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
	/// Provides a class for mocking an <code>HttpServerUtilityBase</code>. 
	/// </summary>
	public class HttpServerUtilitityMock : HttpServerUtilityBase
	{
		private readonly Func<string, string> pathMapper;

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpServerUtilitityMock"/> class.
		/// </summary>
		public HttpServerUtilitityMock()
		{
			pathMapper = delegate(string input) { return input; };
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpServerUtilitityMock"/> class.
		/// </summary>
		/// <param name="pathMapper">The function to use when calling <code>Server.MapPath</code>.</param>
		public HttpServerUtilitityMock(Func<string, string> pathMapper)
		{
			this.pathMapper = pathMapper;
		}

		/// <summary>
		/// Returns the physical file path that corresponds to the specified virtual path on the Web server.
		/// </summary>
		/// <param name="path">The virtual path to get the physical path for.</param>
		/// <returns>
		/// The physical file path that corresponds to <paramref name="path"/>.
		/// </returns>
		public override string MapPath(string path)
		{
			return pathMapper.Invoke(path);
		}
	}
}
