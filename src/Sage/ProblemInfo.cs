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
	/// Contains information about an error that occurred.
	/// </summary>
	public class ProblemInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ProblemInfo" /> class, using the specified <paramref name="type"/>
		/// and <paramref name="filePath"/>.
		/// </summary>
		/// <param name="type">The problem type associated with this error.</param>
		/// <param name="filePath">The path to the file where the error occurred.</param>
		public ProblemInfo(ProblemType type, string filePath = null)
		{
			this.Type = type;
			this.FilePath = filePath;
			this.InfoBlocks = new Dictionary<string, IDictionary<string, string>>();
		}

		/// <summary>
		/// Gets or sets the path to the file where the error occurred.
		/// </summary>
		public string FilePath { get; set; }

		/// <summary>
		/// Gets or sets the type associated with this error.
		/// </summary>
		/// <value>The type.</value>
		public ProblemType Type { get; set; }

		/// <summary>
		/// Gets the dictionary of information blocks that can be used for displaying information about the error.
		/// </summary>
		public IDictionary<string, IDictionary<string, string>> InfoBlocks { get; internal set; }
	}
}
