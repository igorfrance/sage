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
