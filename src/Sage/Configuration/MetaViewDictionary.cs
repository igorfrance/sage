/**
 * Open Source Initiative OSI - The MIT License (MIT):Licensing
 * [OSI Approved License]
 * The MIT License (MIT)
 *
 * Copyright (c) 2011 Igor France
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
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
