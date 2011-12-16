namespace Kelp.HttpMock
{
	using System;
	using System.Web;

	/// <summary>
	/// Provides a class for mocking an <code>HttpCachePolicyBase</code>. 
	/// </summary>
	public class HttpCachePolicyMock : HttpCachePolicyBase
	{
		private HttpCacheability cacheability;
		private DateTime lastModified;
		private string etag;

		/// <summary>
		/// Sets the Cache-Control header to the specified <see cref="HttpCacheability"/> value.
		/// </summary>
		/// <param name="cacheability">The <see cref="HttpCacheability"/> enumeration value to set the 
		/// header to.</param>
		public override void SetCacheability(HttpCacheability cacheability)
		{
			this.cacheability = cacheability;
		}

		/// <summary>
		/// Sets the last modified date.
		/// </summary>
		/// <param name="lastModified">The last modification date.</param>
		public override void SetLastModified(DateTime lastModified)
		{
			this.lastModified = lastModified;
		}

		/// <summary>
		/// Sets the ETag HTTP header to the specified string.
		/// </summary>
		/// <param name="etag">The text to use for the ETag header.</param>
		public override void SetETag(string etag)
		{
			this.etag = etag;
		}
	}
}
