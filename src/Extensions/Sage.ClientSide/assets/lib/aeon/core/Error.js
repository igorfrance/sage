/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the aeon.js
 */

/**
 * Provides error handling utilities
 * @type {Function}
 */
var $error = new function()
{
	var error = function error()
	{
		var name = "Error";
		var message = String(arguments[0]);
		var data = undefined;

		if (arguments.length == 1)
		{
			if ($type.isObject(arguments[0]))
			{
				name = arguments[0].name || name;
				message = arguments[0].message || message;
				data = arguments[0].data || data;
			}
		}
		else if (arguments.length > 1)
		{
			if ($type.isString(arguments[0]))
				name = String(arguments[0]);

			if ($type.isString(arguments[1]))
				message = String(arguments[1]);

			if (!$type.isNull(arguments[2]))
				data = String(arguments[2]);
		}

		var result = new Error();
		result.name = name;
		result.message = message;
		if (data != undefined)
			result.data = data;

		return result;
	};

	return error;
};
