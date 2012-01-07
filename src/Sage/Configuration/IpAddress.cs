namespace Sage.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Net;
	using System.Xml;

	/// <summary>
	/// Provides a structure for storing and comparing IP addresses and ranges.
	/// </summary>
	public struct IpAddress
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="IpAddress"/> structure, using the specified 
		/// <paramref name="address"/> string to initialize it's <see cref="Address"/> property.
		/// </summary>
		/// <param name="address">The IP address as a string (e.g. '204.27.198.20').</param>
		public IpAddress(string address)
			: this()
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(address));

			this.Address = IpAddress.FromString(address);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IpAddress"/> structure, using the specified 
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
		/// Initializes a new instance of the <see cref="IpAddress"/> struct.
		/// </summary>
		/// <param name="configElement">The configuration element that defines this IP address.</param>
		public IpAddress(XmlElement configElement)
			: this()
		{
			string addressAttrib = configElement.GetAttribute("address");
			string toAttrib = configElement.GetAttribute("to");

			this.Address = IpAddress.FromString(addressAttrib);
			if (!string.IsNullOrEmpty(toAttrib))
				this.To = IpAddress.FromString(toAttrib);
		}

		/// <summary>
		/// Gets the 32bit integer version of the lower bound IP address.
		/// </summary>
		public uint Address
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the 32bit integer version of the higher bound IP address. For single IP addresses this value is zero (0).
		/// </summary>
		public uint To
		{
			get;
			private set;
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

		/// <summary>
		/// Returns a <see cref="String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			if (this.To != 0)
				return string.Format("{0} to {1}",
					FromNumber(this.Address),
					FromNumber(this.To));

			return FromNumber(this.Address);
		}

		private static string FromNumber(uint address)
		{
			IPAddress ip = new IPAddress(address);

			List<string> parts = new List<string>(ip.ToString().Split('.'));
			parts.Reverse();

			return string.Join(".", parts.ToArray());
		}
	}
}
