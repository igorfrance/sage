/**
 * Open Source Initiative OSI - The MIT License (MIT):Licensing
 * [OSI Approved License]
 * The MIT License (MIT)
 *
 * Copyright (c) 2011 Igor France
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
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