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

	/// <summary>
	/// Indicates that the method this attribute decorates can be used as a function handler for
	/// function-like expressions in strings; <code>function(arguments);</code>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class TextFunctionAttribute : Attribute
	{
		/// <summary>
		/// Specifies the name of this function; <code>name();</code>
		/// </summary>
		public string Name { get; set; }
	}
}
