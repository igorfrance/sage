namespace Kelp.ResourceHandling
{
	using System;
	using System.Collections.Generic;
	using System.Xml;

	using Microsoft.Ajax.Utilities;

	internal class CssFileConfiguration : FileTypeConfiguration
	{
		private readonly List<string> byteProps = new List<string> { "IndentSize" };
		private readonly List<string> boolProps = new List<string>
		{
			"MinifyExpressions", "TermSemicolons"
		};

		private readonly List<string> enumProps = new List<string>
		{
			"ColorNames", "CommentMode"
		};

		public CssFileConfiguration()
		{
			this.Settings = new CssSettings
			{
				MinifyExpressions = true
			};
		}

		public CssFileConfiguration(XmlElement configurationElement)
			: this()
		{
			this.Enabled = configurationElement == null || configurationElement.GetAttribute("Enabled") == "true";
			this.Parse(configurationElement, typeof(CssSettings), this.Settings);
		}

		public CssSettings Settings
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
			return this.Serialize(typeof(CssSettings), this.Settings);
		}
	}
}
