namespace Sage.Test.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	using Machine.Specifications;

	using Sage.Configuration;

	[Subject(typeof(CategoryConfiguration))]
	internal class When_opening_an_invalid_document
	{
		It Should_throw_an_xml_exception_with_invalid;
	}
}
