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
