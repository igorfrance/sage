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

	/// <summary>
	/// Provides information about a schema validation.
	/// </summary>
	public class ValidationResult
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ValidationResult"/> class.
		/// </summary>
		public ValidationResult()
		{
			this.Success = true;
			this.Line = -1;
			this.Column = -1;
		}

		/// <summary>
		/// Gets or sets the name of the source file that caused the error, if any.
		/// </summary>
		public string SourceFile { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the validation associated with this result succeeded.
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// Gets of sets the line at which the validation error occurred.
		/// </summary>
		public int Line { get; set; }

		/// <summary>
		/// Gets of sets the line column at which the validation error occurred.
		/// </summary>
		public int Column { get; set; }

		/// <summary>
		/// Gets or sets the exception associated with this result.
		/// </summary>
		public Exception Exception { get; set; }
	}
}
