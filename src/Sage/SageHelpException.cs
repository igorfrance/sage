namespace Sage
{
	using System;
	using System.Collections.Generic;

	public class SageHelpException : SageException
	{
		private static readonly string stylesheetPath = @"sageresx://sage/resources/xslt/AssistanceError.xslt";

		public SageHelpException(ProblemType type)
		{
			this.ProblemType = type;
		}

		public SageHelpException(ProblemType type, Exception actual)
			: base(actual)
		{
			this.ProblemType = type;
		}

		public ProblemType ProblemType
		{
			get;
			private set;
		}

		public override string StylesheetPath
		{
			get
			{
				return SageHelpException.stylesheetPath;
			}
		}

		protected override Dictionary<string, object> GetTransformArguments(SageContext context)
		{
			Dictionary<string, object> arguments = base.GetTransformArguments(context);
			arguments.Add("problemType", ProblemType.ToString());

			return arguments;
		}
	}
}
