namespace Sage.XsltExtensions
{
	using System;
	using System.Diagnostics.CodeAnalysis;

	using Sage.Extensibility;

	[XsltExtensionObject(XmlNamespaces.Extensions.Date)]
	[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter")]
	public class Date
	{
		public string date()
		{
			return "date1";
		}

		public string date(string p1)
		{
			return "date2";
		}
	}
}
