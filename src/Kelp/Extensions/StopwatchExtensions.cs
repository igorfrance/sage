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
namespace Kelp.Extensions
{
	using System;
	using System.Diagnostics;

	/// <summary>
	/// Implements the extensions to the <see cref="Stopwatch"/> class.
	/// </summary>
	public static class StopwatchExtensions
	{
		/// <summary>
		/// Executes the specified <paramref name="action"/> and returns the elapsed milliseconds.
		/// </summary>
		/// <param name="sw">The stopwatch instance.</param>
		/// <param name="action">The action to execute.</param>
		/// <returns>The milliseconds it took to execute the specified action.</returns>
		public static long TimeMilliseconds(this Stopwatch sw, Action action)
		{
			sw.Reset();
			sw.Start();

			action.Invoke();

			long result = sw.ElapsedMilliseconds;
			return result;
		}
	}
}
