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
namespace Sage
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Xml;

	using Kelp.Extensions;

	using Kelp.XInclude;
	using Kelp.XInclude.XPointer;

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

		internal static SageHelpException Create(Exception ex, string path = null, ProblemType suggestedProblem = ProblemType.Unknown)
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

			if (problem.Type == ProblemType.Unknown && suggestedProblem != ProblemType.Unknown)
				problem.Type = suggestedProblem;

			var result = new SageHelpException(problem) { Exception = ex };
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
