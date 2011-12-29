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
				MetaViewInfo result = context.Config.MetaViews[viewName];
				if (!result.IsLoaded)
					result.Load(context);

				return result;
			}

			return null;
		}
	}
}
