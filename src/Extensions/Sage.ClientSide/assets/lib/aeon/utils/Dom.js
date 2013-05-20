/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the aeon.js
 */

/**
 * Provides several dom-related utilities.
 * @type {Object}
 */
var $dom = new function dom()
{
	var generatedIds = {};

	function dom()
	{
	}

	dom.uniqueID = function Dom$uniqueID(element)
	{
		if (element && element.jquery)
			element = element[0];

		if (element == null)
			return;

		if ($string.isEmpty(element.id))
		{
			var id = $date.time();
			while (generatedIds[id] != null)
				id++;

			generatedIds[id] = id;
			element.id = "u" + id;
		}

		return element.id;
	};

	dom.unselectable = function Dom$unselectable(off)
	{
		var selectable = value in { "false": true, "off": true };
		var value = selectable ? $string.EMPTY : "none";

		var elements = $array.fromArguments(arguments);
		for (var i = 0; i < elements.length; i++)
		{
			$(elements[i]).css({
				"user-select": value,
				"-o-user-select": value,
				"-moz-user-select": value,
				"-khtml-user-select": value,
				"-webkit-user-select": value,
				"-ms-user-select": value
			});
		}
	}

	$.fn.unselectable = function unselectable(value)
	{
		return this.each(function unselectable()
		{
			dom.selectable(value);
    });
	};

	return dom;
};

