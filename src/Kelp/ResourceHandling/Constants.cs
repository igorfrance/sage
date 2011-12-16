namespace Kelp.ResourceHandling
{
	/// <summary>
	/// Defines code resource types.
	/// </summary>
	public enum ResourceType
	{
		/// <summary>
		/// Specifies a <c>JavaScript</c> resource.
		/// </summary>
		Script = 1,

		/// <summary>
		/// Specifies a <c>CSS</c> resource.
		/// </summary>
		Css = 2,
	}

	/// <summary>
	/// Defines the special characters significant for code processing.
	/// </summary>
	internal static class Chars
	{
		public const char Slash = '/';
		public const char Backslash = '\\';
		public const char Space = ' ';
		public const char Tab = '\t';
		public const char Star = '*';
		public const char NewLine = '\n';
		public const char CarriageReturn = '\r';
		public const char Quote = '"';
		public const char Apos = '\'';
		public const char Eof = (char) 0;
	}
}
