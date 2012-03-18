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
	using System.Diagnostics.Contracts;
	using System.Drawing;

	/// <summary>
	/// Represents a bitmap filter.
	/// </summary>
	[ContractClass(typeof(FilterContract))]
	public interface IFilter
	{
		/// <summary>
		/// Applies the filter to the specified input <paramref name="source"/> bitmap and returns the result
		/// as a new bitmap.
		/// </summary>
		/// <param name="source">The source bitmap to filter.</param>
		/// <returns>The filtered bitmap.</returns>
		Bitmap Apply(Bitmap source);
	}

	[ContractClassFor(typeof(IFilter))]
	internal abstract class FilterContract : IFilter
	{
		public Bitmap Apply(Bitmap source)
		{
			Contract.Requires<ArgumentNullException>(source != null);
			return default(Bitmap);
		}
	}
}
