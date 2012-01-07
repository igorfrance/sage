namespace Sage.XsltExtensions
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Xml.XPath;

	using Sage.Extensibility;

	[XsltExtensionObject(XmlNamespaces.Extensions.Math)]
	[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter")]
	public class Math
	{
		public object iif(bool condition, object result1, object result2)
		{
			return condition ? result1 : result2;
		}

		public object isnull(object result1, object result2)
		{
			switch (result1.GetType().Name)
			{
				case "Boolean":
					return (bool) result1 ? result1 : result2;

				case "String":
					return string.IsNullOrWhiteSpace((string) result1) ? result1 : result2;

				case "Double":
					return !((double) result1).Equals(0) ? result1 : result2;

				default:
					var iterator = result1 as XPathNodeIterator;
					if (iterator == null)
						return result2;

					return iterator.Count != 0 ? result1 : result2;
			}
		}
	}
}
