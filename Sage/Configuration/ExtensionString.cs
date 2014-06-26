namespace Sage.Configuration
{
	/// <summary>
	/// Contains a simple string, with the owning extension name added as an additional property.
	/// </summary>
	public class ExtensionString
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ExtensionString"/> class using the specified
		/// <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		public ExtensionString(string value)
		{
			this.Value = value;
		}

		/// <summary>
		/// Gets the value of this string.
		/// </summary>
		public string Value
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the name of the extension that owns this string.
		/// </summary>
		public string Extension
		{
			get;
			internal set;
		}
	}
}
