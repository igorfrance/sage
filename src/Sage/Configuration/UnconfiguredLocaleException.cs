namespace Sage.Configuration
{
	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// Represents an invalid attempt to work with a locale that the application doesn't know about.
	/// </summary>
	public class UnconfiguredLocaleException : Exception
	{
		private const string MessageTemplate =
			"The locale '{0}' hasn't been configured.\n" +
			"Make sure to add the locale within the main configuration file's <globalization/> section, " +
			"for instance:\n\n" +
			"<locale name=\"{0}\" dictionaryNames=\"{0},en\" resourceNames=\"{0},default\"/>";

		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationError"/> class.
		/// </summary>
		/// <param name="locale">The invalid locale whose usage raised the exception.</param>
		public UnconfiguredLocaleException(string locale)
			: base(string.Format(MessageTemplate, locale))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationError"/> class.
		/// </summary>
		/// <param name="locale">The invalid locale whose usage raised the exception.</param>
		/// <param name="inner">The inner exception that this exception is wrapping.</param>
		public UnconfiguredLocaleException(string locale, Exception inner)
			: base(string.Format(MessageTemplate, locale), inner)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationError"/> class.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception 
		/// being thrown.</param>
		/// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or 
		/// destination.</param>
		public UnconfiguredLocaleException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}