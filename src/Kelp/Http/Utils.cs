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
	using System.Linq;
	using System.Net;
	using System.Text;
	using System.Web;

	using Kelp.ResourceHandling;

	public static class Util
	{
		/// <summary>
		/// Specifies the maximum time difference (in seconds) between the current and the cached files that
		/// will still be allowed for the files to still be considered equal.
		/// </summary>
		public const int MaxDifferenceCachedDate = 2;

		public static bool IsNoCacheRequest(HttpContextBase context)
		{
			return context.Request.Headers["Cache-Control"] == "no-cache";
		}

		public static bool IsCachedRequest(HttpContextBase context)
		{
			return context.Request.Headers["If-Modified-Since"] != null;
		}

		public static bool IsFileUpdatedSinceCached(HttpContextBase context, DateTime lastModified)
		{
			DateTime cachedDate = DateTime.Parse(context.Request.Headers["If-Modified-Since"]);

			TimeSpan elapsed = lastModified - cachedDate;
			return elapsed.TotalSeconds >= MaxDifferenceCachedDate;
		}

		/// <summary>
		/// Sends the not-modified status to the browser.
		/// </summary>
		/// <param name="context">The context under which this code is executing.</param>
		public static void SendNotModified(HttpContextBase context)
		{
			context.Response.SuppressContent = true;
			context.Response.StatusCode = (int) HttpStatusCode.NotModified;
			context.Response.StatusDescription = "Not Modified";
			context.Response.AddHeader("Content-Length", "0");
		}

		public static void SendFileNotFound(HttpContextBase context)
		{
			context.Response.StatusCode = 404;
		}

		/// <summary>
		/// Gets the last-modified datetime of the specified file.
		/// </summary>
		/// <param name="filePath">The file path.</param>
		/// <returns>the last-modified datetime of the specified file</returns>
		/// <remarks>
		/// The latest of the <em>LastWriteTime</em> and <em>CreationTime</em> is used
		/// </remarks>
		public static DateTime GetDateLastModified(string filePath)
		{
			FileInfo i = new FileInfo(filePath);
			DateTime wd = i.LastWriteTime;
			DateTime cd = i.CreationTime;

			return cd > wd ? cd : wd;
		}

		/// <summary>
		/// Gets the last-modified datetime of a list of files.
		/// </summary>
		/// <param name="files">The files.</param>
		/// <returns>The latest last-modified datetime of the <paramref name="files"/> list.</returns>
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
