namespace Sage.Rewriters
{
	using System;
	using System.Web;

	/// <summary>
	/// Intercepts all requests and checks if there is a localized version matching the current request
	/// </summary>
	public class LocalizePathRewriter : IHttpModule
	{
		/// <summary>
		/// Inits the specified application.
		/// </summary>
		/// <param name="application">The application.</param>
		public void Init(HttpApplication application)
		{
			application.BeginRequest += OnApplicationRequestStart;
		}

		/// <summary>
		/// Disposes of the resources (other than memory) used by the module that implements <see cref="IHttpModule"/>.
		/// </summary>
		public void Dispose()
		{
		}

		private void OnApplicationRequestStart(object sender, EventArgs e)
		{
			HttpApplication application = (HttpApplication) sender;
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
	}
}
