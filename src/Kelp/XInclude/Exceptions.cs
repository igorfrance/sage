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
	using System;
	using System.Globalization;

	using Kelp.Properties;

	/// <summary>
	/// Generic XInclude exception.	
	/// </summary>
	public abstract class XIncludeException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="XIncludeException"/> 
		/// class with a specified error message.
		/// </summary>
		/// <param name="message">Error message</param>
		protected XIncludeException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XIncludeException"/> 
		/// class with a specified error message and a reference to the 
		/// inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">Error message</param>
		/// <param name="innerException">Inner exceptiion</param>
		protected XIncludeException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}

	/// <summary>
	/// <c>FatalException</c> represents fatal error as per XInclude spcification.
	/// </summary>
	public abstract class FatalException : XIncludeException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FatalException"/> 
		/// class with a specified error message.
		/// </summary>
		/// <param name="message">Error message</param>
		protected FatalException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FatalException"/> 
		/// class with a specified error message and a reference to the 
		/// inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">Error message</param>
		/// <param name="innerException">Inner exceptiion</param>
		protected FatalException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}

	/// <summary>
	/// Non XML character in a document to include exception.
	/// </summary>
	public class NonXmlCharacterException : FatalException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NonXmlCharacterException"/> 
		/// class with a specified invalid character.
		/// </summary>
		/// <param name="c">Invalid character</param>
		public NonXmlCharacterException(char c)
			: base(string.Format(CultureInfo.CurrentCulture, Resources.NonXmlCharacter, ((int)c).ToString("X2")))
		{
		}
	}

	/// <summary>
	/// Circular inclusion exception.
	/// </summary>
	public class CircularInclusionException : FatalException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CircularInclusionException"/> 
		/// class with a specified Uri that causes inclusion loop.
		/// </summary>
		/// <param name="uri">Uri that causes inclusion loop</param>
		public CircularInclusionException(Uri uri)
			: base(string.Format(CultureInfo.CurrentCulture, Resources.CircularInclusion, uri.AbsoluteUri))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CircularInclusionException"/> 
		/// class with a specified Uri that causes inclusion loop and error location within
		/// XML document.
		/// </summary>
		/// <param name="uri">Uri that causes inclusion loop</param>
		/// <param name="line">Line number</param>
		/// <param name="locationUri">Location Uri</param>
		/// <param name="position">Column number</param>
		public CircularInclusionException(Uri uri, string locationUri, int line, int position)
			: base(string.Format(CultureInfo.CurrentCulture, Resources.CircularInclusionLong, uri.AbsoluteUri, locationUri, line, position))
		{
		}
	}

	/// <summary>
	/// Resource error not backed up by xi:fallback exception.
	/// </summary>	
	public class FatalResourceException : FatalException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FatalResourceException"/> 
		/// class with the specified inner exception that is the cause of this exception.
		/// </summary>        
		/// <param name="re">Inner exceptiion</param>
		public FatalResourceException(Exception re)
			: base(string.Format(CultureInfo.CurrentCulture, Resources.FatalResourceException, re.Message), re)
		{
		}
	}

	/// <summary>
	/// XInclude syntax error exception.
	/// </summary>
	public class XIncludeSyntaxError : FatalException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="XIncludeSyntaxError"/> 
		/// class with a specified error message.
		/// </summary>
		/// <param name="message">Error message</param>
		public XIncludeSyntaxError(string message)
			: base(message)
		{
		}
	}

	/// <summary>
	/// Include location identifies an attribute or namespace node.
	/// </summary>
	public class AttributeOrNamespaceInIncludeLocationError : FatalException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AttributeOrNamespaceInIncludeLocationError"/> 
		/// class with a specified error message.
		/// </summary>
		/// <param name="message">Error message</param>
		public AttributeOrNamespaceInIncludeLocationError(string message)
			: base(message)
		{
		}
	}

	/// <summary>
	/// Not wellformed inclusion result (e.g. top-level xi:include
	/// includes multiple elements).
	/// </summary>
	public class MalformedXInclusionResultError : FatalException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MalformedXInclusionResultError"/> 
		/// class with a specified error message.
		/// </summary>
		/// <param name="message">Error message</param>
		public MalformedXInclusionResultError(string message)
			: base(message)
		{
		}
	}

	/// <summary>
	/// Value of the "accept" attribute contains an invalid for 
	/// HTTP header character (outside #x20 through #x7E range).
	/// </summary>
	public class InvalidAcceptHTTPHeaderValueError : FatalException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidAcceptHTTPHeaderValueError"/> 
		/// class with a specified invalid character.
		/// </summary>
		/// <param name="c">Invalid character</param>
		public InvalidAcceptHTTPHeaderValueError(char c)
			: base(string.Format(CultureInfo.CurrentCulture, Resources.InvalidCharForAccept, ((int)c).ToString("X2")))
		{
		}
	}

	/// <summary>
	/// <c>ResourceException</c> represents resource error as per XInclude specification.
	/// </summary>
	/// <remarks>
	/// Resource error is internal error and should lead to the fallback processing.
	/// <c>ResourceException</c> therefore should never be thrown outside 
	/// the XInclude Processor.
	/// </remarks>
	internal class ResourceException : XIncludeException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ResourceException"/> 
		/// class with a specified error message.
		/// </summary>
		/// <param name="message">Error message</param>
		public ResourceException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ResourceException"/> 
		/// class with a specified error message and a reference to the 
		/// inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">Error message</param>
		/// <param name="innerException">Inner exceptiion</param>
		public ResourceException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}