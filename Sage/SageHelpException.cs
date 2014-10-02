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
	using System.Collections;
	using System.Collections.Generic;
	using System.Text;
	using System.Xml;

	/// <summary>
	/// Implements an exception that provides help about the error that occurred.
	/// </summary>
	internal class SageHelpException : SageException
	{
		private const string DefaultStylesheetPath = @"sageresx://sage/resources/xslt/assistanceerror.xslt";
		private static readonly Dictionary<string, Func<Exception, ProblemInfo>> parsers;

		static SageHelpException()
		{
			parsers = new Dictionary<string, Func<Exception, ProblemInfo>>();
			parsers.Add("DbEntityValidationException", ParseEntityExceptionError);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SageHelpException"/> class.
		/// </summary>
		/// <param name="problemType">The type of problem that occurred.</param>
		public SageHelpException(ProblemType problemType)
		{
			this.Problem = new ProblemInfo(problemType);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SageHelpException"/> class.
		/// </summary>
		/// <param name="problem">An object that describes this error.</param>
		public SageHelpException(ProblemInfo problem)
		{
			this.Problem = problem;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SageHelpException"/> class.
		/// </summary>
		/// <param name="problem">An object that describes this error.</param>
		/// <param name="actual">The actual exception that occurred.</param>
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
			else
			{
				var typeName = ex.GetType().Name;
				if (parsers.ContainsKey(typeName))
					problem = parsers[typeName].Invoke(ex);
			}

			if (problem.Type == ProblemType.Unknown && suggestedProblem != ProblemType.Unknown)
				problem.Type = suggestedProblem;

			var result = new SageHelpException(problem) { Exception = ex };
			return result;
		}

		protected internal override XmlElement ConvertToXml(Exception instance, XmlDocument ownerDocument, ProblemInfo problemInfo = null)
		{
			return base.ConvertToXml(instance, ownerDocument, this.Problem);
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

		private static ProblemInfo ParseEntityExceptionError(Exception e)
		{
			var errors = e.GetType().GetProperty("EntityValidationErrors").GetValue(e ) as IEnumerable;
			var errorMessage = new StringBuilder();
			
			var dictionary = new Dictionary<string, string>();
			foreach (var err1 in errors)
			{
				var validationErrors = err1.GetType().GetProperty("ValidationErrors").GetValue(err1) as IEnumerable;
				foreach (var err2 in validationErrors)
				{
					var message = err2.GetType().GetProperty("ErrorMessage").GetValue(err2) as string;
					var property = err2.GetType().GetProperty("PropertyName").GetValue(err2) as string;

					dictionary.Add(property, message);
				}
			}

			var problem = new ProblemInfo(ProblemType.EntityValidationError);
			problem.InfoBlocks.Add("ValidationErrors", dictionary);
			return problem;
		}
	}
}
