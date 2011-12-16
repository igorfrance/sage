namespace Sage.Configuration
{
	using System;
	using System.Runtime.Serialization;

	/// <summary>
	/// Represents an error in system configuration.
	/// </summary>
	public class ConfigurationError : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationError"/> class.
		/// </summary>
		/// <param name="mesage">The exception's error mesage.</param>
		public ConfigurationError(string mesage)
			: base(mesage)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationError"/> class.
		/// </summary>
		/// <param name="mesage">The exception's error mesage.</param>
		/// <param name="inner">The inner exception that this exception is wrapping.</param>
		public ConfigurationError(string mesage, Exception inner)
			: base(mesage, inner)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationError"/> class.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception 
		/// being thrown.</param>
		/// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or 
		/// destination.</param>
		public ConfigurationError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}