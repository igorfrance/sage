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
namespace Kelp.SyntaxHighlighting
{
	/// <summary>
	/// Represents a pair or character strings that delimit sections of code, such as 
	/// strings or comments.
	/// </summary>
	public class Delimiters
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Delimiters"/> class.
		/// </summary>
		public Delimiters()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Delimiters"/> class, using the specified
		/// <paramref name="start"/> string and <paramref name="end"/> string.
		/// </summary>
		/// <param name="start">The start delimiter.</param>
		/// <param name="end">The end delimiter.</param>
		public Delimiters(string start, string end)
		{
			this.Start = start;
			this.End = end;
		}

		/// <summary>
		/// Gets or sets the start delimiter.
		/// </summary>
		public string Start { get; set; }

		/// <summary>
		/// Gets or sets the end delimiter.
		/// </summary>
		public string End { get; set; }

		/// <summary>
		/// Gets the combined string length of delimiter start and end strings.
		/// </summary>
		public int Length
		{
			get
			{
				return this.Start.Length + this.End.Length;
			}
		}
	}
}
