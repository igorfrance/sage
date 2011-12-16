namespace Kelp.HttpMock
{
	using System;
	using System.Web;

	/// <summary>
	/// Provides a class for mocking an <code>HttpServerUtilityBase</code>. 
	/// </summary>
	public class HttpServerUtilitityMock : HttpServerUtilityBase
	{
		private readonly Func<string, string> pathMapper;

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpServerUtilitityMock"/> class.
		/// </summary>
		public HttpServerUtilitityMock()
		{
			pathMapper = delegate(string input) { return input; };
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpServerUtilitityMock"/> class.
		/// </summary>
		/// <param name="pathMapper">The function to use when calling <code>Server.MapPath</code>.</param>
		public HttpServerUtilitityMock(Func<string, string> pathMapper)
		{
			this.pathMapper = pathMapper;
		}

		/// <summary>
		/// Returns the physical file path that corresponds to the specified virtual path on the Web server.
		/// </summary>
		/// <param name="path">The virtual path to get the physical path for.</param>
		/// <returns>
		/// The physical file path that corresponds to <paramref name="path"/>.
		/// </returns>
		public override string MapPath(string path)
		{
			return pathMapper.Invoke(path);
		}
	}
}
