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
namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Xml;

	using Kelp;

	using log4net;
	using Sage.Extensibility;

	/// <summary>
	/// Implements a custom resolver for various resource providers.
	/// </summary>
	/// <remarks>
	/// In order to hook into this resolver and provide custom resources that can be referred to in
	/// url's, create a method that matches <see cref="XmlProvider"/> delegate and tag it with
	/// <see cref="XmlProviderAttribute"/>. The name that you specify in the constructor for
	/// <c>XmlProviderAttribute</c> is the string that follows <c>sageres://</c>.
	/// </remarks>
	[UrlResolver(Scheme = SageResourceResolver.Scheme)]
	public class SageResourceResolver : IUrlResolver
	{
		/// <summary>
		/// The scheme associated with this resolver;
		/// </summary>
		public const string Scheme = "sageres";
		private const BindingFlags AttributeFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

		private static readonly ILog log = LogManager.GetLogger(typeof(SageResourceResolver).FullName);
		private static readonly Dictionary<string, XmlProvider> providers;

		static SageResourceResolver()
		{
			providers = new Dictionary<string, XmlProvider>();

			foreach (Assembly a in Project.RelevantAssemblies)
			{
				var types = from t in a.GetTypes()
							where
								t.IsClass &&
								t.GetMethods(AttributeFlags).Count(m => m.GetCustomAttributes(typeof(XmlProviderAttribute), false).Count() != 0) != 0
							select t;

				foreach (Type type in types)
				{
					foreach (MethodInfo methodInfo in type.GetMethods(AttributeFlags))
					{
						foreach (XmlProviderAttribute attrib in methodInfo.GetCustomAttributes(typeof(XmlProviderAttribute), false))
						{
							XmlProvider del = (XmlProvider) Delegate.CreateDelegate(typeof(XmlProvider), methodInfo);
							if (providers.ContainsKey(attrib.ResourceName))
							{
								log.WarnFormat("Overwriting existing resource provider '{0}' for resource name '{1}' with provider '{2}'",
									Util.GetMethodSignature(providers[attrib.ResourceName].Method),
									attrib.ResourceName,
									Util.GetMethodSignature(del.Method));
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
				XmlReaderSettings settings = CacheableXmlDocument.CreateReaderSettings(parent);

			CacheableXmlDocument resourceDoc;

			// first check if we have a registered provider for the specified resour name
			if (providers.ContainsKey(resourceName))
			{
				XmlProvider provider = providers[resourceName];
				log.DebugFormat("Found a specific resource provider for {0}: {1}",
					resourceUri,
					Util.GetMethodSignature(provider.Method));

				resourceDoc = provider(context, resourceUri);
			}
			else
			{
				string sourcePath = context.Path.Expand(resourceName);
				if (sourcePath == null || !File.Exists(sourcePath))
					throw new FileNotFoundException(string.Format("The specified resource '{0}' doesn't exist.", resourceUri));

				resourceDoc = context.Resources.LoadXml(sourcePath);
			}

			XmlReader reader = XmlReader.Create(new StringReader(resourceDoc.OuterXml), settings, resourceUri);
			return new EntityResult { Entity = reader, Dependencies = resourceDoc.Dependencies };
		}

		private string GetResourceName(string uri)
		{
			return uri.Replace(GetResourcePrefix(), string.Empty).Trim('/', '\\');
		}

		private string GetResourcePrefix()
		{
			return Scheme + "://";
		}
	}
}
