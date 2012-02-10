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
namespace Kelp.Imaging
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;

	/// <summary>
	/// Provides a container for a list of filters that can be applied as one.
	/// </summary>
	public class FilterSequence : List<IFilter>, IFilter
	{
		/// <summary>
		/// Applies all filters constituent in this filter sequence.
		/// </summary>
		/// <param name="source">The source bitmap to apply the filters on.</param>
		/// <returns>The new bitmap with filters applied</returns>
		public Bitmap Apply(Bitmap source)
		{
			Bitmap result = new Bitmap(source);
			foreach (IFilter filter in this)
				result = filter.Apply(result);

			return result;
		}
	}
}
