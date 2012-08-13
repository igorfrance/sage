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
namespace Kelp
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Xml;

	/// <summary>
	/// Represents an item that can be converted to and from an <see cref="XmlElement"/>.
	/// </summary>
	[ContractClass(typeof(IXmlConvertibleContract))]
	public interface IXmlConvertible
	{
		/// <summary>
		/// Parses the specified <paramref name="element"/> into the current object.
		/// </summary>
		/// <param name="element">The element to parse.</param>
		void Parse(XmlElement element);

		/// <summary>
		/// Generates an <see cref="XmlElement" /> that represents this instance.
		/// </summary>
		/// <param name="document">The document to use to create the element with.</param>
		/// <returns>An <see cref="XmlElement" /> that represents this instance.</returns>
		XmlElement ToXml(XmlDocument document);
	}

	/// <summary>
	/// Defines the contract for <see cref="IXmlConvertible"/>.
	/// </summary>
	[ContractClassFor(typeof(IXmlConvertible))]
	internal abstract class IXmlConvertibleContract : IXmlConvertible
	{
		public void Parse(XmlElement element)
		{
			Contract.Requires<ArgumentNullException>(element != null);
		}

		public XmlElement ToXml(XmlDocument document)
		{
			Contract.Requires<ArgumentNullException>(document != null);
			return null;
		}
	}
}
