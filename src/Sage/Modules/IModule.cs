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
