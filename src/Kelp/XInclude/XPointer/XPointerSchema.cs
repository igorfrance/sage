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

	/// <summary>
	/// XPointer scheme.
	/// </summary>
	internal class XPointerSchema
	{
		public static IDictionary<string, SchemaType> Schemas = CreateSchemasTable();

		public enum SchemaType
		{
			Element, 
			Xmlns, 
			XPath1, 
			XPointer, 
			Unknown
		}

		private static IDictionary<string, SchemaType> CreateSchemasTable()
		{
			IDictionary<string, SchemaType> table = new Dictionary<string, SchemaType>(4);

			// <namespace uri>:<ncname>
			table.Add(":element", SchemaType.Element);
			table.Add(":xmlns", SchemaType.Xmlns);
			table.Add(":xpath1", SchemaType.XPath1);
			table.Add(":xpointer", SchemaType.XPointer);
			return table;
		}
	}
}