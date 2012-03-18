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
