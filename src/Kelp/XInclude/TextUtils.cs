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
namespace Kelp.XInclude
{
	/// <summary>
	/// Text inclusion related utility methods.	
	/// </summary>
	/// <author>Oleg Tkachenko, http://www.xmllab.net</author>
	internal class TextUtils
	{
		/// <summary>
		/// Checks value of the 'accept' attribute for validity.
		/// Characters must be in #x20 through #x7E range.
		/// </summary>        
		public static void CheckAcceptValue(string accept)
		{
			foreach (char c in accept)
			{
				if (c < 0x0020 || c > 0x007E)
				{
					throw new InvalidAcceptHTTPHeaderValueError(c);
				}
			}
		}

		/// <summary>
		/// Checks string for a presense of characters, 
		/// not permitted in XML 1.0 documents.
		/// </summary>
		/// <param name="str">Input string to check.</param>
		/// <exception cref="NonXmlCharacterException">Given string contains a character,
		/// forbidden in XML 1.0.</exception>        
		public static void CheckForNonXmlChars(string str)
		{
			int i = 0;
			while (i < str.Length)
			{
				char c = str[i];

				// Allowed unicode XML characters
				if ((c < 0x0020 || c > 0xD7FF) && (c < 0xE000 || c > 0xFFFD) && c != 0xA && c != 0xD && c != 0x9)
				{
					if (c >= 0xd800 && c <= 0xdbff)
					{
						// Looks like first char in a surrogate pair, check second one
						if (++i < str.Length)
						{
							if (str[i] >= 0xdc00 && str[i] <= 0xdfff)
							{
								// Ok, valid surrogate pair
								i++;
							}

							continue;
						}
					}
				}
				else
				{
					// Ok, approved.
					i++;
					continue;
				}

				throw new NonXmlCharacterException(str[i]);
			}
		}
	}
}