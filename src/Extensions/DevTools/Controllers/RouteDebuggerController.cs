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
namespace Sage.DevTools.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Web.Routing;

	using Kelp.HttpMock;

	using Sage.Controllers;
	using Sage.Routing;

	/// <summary>
	/// Implements a controller that enables testing of route matching.
	/// </summary>
	public class RouteDebuggerController : SageController
	{
		/// <summary>
		/// Tests the specified URL against all configured routes and displays a view that shows the results as an HTML table. 
		/// </summary>
		/// <param name="url">The URL to test</param>
		/// <param name="httpMethod">The HTTP request method to test with.</param>
		/// <returns>The HTML string that contains the table with the test results.</returns>
		public string Index(string url, string httpMethod)
		{
			if (!Context.IsDeveloperRequest)
			{
				Response.StatusCode = 404;
				return string.Empty;
			}

			url = MakeAppRelative(url ?? string.Empty);
			httpMethod = httpMethod ?? "GET";

			var httpContext = new HttpContextMock(url, httpMethod);

			var httpMethodOptions = FormatOptions(httpMethod, new[] { "GET", "POST", "PUT", "DELETE", "HEAD" });
			var routeDataText = GetRoutesText(httpContext);
			return string.Format(HtmlFormat, url, httpMethodOptions, routeDataText);
		}

		private static string GetRoutesText(HttpContextMock fakeContext)
		{
			var sb = new StringBuilder();
			foreach (Route route in RouteTable.Routes)
			{
				RouteData rd = route.GetRouteData(fakeContext);

				var isMatch = false;
				var match = rd == null ? "No" : "Yes";

				// Get values
				var values = "N/A";
				if (rd != null)
				{
					isMatch = true;
					values = FormatValues(rd.Values);
				}

				// Get defaults
				var defaults = FormatValues(route.Defaults);

				// Get constraints
				var constraints = FormatValues(route.Constraints);

				// Get dataTokens
				var dataTokens = FormatValues(route.DataTokens);

				// Create table row
				var name = route is LowerCaseRoute ? ((LowerCaseRoute)route).Name : "No name";
				var row = FormatRow(isMatch, match, name, route.Url, defaults, constraints, dataTokens, values);
				sb.Append(row);
			}

			return sb.ToString();
		}

		private static string FormatValues(RouteValueDictionary values)
		{
			if (values == null)
			{
				return "N/A";
			}

			var col = new List<string>();
			foreach (string key in values.Keys)
			{
				object value = values[key] ?? "[null]";
				col.Add(key + "=" + value);
			}

			return String.Join(", ", col.ToArray());
		}

		private static string FormatOptions(string selected, string[] values)
		{
			var sb = new StringBuilder();
			foreach (string value in values)
			{
				var showSelected = string.Empty;
				if (value == selected)
				{
					showSelected = "selected='selected'";
				}

				sb.AppendFormat("<option value='{0}' {1}>{0}</option>", value, showSelected);
			}

			return sb.ToString();
		}

		private static string FormatRow(bool hilite, params string[] cells)
		{
			var sb = new StringBuilder();
			sb.Append(hilite ? "<tr class='hilite'>" : "<tr>");
			foreach (string cell in cells)
			{
				sb.AppendFormat("<td><div class=\"tooltip\" title=\"{0}\">{0}</div></td>", cell);
			}

			sb.Append("</tr>");
			return sb.ToString();
		}

		private static string MakeAppRelative(string url)
		{
			if (!url.StartsWith("~"))
			{
				if (!url.StartsWith("/"))
				{
					url = "~/" + url;
				}
				else
				{
					url = "~" + url;
				}
			}

			return url;
		}

		#region ViewFormatString

		private const string HtmlFormat =
			@"
			<html>
			<head>
				<title>Route Debugger</title>
				<style type='text/css'>
				* {{ font-size: 11px; font-family: Lucida Sans Unicode; }}
				table {{ table-layout: fixed; border-collapse:collapse }}
				td {{ border: solid 1px black; padding:3px; }}
				td div {{ height: 14px; overflow: hidden; }}
				.hilite {{background-color:lightgreen}}
				#tooltip {{ background-color: #F5F5F5; color: black; border: 1px solid #4F5C71; padding: 2px;
						position: absolute; left: 0; top: 0; height: 18; font-size: 10px; }}
				</style>
			</head>
			<body>
			
			<form action=''>
			<label for='url'>URL:</label>
			<input name='url' size='60' value='{0}' />
			<select name='httpMethod'>
			{1}
			</select>
			<input type='submit' value='Debug' />
			</form>

			<table>
			<tr>
				<th width=""40"">Match</th>
				<th>Name</th>
				<th>Url</th>
				<th>Defaults</th>
				<th>Constraints</th>
				<th>DataTokens</th>
				<th>Values</th>
			</tr>
			{2}
			</table>

			</body>
			</html>

			";

		#endregion
	}
}