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
	using System.Collections.Generic;
	using System.Xml;

	using Microsoft.Ajax.Utilities;

	internal class ScriptFileConfiguration : FileTypeConfiguration
	{
		private readonly List<string> byteProps = new List<string> { "IndentSize" };

		private readonly List<string> boolProps = new List<string>
		{
			"CollapseToLiteral",
			"CombineDuplicateLiterals",
			"EvalLiteralExpressions",
			"IgnoreConditionalCompilation",
			"MacSafariQuirks",
			"ManualRenamesProperties",
			"MinifyCode",
			"PreserveFunctionNames",
			"RemoveFunctionExpressionNames",
			"RemoveUnneededCode",
			"StripDebugStatements",
			"InlineSafeStrings",
		};

		private readonly List<string> enumProps = new List<string>
		{
			"EvalTreatment",
			"LocalRenaming",
			"OutputMode",
		};

		public ScriptFileConfiguration()
		{
			this.Enabled = true;
			this.Settings = new CodeSettings
			{
				MinifyCode = true,
				OutputMode = OutputMode.SingleLine
			};
		}

		public ScriptFileConfiguration(XmlElement configurationElement)
			: this()
		{
			this.Enabled = configurationElement == null || configurationElement.GetAttribute("Enabled") == "true";
			this.Parse(configurationElement, typeof(CodeSettings), this.Settings);
		}

		public new bool Enabled { get; private set; }

		public CodeSettings Settings { get; private set; }

		protected override List<string> BoolProps
		{
			get { return boolProps; }
		}

		protected override List<string> ByteProps
		{
			get { return byteProps; }
		}

		protected override List<string> EnumProps
		{
			get { return enumProps; }
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Serialize(typeof(CodeSettings), this.Settings);
		}
	}
}
