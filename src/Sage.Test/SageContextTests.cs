namespace Sage.Test
{
	using System;
	using System.Diagnostics;
	using Machine.Specifications;

	[Subject(typeof(SageContext))]
	internal class When_processing_text_expressions
	{
		It Should_not_throw_any_exceptions = () =>
			{
				SageContext instance = Mother.CreateSageContext("/");
				Debug.WriteLine("Hello {0}!", instance);
			};
	}
}
