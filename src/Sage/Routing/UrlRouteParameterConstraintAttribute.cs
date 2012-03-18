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
	/// Assigns a constraint to a route parameter in a <see cref="UrlRouteAttribute"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
	public class UrlRouteParameterConstraintAttribute : Attribute
	{
		/// <summary>
		/// Gets or sets the name of the route parameter on which to apply the constraint.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the regular expression constraint to test on the route parameter value in the URL.
		/// </summary>
		public string Expression { get; set; }

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("{0} ({1})", this.Name, this.Expression);
		}
	}
}