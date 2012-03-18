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