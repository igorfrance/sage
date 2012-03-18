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
			this.Parse(configurationElement, typeof(CodeSettings), this.Settings);
		}

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
