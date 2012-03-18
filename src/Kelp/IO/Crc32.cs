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
namespace Kelp.IO
{
	using System;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Security.Cryptography;

	/// <summary>
	/// Implements CRC32 hashing algorithm.
	/// </summary>
	public class Crc32 : HashAlgorithm
	{
		private const uint DefaultPolynomial = 0xedb88320;
		private const uint DefaultSeed = 0xffffffff;

		private readonly uint seed;
		private readonly uint[] table;
		private static readonly Crc32 crc32 = new Crc32();
		private static uint[] defaultTable;
		private uint hash;

		/// <summary>
		/// Initializes a new instance of the <see cref="Crc32"/> class.
		/// </summary>
		public Crc32()
		{
			table = InitializeTable(DefaultPolynomial);
			seed = DefaultSeed;
			Initialize();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Crc32"/> class, using the specified <paramref name="polynomial"/> and <paramref name="seed"/>.
		/// </summary>
		/// <param name="polynomial">The polynomial.</param>
		/// <param name="seed">The seed.</param>
		public Crc32(uint polynomial, uint seed)
		{
			table = InitializeTable(polynomial);
			this.seed = seed;
			Initialize();
		}

		/// <summary>
		/// Gets the size, in bits, of the computed hash code.
		/// </summary>
		/// <returns>The size, in bits, of the computed hash code.</returns>
		public override int HashSize
		{
			get { return 32; }
		}

		/// <summary>
		/// Calculates and returns the CRC32 hash of the file located at the specified <paramref name="filePath"/>.
		/// </summary>
		/// <param name="filePath">The path to the file for which to calculate the hash.</param>
		/// <returns>The CRC32 hash of the file located at the specified <paramref name="filePath"/>.</returns>
		public static string GetHash(string filePath)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(filePath));

			using (Stream fs = File.Open(filePath, FileMode.Open))
				return GetHash(fs);
		}

		/// <summary>
		/// Calculates and returns the CRC32 hash of the file located at the specified <paramref name="bytes"/>.
		/// </summary>
		/// <param name="bytes">The byte stream for which to calculate the hash.</param>
		/// <returns>The CRC32 hash of the file located at the specified <paramref name="bytes"/>.</returns>
		public static string GetHash(Stream bytes)
		{
			Contract.Requires<ArgumentNullException>(bytes != null);

			string[] hashString = new string[8];
			byte[] hashBytes = crc32.ComputeHash(bytes);

			for (int i = 0; i < hashBytes.Length; i++)
				hashString[i] = hashBytes[i].ToString("x2").ToLower();

			return string.Join(string.Empty, hashString);
		}

		/// <inheritdoc/>
		public override sealed void Initialize()
		{
			hash = seed;
		}

		/// <inheritdoc/>
		protected override void HashCore(byte[] buffer, int start, int length)
		{
			hash = CalculateHash(table, hash, buffer, start, length);
		}

		/// <inheritdoc/>
		protected override byte[] HashFinal()
		{
			byte[] hashBuffer = UInt32ToBigEndianBytes(~hash);
			this.HashValue = hashBuffer;
			return hashBuffer;
		}

		private static uint[] InitializeTable(uint polynomial)
		{
			if (polynomial == DefaultPolynomial && defaultTable != null)
				return defaultTable;

			uint[] createTable = new uint[256];
			for (int i = 0; i < 256; i++)
			{
				uint entry = (uint) i;
				for (int j = 0; j < 8; j++)
					if ((entry & 1) == 1)
						entry = (entry >> 1) ^ polynomial;
					else
						entry = entry >> 1;
				createTable[i] = entry;
			}

			if (polynomial == DefaultPolynomial)
				defaultTable = createTable;

			return createTable;
		}

		private static uint CalculateHash(uint[] table, uint seed, byte[] buffer, int start, int size)
		{
			uint crc = seed;
			for (int i = start; i < size; i++)
				unchecked
				{
					crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
				}

			return crc;
		}

		private static byte[] UInt32ToBigEndianBytes(uint x)
		{
			return new[]
			{
				(byte)((x >> 24) & 0xff),
				(byte)((x >> 16) & 0xff),
				(byte)((x >> 8) & 0xff),
				(byte)(x & 0xff)
			};
		}
	}
}
