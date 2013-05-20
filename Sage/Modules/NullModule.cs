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
namespace Sage.Modules
{
	using System;
	using System.Xml;

	using Sage.Views;

	/// <summary>
	/// Defines the default module that doesn't do anything.
	/// </summary>
	/// <remarks>
	/// This class is used with modules that don't have a back-end component.
	/// </remarks>
	public class NullModule : IModule
	{
		/// <inheritdoc/>
		public ModuleResult ProcessElement(XmlElement moduleNode, ViewConfiguration configuration)
		{
			return new ModuleResult(moduleNode);
		}
	}
}
