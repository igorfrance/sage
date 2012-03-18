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
	using System.Drawing.Drawing2D;
	using System.Linq;

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
		/// <summary>
		/// Collection of bitmap filters that this controller can use
		/// </summary>
		private static readonly Dictionary<string, FilterDefinition> filters;
		private readonly FilterSequence activeFilters = new FilterSequence();
		private readonly QueryString activeQuery = new QueryString();

		static QueryFilter()
		{
			filters = new Dictionary<string, FilterDefinition>();
			filters.Add("cp", new FilterDefinition(GetCropFilter, 4));
			filters.Add("rs", new FilterDefinition(GetResampleFilter, 1));
			filters.Add("bt", new FilterDefinition(GetBrigthnessFilter, 1));
			filters.Add("ct", new FilterDefinition(GetContrastFilter, 1));
			filters.Add("gm", new FilterDefinition(GetGammaFilter, 1));
			filters.Add("hsl", new FilterDefinition(GetHslFilter, 3));
			filters.Add("rgb", new FilterDefinition(GetColorFilter, 3));
			filters.Add("se", new FilterDefinition(GetSepiaFilter, 1));
			filters.Add("sp", new FilterDefinition(GetSharpenFilter, 1));
			filters.Add("gs", new FilterDefinition(GetGrayscaleFilter, 1));
			filters.Add("sx", new FilterDefinition(GetSharpenExFilter, 1));
			filters.Add("mh", new FilterDefinition(GetMirrorHFilter, 1));
			filters.Add("mv", new FilterDefinition(GetMirrorVFilter, 1));
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
			ParseQuery(query);
		}

		private delegate IFilter GetFilter(string[] param, QueryString query);

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

		private static IFilter GetCropFilter(string[] param, QueryString query)
		{
			int x, y, w, h;

			int.TryParse(param[0], out x);
			int.TryParse(param[1], out y);
			int.TryParse(param[2], out w);
			int.TryParse(param[3], out h);

			if (w != 0 && h != 0)
				return new Crop(new Rectangle(x, y, w, h));

			return null;
		}

		private static IFilter GetResampleFilter(string[] param, QueryString query)
		{
			int width, height = 0;
			bool preserveRatio = true;
			bool dontEnlarge = true;
			InterpolationMode interpolation = InterpolationMode.HighQualityBicubic;

			int.TryParse(param[0], out width);

			if (param.Length > 1)
				int.TryParse(param[1], out height);

			if (param.Length > 2)
				preserveRatio = param[2] != "0";

			if (param.Length > 3)
				dontEnlarge = param[3] != "0";

			if (param.Length > 4)
			{
				interpolation = (InterpolationMode) 
					Enum.Parse(typeof(InterpolationMode), param[4]);
			}

			if (width != 0 || height != 0)
			{
				Resample filter = new Resample(width, height, interpolation, preserveRatio, dontEnlarge);
				if (query["ft"] == "min")
					filter.FitType = ResampleFitType.ToMinimums;

				return filter;
			}

			return null;
		}

		private static IFilter GetBrigthnessFilter(string[] param, QueryString query)
		{
			int amount;
			return int.TryParse(param[0], out amount) ? new BrightnessMatrix(amount) : null;
		}

		private static IFilter GetContrastFilter(string[] param, QueryString query)
		{
			int amount;
			return int.TryParse(param[0], out amount) ? new ContrastMatrix(amount) : null;
		}

		private static IFilter GetGammaFilter(string[] param, QueryString query)
		{
			int amount;
			return int.TryParse(param[0], out amount) ? new GammaMatrix(amount) : null;
		}

		private static IFilter GetMirrorHFilter(string[] param, QueryString query)
		{
			return param[0] == "1" ? new MirrorH() : null;
		}

		private static IFilter GetMirrorVFilter(string[] param, QueryString query)
		{
			return param[0] == "1" ? new MirrorV() : null;
		}

		private static IFilter GetGrayscaleFilter(string[] param, QueryString query)
		{
			return param[0] == "1" ? new GrayscaleMatrix() : null;
		}

		private static IFilter GetHslFilter(string[] param, QueryString query)
		{
			int h;
			int s;
			int l;

			int.TryParse(param[0], out h);
			int.TryParse(param[1], out s);
			int.TryParse(param[2], out l);

			if (h != 0 || s != 0 || l != 0)
				return new HSLFilter(h, s, l);

			return null;
		}

		private static IFilter GetColorFilter(string[] param, QueryString query)
		{
			int r;
			int g;
			int b;

			int.TryParse(param[0], out r);
			int.TryParse(param[1], out g);
			int.TryParse(param[2], out b);

			if (r != 0 || g != 0 || b != 0)
				return new ColorBalance(r, g, b);

			return null;
		}

		private static IFilter GetSepiaFilter(string[] param, QueryString query)
		{
			return param[0] == "1" ? new SepiaMatrix() : null;
		}

		private static IFilter GetSharpenFilter(string[] param, QueryString query)
		{
			return param[0] == "1" ? new GaussianSharpen(0.9, 1) : null;
		}

		private static IFilter GetSharpenExFilter(string[] param, QueryString query)
		{
			int amount;
			int.TryParse(param[0], out amount);

			if (amount > 0 && amount < 100)
				return new GaussianSharpen(amount * 0.025, 3);

			return GetSharpenFilter(param, query);
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

				IFilter filter = filterDef.GetFilter(param[key], query);
				if (filter != null)
				{
					this.activeFilters.Add(filter);
					this.activeQuery.Add(key, query[key]);
				}
			}
		}

		private struct FilterDefinition
		{
			public readonly GetFilter GetFilter;

			public readonly byte Arguments;

			public FilterDefinition(GetFilter getter, byte argCount)
			{
				this.GetFilter = getter;
				this.Arguments = argCount;
			}
		}
	}
}
