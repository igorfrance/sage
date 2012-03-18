/**
 * Copyright 2012 Igor France
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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