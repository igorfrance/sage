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
namespace Kelp
{
	using System.Xml;

	/// <summary>
	/// Defines the namespaces and the namespace manager in use throughout the system.
	/// </summary>
	public static class XmlNamespaces
	{
		/// <summary>
		/// Defines the prefix for the main Kelp namespace.
		/// </summary>
		public const string KelpNsPrefix = "kelp";

		/// <summary>
		/// Defines the main Kelp namespace.
		/// </summary>
		public const string KelpNamespace = "http://www.cycle99.com/projects/kelp";

		private static volatile XmlNamespaceManager nsman;

		/// <summary>
		/// Gets the <see cref="XmlNamespaceManager"/> that can be used everywhere where selecting with namespaces needs to be done.
		/// </summary>
		public static XmlNamespaceManager Manager
		{
			get
			{
				if (nsman == null)
				{
					lock (KelpNsPrefix)
					{
						if (nsman == null)
						{
							var temp = new XmlNamespaceManager(new NameTable());
							temp.AddNamespace(KelpNsPrefix, KelpNamespace);

							nsman = temp;
						}
					}
				}

				return nsman;
			}
		}
	}
}
