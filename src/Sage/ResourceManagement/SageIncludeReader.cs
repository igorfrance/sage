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
namespace Sage.ResourceManagement
{
	using System;
	using System.Xml;

	using Mvp.Xml.XInclude;

	/// <summary>
	/// Extends the <see cref="XIncludingReader"/> with additional functionality.
	/// </summary>
	internal class SageIncludeReader : XIncludingReader
	{
		private readonly XmlReader wrapped;
		private bool processIncludes;

		/// <summary>
		/// Initializes a new instance of the <see cref="SageIncludeReader"/> class.
		/// </summary>
		/// <param name="wrapped">The reader that this reader wraps.</param>
		public SageIncludeReader(XmlReader wrapped)
			: base(wrapped)
		{
			this.wrapped = wrapped;
			this.processIncludes = true;
		}

		/// <inheritdoc/>
		public override bool Read()
		{
			if (this.Name == "sage:literal")
				this.processIncludes = this.NodeType == XmlNodeType.EndElement;

			return this.processIncludes ? base.Read() : this.wrapped.Read();
		}
	}
}
