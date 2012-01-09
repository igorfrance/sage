namespace Sage.Extensibility
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml;

	using Kelp.Core.Extensions;

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
			logElement.SetAttribute("dateTime", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss"));
			logElement.SetAttribute("result", this.Result.ToString());

			XmlElement filesElement = logElement.AppendElement("files");
			foreach (InstallItem file in this.Items)
			{
				XmlElement fileElement = filesElement.AppendElement("file");
				fileElement.SetAttribute("path", file.Path);
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
