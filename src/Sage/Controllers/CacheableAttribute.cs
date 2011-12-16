namespace Sage.Controllers
{
	using System;
	using System.Net;
	using System.Web;
	using System.Web.Mvc;

	/// <summary>
	/// Indicates that an action can be cached for a certain duration.
	/// </summary>
	/// <remarks>
	/// The MVC's <see cref="OutputCacheAttribute"/> doesn't work well in a clustered setup, 
	/// since it generates different ETags and Last-Modified's for different servers, thereby potentially
	/// confusing both Akamai and browsers. 
	/// <para>This attribute ensures that the same ETags and Last-Modified response headers get set across
	/// different servers.</para>
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class CacheableAttribute : ActionFilterAttribute
	{
		/// <summary>
		/// Specifies the maximum time difference (in seconds) between the current and the cached files that
		/// will still be allowed for the files to still be considered equal.
		/// </summary>
		public const int MaxDifferenceCachedDate = 2;

		private TimeSpan? duration;

		/// <summary>
		/// Gets or sets the number of seconds after which the associated action expires.
		/// </summary>
		public double Seconds
		{
			get
			{
				return duration.Value.TotalSeconds;
			}

			set
			{
				duration = TimeSpan.FromSeconds((duration != null ? duration.Value.TotalSeconds : 0) + value);
			}
		}

		/// <summary>
		/// Gets or sets the number of minutes after which the associated action expires.
		/// </summary>
		public double Minutes
		{
			get
			{
				return duration.Value.TotalMinutes;
			}

			set
			{
				duration = TimeSpan.FromMinutes((duration != null ? duration.Value.TotalMinutes : 0) + value);
			}
		}

		/// <summary>
		/// Gets or sets the number of hours after which the associated action expires.
		/// </summary>
		public double Hours
		{
			get
			{
				return duration.Value.TotalHours;
			}

			set
			{
				duration = TimeSpan.FromHours((duration != null ? duration.Value.TotalHours : 0) + value);
			}
		}

		/// <summary>
		/// Gets or sets the number of days after which the associated action expires.
		/// </summary>
		public double Days
		{
			get
			{
				return duration.Value.TotalDays;
			}

			set
			{
				duration = TimeSpan.FromDays((duration != null ? duration.Value.TotalDays : 0) + value);
			}
		}

		/// <summary>
		/// Gets or sets the number of weeks after which the associated action expires.
		/// </summary>
		public double Weeks
		{
			get
			{
				return duration.Value.TotalDays / 7;
			}

			set
			{
				duration = TimeSpan.FromDays((duration != null ? duration.Value.TotalDays : 0) + (value * 7));
			}
		}

		/// <summary>
		/// Gets or sets the number of months after which the associated action expires.
		/// </summary>
		public double Months
		{
			get
			{
				return duration.Value.TotalDays / 30;
			}

			set
			{
				duration = TimeSpan.FromSeconds((duration != null ? duration.Value.TotalDays : 0) + (value * 30));
			}
		}

		/// <summary>
		/// Gets or sets the number of years after which the associated action expires.
		/// </summary>
		public double Years
		{
			get
			{
				return duration.Value.TotalDays / 365;
			}

			set
			{
				duration = TimeSpan.FromSeconds((duration != null ? duration.Value.TotalDays : 0) + (value * 365));
			}
		}

		/// <summary>
		/// Called by the MVC framework before the action method executes.
		/// </summary>
		/// <param name="filterContext">The filter context.</param>
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var lastModified = GetLastModified(filterContext);
			if (this.duration == null && lastModified == null)
				return;

			var context = filterContext.HttpContext;
			var isCachedRequest = context.Request.Headers["If-Modified-Since"] != null;
			if (isCachedRequest)
			{
				DateTime cachedDate = DateTime.Parse(context.Request.Headers["If-Modified-Since"]);
				bool fileChanged = true;

				if (this.duration != null)
				{
					TimeSpan elapsed = DateTime.Now - cachedDate;
					fileChanged = elapsed > duration.Value;
				}
				else
				{
					TimeSpan elapsed = lastModified.Value - cachedDate;
					fileChanged = elapsed.TotalSeconds > MaxDifferenceCachedDate;
				}

				if (!fileChanged)
				{
					var response = context.Response;
					response.StatusCode = (int) HttpStatusCode.NotModified;
					response.StatusDescription = "Not Modified";
					response.AddHeader("Content-Length", "0");
					filterContext.Result = new EmptyResult();
				}
			}
		}

		/// <summary>
		/// Called by the MVC framework after the action method is executed.
		/// </summary>
		/// <param name="filterContext">The filter context.</param>
		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			if (filterContext.Exception != null)
				return;

			var context = filterContext.HttpContext;
			var response = context.Response;
			var lastModified = GetLastModified(filterContext);

			response.Cache.SetCacheability(HttpCacheability.Public);
			if (lastModified != null)
				response.Cache.SetLastModified(lastModified.Value);
			else
				response.Cache.SetLastModified(DateTime.Now);
		}

		private static DateTime? GetLastModified(ActionExecutingContext filterContext)
		{
			if (!(filterContext.Controller is SageController))
				return null;

			string actionName = filterContext.ActionDescriptor.ActionName;
			return ((SageController) filterContext.Controller).GetLastModificationDate(actionName);
		}

		private static DateTime? GetLastModified(ActionExecutedContext filterContext)
		{
			if (!(filterContext.Controller is SageController))
				return null;

			string actionName = filterContext.ActionDescriptor.ActionName;
			return ((SageController) filterContext.Controller).GetLastModificationDate(actionName);
		}
	}
}