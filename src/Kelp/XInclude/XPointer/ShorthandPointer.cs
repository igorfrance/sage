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
	using System.Globalization;
	using System.Xml;
	using System.Xml.XPath;

	using Kelp.Properties;
	using Kelp.XInclude.XPath;

	/// <summary>
	/// Shorthand XPointer pointer.
	/// </summary>
	internal class ShorthandPointer : Pointer
	{
		private readonly string name;

		/// <summary>
		/// Creates shorthand XPointer given bare name.
		/// </summary>
		/// <param name="name">Shorthand (bare) name</param>
		public ShorthandPointer(string name)
		{
			this.name = name;
		}

		/// <summary>
		/// Evaluates <see cref="XPointer"/> pointer and returns 
		/// iterator over pointed nodes.
		/// </summary>
		/// <remarks>Note, that returned XPathNodeIterator is already moved once.</remarks>
		/// <param name="nav">XPathNavigator to evaluate the 
		/// <see cref="XPointer"/> on.</param>
		/// <returns><see cref="XPathNodeIterator"/> over pointed nodes</returns>	    					
		public override XPathNodeIterator Evaluate(XPathNavigator nav)
		{
			XPathNodeIterator result = XPathCache.Select("id('" + this.name + "')", nav, (XmlNamespaceManager)null);
			if (result != null && result.MoveNext())
			{
				return result;
			}

			throw new NoSubresourcesIdentifiedException(
				string.Format(CultureInfo.CurrentCulture, Resources.NoSubresourcesIdentifiedException, this.name));
		}
	}
}