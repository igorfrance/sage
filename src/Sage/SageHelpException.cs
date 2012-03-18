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
namespace Sage
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Xml;

	using Kelp.Extensions;

	using Mvp.Xml.XInclude;
	using Mvp.Xml.XPointer;

	/// <summary>
	/// Implements an exception that provides help about the error that occured.
	/// </summary>
	internal class SageHelpException : SageException
	{
		private const string DefaultStylesheetPath = @"sageresx://sage/resources/xslt/AssistanceError.xslt";

		/// <summary>
		/// Initializes a new instance of the <see cref="SageHelpException"/> class.
		/// </summary>
		/// <param name="problemType">The type of problem that occured.</param>
		public SageHelpException(ProblemType problemType)
		{
			this.Problem = new ProblemInfo(problemType);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SageHelpException"/> class.
		/// </summary>
		/// <param name="problem">An object that descibes this error.</param>
		public SageHelpException(ProblemInfo problem)
		{
			this.Problem = problem;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SageHelpException"/> class.
		/// </summary>
		/// <param name="problem">An object that descibes this error.</param>
		/// <param name="actual">The actual exception that occured.</param>
		public SageHelpException(ProblemInfo problem, Exception actual)
			: base(actual)
		{
			this.Problem = problem;
		}

		/// <summary>
		/// Gets problem info associated with this exception
		/// </summary>
		public ProblemInfo Problem { get; private set; }

		/// <inheritdoc/>
		public override string StylesheetPath
		{
			get
			{
				return DefaultStylesheetPath;
			}
		}

		internal static SageHelpException Create(Exception ex, string path = null)
		{
			ProblemInfo problem = new ProblemInfo(ProblemType.Unknown);
			if (ex is XmlException)
			{
				if (ex.Message.Contains("undeclared prefix"))
					problem = new ProblemInfo(ProblemType.MissingNamespaceDeclaration, path);

				else if (path != null && (path.EndsWith("html") || path.EndsWith("htm")))
					problem = new ProblemInfo(ProblemType.InvalidHtmlMarkup, path);

				else
					problem = new ProblemInfo(ProblemType.InvalidMarkup, path);
			}

			if (ex is FatalResourceException)
			{
				if (ex.Root() is FileNotFoundException)
					problem = new ProblemInfo(ProblemType.IncludeNotFound, path);

				if (ex.Root() is NoSubresourcesIdentifiedException)
					problem = new ProblemInfo(ProblemType.IncludeFragmentNotFound, path);

				if (ex.Root() is XPointerSyntaxException)
					problem = new ProblemInfo(ProblemType.IncludeSyntaxError, path);
			}

			var result = new SageHelpException(problem);
			result.Exception = ex;

			return result;
		}

		/// <inheritdoc/>
		protected override Dictionary<string, object> GetTransformArguments(SageContext context)
		{
			Dictionary<string, object> arguments = base.GetTransformArguments(context);
			arguments.Add("problemType", this.Problem.Type.ToString());
			if (this.Problem.FilePath != null)
				arguments.Add("path", this.Problem.FilePath);

			return arguments;
		}
	}
}
