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
namespace Kelp.ResourceHandling
{
	using System;
	using System.Diagnostics.Contracts;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.IO;

	using Kelp.Core;
	using Kelp.Core.Extensions;
	using Kelp.Http;
	using Kelp.Imaging.Filters;
	using log4net;

	/// <summary>
	/// Represents an image file resource.
	/// </summary>
	public abstract class ImageFile
	{
		/// <summary>
		/// The <see cref="QueryFilter"/> associated with the current <see cref="ImageFile"/>.
		/// </summary>
		protected QueryFilter filter;

		/// <summary>
		/// The <see cref="QueryString"/> associated with the current <see cref="ImageFile"/>.
		/// </summary>
		protected QueryString parameters;

		private static readonly ILog log = LogManager.GetLogger(typeof(ImageFile).FullName);
		private static readonly string[] extensions = new[] { "gif", "jpg", "jpeg", "bmp", "png" };
		private readonly string absolutePath;
		private byte[] imageBytes;

		/// <summary>
		/// Initializes a new instance of the <see cref="ImageFile"/> class.
		/// </summary>
		/// <param name="absolutePath">The absolute path of the image.</param>
		protected ImageFile(string absolutePath)
		{
			this.absolutePath = absolutePath;
			this.UseCache = true;
		}

		/// <summary>
		/// Gets the contents of this image as a byte stream.
		/// </summary>
		public byte[] Bytes
		{
			get
			{
				return this.imageBytes ?? (this.imageBytes = this.Load());
			}
		}

		/// <summary>
		/// Gets the delegate method that handles resolving (mapping) or relative paths.
		/// </summary>
		public Func<string, string> MapPath { get;  set; }

		/// <summary>
		/// Gets the query filter associated with this image.
		/// </summary>
		public QueryFilter Filter
		{
			get
			{
				return filter;
			}
		}

		/// <summary>
		/// Gets the temporary directory in which to store the processed version of this image file instance.
		/// </summary>
		public string CacheDirectory
		{
			get
			{
				return MapPath(Configuration.Current.TemporaryDirectory);
			}
		}

		/// <summary>
		/// Gets the physical file name of the processed and cached version of this image file instance.
		/// </summary>
		public string CacheName
		{
			get
			{
				return string.Concat(absolutePath, filter.Query.ToString(true))
						.Replace('/', '_')
						.Replace('\\', '_')
						.Replace(':', '_')
						.Replace('?', '_')
						.Replace('>', '_')
						.Replace('<', '_');
			}
		}

		/// <summary>
		/// Gets the physical file path of the processed and cached version of this image file instance.
		/// </summary>
		public string CachePath
		{
			get
			{
				if (this.CacheDirectory == null)
					return null;

				return Path.Combine(CacheDirectory, CacheName);
			}
		}

		/// <summary>
		/// Gets the the content type associated with this image file instance.
		/// </summary>
		public string ContentType
		{
			get
			{
				var extension = Path.GetExtension(absolutePath).Replace(".", string.Empty).ToLower();
				if (extension == "jpg")
					extension = "jpeg";

				return "image/" + extension;
			}
		}

		/// <summary>
		/// Gets the modificaition date of this image file instance.
		/// </summary>
		public DateTime LastModified
		{
			get
			{
				DateTime dateSource = Util.GetDateLastModified(absolutePath);
				if (File.Exists(CachePath))
				{
					DateTime dateCache = Util.GetDateLastModified(CachePath);
					if (dateCache > dateSource)
						return dateCache;
				}

				return dateSource;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to use cache with this image file instance
		/// </summary>
		public bool UseCache
		 { get; set; }

		/// <summary>
		/// Gets the codec info to use when saving this image file instance.
		/// </summary>
		protected abstract ImageCodecInfo CodecInfo { get; }

		/// <summary>
		/// Gets the encode parameters to use when saving this image file instance.
		/// </summary>
		protected abstract EncoderParameters CodecParameters { get; }

		/// <summary>
		/// Creates <see cref="ImageFile"/> instances matching the specified absolute path by extension..
		/// </summary>
		/// <param name="absolutePath">The absolute path of the image.</param>
		/// <param name="parameters">The query string parameters of the <see cref="QueryFilter"/> associated 
		/// with the <see cref="ImageFile"/> that will be created.</param>
		/// <param name="mappingFunction">The mapping function to use when resolving temporary directory location.</param>
		/// <returns>A new <see cref="ImageFile"/> instance matching the specified absolute path by extension.</returns>
		public static ImageFile Create(string absolutePath, string parameters, Func<string, string> mappingFunction)
		{
			return Create(absolutePath, new QueryString(parameters), mappingFunction);
		}

		/// <summary>
		/// Creates <see cref="ImageFile"/> instances matching the specified absolute path by extension..
		/// </summary>
		/// <param name="absolutePath">The absolute path of the image.</param>
		/// <param name="parameters">The query string parameters of the <see cref="QueryFilter"/> associated 
		/// with the <see cref="ImageFile"/> that will be created.</param>
		/// <param name="mappingFunction">The mapping function to use when resolving temporary directory location.</param>
		/// <returns>A new <see cref="ImageFile"/> instance matching the specified absolute path by extension.</returns>
		public static ImageFile Create(string absolutePath, QueryString parameters, Func<string, string> mappingFunction)
		{
			ImageFile instance;
			if (absolutePath.ToLower().EndsWith("png"))
				instance = new PngFile(absolutePath);
			else if (absolutePath.ToLower().EndsWith("gif"))
				instance = new GifFile(absolutePath);
			else
				instance = new JpegFile(absolutePath);

			instance.MapPath = mappingFunction;
			instance.filter = new QueryFilter(parameters);
			instance.parameters = parameters;
			return instance;
		}

		internal static bool IsFileExtensionSupported(string extension)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(extension));
			return extension.Replace(".", string.Empty).ToLower().ContainsAnyOf(extensions);
		}

		private byte[] Load()
		{
			// usecache = false
			// do we have a cached file with all the specifications ?
				// yes:
					// is the source older than cache file?
						// usecache = true
			bool useCache = false;
			byte[] imageData = null;
			DateTime dateSource = Util.GetDateLastModified(absolutePath);

			if (this.UseCache && File.Exists(CachePath) && Util.GetDateLastModified(CachePath) > dateSource)
				useCache = true;

			if (useCache)
			{
				return File.ReadAllBytes(CachePath);
			}

			using (Bitmap inputImage = new Bitmap(absolutePath))
			using (Bitmap outputImage = filter.Apply(inputImage))
			{
				try
				{
					var cacheDir = Path.GetDirectoryName(CachePath);
					if (!Directory.Exists(cacheDir))
						Directory.CreateDirectory(cacheDir);

					outputImage.Save(CachePath);
					MemoryStream outputStream = new MemoryStream();

					outputImage.Save(outputStream, CodecInfo, CodecParameters);
					imageData = outputStream.GetBuffer();
				}
				catch (Exception ex)
				{
					log.ErrorFormat("Failed to save image to '{0}': {1}", CachePath, ex.Message);
				}
			}

			return imageData;
		}
	}
}
