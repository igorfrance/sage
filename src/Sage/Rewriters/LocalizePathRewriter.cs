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
	using System.Web;

	using log4net;

	/// <summary>
	/// Intercepts all requests and checks if there is a localized version matching the current request
	/// </summary>
	public sealed class LocalizePathRewriter : IHttpModule
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(LocalizePathRewriter).FullName);

		/// <summary>
		/// Inits the specified application.
		/// </summary>
		/// <param name="application">The application.</param>
		public void Init(HttpApplication application)
		{
			application.BeginRequest += (sender, e) => RewritePath((HttpApplication) sender);
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

				string requestPath = context.Request.Path;
				string localizedPath = context.Path.Localize(requestPath);
				if (localizedPath != requestPath)
				{
					context.HttpContext.RewritePath(localizedPath);
					if (context.IsDeveloperRequest)
						context.Response.AddHeader("OriginalFilePath", requestPath);
				}
			}
			catch (Exception ex)
			{
				log.ErrorFormat("Failed to rewrite path: {0}", ex.Message);
			}
		}
	}
}
