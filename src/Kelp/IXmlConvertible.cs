﻿/**
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
	using System.Xml;

	/// <summary>
	/// Defines the interface for objects that support simple conversion to XML.
	/// </summary>
	public interface IXmlConvertible
	{
		/// <summary>
		/// Returns an <see cref="XmlElement"/> that represents the current object.
		/// </summary>
		/// <param name="ownerDoc">The <see cref="XmlDocument"/> to use to create XML elements.</param>
		/// <returns>An <see cref="XmlElement"/> that represents the current object.</returns>
		XmlElement ToXml(XmlDocument ownerDoc);
	}
}
