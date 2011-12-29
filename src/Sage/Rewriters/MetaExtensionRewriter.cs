namespace Sage.Rewriters
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Web;

	using Kelp.Core;

	using log4net;

	using Sage.Configuration;

	/// <summary>
	/// Intercepts all requests and checks if there is a localized version matching the current request
	/// </summary>
	public class MetaExtensionRewriter : IHttpModule
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(MetaExtensionRewriter).FullName);
		private static Regex metaViewExpression;

		private static Regex MetaViewExpression
		{
			get
			{
				lock (log)
				{
					if (metaViewExpression == null)
					{
						lock (log)
						{
							List<string> extensions = ProjectConfiguration.Current.MetaViews.Keys.ToList();
							extensions.Add("html");

							metaViewExpression = new Regex(string.Format(@"\.({0})$", string.Join("|", extensions)));
						}
					}
				}

				return metaViewExpression;
			}
		}

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

			if (File.Exists(context.Request.PhysicalPath))
				return;

			
			QueryString query = new QueryString(HttpContext.Current.Request.Url.Query);
			string view = query.GetString(MetaViewDictionary.ParamNameMetaView);
			if (view != null && !context.Config.MetaViews.ContainsKey(view))
				view = null;

			//// \.(html|xml|xmlx|htmlx)$
			//// Meta view extension gets removed from the path
			//// Path is rewritten with ?view=$1

			string path = MetaViewExpression.Replace(context.Request.Path, delegate(Match m)
			{
				if (view == null)
					view = m.Groups[1].Value;

				return string.Empty;
			});

			if (view != null)
			{
				query.Remove(MetaViewDictionary.ParamNameMetaView);
				query.Add(MetaViewDictionary.ParamNameMetaView, view);

				string newUri = new Uri(path + query.ToString(true), UriKind.RelativeOrAbsolute).ToString();
				log.DebugFormat("Rewriting the context path from '{0}' to '{1}'.", context.Request.Path, newUri);
				application.Context.RewritePath(newUri);
			}
		}
	}
}
