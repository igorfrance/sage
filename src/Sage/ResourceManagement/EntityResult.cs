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
	using System;
	using System.Collections.Generic;
	using System.Xml;

	/// <summary>
	/// Provides the result of calling <see cref="ISageXmlUrlResolver.GetEntity"/> on a give <c>uri</c>.
	/// </summary>
	public class EntityResult
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EntityResult"/> class.
		/// </summary>
		public EntityResult()
		{
			this.Dependencies = new List<string>();
		}

		/// <summary>
		/// Gets or sets the entity that was opened.
		/// </summary>
		/// <value>
		/// This will typically be an <see cref="XmlReader"/> around the resource that was opened.
		/// </value>
		public object Entity { get; set; }

		/// <summary>
		/// Gets or sets the list of files that the resource that was opened depends on.
		/// </summary>
		public IList<string> Dependencies { get; set; }
	}
}
