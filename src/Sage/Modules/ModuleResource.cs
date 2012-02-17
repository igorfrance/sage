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

	using Sage.ResourceManagement;

	/// <summary>
	/// Represents a resource for use with modules.
	/// </summary>
	public class ModuleResource : Resource
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ModuleResource"/> class.
		/// </summary>
		/// <param name="resourceNode">The XML configuration node that represent this resource.</param>
		/// <param name="moduleName">The name of the module this resource belongs to.</param>
		public ModuleResource(XmlElement resourceNode, string moduleName)
			: base(resourceNode)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(moduleName));

			this.ModuleName = moduleName;
		}

		/// <summary>
		/// Gets the name of the module this resource belongs to.
		/// </summary>
		public string ModuleName { get; private set; }

		/// <inheritdoc/>
		public override string GetResolvedPhysicalPath(SageContext context)
		{
			return context.Path.GetModulePath(this.ModuleName, this.Path);
		}
	}
}
