namespace Sage.Extensibility
{
	using System;

	/// <summary>
	/// TODO: Update summary.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class XsltExtensionObjectAttribute : Attribute
	{
		public XsltExtensionObjectAttribute(string xmlNamespace)
		{
			this.Namespace = xmlNamespace;
		}

		public string Namespace { get; set; }
	}
}
