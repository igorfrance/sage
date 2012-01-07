namespace Sage.XsltExtensions
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Text.RegularExpressions;

	using Sage.Extensibility;

	[XsltExtensionObject(XmlNamespaces.Extensions.Regexp)]
	[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter")]
	public class Regexp
	{
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
