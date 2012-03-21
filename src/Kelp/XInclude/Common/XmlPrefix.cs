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
namespace Kelp.XInclude.Common
{
	using System.Xml;

	/// <summary>
	/// Represents a mapping between a prefix and a namespace.
	/// </summary>
	public class XmlPrefix
	{
		/// <summary>
		/// Creates the prefix mapping.
		/// </summary>
		/// <param name="prefix">Prefix associated with the namespace.</param>
		/// <param name="ns">Namespace to associate with the prefix.</param>
		public XmlPrefix(string prefix, string ns)
		{
			this.Prefix = prefix;
			this.NamespaceURI = ns;
		}

		/// <summary>
		/// Creates the prefix mapping, using atomized strings from the 
		/// <paramref name="nameTable"/> for faster lookups and comparisons.
		/// </summary>
		/// <param name="prefix">Prefix associated with the namespace.</param>
		/// <param name="ns">Namespace to associate with the prefix.</param>
		/// <param name="nameTable">The name table to use to atomize strings.</param>
		/// <remarks>
		/// This is the recommended way to construct this class, as it uses the 
		/// best approach to handling strings in XML.
		/// </remarks>
		public XmlPrefix(string prefix, string ns, XmlNameTable nameTable)
		{
			this.Prefix = nameTable.Add(prefix);
			this.NamespaceURI = nameTable.Add(ns);
		}

		/// <summary>
		/// Gets the namespace associated with the <see cref="Prefix"/>.
		/// </summary>
		public string NamespaceURI { get; private set; }

		/// <summary>
		/// Gets the prefix associated with the <see cref="NamespaceURI"/>.
		/// </summary>
		public string Prefix { get; private set; }
	}
}