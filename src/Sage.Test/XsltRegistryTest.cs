/**
 * Open Source Initiative OSI - The MIT License (MIT):Licensing
 * [OSI Approved License]
 * The MIT License (MIT)
 *
 * Copyright (c) 2011 Igor France
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */
namespace Sage.Test
{
	using System;
	using System.Diagnostics.CodeAnalysis;

	using Machine.Specifications;

	using Sage.Views;

	[Subject(typeof(XsltTransform))]
	[Tags(Categories.ResourceManagement)]
	[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Unit tests")]
	[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Unit tests")]
	public class When_opening_an_xslt_file
	{
		private static SageContext context = Mother.CreateSageContext(string.Empty);

		private It Should_store_that_file_in_cache = () => { throw new NotImplementedException(); };

		private It Should_reuse_that_file_if_exists_in_cache = () => { throw new NotImplementedException(); };

		private It Should_pick_the_fresh_version_if_the_source_file_changes = () => { throw new NotImplementedException(); };

		private It Should_pick_the_fresh_version_if_any_dependencies_change = () => { throw new NotImplementedException(); };

		private It Should_use_project_default_stylesheet_if_specific_one_is_missing_and_project_default_exists =
			() => { throw new NotImplementedException(); };

		private It Should_use_category_default_stylesheet_if_specific_one_is_missing_and_category_default_exists =
			() => { throw new NotImplementedException(); };

		private It Should_use_builtin_default_stylesheet_if_specific_one_is_missing_and_no_other_default_stylesheets_exist =
			() => { throw new NotImplementedException(); };
	}
}