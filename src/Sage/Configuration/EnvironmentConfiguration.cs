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
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Xml;

	using Kelp;

	using XmlNamespaces = Sage.XmlNamespaces;

	/// <summary>
	/// Provides environment configuration options, such as IP address ranges.
	/// </summary>
	public class EnvironmentConfiguration : IXmlConvertible
	{
		private static bool allBlocked;
		private static bool allDeveloper;

		private readonly List<IpAddress> blockedIPs = new List<IpAddress>();
		private readonly List<IpAddress> developerIPs = new List<IpAddress>();

		/// <summary>
		/// Initializes a new instance of the <see cref="EnvironmentConfiguration" /> class.
		/// </summary>
		public EnvironmentConfiguration()
		{
			this.BlockedIPs = this.blockedIPs.AsReadOnly();
			this.DeveloperIPs = this.developerIPs.AsReadOnly();
		}

		/// <summary>
		/// Gets the IP addresses to be blocked.
		/// </summary>
		public ReadOnlyCollection<IpAddress> BlockedIPs { get; private set; }

		/// <summary>
		/// Gets the IP addresses of users that should be treated as developers.
		/// </summary>
		public ReadOnlyCollection<IpAddress> DeveloperIPs { get; private set; }

		/// <summary>
		/// Determines whether the specified <paramref name="clientIpAddress"/> is configured to be treated as a developer.
		/// </summary>
		/// <param name="clientIpAddress">The client IP address to test.</param>
		/// <returns>
		/// <c>true</c> if the specified <paramref name="clientIpAddress"/> is configured to be treated as a developer;
		/// otherwise, <c>false</c>.
		/// </returns>
		public bool IsDeveloperIp(string clientIpAddress)
		{
			if (allDeveloper)
				return true;

			return this.DeveloperIPs.Count(a => a.Matches(clientIpAddress)) != 0;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="clientIpAddress"/> is configured to be blocked.
		/// </summary>
		/// <param name="clientIpAddress">The client IP address to test.</param>
		/// <returns>
		/// <c>true</c> if the specified <paramref name="clientIpAddress"/> is configured to be blocked;
		/// otherwise, <c>false</c>.
		/// </returns>
		public bool IsBlockedIp(string clientIpAddress)
		{
			if (allBlocked)
				return true;

			return this.BlockedIPs.Count(a => a.Matches(clientIpAddress)) != 0;
		}

		/// <inheritdoc/>
		public void Parse(XmlElement configuration)
		{
			foreach (XmlElement element in configuration.SelectNodes("p:addresses/p:developers/p:ip", XmlNamespaces.Manager))
			{
				if (element.GetAttribute("address") == "*")
				{
					allDeveloper = true;
					continue;
				}

				var address = new IpAddress(element);
				if (!this.developerIPs.Contains(address))
					this.developerIPs.Add(address);
			}

			foreach (XmlElement element in configuration.SelectNodes("p:addresses/p:blocked/p:ip", XmlNamespaces.Manager))
			{
				if (element.GetAttribute("address") == "*")
				{
					allBlocked = true;
					continue;
				}

				var address = new IpAddress(element);
				if (!this.blockedIPs.Contains(address))
					this.blockedIPs.Add(address);
			}
		}

		/// <inheritdoc/>
		public XmlElement ToXml(XmlDocument document)
		{
			const string Ns = XmlNamespaces.ProjectConfigurationNamespace;
			XmlElement result = document.CreateElement("environment", Ns);

			XmlNode addresses = result.AppendChild(document.CreateElement("addresses", Ns));
			if (this.developerIPs.Count != 0)
			{
				XmlNode blockedNode = addresses.AppendChild(document.CreateElement("developers", Ns));
				foreach (IpAddress ip in this.developerIPs)
				{
					blockedNode.AppendChild(ip.ToXml(document));
				}
			}
			if (this.blockedIPs.Count != 0)
			{
				XmlNode blockedNode = addresses.AppendChild(document.CreateElement("blocked", Ns));
				foreach (IpAddress ip in this.blockedIPs)
				{
					blockedNode.AppendChild(ip.ToXml(document));
				}
			}

			return result;
		}
	}
}
