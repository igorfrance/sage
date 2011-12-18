namespace Sage
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Xml.Xsl;

	/// <summary>
	/// TODO: Update summary.
	/// </summary>
	public class SageHelpException : SageException
	{
		private static readonly string stylesheetPath = @"sageresx://sage/resources/xslt/AssistanceError.xslt";

		public SageHelpException(Exception actual, ProblemType type)
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

		protected override XsltArgumentList GetTransformArguments(SageContext context)
		{
			XsltArgumentList arguments = base.GetTransformArguments(context);
			arguments.AddParam("problemType", string.Empty, ProblemType.ToString());

			return arguments;
		}
	}
}
