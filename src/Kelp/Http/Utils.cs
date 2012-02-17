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
namespace Kelp.Http
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Net;
	using System.Web;

	/// <summary>
	/// Provides several common HTTP related utility methods.
	/// </summary>
	public static class Util
	{
		/// <summary>
		/// Specifies the maximum time difference (in seconds) between the current and the cached files that
		/// will still be allowed for the files to still be considered equal.
		/// </summary>
		public const int MaxDifferenceCachedDate = 2;

		/// <summary>
		/// Determines whether the specified <paramref name="context"/> represents a no-cache request.
		/// </summary>
		/// <param name="context">The context to check.</param>
		/// <returns>
		/// <c>true</c> if the specified <paramref name="context"/> represents a no-cache request; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsNoCacheRequest(HttpContextBase context)
		{
			return context.Request.Headers["Cache-Control"] == "no-cache";
		}

		/// <summary>
		/// Determines whether the specified <paramref name="context"/> represents a cached request.
		/// </summary>
		/// <param name="context">The context to check.</param>
		/// <returns>
		/// <c>true</c> if the specified <paramref name="context"/> represents a cached request; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsCachedRequest(HttpContextBase context)
		{
			return context.Request.Headers["If-Modified-Since"] != null;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="lastModified"/> date is greater than the last modified
		/// date associated with the request.
		/// </summary>
		/// <param name="context">The context to check.</param>
		/// <param name="lastModified">The last modification date to test with.</param>
		/// <returns>
		///   <c>true</c> if the specified <paramref name="lastModified"/> date is greater than the last modified
		/// date associated with the request; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsFileUpdatedSinceCached(HttpContextBase context, DateTime lastModified)
		{
			DateTime cachedDate = DateTime.Parse(context.Request.Headers["If-Modified-Since"]);

			TimeSpan elapsed = lastModified - cachedDate;
			return elapsed.TotalSeconds >= MaxDifferenceCachedDate;
		}

		/// <summary>
		/// Sends the not-modified status header to the responnse.
		/// </summary>
		/// <param name="context">The HTTP context that contains the request.</param>
		public static void SendNotModified(HttpContextBase context)
		{
			context.Response.SuppressContent = true;
			context.Response.StatusCode = (int) HttpStatusCode.NotModified;
			context.Response.StatusDescription = "Not Modified";
			context.Response.AddHeader("Content-Length", "0");
		}

		/// <summary>
		/// Sets the response status code to 404 for the specified <paramref name="context"/>.
		/// </summary>
		/// <param name="context">The HTTP context that contains the request.</param>
		public static void SendFileNotFound(HttpContextBase context)
		{
			context.Response.StatusCode = 404;
		}

		/// <summary>
		/// Gets the last modification date of the specified <paramref name="filePath"/>.
		/// </summary>
		/// <param name="filePath">The file path.</param>
		/// <returns>the last modification date of the specified file</returns>
		/// <remarks>
		/// The greaer of the <em>LastWriteTime</em> and <em>CreationTime</em> associated with the request.
		/// </remarks>
		public static DateTime GetDateLastModified(string filePath)
		{
			FileInfo i = new FileInfo(filePath);
			DateTime wd = i.LastWriteTime;
			DateTime cd = i.CreationTime;

			return cd > wd ? cd : wd;
		}

		/// <summary>
		/// Gets the latest modification date of specified list of <paramref name="files"/>.
		/// </summary>
		/// <param name="files">The files to check.</param>
		/// <returns>The latest modification date of the specified list of <paramref name="files"/>.</returns>
		public static DateTime GetDateLastModified(IEnumerable<string> files)
		{
			DateTime latestModified = new DateTime();
			foreach (string file in files)
			{
				if (!File.Exists(file))
					return DateTime.Now;

				DateTime lastModified = GetDateLastModified(file);

				if (lastModified > latestModified)
					latestModified = lastModified;
			}

			return latestModified;
		}
	}
}
