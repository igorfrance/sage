﻿/**
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

	/// <summary>
	/// Defines an interface similar to <see cref="XmlUrlResolver"/>, with knowledge of Sage classes.
	/// </summary>
	public interface ISageXmlUrlResolver
	{
		/// <summary>
		/// Gets an <see cref="EntityResult"/> that represents the actual resource mapped from the specified <paramref name="uri"/>.
		/// </summary>
		/// <param name="parent">The <see cref="UrlResolver"/> that owns this resolved and calls this method.</param>
		/// <param name="context">The current <see cref="SageContext"/> under which this code is executing.</param>
		/// <param name="uri">The uri to resolve.</param>
		/// <returns>An object that represents the resource mapped from the specified <paramref name="uri"/>.</returns>
		EntityResult GetEntity(UrlResolver parent, SageContext context, string uri);
	}
}
