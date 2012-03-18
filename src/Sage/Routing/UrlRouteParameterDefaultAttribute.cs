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
namespace Sage.Routing
{
	using System;

	/// <summary>
	/// Assigns a default value to a route parameter in a <see cref="UrlRouteAttribute"/>.
	/// if not specified in the URL.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
	public class UrlRouteParameterDefaultAttribute : Attribute
	{
		/// <summary>
		/// Gets or sets the name of the route parameter for which to supply the default value.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the default value to set on the route parameter if not specified in the URL.
		/// </summary>
		public object Value { get; set; }

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format("{0} (default: {1})", this.Name, this.Value);
		}
	}
}