namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.IO;

	[UrlResolver(Scheme = MappedAssetResolver.Scheme)]
	public class MappedAssetResolver : ISageXmlUrlResolver
	{
		public const string Scheme = "sageres";

		public EntityResult GetEntity(UrlResolver parent, SageContext context, string uri)
		{
			string resolved = GetAssetPath(context, uri);
			if (resolved != null)
			{
				return new EntityResult
				{
					Entity = new FileStream(resolved, FileMode.Open, FileAccess.Read),
					Dependencies = new List<string> { resolved }
				};
			}

			throw new NotSupportedException(string.Format("The uri '{0}' could not be resolved.", uri));
		}

		private string GetAssetPath(SageContext context, string resource)
		{
			foreach (string key in context.ProjectConfiguration.AssetPrefixes.Keys)
			{
				if (!resource.StartsWith(key))
					continue;

				string assetPath = context.ProjectConfiguration.AssetPrefixes[key];
				string resourcePath = resource.Replace(key, assetPath).Trim('/');
				return context.Path.Resolve(resourcePath);
			}

			return null;
		}
	}
}
