namespace Kelp.Core.Extensions
{
	using System;

	/// <summary>
	/// Provides <see cref="Exception"/> extension methods.
	/// </summary>
	public static class ExceptionExtensions
	{
		/// <summary>
		/// Gets the message of the <see cref="Exception"/> innermost to the current <see cref="Exception"/>.
		/// </summary>
		/// <param name="instance">The <see cref="Exception"/> instance.</param>
		/// <returns>The message of the <see cref="Exception"/> innermost to the current <see cref="Exception"/>.</returns>
		public static string InnermostExceptionMessage(this Exception instance)
		{
			Exception inner = GetInnermostException(instance);
			return inner.Message;
		}

		/// <summary>
		/// Gets the type name of the <see cref="Exception"/> innermost to the current <see cref="Exception"/>.
		/// </summary>
		/// <param name="instance">The <see cref="Exception"/> instance.</param>
		/// <returns>The type name of the <see cref="Exception"/> innermost to the current <see cref="Exception"/>.</returns>
		public static string InnermostExceptionTypeName(this Exception instance)
		{
			Exception inner = GetInnermostException(instance);
			return inner.GetType().Name;
		}

		private static Exception GetInnermostException(Exception instance)
		{
			Exception result = instance;
			while (result.InnerException != null)
				result = result.InnerException;

			return result;
		}
	}
}
