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
	using System.Diagnostics.Contracts;
	using System.Net;
	using System.Xml;

	using Kelp;

	/// <summary>
	/// Provides a structure for storing and comparing IP addresses and ranges.
	/// </summary>
	public class IpAddress : IXmlConvertible
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="IpAddress"/> class, using the specified
		/// <paramref name="address"/> string to initialize it's <see cref="Address"/> property.
		/// </summary>
		/// <param name="address">The IP address as a string (e.g. '204.27.198.20').</param>
		public IpAddress(string address)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(address));

			this.Address = IpAddress.FromString(address);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IpAddress"/> class, using the specified
		/// <paramref name="address"/> and <paramref name="to"/> strings to initialize it's 
		/// <see cref="Address"/> and <see cref="To"/> properties.
		/// </summary>
		/// <param name="address">The begin IP address of a range, represented with a string (e.g. '204.27.198.20').</param>
		/// <param name="to">The end IP address of a range, represented with a string (e.g. '204.27.198.60').</param>
		public IpAddress(string address, string to)
			: this(address)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(address));

			this.To = IpAddress.FromString(to);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IpAddress"/> class.
		/// </summary>
		/// <param name="configElement">The configuration element that defines this IP address.</param>
		public IpAddress(XmlElement configElement)
		{
			Contract.Requires<ArgumentNullException>(configElement != null);

			this.Parse(configElement);
		}

		/// <summary>
		/// Gets the 32bit integer version of the lower bound IP address.
		/// </summary>
		public uint Address { get; private set; }

		/// <summary>
		/// Gets the 32bit integer version of the higher bound IP address. For single IP addresses this value is zero (0).
		/// </summary>
		public uint To { get; private set; }

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The value to the left of the operator.</param>
		/// <param name="right">The value to the right of the operator.</param>
		/// <returns>
		/// <c>true</c> if the two values are equal; otherwise <c>false</c>.
		/// </returns>
		public static bool operator ==(IpAddress left, IpAddress right)
		{
			return object.Equals(left, right);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="left">The value to the left of the operator.</param>
		/// <param name="right">The value to the right of the operator.</param>
		/// <returns>
		/// <c>true</c> if the two values are NOT equal; otherwise <c>false</c>.
		/// </returns>
		public static bool operator !=(IpAddress left, IpAddress right)
		{
			return !object.Equals(left, right);
		}

		/// <summary>
		/// Converts the specified <paramref name="address"/> from string to a 32bit integer.
		/// </summary>
		/// <param name="address">The IP address string to convert.</param>
		/// <returns>The specified <paramref name="address"/> from string to a 32bit integer.</returns>
		public static uint FromString(string address)
		{
			if (string.IsNullOrEmpty(address))
				return 0;

			if (address == "::1")
				address = "127.0.0.1";

			IPAddress ip;
			try
			{
				ip = IPAddress.Parse(address);
			}
			catch (FormatException)
			{
				throw new ArgumentException(
					string.Format("The IP address '{0}' is invalid and cannot be parsed as either IPv4 or IPv6 format", address), "address");
			}

			byte[] ipbytes = ip.GetAddressBytes();

			uint result = (uint)ipbytes[0] << 24;
			result += (uint) ipbytes[1] << 16;
			result += (uint) ipbytes[2] << 8;
			result += (uint) ipbytes[3];

			return result;
		}

		/// <summary>
		/// Gets the string representation of the specified <paramref name="address"/>.
		/// </summary>
		/// <param name="address">The address to convert.</param>
		/// <returns>The specified <paramref name="address"/> converted to a string value, such as "192.154.11.8".</returns>
		public static string FromNumber(uint address)
		{
			IPAddress ip = new IPAddress(address);

			List<string> parts = new List<string>(ip.ToString().Split('.'));
			parts.Reverse();

			return string.Join(".", parts.ToArray());
		}

		/// <inheritdoc/>
		public void Parse(XmlElement element)
		{
			string addressAttrib = element.GetAttribute("address");
			string toAttrib = element.GetAttribute("to");

			this.Address = IpAddress.FromString(addressAttrib);
			if (!string.IsNullOrEmpty(toAttrib))
				this.To = IpAddress.FromString(toAttrib);
		}

		/// <inheritdoc/>
		public XmlElement ToXml(XmlDocument document)
		{
			XmlElement result = document.CreateElement("ip", Sage.XmlNamespaces.ProjectConfigurationNamespace);
			result.SetAttribute("address", IpAddress.FromNumber(this.Address));
			if (this.To != 0)
				result.SetAttribute("to", IpAddress.FromNumber(this.To));

			return result;
		}

		/// <summary>
		/// Matches this <see cref="IpAddress"/> with the specified <paramref name="address"/>.
		/// </summary>
		/// <param name="address">The IP address to test.</param>
		/// <returns><c>true</c> if the specified <paramref name="address"/> matches this <see cref="IpAddress"/>.</returns>
		public bool Matches(string address)
		{
			if (string.IsNullOrEmpty(address))
				return false;

			uint test = IpAddress.FromString(address);
			if (this.To == 0)
				return test == this.Address;

			return Address <= test && test <= To;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			if (this.To != 0)
				return string.Format("{0} to {1}",
					FromNumber(this.Address),
					FromNumber(this.To));

			return FromNumber(this.Address);
		}

		/// <summary>
		/// Compares this ip address with another ip address.
		/// </summary>
		/// <param name="other">The ip address to compare this ip address with.</param>
		/// <returns>
		/// <c>true</c> if the two addresses are equal; otherwise <c>false</c>.
		/// </returns>
		public bool Equals(IpAddress other)
		{
			return
				object.Equals(other.Address, this.Address) &&
				object.Equals(other.To, this.To);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return this.Equals((IpAddress) obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			unchecked
			{
				int result = this.Address.GetHashCode();
				result = (result * 397) ^ this.To.GetHashCode();
				return result;
			}
		}
	}
}
