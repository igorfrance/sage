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
namespace Kelp.ResourceHandling
{
	/// <summary>
	/// Defines code resource types.
	/// </summary>
	public enum ResourceType
	{
		/// <summary>
		/// Specifies a <c>JavaScript</c> resource.
		/// </summary>
		Script = 1,

		/// <summary>
		/// Specifies a <c>CSS</c> resource.
		/// </summary>
		Css = 2,
	}

	/// <summary>
	/// Defines the special characters significant for code processing.
	/// </summary>
	internal static class Chars
	{
		public const char Slash = '/';
		public const char Backslash = '\\';
		public const char Space = ' ';
		public const char Tab = '\t';
		public const char Star = '*';
		public const char NewLine = '\n';
		public const char CarriageReturn = '\r';
		public const char Quote = '"';
		public const char Apos = '\'';
		public const char Eof = (char) 0;
	}
}
