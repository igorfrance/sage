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

	/// <summary>
	/// Generic XPointer exception.
	/// </summary>
	public abstract class XPointerException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ApplicationException"/> 
		/// class with a specified error message.
		/// </summary>
		/// <param name="message">Error message</param>
		protected XPointerException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ApplicationException"/> 
		/// class with a specified error message and a reference to the 
		/// inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">Error message</param>
		/// <param name="innerException">Inner exception</param>
		protected XPointerException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}

	/// <summary>
	/// XPointer Framework syntax error.
	/// </summary>
	public class XPointerSyntaxException : XPointerException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="XPointerSyntaxException"/> 
		/// class with a specified error message.
		/// </summary>
		/// <param name="message">Error message</param>
		public XPointerSyntaxException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XPointerSyntaxException"/> 
		/// class with a specified error message and a reference to the 
		/// inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">Error message</param>
		/// <param name="innerException">Inner exception</param>
		public XPointerSyntaxException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}

	/// <summary>
	/// XPointer doesn't identify any subresources - it's an error as per 
	/// XPointer Framework.
	/// </summary>
	public class NoSubresourcesIdentifiedException : XPointerException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NoSubresourcesIdentifiedException"/> 
		/// class with a specified error message.
		/// </summary>
		/// <param name="message">Error message</param>
		public NoSubresourcesIdentifiedException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NoSubresourcesIdentifiedException"/> 
		/// class with a specified error message and a reference to the 
		/// inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">Error message</param>
		/// <param name="innerException">Inner exception</param>
		public NoSubresourcesIdentifiedException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}