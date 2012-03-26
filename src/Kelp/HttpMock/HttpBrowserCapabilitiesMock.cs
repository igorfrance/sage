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
	/// Mocks an <see cref="HttpBrowserCapabilities"/>, enabling testing and independent execution of web context 
	/// dependent code.
	/// </summary>
	public class HttpBrowserCapabilitiesMock : HttpBrowserCapabilitiesBase
	{
		/// <inheritdoc/>
		public override string Type
		{
			get
			{
				return "Firefox10";
			}
		}

		/// <inheritdoc/>
		public override bool Crawler
		{
			get
			{
				return false;
			}
		}
	}
}
