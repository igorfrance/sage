﻿/**
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
namespace Kelp.ResourceHandling
{
	using System;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Web;

	using Kelp.Core;
	using Kelp.Http;

	/// <summary>
	/// Implements an <see cref="IHttpHandler"/> for handling image, script and css requests.
	/// </summary>
	public class ResourceHandler : IHttpHandler
	{
		/// <summary>
		/// The name of the query string property that instructs <see cref="ResourceHandler"/> to return 
		/// only the raw content, without processing the includes.
		/// </summary>
		/// <example>?noprocess=1</example>
		public const string SkipProcessingKey = "noprocess";
		
		/// <summary>
		/// The vallue of the query string property (defined with <see cref="SkipProcessingKey"/>) 
		/// that instructs <see cref="ResourceHandler"/> to return only the raw content, without 
		/// processing the includes.
		/// </summary>
		/// <example>?noprocess=1</example>
		public const string SkipProcessingValue = "1";

		/// <summary>
		/// Specifies the maximum time difference (in seconds) between the current and the cached files that
		/// will still be allowed for the files to still be considered equal.
		/// </summary>
		public const int MaxDifferenceCachedDate = 2;
		
		/// <summary>
		/// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.
		/// </summary>
		/// <value></value>
		/// <returns><c>true</c> if the <see cref="IHttpHandler"/> instance is reusable; otherwise, <c>false</c>.
		/// </returns>
		public bool IsReusable
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Enables processing of HTTP Web requests by a custom HTTP handler that implements the <see cref="IHttpHandler"/> interface.
		/// </summary>
		/// <param name="context">An <see cref="HttpContext"/> object that provides references to the intrinsic server objects (for example, 
		/// Request, Response, Session, and Server) used to service HTTP requests.</param>
		public void ProcessRequest(HttpContext context)
		{
			ProcessRequest(new HttpContextWrapper(context), context.Server.MapPath);
		}

		/// <summary>
		/// Enables processing of HTTP Web requests by a custom HTTP handler that implements the <see cref="IHttpHandler"/> interface.
		/// </summary>
		/// <param name="context">An <see cref="HttpContext"/> object that provides references to the intrinsic server objects (for example,
		/// Request, Response, Session, and Server) used to service HTTP requests.</param>
		/// <param name="mapPath">The function to use to map virtual paths to absolute.</param>
		/// <exception cref="ArgumentNullException"><c>context</c> or <c>mapPath</c> is <c>null</c>.</exception>
		public void ProcessRequest(HttpContextBase context, Func<string, string> mapPath)
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(mapPath != null);

			string absolutePath = context.Request.PhysicalPath;
			string extension = Path.GetExtension(absolutePath);

			if (File.Exists(absolutePath))
			{
				if (CodeFile.IsFileExtensionSupported(extension))
				{
					ProcessCodeFileRequest(context, mapPath);
				}
				else if (ImageFile.IsFileExtensionSupported(extension))
				{
					ProcessImageFileRequest(context, mapPath);
				}
				else
				{
					SendContent(context, absolutePath);
				}
			}
			else
			{
				Util.SendFileNotFound(context);
			}
		}

		/// <summary>
		/// Processes requests for file types handled by <see cref="CodeFile"/>.
		/// </summary>
		/// <param name="context">An <see cref="HttpContext"/> object that provides references to the intrinsic server objects (for example,
		/// Request, Response, Session, and Server) used to service HTTP requests.</param>
		/// <param name="mapPath">The function to use to map virtual paths to absolute.</param>
		/// <exception cref="ArgumentNullException"><c>context</c> or <c>mapPath</c> is <c>null</c>.</exception>
		/// <remarks>
		/// The logic of getting and serving a resource file, in a nutshell is similar to:
		/// <list>
		/// <item>1) Get the resource item</item>
		/// <item>2) Is this a previously cached request? (YES: goto 3; NO: goto 5)</item>
		/// <item>3) Is the resource newer than the cached date? (YES: goto 5; NO: goto 4)</item>
		/// <item>4) Send 304 headers back to the client</item>
		/// <item>5) Send content down the wire</item>
		/// </list>
		/// </remarks>
		private static void ProcessCodeFileRequest(HttpContextBase context, Func<string, string> mapPath)
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(mapPath != null);

			// 1
			CodeFile file = CodeFile.Create(context.Request.PhysicalPath, context.Request.Path, mapPath);

			// 2 & 3
			if (!Util.IsNoCacheRequest(context) && Util.IsCachedRequest(context) && !Util.IsFileUpdatedSinceCached(context, file.LastModified))
			{
				// 4
				Util.SendNotModified(context);
				return;
			}

			// 5
			SendContent(context, file);
		}

		/// <summary>
		/// Processes requests for file types handled by <see cref="ImageFile"/>.
		/// </summary>
		/// <param name="context">An <see cref="HttpContext"/> object that provides references to the intrinsic server objects (for example,
		/// Request, Response, Session, and Server) used to service HTTP requests.</param>
		/// <param name="mapPath">The function to use to map virtual paths to absolute.</param>
		/// <exception cref="ArgumentNullException"><c>context</c> or <c>mapPath</c> is <c>null</c>.</exception>
		private static void ProcessImageFileRequest(HttpContextBase context, Func<string, string> mapPath)
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(mapPath != null);

			// 1
			ImageFile file = ImageFile.Create(
				context.Request.PhysicalPath, new QueryString(context.Request.QueryString), mapPath);

			// 2 & 3
			if (!Util.IsNoCacheRequest(context) && Util.IsCachedRequest(context) && !Util.IsFileUpdatedSinceCached(context, file.LastModified))
			{
				// 4
				Util.SendNotModified(context);
				return;
			}

			// 5
			SendContent(context, file);
		}

		private static void SendContent(HttpContextBase context, string filename)
		{
			context.Response.ContentType = "application/octet-stream";
			context.Response.BinaryWrite(File.ReadAllBytes(filename));
		}

		/// <summary>
		/// Sends the contents of the file down the response.
		/// </summary>
		/// <param name="context">The context under which this code is executing.</param>
		/// <param name="file">The file to send.</param>
		private static void SendContent(HttpContextBase context, CodeFile file)
		{
			bool rawContent = context.Request.QueryString[SkipProcessingKey] == SkipProcessingValue;
			string content = rawContent ? file.RawContent : file.Content;

			context.Response.ContentType = file.ContentType;
			context.Response.Cache.SetCacheability(HttpCacheability.Public);
			context.Response.Cache.SetLastModified(file.LastModified);
			context.Response.Cache.SetETag(file.ETag);
			context.Response.Write(content);
		}

		/// <summary>
		/// Sends the contents of the file down the response.
		/// </summary>
		/// <param name="context">The context under which this code is executing.</param>
		/// <param name="file">The file to send.</param>
		private static void SendContent(HttpContextBase context, ImageFile file)
		{
			if (Util.IsNoCacheRequest(context))
				file.UseCache = false;

			context.Response.ContentType = file.ContentType;
			context.Response.Cache.SetCacheability(HttpCacheability.Public);
			context.Response.Cache.SetLastModified(file.LastModified);
			context.Response.BinaryWrite(file.Bytes);
		}
	}
}