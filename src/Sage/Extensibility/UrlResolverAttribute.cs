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
namespace Sage.Extensibility
{
	using System;

	using Sage.ResourceManagement;

	/// <summary>
	/// Indicates that the attached class should be automatically registered with <see cref="UrlResolver"/>
	/// as a resolver for the specified <see cref="Scheme"/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class UrlResolverAttribute : Attribute
	{
		/// <summary>
		/// Gets or sets the scheme that the attached class resolves.
		/// </summary>
		public string Scheme { get; set; }
	}
}
