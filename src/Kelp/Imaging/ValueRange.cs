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
