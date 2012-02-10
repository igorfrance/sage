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
namespace Sage.Test
{
	using System;
	using System.Linq;
	using System.Reflection;
	using System.Web.Mvc;

	public static class ControllerFactoryTestExtension
	{
		private static readonly PropertyInfo typeCacheProperty;
		private static readonly FieldInfo cacheField;

		static ControllerFactoryTestExtension()
		{
			typeCacheProperty = typeof(DefaultControllerFactory).GetProperty("ControllerTypeCache", BindingFlags.Instance | BindingFlags.NonPublic);
			cacheField = typeCacheProperty.PropertyType.GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		/// <summary>
		/// Replaces the cache field of a the DefaultControllerFactory's ControllerTypeCache.
		/// This ensures that only the specified controller types will be searched when instantiating a controller.
		/// As the ControllerTypeCache is internal, this uses some reflection hackery.
		/// </summary>
		/// <param name="factory">The factory.</param>
		/// <param name="controllerTypes">The controller types.</param>
		public static void InitializeWithControllerTypes(this IControllerFactory factory, params Type[] controllerTypes)
		{
			var cache = controllerTypes
				.GroupBy(t => t.Name.Substring(0, t.Name.Length - "Controller".Length), StringComparer.OrdinalIgnoreCase)
				.ToDictionary(g => g.Key, g => g.ToLookup(t => t.Namespace ?? string.Empty, StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);

			var buildManager = typeCacheProperty.GetValue(factory, null);
			cacheField.SetValue(buildManager, cache);
		}
	}
}
