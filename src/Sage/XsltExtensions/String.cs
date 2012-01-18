namespace Sage.XsltExtensions
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Text.RegularExpressions;

	using Sage.Extensibility;

	[XsltExtensionObject(XmlNamespaces.Extensions.String)]
	[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter")]
	public class String
	{
		public string format1(string format, string value1)
		{
			return string.Format(format, value1);
		}

		public string format2(string format, string value1, string value2)
		{
			return string.Format(format, value1, value2);
		}

		public string format3(string format, string value1, string value2, string value3)
		{
			return string.Format(format, value1, value2, value3);
		}

		public string format4(string format, string value1, string value2, string value3, string value4)
		{
			return string.Format(format, value1, value2, value3, value4);
		}

		public string format5(string format, string value1, string value2, string value3, string value4, string value5)
		{
			return string.Format(format, value1, value2, value3, value4, value5);
		}

		public string replace(string input, string expression, string replacement)
		{
			if (string.IsNullOrEmpty(input))
				return input;

			try
			{
				Regex expr = new Regex(expression);
				return expr.Replace(input, replacement ?? string.Empty);
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}
	}
}
