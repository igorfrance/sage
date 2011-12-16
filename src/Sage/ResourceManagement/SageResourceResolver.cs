namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Xml;

	using log4net;
	using Sage.Configuration;

	/// <summary>
	/// Implements a custom resolver for various resource providers.
	/// </summary>
	/// <remarks>
	/// In order to hook into this resolver and provide custom resources that can be referred to in
	/// url's, create a method that matches <see cref="SageResourceProvider"/> delegate and tag it with
	/// <see cref="SageResourceProviderAttribute"/>. The name that you specify in the constructor for
	/// <see cref="SageResourceProviderAttribute"/> is the string that follows <c>sage://resources/</c>.
	/// </remarks>
	[UrlResolver(Scheme = SageResourceResolver.Scheme)]
	public class SageResourceResolver : ISageXmlUrlResolver
	{
		/// <summary>
		/// The scheme associated with this resolver;
		/// </summary>
		public const string Scheme = "sage";
		private static readonly ILog log = LogManager.GetLogger(typeof(SageResourceResolver).FullName);
		private static readonly Dictionary<string, SageResourceProvider> providers;
		private readonly List<string> dependencies = new List<string>();

		static SageResourceResolver()
		{
			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
			providers = new Dictionary<string, SageResourceProvider>();

			foreach (Assembly a in ProjectConfiguration.RelevantAssemblies)
			{
				var types = from t in a.GetTypes()
							where 
								t.IsClass && 
								t.GetMethods(flags).Where(m => m.GetCustomAttributes(typeof(SageResourceProviderAttribute), false).Count() != 0).Count() != 0
							select t;

				foreach (Type type in types)
				{
					foreach (MethodInfo methodInfo in type.GetMethods(flags))
					{
						foreach (SageResourceProviderAttribute attrib in methodInfo.GetCustomAttributes(typeof(SageResourceProviderAttribute), false))
						{
							SageResourceProvider del = (SageResourceProvider) Delegate.CreateDelegate(typeof(SageResourceProvider), methodInfo);
							if (providers.ContainsKey(attrib.ResourceName))
							{
								log.WarnFormat("Overwriting existing resource provider '{0}' for resource name '{1}' with provider '{2}'",
									ResourceManager.GetDelegateSignature(providers[attrib.ResourceName]), 
									attrib.ResourceName, 
									ResourceManager.GetDelegateSignature(del));
							}

							providers[attrib.ResourceName] = del;
						}
					}
				}
			}
		}

		/// <summary>
		/// Defines the signature of the delegate that can return the <see cref="XmlReader"/> for the specified
		/// <paramref name="resourceUri"/> and <paramref name="context"/>.
		/// </summary>
		/// <param name="context">The context under which the code is executing.</param>
		/// <param name="resourceUri">The URI of the resource to open.</param>
		/// <returns>An <see cref="XmlReader"/> that reads the resource referred to with <paramref name="resourceUri"/>.</returns>
		public delegate XmlReader SageResourceProvider(SageContext context, string resourceUri);

		/// <inheritdoc/>
		public EntityResult GetEntity(UrlResolver parent, SageContext context, string resourceUri)
		{
			XmlReader reader = GetXmlResourceReader(context, resourceUri);
			if (reader == null)
			{
				if (providers.Count == 0)
				{
					throw new ArgumentException(string.Format("The resource with uri '{0}' could not be resolved. There are currently no resource providers registered, so any call to '{1}'... will fail.", resourceUri, GetResourcePrefix()));
				}

				throw new ArgumentException(string.Format("The resource with uri '{0}' could not be resolved. The supported uris are: \n{1}", resourceUri, string.Join("\n", GetValidResourceUris())));
			}

			return new EntityResult { Entity = reader, Dependencies = dependencies };
		}

		protected XmlReader GetXmlResourceReader(SageContext context, string resourceUri)
		{
			string resourceName = GetResourceName(resourceUri);
			if (providers.ContainsKey(resourceName))
				return providers[resourceName](context, resourceUri);

			return null;
		}

		private string GetResourceName(string uri)
		{
			return uri.Replace(GetResourcePrefix(), string.Empty);
		}

		private string GetResourcePrefix()
		{
			return Scheme + "://resources/";
		}

		private IEnumerable<string> GetValidResourceUris()
		{
			List<string> result = new List<string>();

			string prefix = GetResourcePrefix();
			foreach (string key in providers.Keys)
			{
				result.Add(prefix + key);
			}

			return result;
		}
	}
}
