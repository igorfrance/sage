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
namespace Sage.Configuration
{
	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// Represents an error in system configuration.
	/// </summary>
	public class ConfigurationError : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationError"/> class.
		/// </summary>
		/// <param name="mesage">The exception's error mesage.</param>
		public ConfigurationError(string mesage)
			: base(mesage)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationError"/> class.
		/// </summary>
		/// <param name="mesage">The exception's error mesage.</param>
		/// <param name="inner">The inner exception that this exception is wrapping.</param>
		public ConfigurationError(string mesage, Exception inner)
			: base(mesage, inner)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationError"/> class.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception 
		/// being thrown.</param>
		/// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or 
		/// destination.</param>
		public ConfigurationError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}