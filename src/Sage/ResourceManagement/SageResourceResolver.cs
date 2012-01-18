namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Xml;

	using Sage.Extensibility;

	using log4net;

	/// <summary>
	/// Implements a custom resolver for various resource providers.
	/// </summary>
	/// <remarks>
	/// In order to hook into this resolver and provide custom resources that can be referred to in
	/// url's, create a method that matches <see cref="GetResource"/> delegate and tag it with
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
		private static readonly Dictionary<string, GetResource> providers;
		private readonly List<string> dependencies = new List<string>();

		static SageResourceResolver()
		{
			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
			providers = new Dictionary<string, GetResource>();

			foreach (Assembly a in Application.RelevantAssemblies)
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
							GetResource del = (GetResource) Delegate.CreateDelegate(typeof(GetResource), methodInfo);
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

		/// <inheritdoc/>
		public EntityResult GetEntity(UrlResolver parent, SageContext context, string resourceUri)
		{
			string resourceName = GetResourceName(resourceUri);
			CacheableXmlDocument resourceDoc = null;
			if (providers.ContainsKey(resourceName))
				resourceDoc = providers[resourceName](context, resourceUri);

			if (resourceDoc == null)
			{
				if (providers.Count == 0)
				{
					throw new ArgumentException(string.Format("The resource with uri '{0}' could not be resolved. There are currently no resource providers registered, so any call to '{1}'... will fail.", resourceUri, GetResourcePrefix()));
				}

				throw new ArgumentException(string.Format("The resource with uri '{0}' could not be resolved. The supported uris are: \n{1}", resourceUri, string.Join("\n", GetValidResourceUris())));
			}

			XmlReaderSettings settings = CacheableXmlDocument.CreateReaderSettings(parent);
			XmlReader reader = XmlReader.Create(new StringReader(resourceDoc.OuterXml), settings, resourceUri);
			return new EntityResult { Entity = reader, Dependencies = resourceDoc.Dependencies };
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
