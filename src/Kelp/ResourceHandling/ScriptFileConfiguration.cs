namespace Kelp.ResourceHandling
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
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

		public CodeSettings Settings
		{
			get;
			private set;
		}

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

		public override string ToString()
		{
			return this.Serialize(typeof(CodeSettings), this.Settings);
		}
	}
}
