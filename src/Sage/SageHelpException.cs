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

		public SageHelpException(ProblemType type, Exception actual, string path)
			: this(type, actual)
		{
			this.Path = path;
		}

		public ProblemType ProblemType { get; private set; }

		public string Path { get; private set; }

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
			if (this.Path != null)
				arguments.Add("path", this.Path);

			return arguments;
		}
	}
}
