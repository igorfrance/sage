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
namespace Sage.Rewriters
{
	using System;
	using System.Collections;
	using System.IO;
	using System.Linq;
	using System.Web;

	using Kelp.Extensions;
	using log4net;

	/// <summary>
	/// Intercepts all requests and tries to match a missing path against a path within an extension.
	/// </summary>
	public class ExtensionPathRewriter : IHttpModule
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ExtensionPathRewriter).FullName);

		/// <summary>
		/// Initializes the rewriter, using specified application.
		/// </summary>
		/// <param name="application">The current web application.</param>
		public void Init(HttpApplication application)
		{
			application.BeginRequest += (sender, e) => ExtensionPathRewriter.RewritePath((HttpApplication) sender);
		}

		/// <summary>
		/// Disposes of the resources (other than memory) used by the module that implements <see cref="IHttpModule"/>.
		/// </summary>
		public void Dispose()
		{
		}

		private static void RewritePath(HttpApplication application)
		{
			try
			{
				SageContext context = new SageContext(application.Context);
				string requestedPath = context.Request.PhysicalPath;

				string rewrittenPath = context.Path.Resolve(requestedPath);
				if (rewrittenPath != requestedPath)
				{
					string relativePath = context.Path.GetRelativeWebPath(rewrittenPath, true);
					context.HttpContext.RewritePath(relativePath);
					if (context.IsDeveloperRequest)
						context.Response.AddHeader("OriginalFilePath", requestedPath);
				}
			}
			catch (Exception ex)
			{
				log.ErrorFormat("Failed to rewrite path: {0}", ex.Message);
			}
		}
	}
}
