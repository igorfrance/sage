﻿// <auto-generated>Marked as auto-generated so StyleCop will ignore BDD style tests</auto-generated>
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
namespace Sage.Test.Extensibility
{
	// ReSharper disable InconsistentNaming
	// ReSharper disable UnusedMember.Global
	// ReSharper disable UnusedMember.Local

	using System;
	using System.Linq;
	using Machine.Specifications;
	using Sage.Extensibility;

	[Subject(typeof(TextEvaluator)), Tags(Categories.Extensibility)]
	public class When_registering_text_handler
	{
		static readonly SageContext context = Mother.CreateSageContext("default", "uk");
		static string varName = "myvar";

		It Should_throw_exceptions_with_null_parameters = () =>
		{
			Catch.Exception(() => TextEvaluator.RegisterVariable(null, null)).ShouldBeOfType<ArgumentNullException>();
			Catch.Exception(() => TextEvaluator.RegisterVariable(varName, null)).ShouldBeOfType<ArgumentNullException>();
		};

		It Should_use_the_handler_correctly = () =>
		{
			TextEvaluator.RegisterVariable(varName, SubstituteText);
			string result = TextEvaluator.Process("I say: ${myvar}", context);
			result.ShouldEqual("I say: Hello!");
		};

		static string SubstituteText(SageContext context, string varName)
		{
			return "Hello!";
		}
	}

	[Subject(typeof(TextEvaluator)), Tags(Categories.Extensibility)]
	public class When_processing_complex_expressions
	{
		static SageContext context = Mother.CreateSageContext("/");
		static TextEvaluator text;

		Establish c = () =>
		{
			context = Mother.CreateSageContext("/");
			text = new TextEvaluator(context);

			TextEvaluator.RegisterFunction("str:join", Join);
			TextEvaluator.RegisterFunction("str:concat", Concatenate);
			TextEvaluator.RegisterFunction("str:upper", ToUpper);
			TextEvaluator.RegisterFunction("str:lower", ToLower);
			TextEvaluator.RegisterFunction("str:proper", ToProper);
			TextEvaluator.RegisterVariable("a", VariableApple);
			TextEvaluator.RegisterVariable("p", VariablePear);
			TextEvaluator.RegisterVariable("b", VariableBanana);
			TextEvaluator.RegisterVariable("c", VariableCoconut);
		};

		It Should_properly_resolve_multiple_variables = () =>
			text.Process("${var:a is red, var:b is yellow, var:x is nothing}")
			.ShouldEqual("apple is red, banana is yellow, undefined is nothing");

		It Should_properly_resolve_multiple_functions = () =>
			text.Process("${str:join(',', a, b, c) is a,b,c, and str:concat(a,b,c) is abc}")
			.ShouldEqual("a,b,c is a,b,c, and abc is abc");

		It Should_properly_resolve_multiple_variables_and_functions = () =>
			text.Process("${str:upper(var:a)}")
			.ShouldEqual("APPLE");

		It Should_properly_resolve_multiple_nested_functions_and_variables = () =>
			text.Process("${str:upper(var:a) is str:lower(var:a) is str:proper(str:lower(str:upper(var:a)))}")
			.ShouldEqual("APPLE is apple is Apple");

		It Should_ignore_escaped_expressions = () =>
			text.Process("${str:upper(var:a) is APPLE} and \\${escaped is str:upper(ignored)}")
			.ShouldEqual("APPLE is APPLE and ${escaped is str:upper(ignored)}");

		public static string Join(SageContext context, params string[] arguments)
		{
			if (arguments.Length < 2)
				return string.Empty;

			var separator = arguments[0];
			var parameters = arguments.Where((s, i) => i > 0);

			return string.Join(separator, parameters);
		}

		public static string Concatenate(SageContext context, params string[] arguments)
		{
			return string.Concat(arguments);
		}

		public static string ToUpper(SageContext context, params string[] arguments)
		{
			if (arguments.Length < 1 || arguments[0] == null)
				return string.Empty;

			return arguments[0].ToUpper();
		}

		public static string ToLower(SageContext context, params string[] arguments)
		{
			if (arguments.Length < 1 || arguments[0] == null)
				return string.Empty;

			return arguments[0].ToLower();
		}

		public static string ToProper(SageContext context, params string[] arguments)
		{
			if (arguments.Length < 1 || arguments[0] == null)
				return string.Empty;

			return string.Concat(arguments[0][0].ToString().ToUpper(), arguments[0].Substring(1));
		}

		public static string VariableApple(SageContext context, string varName)
		{
			return "apple";
		}

		public static string VariablePear(SageContext context, string varName)
		{
			return "pear";
		}

		public static string VariableBanana(SageContext context, string varName)
		{
			return "banana";
		}

		public static string VariableCoconut(SageContext context, string varName)
		{
			return "coconut";
		}
	}
}
