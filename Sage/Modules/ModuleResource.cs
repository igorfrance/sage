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
	using System.Diagnostics.Contracts;
	using System.Xml;

	using Sage.ResourceManagement;

	/// <summary>
	/// Represents a resource for use with modules.
	/// </summary>
	public class ModuleResource : Resource
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ModuleResource"/> class.
		/// </summary>
		/// <param name="resourceNode">The XML configuration node that represent this resource.</param>
		/// <param name="moduleKey">The name of the module this resource belongs to.</param>
		/// <param name="projectId">The identification string of the project this library belongs to.</param>
		public ModuleResource(XmlElement resourceNode, string moduleKey, string projectId)
			: base(resourceNode, projectId)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(moduleKey));

			this.ModuleKey = moduleKey;
		}

		/// <summary>
		/// Gets the key of the module this resource belongs to.
		/// </summary>
		public string ModuleKey { get; private set; }

		/// <inheritdoc/>
		public override string GetResolvedPhysicalPath(SageContext context)
		{
			return context.Path.GetModulePath(this.ModuleKey, this.Path);
		}
	}
}
