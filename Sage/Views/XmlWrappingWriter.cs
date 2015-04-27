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
namespace Sage.Views
{
	using System;
	using System.Xml;

	/// <summary>
	/// Implements an <see cref="XmlWriter"/> that wraps another <see cref="XmlWriter"/>.
	/// </summary>
	public class XmlWrappingWriter : XmlWriter
	{
		private XmlWriter writer; 

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlWrappingWriter"/> class.
		/// </summary>
		/// <param name="baseWriter">The base writer this writer is wrapping.</param>
		public XmlWrappingWriter(XmlWriter baseWriter)
		{
			this.Writer = baseWriter;
		}

		/// <inheritdoc/>
		public override XmlWriterSettings Settings
		{
			get
			{
				return writer.Settings;
			}
		}

		/// <inheritdoc/>
		public override System.Xml.WriteState WriteState
		{
			get
			{
				return writer.WriteState;
			}
		}

		/// <inheritdoc/>
		public override string XmlLang
		{
			get
			{
				return writer.XmlLang;
			}
		}

		/// <inheritdoc/>
		public override System.Xml.XmlSpace XmlSpace
		{
			get
			{
				return writer.XmlSpace;
			}
		}

		/// <inheritdoc/>
		protected XmlWriter Writer
		{
			get
			{
				return writer;
			}

			set
			{
				writer = value;
			}
		}

		/// <inheritdoc/>
		public override void Close()
		{
			writer.Close();
		}

		/// <inheritdoc/>
		public override void Flush()
		{
			writer.Flush();
		}

		/// <inheritdoc/>
		public override string LookupPrefix(string ns)
		{
			return writer.LookupPrefix(ns);
		}

		/// <inheritdoc/>
		public override void WriteBase64(byte[] buffer, int index, int count)
		{
			writer.WriteBase64(buffer, index, count);
		}

		/// <inheritdoc/>
		public override void WriteCData(string text)
		{
			writer.WriteCData(text);
		}

		/// <inheritdoc/>
		public override void WriteCharEntity(char ch)
		{
			writer.WriteCharEntity(ch);
		}

		/// <inheritdoc/>
		public override void WriteChars(char[] buffer, int index, int count)
		{
			writer.WriteChars(buffer, index, count);
		}

		/// <inheritdoc/>
		public override void WriteComment(string text)
		{
			writer.WriteComment(text);
		}

		/// <inheritdoc/>
		public override void WriteDocType(string name, string pubId, string sysId, string subset)
		{
			writer.WriteDocType(name, pubId, sysId, subset);
		}

		/// <inheritdoc/>
		public override void WriteEndAttribute()
		{
			writer.WriteEndAttribute();
		}

		/// <inheritdoc/>
		public override void WriteEndDocument()
		{
			writer.WriteEndDocument();
		}

		/// <inheritdoc/>
		public override void WriteEndElement()
		{
			writer.WriteEndElement();
		}

		/// <inheritdoc/>
		public override void WriteEntityRef(string name)
		{
			writer.WriteEntityRef(name);
		}

		/// <inheritdoc/>
		public override void WriteFullEndElement()
		{
			writer.WriteFullEndElement();
		}

		/// <inheritdoc/>
		public override void WriteProcessingInstruction(string name, string text)
		{
			writer.WriteProcessingInstruction(name, text);
		}

		/// <inheritdoc/>
		public override void WriteRaw(string data)
		{
			writer.WriteRaw(data);
		}

		/// <inheritdoc/>
		public override void WriteRaw(char[] buffer, int index, int count)
		{
			writer.WriteRaw(buffer, index, count);
		}

		/// <inheritdoc/>
		public override void WriteStartAttribute(string prefix, string localName, string ns)
		{
			writer.WriteStartAttribute(prefix, localName, ns);
		}

		/// <inheritdoc/>
		public override void WriteStartDocument()
		{
			writer.WriteStartDocument();
		}

		/// <inheritdoc/>
		public override void WriteStartDocument(bool standalone)
		{
			writer.WriteStartDocument(standalone);
		}

		/// <inheritdoc/>
		public override void WriteStartElement(string prefix, string localName, string ns)
		{
			writer.WriteStartElement(prefix, localName, ns);
		}

		/// <inheritdoc/>
		public override void WriteString(string text)
		{
			writer.WriteString(text);
		}

		/// <inheritdoc/>
		public override void WriteSurrogateCharEntity(char lowChar, char highChar)
		{
			writer.WriteSurrogateCharEntity(lowChar, highChar);
		}

		/// <inheritdoc/>
		public override void WriteValue(bool value)
		{
			writer.WriteValue(value);
		}

		/// <inheritdoc/>
		public override void WriteValue(DateTime value)
		{
			writer.WriteValue(value);
		}

		/// <inheritdoc/>
		public override void WriteValue(decimal value)
		{
			writer.WriteValue(value);
		}

		/// <inheritdoc/>
		public override void WriteValue(double value)
		{
			writer.WriteValue(value);
		}

		/// <inheritdoc/>
		public override void WriteValue(int value)
		{
			writer.WriteValue(value);
		}

		/// <inheritdoc/>
		public override void WriteValue(long value)
		{
			writer.WriteValue(value);
		}

		/// <inheritdoc/>
		public override void WriteValue(object value)
		{
			writer.WriteValue(value);
		}

		/// <inheritdoc/>
		public override void WriteValue(float value)
		{
			writer.WriteValue(value);
		}

		/// <inheritdoc/>
		public override void WriteValue(string value)
		{
			writer.WriteValue(value);
		}

		/// <summary>
		/// When overridden in a derived class, writes out the given white space.
		/// </summary>
		/// <param name="ws">The string of white space characters.</param>
		public override void WriteWhitespace(string ws)
		{
			writer.WriteWhitespace(ws);
		}

		/// <inheritdoc/>
		protected override void Dispose(bool disposing)
		{
			((IDisposable) writer).Dispose();
		}
	}
}
