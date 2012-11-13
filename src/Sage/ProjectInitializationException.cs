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
namespace Sage
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Represents an error that was raised due to a missing dependency.
	/// </summary>
	internal class ProjectInitializationException : Exception
	{
		public ProjectInitializationException(string message)
			: base(message)
		{
			this.Dependencies = new string[0];
		}

		public ProjectInitializationException(string message, Exception ex)
			: base(message, ex)
		{
			this.Dependencies = new string[0];
		}

		public ProblemType Reason { get; set; }

		public string SourceFile { get; set; }

		public IEnumerable<string> Dependencies { get; set; }
	}
}
