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
	using System.Collections.Generic;
	using System.Globalization;
	using System.Xml;
	using System.Xml.XPath;

	using Kelp.Properties;

	/// <summary>
	/// SchemaBased XPointer pointer.
	/// </summary>
	internal class SchemaBasedPointer : Pointer
	{
		private readonly IList<PointerPart> parts;
		private readonly string xpointer;

		/// <summary>
		/// Creates scheme based XPointer given list of pointer parts.
		/// </summary>
		/// <param name="parts">List of pointer parts</param>
		/// <param name="xpointer">String representation of the XPointer 
		/// (for error diagnostics)</param>
		public SchemaBasedPointer(IList<PointerPart> parts, string xpointer)
		{
			this.parts = parts;
			this.xpointer = xpointer;
		}

		/// <summary>
		/// Evaluates <see cref="XPointer"/> pointer and returns 
		/// iterator over pointed nodes.
		/// </summary>
		/// <param name="nav">XPathNavigator to evaluate the 
		/// <see cref="XPointer"/> on.</param>
		/// <returns><see cref="XPathNodeIterator"/> over pointed nodes</returns>	    					
		public override XPathNodeIterator Evaluate(XPathNavigator nav)
		{
			var nm = new XmlNamespaceManager(nav.NameTable);
			foreach (PointerPart part in this.parts)
			{
				XPathNodeIterator result = part.Evaluate(nav, nm);
				if (result != null && result.MoveNext())
				{
					return result;
				}
			}

			throw new NoSubresourcesIdentifiedException(
				string.Format(CultureInfo.CurrentCulture, Resources.NoSubresourcesIdentifiedException, this.xpointer, nav.BaseURI));
		}
	}
}