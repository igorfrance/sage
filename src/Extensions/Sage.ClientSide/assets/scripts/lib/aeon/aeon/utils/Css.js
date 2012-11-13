Type.registerNamespace("aeon.utils");

aeon.utils.Css = {};

aeon.utils.Css.parseCssFromStylesheet = function Css$parseCssFromStylesheet(stylesheet)
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

aeon.utils.Css.computedCss = function Css$computedCss(element)
{
	var result = {};

	if (!element)
		return result;

	if (element.length)
		element = element[0];

	if (window.getComputedStyle)
	{
		var computedStyle = window.getComputedStyle(element, null);
		for (var i = 0; i < computedStyle.length; i++)
		{
			var propName = computedStyle[i];
			var camelCased = propName.replace(/\-([a-z])/g, function(a,b){
				return b.toUpperCase();
			});

			result[camelCased] = computedStyle.getPropertyValue(propName);
		}
	}

	if (element.currentStyle)
	{
		for (var propName in element.currentStyle)
			result[propName] = element.currentStyle[propName];
	}

	return result;
};

aeon.utils.Css.computedStyle = function Css$computedStyle(element)
{
	if (window.getComputedStyle)
	{
		return window.getComputedStyle(element, null);
	}
	else if (element.currentStyle)
	{
		return element.currentStyle;
	}
	else
	{
		return {};
	}
};

aeon.utils.Css.parseCssFromDocument = function Css$parseCssFromDocument(doc)
{
	var result = {};
	for (var i = 0; i < doc.styleSheets.length; i++)
	{
		var rules = aeon.utils.Css.parseCssFromStylesheet(doc.styleSheets[i]);
		for (var selector in rules)
			result[selector] = rules[selector];
	}

	return result;
};

aeon.utils.Css.getAllRules = function Css$getCssRules(resetCache)
{
	if (!aeon.utils.Css.__cachedCss || resetCache)
		aeon.utils.Css.__cachedCss = aeon.utils.Css.parseCssFromDocument(document);

	return aeon.utils.Css.__cachedCss;
}

aeon.utils.Css.findRules = function Css$findRules(expression)
{
	var rules = this.getAllRules();
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

aeon.utils.Css.getCssRule = function Css$getCssRule(selectorText, resetCache)
{
	var selectedCss = aeon.utils.Css.getAllRules(resetCache);
	return selectedCss[selectorText];
};

aeon.utils.Css.getCssStyle = function Css$getCssStyle(selectorText, resetCache)
{
	var rule = aeon.utils.Css.getCssRule(selectorText, resetCache);
	if (rule)
		return rule.style.cssText;

	return String.EMPTY;
};

aeon.utils.Css.getCssProperty = function Css$getCssProperty(selectorText, property, resetCache)
{
	var rule = aeon.utils.Css.getCssRule(selectorText, resetCache);
	if (rule)
		return rule.style[property];

	return String.EMPTY;
};

// Register jquery plugin
jQuery.fn.computedCss = function Css$jQuery$computedCss()
{
	return $css.computedCss(this.eq(0));
};

/**
 * Global alias to <c>aeon.utils.Css</c>
 * @type {aeon.utils.Css}
 */
var $css = aeon.utils.Css;

