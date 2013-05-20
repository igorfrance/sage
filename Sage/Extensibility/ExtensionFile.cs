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
namespace Sage.Extensibility
{
	using System.IO;

	using ICSharpCode.SharpZipLib.Core;
	using ICSharpCode.SharpZipLib.Zip;

	using Kelp.IO;

	/// <summary>
	/// Represents an extension file.
	/// </summary>
	internal class ExtensionFile
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ExtensionFile"/> class.
		/// </summary>
		/// <param name="extensionArchive">The extension archive.</param>
		/// <param name="file">The file.</param>
		public ExtensionFile(ZipFile extensionArchive, ZipEntry file)
		{
			this.CrcCode = Crc32.GetHash(extensionArchive.GetInputStream(file));
			this.Entry = file;
			this.Name = file.Name;
		}

		/// <summary>
		/// Gets the CRC code of this file.
		/// </summary>
		public string CrcCode { get; private set; }

		/// <summary>
		/// Gets the name of this file.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the <see cref="ZipEntry"/> that corresponds to this file.
		/// </summary>
		public ZipEntry Entry { get; private set; }

		public void Extract(ZipFile extensionArchive, string targetPath)
		{
			byte[] buffer = new byte[4096]; //// 4K is optimum
			Stream zipStream = extensionArchive.GetInputStream(this.Entry);

			string directoryName = Path.GetDirectoryName(targetPath);
			if (directoryName.Length > 0)
				Directory.CreateDirectory(directoryName);

			//// Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
			//// of the file, but does not waste memory.
			using (FileStream streamWriter = File.Create(targetPath))
			{
				StreamUtils.Copy(zipStream, streamWriter, buffer);
			}
		}

		public byte[] Read(ZipFile extensionArchive)
		{
			using (Stream stream = extensionArchive.GetInputStream(this.Entry))
			{
				MemoryStream memory = new MemoryStream();
				byte[] writeData = new byte[4096];

				while (true)
				{
					int size = stream.Read(writeData, 0, writeData.Length);
					if (size > 0)
					{
						memory.Write(writeData, 0, size);
					}
					else break;
				}

				stream.Close();
				return memory.ToArray();
			}
		}
	}
}
