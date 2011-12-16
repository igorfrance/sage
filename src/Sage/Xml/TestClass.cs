namespace Sage.Xml
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Xml;

	/// <summary>
	/// Implements an <see cref="XmlWriter"/> that wraps another <see cref="XmlWriter"/>.
	/// </summary>
	public class TestClass : XmlWriter
	{
		public override void WriteStartDocument()
		{
			throw new NotImplementedException();
		}

		public override void WriteStartDocument(bool standalone)
		{
			throw new NotImplementedException();
		}

		public override void WriteEndDocument()
		{
			throw new NotImplementedException();
		}

		[System.Diagnostics.Contracts.ContractVerification(false)]
		public override void WriteDocType(string name, string pubid, string sysid, string subset)
		{
			throw new NotImplementedException();
		}

		[System.Diagnostics.Contracts.ContractVerification(false)]
		public override void WriteStartElement(string prefix, string localName, string ns)
		{
			throw new NotImplementedException();
		}

		public override void WriteEndElement()
		{
			throw new NotImplementedException();
		}

		public override void WriteFullEndElement()
		{
			throw new NotImplementedException();
		}

		public override void WriteStartAttribute(string prefix, string localName, string ns)
		{
			throw new NotImplementedException();
		}

		public override void WriteEndAttribute()
		{
			throw new NotImplementedException();
		}

		public override void WriteCData(string text)
		{
			throw new NotImplementedException();
		}

		public override void WriteComment(string text)
		{
			throw new NotImplementedException();
		}

		public override void WriteProcessingInstruction(string name, string text)
		{
			throw new NotImplementedException();
		}

		public override void WriteEntityRef(string name)
		{
			throw new NotImplementedException();
		}

		public override void WriteCharEntity(char ch)
		{
			throw new NotImplementedException();
		}

		public override void WriteWhitespace(string ws)
		{
			throw new NotImplementedException();
		}

		public override void WriteString(string text)
		{
			throw new NotImplementedException();
		}

		public override void WriteSurrogateCharEntity(char lowChar, char highChar)
		{
			throw new NotImplementedException();
		}

		[ContractVerification(false)]
		public override void WriteChars(char[] buffer, int index, int count)
		{
			throw new NotImplementedException();
		}

		public override void WriteRaw(char[] buffer, int index, int count)
		{
			throw new NotImplementedException();
		}

		public override void WriteRaw(string data)
		{
			throw new NotImplementedException();
		}

		public override void WriteBase64(byte[] buffer, int index, int count)
		{
			throw new NotImplementedException();
		}

		public override void Close()
		{
			throw new NotImplementedException();
		}

		public override void Flush()
		{
			throw new NotImplementedException();
		}

		public override string LookupPrefix(string ns)
		{
			throw new NotImplementedException();
		}

		public override WriteState WriteState
		{
			get
			{
				throw new NotImplementedException();
			}
		}
	}
}
