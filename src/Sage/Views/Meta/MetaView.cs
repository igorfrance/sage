﻿namespace Sage.Views.Meta
{
	using System;
	using System.Diagnostics.Contracts;
	using System.IO;
	using System.Reflection;
	using System.Web.Mvc;

	using log4net;
	using Sage.Configuration;

	/// <summary>
	/// Provides a meta view of the actual view selected and executed for a controller.
	/// </summary>
	public abstract class MetaView : IView
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(MetaView).FullName);
		private readonly IView wrapped;
		private readonly MetaViewInfo viewInfo;

		protected MetaView(MetaViewInfo viewInfo, IView wrapped)
		{
			Contract.Requires<ArgumentNullException>(viewInfo != null);
			Contract.Requires<ArgumentNullException>(wrapped != null);

			this.wrapped = wrapped;
			this.viewInfo = viewInfo;
		}

		/// <summary>
		/// Gets the view that this meta view inspects.
		/// </summary>
		public IView View
		{
			get
			{
				return wrapped;
			}
		}

		/// <summary>
		/// Gets the object that defines this meta view.
		/// </summary>
		public MetaViewInfo Info
		{
			get
			{
				return viewInfo;
			}
		}

		/// <summary>
		/// Creates <see cref="MetaView"/> instances.
		/// </summary>
		/// <param name="viewInfo">The object that defines the meta view to create.</param>
		/// <param name="wrapped">The view that the meta view inspects.</param>
		/// <returns></returns>
		public static MetaView Create(MetaViewInfo viewInfo, IView wrapped)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(viewInfo.TypeName));
			Contract.Requires<ArgumentNullException>(wrapped != null);

			Type type = Type.GetType(viewInfo.TypeName);
			if (type == null)
			{
				type = typeof(MetaView);
				log.ErrorFormat("The type name '{0}' could not be found, reverting to '{1}'", viewInfo.TypeName, type.FullName);
			}
			else if (!typeof(MetaView).IsAssignableFrom(type))
			{
				type = typeof(MetaView);
				log.ErrorFormat("The type name '{0}' could is not an instance of '{1}', reverting to '{1}'", viewInfo.TypeName, type.FullName);
			}

			ConstructorInfo ctor = type.GetConstructor(new[] { typeof(MetaViewInfo), typeof(IView) });
			if (ctor == null)
			{
				throw new ArgumentException(string.Format(
					"The type name '{0}' doesn't implement the required constructor", viewInfo.TypeName), "viewInfo");
			}

			MetaView result = (MetaView) ctor.Invoke(new object[] { viewInfo, wrapped });
			return result;
		}

		/// <summary>
		/// Renders the specified view context by using the specified the writer object.
		/// </summary>
		/// <param name="viewContext">The view context.</param>
		/// <param name="writer">The writer object.</param>
		public virtual void Render(ViewContext viewContext, TextWriter writer)
		{
			viewContext.HttpContext.Response.ContentType = this.Info.ContentType;
			wrapped.Render(viewContext, writer);
		}

		protected void DisableCaching(ViewContext viewContext)
		{
			viewContext.HttpContext.Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
			viewContext.HttpContext.Response.Cache.SetNoStore();
		}
	}
}
