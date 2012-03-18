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
	using System.Collections.Generic;

	/// <summary>
	/// Implements a generic meta view dictionary, extending it with the capability to select the meta view 
	/// appropriate for a specific request.
	/// </summary>
	public class MetaViewDictionary : Dictionary<string, MetaViewInfo>
	{
		internal const string ParamNameMetaView = "view";

		/// <summary>
		/// Selects the meta view appropriate for the specified <paramref name="context"/>.
		/// </summary>
		/// <param name="context">The context under which this method is being executed.</param>
		/// <returns>The meta view to apply for the specified <paramref name="context"/>.</returns>
		public MetaViewInfo SelectView(SageContext context)
		{
			string viewName = context.Query[ParamNameMetaView];
			if (viewName != null && this.ContainsKey(viewName) && context.IsDeveloperRequest)
			{
				MetaViewInfo result = context.ProjectConfiguration.MetaViews[viewName];
				return result.Load(context);
			}

			return null;
		}
	}
}
