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
namespace Sage.Modules
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Xml;

	/// <summary>
	/// Specifies the interface for Sage module factories.
	/// </summary>
	[ContractClass(typeof(ModuleFactoryContract))]
	public interface IModuleFactory
	{
		/// <summary>
		/// Creates a module instance based on the specified <paramref name="moduleElement"/>.
		/// </summary>
		/// <param name="moduleElement">The XML element that represents the module.</param>
		/// <returns>
		/// A module instance associated with the specified <paramref name="moduleElement"/>.
		/// </returns>
		IModule CreateModule(XmlElement moduleElement);
	}

	[ContractClassFor(typeof(IModuleFactory))]
	internal abstract class ModuleFactoryContract : IModuleFactory
	{
		/// <inheritdoc/>
		public IModule CreateModule(XmlElement moduleElement)
		{
			Contract.Requires<ArgumentNullException>(moduleElement != null);
			return default(IModule);
		}
	}
}