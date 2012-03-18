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
	/// Assigns a URL route to an MVC Controller class method.
	/// </summary>
	/// <remarks>
	/// For example:
	/// <example><code>
	/// [UrlRoute(Name = "ProductCategory", Path = "products/{category}")]
	/// public ActionResult ProductCategory(string category)
	/// {
	///   ...
	/// }
	/// </code>
	/// </example>
	/// </remarks>
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
	public class UrlRouteAttribute : Attribute
	{
		/// <summary>
		/// Gets or sets the name of the route.
		/// </summary>
		/// <remarks>
		/// This value is optional, but route names must be unique.
		/// </remarks>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the path of the URL route.
		/// </summary>
		/// <remarks>
		/// This is relative to the root of the web site.
		/// Do not append a "/" prefix. Specify empty string for the root page.
		/// </remarks>
		public string Path { get; set; }

		/// <summary>
		/// Gets or sets the order in which to add the route (default is 0).
		/// </summary>
		/// <remarks>
		/// Routes with lower order values will be added before those with higher.
		/// Routes that have the same order value will be added in undefined
		/// order with respect to each other.
		/// </remarks>
		public int Order { get; set; }

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("{0} ({1})", this.Name, this.Path);
		}
	}
}