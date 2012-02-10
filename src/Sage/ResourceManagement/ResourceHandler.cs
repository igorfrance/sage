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
namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Text;
	using System.Web;

	using Kelp.Http;

	using log4net;

	/// <summary>
	/// Implements an <see cref="IHttpHandler"/> for handling XML requests.
	/// </summary>
	public class ResourceHandler : IHttpHandler
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ResourceHandler).FullName);

		public void ProcessRequest(HttpContext context)
		{
			ProcessRequest(new HttpContextWrapper(context), context.Server.MapPath);
		}

		public void ProcessRequest(HttpContextBase context, Func<string, string> mapPath)
		{
			Contract.Requires<ArgumentNullException>(context != null);
			Contract.Requires<ArgumentNullException>(mapPath != null);

			string absolutePath = context.Request.PhysicalPath;

			if (File.Exists(absolutePath))
			{
				DateTime lastModifield = File.GetLastWriteTime(absolutePath);

				if (!Util.IsNoCacheRequest(context) && Util.IsCachedRequest(context) && !Util.IsFileUpdatedSinceCached(context, lastModifield))
				{
					// 4
					Util.SendNotModified(context);
					return;
				}

				try
				{
					SageContext sageContext = new SageContext(context, mapPath);
					CacheableXmlDocument document = sageContext.Resources.LoadXml(absolutePath);
					SendContent(context, document);
				}
				catch (Exception ex)
				{
					log.ErrorFormat("Failed to send '{0}' as XML to response: {1}", absolutePath, ex.Message);
					SendContent(context, absolutePath);
				}
			}
			else
			{
				Util.SendFileNotFound(context);
			}
		}

		/// <summary>
		/// Gets a value indicating whether another request can use the <see cref="IHttpHandler"/> instance.
		/// </summary>
		/// <returns>true if the <see cref="IHttpHandler"/> instance is reusable; otherwise, false.</returns>
		public bool IsReusable
		{
			get
			{
				return false;
			}
		}

		private static void SendContent(HttpContextBase context, CacheableXmlDocument document)
		{
			context.Response.ContentType = "text/xml";
			context.Response.Write(document.OuterXml);
		}

		private static void SendContent(HttpContextBase context, string filename)
		{
			context.Response.ContentType = "text/xml";
			context.Response.BinaryWrite(File.ReadAllBytes(filename));
		}
	}
}
