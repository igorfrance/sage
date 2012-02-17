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
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Represents an integer range with min and max values
	/// </summary>
	public struct IntRange
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="IntRange"/> struct.
		/// </summary>
		/// <param name="min">The minimum value in the range.</param>
		/// <param name="max">The maximum value in the range.</param>
		public IntRange(int min, int max)
			: this()
		{
			this.Min = min;
			this.Max = max;
		}

		/// <summary>
		/// Gets the minimum value in the range.
		/// </summary>
		public int Min { get; private set; }

		/// <summary>
		/// Gets the maximum value in the range.
		/// </summary>
		public int Max { get; private set; }

		/// <summary>
		/// Returns the specified <paramref name="value"/> if it falls in range, otherwise either tha 
		/// <see cref="Min"/> or the <see cref="Max"/> value, depending on whether the value was too large or too
		/// small.
		/// </summary>
		/// <param name="value">The input value.</param>
		/// <returns>The value that is in range</returns>
		public int GetValue(int value)
		{
			if (value < this.Min)
				value = this.Min;
			else if (value > this.Max)
				value = this.Max;

			return value;
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to a value in range and multiplies it with the specified 
		/// <paramref name="multiplier"/>.
		/// </summary>
		/// <param name="value">The input value.</param>
		/// <param name="multiplier">The value to multiply the ranged <paramref name="value"/> with.</param>
		/// <returns>The value that is in range, multiplied with the specified <paramref name="multiplier"/>.</returns>
		public int GetValue(int value, decimal multiplier)
		{
			return Decimal.ToInt32(GetValue(value) * multiplier);
		}
	}

	/// <summary>
	/// Represents a double range with min and max values
	/// </summary>
	public struct DoubleRange
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DoubleRange"/> struct.
		/// </summary>
		/// <param name="min">The minimum value in the range.</param>
		/// <param name="max">The maximum value in the range.</param>
		public DoubleRange(double min, double max)
			: this()
		{
			this.Min = min;
			this.Max = max;
		}

		/// <summary>
		/// Gets the minimum value in the range.
		/// </summary>
		public double Min { get; private set; }

		/// <summary>
		/// Gets the maximum value in the range.
		/// </summary>
		public double Max { get; private set; }
	}
}
