/**
 * Open Source Initiative OSI - The MIT License (MIT):Licensing
 * [OSI Approved License]
 * The MIT License (MIT)
 *
 * Copyright (c) 2011 Igor France
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */
namespace Sage.Extensibility
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml;

	using Kelp.Core.Extensions;
	using Kelp.Extensions;

	internal class InstallLog
	{
		public InstallLog(XmlElement element)
		{
			string dateString = element.GetAttribute("dateTime");
			string resultString = element.GetAttribute("result");

			if (!string.IsNullOrWhiteSpace(dateString))
			{
				DateTime dateTime;
				if (DateTime.TryParse(dateString, out dateTime))
					this.Date = dateTime;
			}

			if (!string.IsNullOrWhiteSpace(resultString))
			{
				InstallState result;
				if (Enum.TryParse(resultString, true, out result))
					this.Result = result;
			}

			this.Items = new List<InstallItem>();
			foreach (XmlElement fileElem in element.SelectNodes("files/file"))
			{
				InstallItem file = this.AddFile(fileElem.GetAttribute("path"));
				file.CrcCode = fileElem.GetAttribute("crc");
				string stateString = fileElem.GetAttribute("state");
				if (!string.IsNullOrWhiteSpace(stateString))
				{
					InstallState state;
					if (Enum.TryParse(resultString, true, out state))
						file.State = state;
				}
			}
		}

		public InstallLog(DateTime date)
		{
			this.Date = date;
			this.Items = new List<InstallItem>();
		}

		public DateTime? Date { get; private set; }

		public Exception Error { get; set; }

		public InstallState Result { get; set; }

		public List<InstallItem> Items { get; private set; }

		public InstallItem AddFile(string path)
		{
			InstallItem file = new InstallItem { Path = path };
			this.Items.Add(file);
			return file;
		}

		public XmlElement ToXml(XmlDocument ownerDoc)
		{
			XmlElement logElement = ownerDoc.CreateElement("install");
			logElement.SetAttribute("dateTime", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss.ffff"));
			logElement.SetAttribute("result", this.Result.ToString());

			XmlElement filesElement = logElement.AppendElement("files");
			foreach (InstallItem file in this.Items)
			{
				XmlElement fileElement = filesElement.AppendElement("file");
				fileElement.SetAttribute("path", file.Path);
				fileElement.SetAttribute("crc", file.CrcCode);
				fileElement.SetAttribute("state", file.State.ToString().ToLower());
			}

			if (this.Error != null)
			{
				XmlElement errorElement = logElement.AppendElement("exception");
				errorElement.SetAttribute("message", this.Error.InnermostExceptionMessage());
				errorElement.SetAttribute("type", this.Error.InnermostExceptionTypeName());
				errorElement.InnerText = this.Error.InnermostExceptionStackTrace();
			}

			return logElement;
		}
	}
}
