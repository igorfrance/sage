namespace Kelp.Core.Extensions
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
