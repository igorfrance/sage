namespace Kelp.Core.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Implements extensions to <see cref="IEnumerable&lt;T&gt;"/>.
	/// </summary>
	public static class IEnumerableExtensions
	{
		/// <summary>
		/// Creates a string of all values in the collection joined with the specified separator.
		/// </summary>
		/// <typeparam name="T">The <see cref="IEnumerable&lt;T&gt;"/> collection.</typeparam>
		/// <param name="collection">The collection whose values should be joined.</param>
		/// <param name="separator">The separator to use for joining the collection's values.</param>
		/// <returns>The joined collection.</returns>
		/// <example>
		/// var myList = new List&lt;string&gt; { "red", "green", "blue" };
		/// // The following line returns "red,green,blue":
		/// var joined = myList.Join(","); 
		/// </example>
		public static string Join<T>(this IEnumerable<T> collection, string separator)
		{
			Contract.Requires<ArgumentNullException>(collection != null);
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(separator));

			StringBuilder result = new StringBuilder();
			for (int i = 0; i < collection.Count(); i++)
			{
				object item = collection.ElementAt(i);
				if (item == null)
					result.Append(string.Empty);
				else
					result.Append(item.ToString());

				if (i < collection.Count() - 1)
					result.Append(separator);
			}

			return result.ToString();
		}

		/// <summary>
		/// Executes the specified <paramref name="action"/> on each of the elements in the <paramref name="collection"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection">The collection on which to operate.</param>
		/// <param name="action">The action to apply to each of the elements.</param>
		public static void ForEach<T>(this IEnumerable<T> collection, Action<T, int> action)
		{
			for (int i = 0; i < collection.Count(); i++)
			{
				action(collection.ElementAt(i), i);
			}
		}

		/// <summary>
		/// Returns the index of the specified <paramref name="item"/> in the specified <paramref name="collection"/>.
		/// </summary>
		/// <param name="collection">The collection to search</param>
		/// <param name="item">The item to find</param>
		/// <returns>The index of the specified <paramref name="item"/> in the specified <paramref name="collection"/>,
		/// if found and -1 if the item could not be found</returns>
		public static int IndexOf(this IEnumerable<string> collection, string item)
		{
			for (int i = 0; i < collection.Count(); i++)
			{
				if (collection.ElementAt(i) == item)
					return i;
			}

			return -1;
		}
	}
}
