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
namespace Kelp.XInclude.XPath
{
	/// <summary>
	/// Represents a variable to use in dynamic XPath expression 
	/// queries.
	/// </summary>
	/// <remarks>Author: Daniel Cazzulino, <a href="http://clariusconsulting.net/kzu">blog</a></remarks>
	public struct XPathVariable
	{
		/// <summary>
		/// Initializes the new variable.
		/// </summary>
		/// <param name="name">The name to assign to the variable.</param>
		/// <param name="value">The variable value.</param>
		public XPathVariable(string name, object value)
			: this()
		{
			this.Name = name;
			this.Value = value;
		}

		/// <summary>
		/// Gets the variable name.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the variable value.
		/// </summary>
		public object Value { get; private set; }

		/// <summary>
		/// Checks equality of two variables. They are equal 
		/// if both their <see cref="Name"/> and their <see cref="Value"/> 
		/// are equal.
		/// </summary>
		public static bool operator ==(XPathVariable a, XPathVariable b)
		{
			return a.Equals(b);
		}

		/// <summary>
		/// Checks equality of two variables. They are not equal 
		/// if both their <see cref="Name"/> and their <see cref="Value"/> 
		/// are different.
		/// </summary>
		public static bool operator !=(XPathVariable a, XPathVariable b)
		{
			return !a.Equals(b);
		}

		/// <summary>
		/// Checks equality of two variables. They are equal 
		/// if both their <see cref="Name"/> and their <see cref="Value"/> 
		/// are equal.
		/// </summary>
		public override bool Equals(object obj)
		{
			return this.Name == ((XPathVariable)obj).Name && this.Value == ((XPathVariable)obj).Value;
		}

		/// <summary>
		/// See <see cref="object.GetHashCode"/>.
		/// </summary>
		public override int GetHashCode()
		{
			return (this.Name + "." + this.Value.GetHashCode()).GetHashCode();
		}
	}
}