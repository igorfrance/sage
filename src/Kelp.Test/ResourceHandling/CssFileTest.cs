﻿//-------------------------------------------------------------------------------------------------
// <auto-generated> 
// Marked as auto-generated so StyleCop will ignore BDD style tests
// </auto-generated>
//-------------------------------------------------------------------------------------------------
namespace Kelp.Test.ResourceHandling
{
	using System;
	using System.IO;

	using Kelp.ResourceHandling;
	using Machine.Specifications;

	[Subject(typeof(CssFile)), Tags(Categories.ResourceHandling)]
	public class When_requesting_a_non_minified_css_file
	{
		private static readonly string scriptPath = Utilities.GetStylePath("stylesheet1.css");
		private static string contents;
		private static CssFile proc;

		private Establish ctx = () =>
		{
			var tempDirectory = Utilities.MapPath(Configuration.Current.TemporaryDirectory);
			if (Directory.Exists(tempDirectory))
				Directory.Delete(tempDirectory, true);

			proc = new CssFile(scriptPath, "stylesheet1.css", Utilities.MapPath);
		};

		private Because of = () => contents = proc.Content;

		private It Should_not_be_empty = () => contents.ShouldNotBeEmpty();
		private It Should_contain_child_stylesheets = () => contents.ShouldContain("#example1");
		private It Should_contain_child_stylesheets2 = () => contents.ShouldContain("url(image1.jpg)");
		private It Should_still_contain_empty_styles = () => contents.ShouldContain("#empty");
	}

	[Subject(typeof(CssFile)), Tags(Categories.ResourceHandling)]
	public class When_requesting_a_minified_css_file
	{
		private static readonly string scriptPath = Utilities.GetStylePath("stylesheet1.css");
		private static string contents;
		private static CssFile proc;

		private Establish ctx = () =>
		{
			var tempDirectory = Utilities.MapPath(Configuration.Current.TemporaryDirectory);
			if (Directory.Exists(tempDirectory))
				Directory.Delete(tempDirectory, true);

			proc = new CssFile(scriptPath, "stylesheet1.css", Utilities.MapPath);
		};

		private Because of = () => contents = proc.Content;

		private It Should_not_be_empty = () => contents.ShouldNotBeEmpty();
		private It Should_contain_child_stylesheets = () => contents.ShouldContain("#example1");
		private It Should_contain_child_stylesheets2 = () => contents.ShouldContain("url(image1.jpg)");
	}
}
