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
namespace Kelp.XInclude.XPointer
{
	using System.Xml.XPath;

	/// <summary>
	/// Abstract XPointer pointer.
	/// </summary>
	public abstract class Pointer
	{
		/// <summary>
		/// Parses XPointer pointer and compiles it into
		/// an instance of <see cref="Pointer"/> class.
		/// </summary>
		/// <param name="xpointer">XPointer pointer</param>
		/// <returns>Parsed and compiled XPointer</returns>
		public static Pointer Compile(string xpointer)
		{
			return XPointerParser.ParseXPointer(xpointer);
		}

		/// <summary>
		/// Evaluates <see cref="XPointer"/> pointer and returns 
		/// iterator over pointed nodes.
		/// </summary>
		/// <param name="nav">Navigator to evaluate the 
		/// <see cref="XPointer"/> on.</param>
		/// <returns><see cref="XPathNodeIterator"/> over pointed nodes. Note - this iterator is moved to the first node already.</returns>	    					
		public abstract XPathNodeIterator Evaluate(XPathNavigator nav);
	}
}