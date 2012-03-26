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
namespace Kelp.Imaging.Filters
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Drawing;
	using System.Linq;
	using System.Reflection;

	using log4net;

	/// <summary>
	/// Provides a predefined sequence of filters that can be applied to an image.
	/// </summary>
	/// <remarks>
	/// The filters that can be used are: 
	/// <list type="table">
	/// <listheader>
	/// <term>Key</term>
	/// <description>Filter and arguments</description>
	/// </listheader>
	/// <item><term>cp</term><description><see cref="Crop"/> (int x, int y, int width, int height)</description></item>
	/// <item><term>rs</term><description><see cref="Resample"/> (int width, int height, byte preserveRatio (1|0), byte dontEnlarge (1|0), byte interpolationMode (1|5|7))</description></item>
	/// <item><term>bt</term><description><see cref="BrightnessMatrix"/> (int amount)</description></item>
	/// <item><term>ct</term><description><see cref="ContrastMatrix"/> (int amount)</description></item>
	/// <item><term>gm</term><description><see cref="GammaMatrix"/> (int amount)</description></item>
	/// <item><term>hsl</term><description><see cref="HSLFilter"/> (int hue, int saturation, int lightness)</description></item>
	/// <item><term>rgb</term><description><see cref="ColorBalance"/> (int red, int green, int blue)</description></item>
	/// <item><term>se</term><description><see cref="SepiaMatrix"/> (byte 1|0)</description></item>
	/// <item><term>sp</term><description><see cref="GaussianSharpen"/> (byte 1|0)</description></item>
	/// <item><term>mh</term><description><see cref="MirrorH"/> (byte 1|0)</description></item>
	/// <item><term>mv</term><description><see cref="MirrorV"/> (byte 1|0)</description></item>
	/// </list>
	/// </remarks>
	public class QueryFilter : BaseFilter
	{
		private static ILog log = LogManager.GetLogger(typeof(QueryFilter).FullName);

		/// <summary>
		/// Collection of bitmap filters that this controller can use
		/// </summary>
		private static readonly Dictionary<string, FilterDefinition> filters;
		private readonly FilterSequence activeFilters = new FilterSequence();
		private readonly QueryString activeQuery = new QueryString();

		static QueryFilter()
		{
			const BindingFlags BindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

			filters = new Dictionary<string, FilterDefinition>();
			List<Assembly> assemblies = Util.GetAssociatedAssemblies(typeof(QueryFilter).Assembly);
			foreach (Assembly a in assemblies)
			{
				var types = from t in a.GetTypes()
							where t.IsClass && !t.IsAbstract
							select t;

				foreach (Type type in types)
				{
					foreach (MethodInfo methodInfo in type.GetMethods(BindingFlags))
					{
						foreach (QueryFilterFactoryAttribute attrib in methodInfo.GetCustomAttributes(typeof(QueryFilterFactoryAttribute), false))
						{
							Func<string[], IFilter> del = (Func<string[], IFilter>) Delegate.CreateDelegate(typeof(Func<string[], IFilter>), methodInfo);
							if (filters.ContainsKey(attrib.ID))
								log.WarnFormat("A filter factory method {0} registered with the id '{1}' will be overwritten by delegate {2} registered for the same id.",
									Util.GetMethodSignature(filters[attrib.ID].FactoryMethod.Method),
									attrib.ID,
									Util.GetMethodSignature(del.Method));

							filters[attrib.ID] = new FilterDefinition(del, attrib.ParameterCount);
						}
					}
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="QueryFilter"/> class, using the specified 
		/// <paramref name="query"/> string to initialize it's filters.
		/// </summary>
		/// <param name="query">The query that specifies the filters to use.</param>
		public QueryFilter(string query)
			: this(new QueryString(query))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="QueryFilter"/> class, using the specified 
		/// <paramref name="query"/> to initialize it's filters.
		/// </summary>
		/// <param name="query">The query that specifies the filters to use.</param>
		public QueryFilter(QueryString query)
		{
			this.ParseQuery(query);
		}

		/// <summary>
		/// Gets the <see cref="QueryString"/> associated with this <see cref="QueryFilter"/>.
		/// </summary>
		public QueryString Query
		{
			get
			{
				return this.activeQuery;
			}
		}

		/// <summary>
		/// Gets the <see cref="FilterSequence"/> associated with this <see cref="QueryFilter"/>.
		/// </summary>
		public FilterSequence Filters
		{
			get
			{
				return activeFilters;
			}
		}
 
		/// <summary>
		/// Applies all filters in the current <see cref="QueryFilter"/> to the specified input image, and returns
		/// a new image.
		/// </summary>
		/// <param name="inputImage">The input image to process.</param>
		/// <returns>A new image with current <see cref="QueryFilter"/> filters applied to the 
		/// <paramref name="inputImage"/>. If current <see cref="QueryFilter"/> is empty, the original image is
		/// returned.</returns>
		public override Bitmap Apply(Bitmap inputImage)
		{
			if (this.activeFilters.Count != 0)
			{
				Bitmap result = AForge.Imaging.Image.Clone(inputImage);
				return this.activeFilters.Aggregate(result, (current, t) => t.Apply(current));
			}

			return inputImage;
		}

		private Dictionary<string, string[]> GetQueryParam(NameValueCollection param)
		{
			Dictionary<string, string[]> query = new Dictionary<string, string[]>();
			foreach (string name in param.Keys)
			{
				if (string.IsNullOrEmpty(name) || param[name] == null)
					continue;

				string[] value = param[name].Split(',');
				query.Add(name, value);
			}

			return query;
		}

		private void ParseQuery(QueryString query)
		{
			this.activeQuery.Clear();
			this.activeFilters.Clear();

			Dictionary<string, string[]> param = this.GetQueryParam(query);
			foreach (string key in param.Keys)
			{
				if (!filters.ContainsKey(key))
					continue;

				FilterDefinition filterDef = filters[key];
				if (param[key].Length < filterDef.Arguments)
					continue;

				IFilter filter = filterDef.FactoryMethod(param[key]);
				if (filter != null)
				{
					this.activeFilters.Add(filter);
					this.activeQuery.Add(key, query[key]);
				}
			}
		}

		private struct FilterDefinition
		{
			public readonly Func<string[], IFilter> FactoryMethod;

			public readonly byte Arguments;

			public FilterDefinition(Func<string[], IFilter> getter, byte argCount)
			{
				this.FactoryMethod = getter;
				this.Arguments = argCount;
			}
		}
	}
}
