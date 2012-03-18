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
namespace Sage.Controllers
{
	using System;

	/// <summary>
	/// Represents a single message that a controller is sending to the view.
	/// </summary>
	public class ControllerMessage
	{
		/// <summary>
		/// Gets or sets the type of this message.
		/// </summary>
		public MessageType Type { get; set; }

		/// <summary>
		/// Gets or sets the name of this message.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the text of this message.
		/// </summary>
		public string Text { get; set; }

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("{0} ({1})", this.Name, this.Type);
		}
	}
}
