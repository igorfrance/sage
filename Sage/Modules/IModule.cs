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
	using Sage.Views;

	/// <summary>
	/// Defines the common interface for all modules.
	/// </summary>
	[ContractClass(typeof(ModuleContract))]
	public interface IModule
	{
		/// <summary>
		/// Processes the specified <paramref name="moduleElement"/> and returns a <see cref="ModuleResult"/>.
		/// </summary>
		/// <param name="moduleElement">The XML element which represents this module.</param>
		/// <param name="configuration">The <see cref="ViewConfiguration"/> instance in which the 
		/// <paramref name="moduleElement"/> was used.</param>
		/// <returns>
		/// An object that contains the result of processing the element.
		/// </returns>
		/// <remarks>
		/// A module will typically perform some back-end work (to getting some remote data from somewhere) and
		/// provide the XML node with data to insert back into the <paramref name="moduleElement"/>. To remove the
		/// element completely from the configuration document so that it doesn't appear at all in the result, this
		/// method should return a <c>null</c>.
		/// </remarks>
		ModuleResult ProcessElement(XmlElement moduleElement, ViewConfiguration configuration);
	}

	/// <summary>
	/// Provides contracts for <see cref="IModule"/>.
	/// </summary>
	[ContractClassFor(typeof(IModule))]
	internal abstract class ModuleContract : IModule
	{
		/// <inheritdoc/>
		public ModuleResult ProcessElement(XmlElement moduleElement, ViewConfiguration configuration)
		{
			Contract.Requires<ArgumentNullException>(moduleElement != null);
			Contract.Requires<ArgumentNullException>(configuration != null);

			return default(ModuleResult);
		}
	}
}
