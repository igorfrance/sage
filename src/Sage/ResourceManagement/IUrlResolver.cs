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
namespace Sage.ResourceManagement
{
	using System.Xml;

	/// <summary>
	/// Defines an interface similar to <see cref="XmlUrlResolver"/>, adding <see cref="SageContext"/> as an argument.
	/// </summary>
	public interface IUrlResolver
	{
		/// <summary>
		/// Gets an <see cref="EntityResult"/> that represents the actual resource mapped from the specified <paramref name="uri"/>.
		/// </summary>
		/// <param name="parent">The <see cref="UrlResolver"/> that owns this resolved and calls this method.</param>
		/// <param name="context">The current <see cref="SageContext"/> under which this code is executing.</param>
		/// <param name="uri">The uri to resolve.</param>
		/// <returns>An object that represents the resource mapped from the specified <paramref name="uri"/>.</returns>
		EntityResult GetEntity(UrlResolver parent, SageContext context, string uri);
	}
}
