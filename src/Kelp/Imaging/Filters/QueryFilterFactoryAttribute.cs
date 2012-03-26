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
namespace Kelp.Imaging.Filters
{
	using System;

	/// <summary>
	/// Indicates that the decorated method creates an <see cref="IFilter"/> instance.
	/// </summary>
	public class QueryFilterFactoryAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="QueryFilterFactoryAttribute"/> class.
		/// </summary>
		public QueryFilterFactoryAttribute()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="QueryFilterFactoryAttribute"/> class.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <param name="parameterCount">The parameter count.</param>
		public QueryFilterFactoryAttribute(string id, byte parameterCount)
		{
			this.ID = id;
			this.ParameterCount = parameterCount;
		}

		/// <summary>
		/// Gets or sets the name of the query string parameter that the method this attribute decorates will
		/// be registered for.
		/// </summary>
		public string ID { get; set; }

		/// <summary>
		/// Gets or sets the minimum number of query string parameter values that should be present.
		/// </summary>
		public byte ParameterCount { get; set; }
	}
}
