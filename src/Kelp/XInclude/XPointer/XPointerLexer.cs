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
 * 
 * Original source for XPointer released under BSD licence, hence the disclaimer:
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
 * FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
 * COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
 * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */
namespace Kelp.XInclude.XPointer
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.Text;
	using System.Xml;

	using Kelp.Properties;

	/// <summary>
	/// XPointer lexical analyzer.
	/// </summary>
	internal class XPointerLexer
	{
		private readonly string xpointer;
		private char currChar;
		private int pointerIndex;

		public XPointerLexer(string xpointer)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(xpointer));

			this.xpointer = xpointer;
			this.NextChar();
		}

		public enum LexKind
		{
			NCName = 'N', 
			QName = 'Q', 
			LRBracket = '(', 
			RRBracket = ')', 
			Circumflex = '^', 
			Number = 'd', 
			Eq = '=', 
			Space = 'S', 
			Slash = '/', 
			EscapedData = 'D', 
			Eof = 'E'
		}

		public bool CanBeSchemaName { get; private set; }

		public LexKind Kind { get; private set; }

		public string NCName { get; private set; }

		public int Number { get; private set; }

		public string Prefix { get; private set; }

		public bool NextChar()
		{
			if (this.pointerIndex < this.xpointer.Length)
			{
				this.currChar = this.xpointer[this.pointerIndex++];
				return true;
			}

			this.currChar = '\0';
			return false;
		}

		public bool NextLexeme()
		{
			switch (this.currChar)
			{
				case '\0':
					this.Kind = LexKind.Eof;
					return false;
				case '(':
				case ')':
				case '=':
				case '/':
					this.Kind = (LexKind)Convert.ToInt32(this.currChar);
					this.NextChar();
					break;
				case '^':
					this.NextChar();
					if (this.currChar == '^' || this.currChar == '(' || this.currChar == ')')
					{
						this.Kind = LexKind.EscapedData;
						this.NextChar();
					}
					else
					{
						throw new XPointerSyntaxException(Resources.CircumflexCharMustBeEscaped);
					}

					break;
				default:
					if (char.IsDigit(this.currChar))
					{
						this.Kind = LexKind.Number;
						int start = this.pointerIndex - 1;
						int len = 0;
						while (char.IsDigit(this.currChar))
						{
							this.NextChar();
							len++;
						}

						this.Number = XmlConvert.ToInt32(this.xpointer.Substring(start, len));
						break;
					}

					if (LexUtils.IsStartNameChar(this.currChar))
					{
						this.Kind = LexKind.NCName;
						this.Prefix = string.Empty;
						this.NCName = this.ParseName();
						if (this.currChar == ':')
						{
							// QName?
							this.NextChar();
							this.Prefix = this.NCName;
							this.Kind = LexKind.QName;
							if (LexUtils.IsStartNCNameChar(this.currChar))
							{
								this.NCName = this.ParseName();
							}
							else
							{
								throw new XPointerSyntaxException(
									string.Format(CultureInfo.CurrentCulture, Resources.InvalidNameToken, this.Prefix, this.currChar));
							}
						}

						this.CanBeSchemaName = this.currChar == '(';
						break;
					}

					if (LexUtils.IsWhitespace(this.currChar))
					{
						this.Kind = LexKind.Space;
						while (LexUtils.IsWhitespace(this.currChar))
						{
							this.NextChar();
						}

						break;
					}

					this.Kind = LexKind.EscapedData;
					break;
			}

			return true;
		}

		public string ParseEscapedData()
		{
			int depth = 0;
			var sb = new StringBuilder();
			while (true)
			{
				switch (this.currChar)
				{
					case '^':
						if (!this.NextChar())
						{
							throw new XPointerSyntaxException(Resources.UnexpectedEndOfSchemeData);
						}

						if (this.currChar == '^' || this.currChar == '(' || this.currChar == ')')
						{
							sb.Append(this.currChar);
						}
						else
						{
							throw new XPointerSyntaxException(Resources.CircumflexCharMustBeEscaped);
						}

						break;
					case '(':
						depth++;
						goto default;
					case ')':
						if (depth-- == 0)
						{
							// Skip ')'
							this.NextLexeme();
							return sb.ToString();
						}

						goto default;

					default:
						sb.Append(this.currChar);
						break;
				}

				if (!this.NextChar())
				{
					throw new XPointerSyntaxException(Resources.UnexpectedEndOfSchemeData);
				}
			}
		}

		public void SkipWhiteSpace()
		{
			while (LexUtils.IsWhitespace(this.currChar))
			{
				this.NextChar();
			}
		}

		private string ParseName()
		{
			int start = this.pointerIndex - 1;
			int len = 0;
			while (LexUtils.IsNCNameChar(this.currChar))
			{
				this.NextChar();
				len++;
			}

			return this.xpointer.Substring(start, len);
		}
	}
}