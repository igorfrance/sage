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
	using System.Collections.Generic;
	using System.Linq;

	using Sage.ResourceManagement;

	/// <summary>
	/// Indicates that the method this attribute decorates should be used as a variable handler for the specified variable name(s).
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class TextVariableAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TextVariableAttribute"/> class.
		/// </summary>
		/// <param name="variable">The name of a variable handled by the method this attribute decorates.</param>
		public TextVariableAttribute(string variable)
			: this()
		{
			this.Variables.Add(variable);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TextVariableAttribute"/> class.
		/// </summary>
		/// <param name="variables">The names of variables handled by the method this attribute decorates.</param>
		public TextVariableAttribute(params string[] variables)
			: this()
		{
			foreach (string variable in variables.Where(variable => !this.Variables.Contains(variable)))
			{
				this.Variables.Add(variable);
			}
		}

		private TextVariableAttribute()
		{
			this.Variables = new List<string>();
		}

		/// <summary>
		/// Gets the names of variables handled by the method this attribute decorates.
		/// </summary>
		public IList<string> Variables { get; private set; }
	}
}
