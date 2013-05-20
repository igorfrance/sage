/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the aeon.js
 */

/**
 * @type Function
 */
var $css = new function css()
{
	var parsedCss = null;

	var styleGroups =
	{
		text: [
			"font-family", "font-size", "font-weight", "font-style", "color", "text-transform", "text-decoration", "letter-spacing", "word-spacing", "line-height", "text-align", "vertical-align", "direction", "column-count", "column-gap", "column-width"
		],

		background: [
			"background-color", "background-image", "background-repeat", "background-position", "background-attachment", "opacity"
		],

		box: [
			"width", "height", "top", "right", "bottom", "left", "margin-top", "margin-right", "margin-bottom", "margin-left", "padding-top", "padding-right", "padding-bottom", "padding-left", "border-top-width", "border-right-width", "border-bottom-width", "border-left-width", "border-top-color", "border-right-color", "border-bottom-color", "border-left-color", "border-top-style", "border-right-style", "border-bottom-style", "border-left-style", "-moz-border-top-radius", "-moz-border-right-radius", "-moz-border-bottom-radius", "-moz-border-left-radius", "outline-top-width", "outline-right-width", "outline-bottom-width", "outline-left-width", "outline-top-color", "outline-right-color", "outline-bottom-color", "outline-left-color", "outline-top-style", "outline-right-style", "outline-bottom-style", "outline-left-style"
		],

		layout: [
			"position", "display", "visibility", "z-index", "overflow", "overflow-x", "overflow-y", "overflow-clip", "white-space", "clip", "float", "clear", "-moz-box-sizing"
		],

		other: [
			"cursor", "list-style-image", "list-style-position", "list-style-type", "marker-offset", "user-focus", "user-select", "user-modify", "user-input"
		]
	};

	function css()
	{
		if (arguments.length == 0)
		{
			if (parsedCss == null)
				parsedCss = css.parseDocument(document);

			return parsedCss;
		}

		return css.computed(arguments[0]);
	}

	css.reset = function Css$reset()
	{
		parsedCss = null;
	};

	css.parseDocument = function Css$parseDocument(doc)
	{
		var result = {};
		for (var i = 0; i < doc.styleSheets.length; i++)
		{
			var rules = css.parseStylesheet(doc.styleSheets[i]);
			for (var selector in rules)
				result[selector] = rules[selector];
		}

		return result;
	};

	css.parseStylesheet = function Css$parseStylesheet(stylesheet)
	{
		var result = {};
		var rules = stylesheet.rules || stylesheet.cssRules;
		for (var j = 0; j < rules.length; j++)
		{
			if (rules[j].selectorText)
			{
				result[rules[j].selectorText] = rules[j];
			}
		}

		return result;
	};

	css.computed = function Css$computed(element, prop)
	{
		var result = {};

		if (element)
			element = $(element)[0];
		if (!element)
			return result;

		var computedStyle = document.defaultView.getComputedStyle(element, null);
		for (var i = 0; i < computedStyle.length; i++)
		{
			var propName = computedStyle[i];
			var camelCased = propName.replace(/\-([a-z])/g, function(a,b){
				return b.toUpperCase();
			});

			result[camelCased] = computedStyle.getPropertyValue(propName);
		}

		if (prop)
			return result[prop];

		return result;
	};

	css.findRules = function Css$findRules(expression)
	{
		var rules = css();
		if (expression)
		{
			var re = new RegExp(expression);
			var result = {};
			for (var selectorText in rules)
			{
				if (selectorText.match(re))
				{
					result[selectorText] = rules[selectorText];
				}
			}

			return result;
		}

		return rules;
	};

	css.getRule = function Css$getRule(selectorText)
	{
		return parsedCss[selectorText];
	};

	css.getStyle = function Css$getStyle(selectorText)
	{
		var rule = css.getRule(selectorText);
		return rule ? rule.style.cssText : $string.EMPTY;
	};

	css.getProperty = function Css$getProperty(selectorText, property)
	{
		var rule = css.getRule(selectorText);
		return rule ? rule.style[property] : $string.EMPTY;
	};

	css.copyStyles = function Css$copyStyles(from, to, group)
	{
		if (from && from.jquery)
			from = from[0];
		if (to && to.jquery)
			to = to[0];

		if (!from || !to)
			return;

		var computed = css.computed(from);
		for (var name in styleGroups)
		{
			if (group && name != group)
				continue;

			for (var i = 0; i < styleGroups[name].length; i++)
			{
				var prop = styleGroups[name][i];
				if (computed[prop] == undefined)
					continue;

				to.style[prop] = computed[prop];

				aeon.log("{0}: {1}", prop, computed[prop]);
			}
		}
	};

	return css;

};
