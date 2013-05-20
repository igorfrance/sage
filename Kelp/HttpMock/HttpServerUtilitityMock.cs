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
