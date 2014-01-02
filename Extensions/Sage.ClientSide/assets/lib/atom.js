/*!
 * atom.js  v.2.0
 *
 * This library depends on jQuery. Make sure jQuery 1.8 or higher is included before this script.
 *
 * Copyright 2012 Igor France
 * Licensed under the MIT License
 *
 */
var atom = new function atom()
{
	var atom = {};
	var $ = window.jQuery;

/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the atom.js
 */

/**
 * Provides utility methods for working with javascript types.
 */
var $type = new function type()
{
	function type()
	{
		if (arguments.length)
			return typeof(arguments[0]);

		return null;
	}

	/**
	 * Registers the supplied namespace.
	 * @param {String} namespace The string that specifies the namespace to register, eg: <c>atom.utils.types</c>
	 */
	type.registerNamespace = function type$registerNamespace(namespace)
	{
		var parts = namespace.split(".");
		var context = window;
		for (var i = 0; i < parts.length; i++)
		{
			if (context[parts[i]] == null)
				context[parts[i]] = {};

			context = context[parts[i]];
		}
	};

	/**
	 * Returns <c>true</c> if the specified object is an array.
	 * @param {Object} value The value to test.
	 * @return {Boolean} <c>true</c> if the specified object is an array, otherwise <c>false</c>.
	 */
	type.isArray = function type$isArray(value)
	{
		if (value == null)
			return false;

		return (
			(value.constructor == Array) ||
			(value != window && !$type.isString(value) && !$type.isFunction(value) && $type.isNumber(value.length))
		);
	};

	/**
	 * Returns <c>true</c> if the specified object is a boolean.
	 * @param {Object} value The value to test.
	 * @return {Boolean} <c>true</c> if the specified object is a boolean, otherwise <c>false</c>.
	 */
	type.isBoolean = function type$isBoolean(value)
	{
		if (value == null)
			return false;

		return (
			(type(value) == 'boolean') ||
			(value.constructor == Boolean)
		);
	};

	/**
	 * Returns <c>true</c> if the specified object is a function.
	 * @param {Object} value The value to test.
	 * @return {Boolean} <c>true</c> if the specified object is a function, otherwise <c>false</c>.
	 */
	type.isFunction = function type$isFunction(value)
	{
		if (value == null)
			return false;

		return (
			(type(value) == 'function') ||
			(value.constructor == Function) ||
			(!$type.isString(value) && Boolean(String(value).match(/\bfunction\b/)))
		);
	};

	/**
	 * Returns <c>true</c> if the specified object is a null.
	 * @param {Object} value The value to test.
	 * @return {Boolean} <c>true</c> if the specified object is a null, otherwise <c>false</c>.
	 */
	type.isNull = function type$isNull(value)
	{
		return (value == null || value == undefined);
	};

	/**
	 * Returns <c>true</c> if the specified object is a number.
	 * @param {Object} value The value to test.
	 * @return {Boolean} <c>true</c> if the specified object is a number, otherwise <c>false</c>.
	 */
	type.isNumber = function type$isNumber(value)
	{
		if (value == null)
			return false;

		return (
			(type(value) == 'number' && isFinite(value)) ||
			(value.constructor == Number)
		);
	};

	/**
	 * Returns <c>true</c> if the specified object can be converted to a number.
	 * @param {Object} value The value to test.
	 * @return {Boolean} <c>true</c> if the specified object can be converted to a number, otherwise <c>false</c>.
	 */
	type.isNumeric = function type$isNumeric(value)
	{
		return !isNaN(parseFloat(value));
	};

	/**
	 * Returns <c>true</c> if the specified object is a date.
	 * @param {Object} value The value to test.
	 * @return {Boolean} <c>true</c> if the specified object is a date, otherwise <c>false</c>.
	 */
	type.isDate = function type$isDate(value)
	{
		if (value == null)
			return false;

		return (
			($type.isFunction(value.getDate) && $type.isFunction(value.getTime))
		);
	};

	/**
	 * Returns <c>true</c> if the specified value is an object (other than String or Number).
	 * @param {Object} value The value to test.
	 * @return {Boolean} <c>true</c> if the specified object is an object, otherwise <c>false</c>.
	 */
	type.isObject = function type$isObject(value)
	{
		if (value == null || value == undefined)
			return false;

		return (
			type(value) == 'object' &&
			value.constructor != String &&
			value.constructor != Number

		) || $type.isFunction(value);
	};

	/**
	 * Returns <c>true</c> if the specified object is a string.
	 * @param {Object} value The value to test.
	 * @return {Boolean} <c>true</c> if the specified object is a string, otherwise <c>false</c>.
	 */
	type.isString = function type$isString(value)
	{
		if (value == null)
			return false;

		return (
			type(value) == 'string' ||
			(value != null && value.constructor == String)
		);
	};

	/**
	 * Returns <c>true</c> if the specified value is an HTML or XML node.
	 * @param {Object} value The value to test.
	 * @return {Boolean} <c>true</c> if the specified object is an XML or HTML node, otherwise <c>false</c>.
	 */
	type.isNode = function type$isNode(value)
	{
		return ($type.isObject(value) && $type.isNumber(value.nodeType));
	};

	/**
	 * Returns <c>true</c> if the specified value is an HTML or XML document.
	 * @param {Object} value The value to test.
	 * @return {Boolean} <c>true</c> if the specified object is an HTML or XML document, otherwise <c>false</c>.
	 */
	type.isDocument = function type$isDocument(value)
	{
		return ($type.isNode(value) && value.nodeType == 9);
	};

	/**
	 * Returns <c>true</c> if the specified value is an XML or HTML element.
	 * @param {Object} value The object to test.
	 * @return {Boolean} <c>true</c> if the specified object is an XML or HTML element, otherwise <c>false</c>.
	 */
	type.isElement = function type$isElement(value)
	{
		return ($type.isNode(value) && value.nodeType == 1);
	};

	/**
	 * Returns <c>true</c> if the specified object is an HTML element.
	 * @param {Object} object The object to test.
	 * @return {Boolean} <c>true</c> if the specified object is an HTML element, otherwise <c>false</c>.
	 */
	type.isHtmlElement = function type$isHtmlElement(object)
	{
		return ($type.isElement(object) && $type.isString(object.className));
	};

	/**
	 * Returns <c>true</c> if the specified object is an instance of the specified $type.
	 * @param {Object} object The object to test.
	 * @param {Function} type The type to test against.
	 * @return {Boolean} <c>true</c> if the specified object is an instance of the specified type, otherwise <c>false</c>.
	 */
	type.instanceOf = function type$instanceOf(object, type)
	{
		if (!$type.isObject(object) || !$type.isFunction(type))
			return false;

		if (object instanceof type)
			return true;

		if (object.__type && object.__type.constructor == type)
			return true;

		if (object.__type && object.__type.base != null)
		{
			var parent = object;
			while (parent = parent.__type.base)
			{
				if (parent.__type.constructor == type)
					return true;
			}
		}
		return false;
	};

	/**
	 * Returns <c>true</c> if the specified object implements the specified interface.
	 * @param {Object} object The object to test.
	 * @param {Object} type The interface to test with.
	 * @return {Boolean} <c>true</c> if the specified object implements the specified interface, otherwise <c>false</c>.
	 */
	type.implements = function type$implements(object, type)
	{
		var result = true;
		if (object == null || type == null)
		{
			result = false;
		}
		else
		{
			for (var name in type)
			{
				if (!type.hasOwnProperty(name))
					continue;

				if (!$type.isFunction(object[name]))
				{
					result = false;
					break;
				}
			}
		}

		return result;
	};

	/**
	 * Copies all properties from source to target.
	 * @param {Object} target The target object to copy the properties to.
	 * @param {Object} source The source object to copy the properties from.
	 * @param {Boolean} overwrite If <c>true</c>, a property will be copied from source even if it exists on the target already.
	 * @returns {Object} The target object that was supplied.
	 */
	type.extend = function type$extend(target, source, overwrite)
	{
		overwrite = overwrite || false;

		if (target && source)
		{
			for (var name in source)
			{
				if (!source.hasOwnProperty(name))
					continue;

				if (target[name] == undefined || overwrite)
					target[name] = source[name];
			}
		}

		return target;
	};

	return type;

};
/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the atom.js
 */
var $string = new function()
{
	var $string = this;

	/**
	 * Defines an empty string as a constant value.
	 * @type {String}
	 */
	$string.EMPTY = "";

	var RX_FORMAT;

	function applyPadding(str, count, character, direction)
	{
		var value = String(str);
		var diff = count - value.length;
		var output = [];
		while (output.length < diff)
			output.push(character);

		output = output.join($string.EMPTY);
		return (direction == 2 ? value + output : output + value);
	}

	/**
	 * Replaces the specified format string with supplied arguments.
	 * @param {String} value The format string.
	 * @arguments {String} 1-n The values to replace the string with.
	 * @return {String} The formatted version of the supplied string.
	 */
	this.format = function format(value)
	{
		var args = $array.fromArguments(arguments, 1);

		if (RX_FORMAT == null)
			RX_FORMAT = /\{(\d+)([$#])?(\d+)?([+-])?\}/g;

		return String(value).replace(RX_FORMAT, function applyFormat()
		{
			try
			{
				var value = String(args[arguments[1]]);
			}
			catch(e) { value = $string.EMPTY; }

			if (arguments[2] != $string.EMPTY && arguments[3] != $string.EMPTY)
			{
				var direction = arguments[4] == "+" ? 2 : 1;
				value = applyPadding(value, arguments[3], arguments[2] == "#" ? "0" : " ", direction);
			}
			return value;
		});
	};

	/**
	 * Indicates whether the specified <c>value</c> is null or an empty string.
	 * @param {String} value The string to check.
	 * @returns {Boolean} <c>true</c> if the value parameter is null or an empty string ("");
	 * otherwise, <c>false</c>.
	 */
	this.isEmpty = function isEmpty(value)
	{
		return value == null || $string.trim(value) == $string.EMPTY;
	};

	/**
	 * Removes characters from the beginning and the end of the supplied string value.
	 * @param {String} value The string to trim.
	 * @param {String} [expression] Optional string specifying the regular expression that selects the characters to remove.
	 * The default expression selects and removes any whitespace characters (<c>[\s]*</c>).
	 * @return {String} The trimmed value.
	 */
	this.trim = function trim(value, expression)
	{
		if (expression == null)
			expression = "\\s";

		var re = new RegExp("^[" + expression + "]*([\\s\\S]*?)[" + expression + "]*$");
		return String(value).replace(re, "$1");
	};

	/**
	 * Prepends characters to the specified <c>value</c> until it reaches the specified length.
	 * @param {String} value The string to pad.
	 * @param {Number} length The final length that the string should be.
	 * @param {String} padChar The character to pad the string with, default is ' '. Optional.
	 * @return {String} The padded version of the specified <c>value</c>.
	 */
	this.padLeft = function padLeft(value, length, padChar)
	{
		if (padChar == undefined)
			padChar = " ";

		var result = String(value);
		while(result.length < length)
			result = padChar + result;

		return result;
	};

	/**
	 * Appends characters to the specified <c>value</c> until it reaches the specified length.
	 * @param {String} value The string to pad.
	 * @param {Number} length The final length that the string should be.
	 * @param {String} padChar The character to pad the string with, default is ' '. Optional.
	 * @return {String} The padded version of the specified <c>value</c>.
	 */
	this.padRight = function padRight(value, length, padChar)
	{
		if (padChar == undefined)
			padChar = " ";

		var result = String(value);
		while(result.length < length)
			result = result + padChar;

		return result;
	};

	/**
	 * Returns a string that repeats the specified <c>value</c> the specified <c>count</c> of times.
	 * @param {String} value The string to repeat.
	 * @param {Number} count The count of times to repeat the string.
	 * @return {String} The specified <c>value</c>, repeated the specified <c>count</c> of times.
	 */
	this.repeat = function repeat(value, count)
	{
		var result = [];
		for (var i = 0; i < count; i++)
			result.push(value);

		return result.join($string.EMPTY);
	};

	/**
	 * Combines the text of all arguments into a single string.
	 * @arguments {String} 0-n The values to concatenate.
	 * @return {String} The text of all arguments combined into a single string.
	 */
	this.concat = function concat()
	{
		var result = [];
		for (var i = 0; i < arguments.length; i++)
		{
			result.push(arguments[i]);
		}

		return result.join($string.EMPTY);
	};


	/**
	 * Removes characters from the beginning and the end of the current string.
	 * @param {String} expression The string specifying the regular expression that selects the characters to remove. The default expression
	 * selected any whitespace characters. Optional.
	 * @return {String} The trimmed version of the current string
	 */
	String.prototype.trim = function String$trim(expression)
	{
		return $string.trim(this, expression);
	};

	/**
	 * Replaces the current string with supplied arguments.
	 * @arguments {String} [0-n] The values to replace the string with.
	 * @return {String} The formatted version of the current string.
	 */
	String.prototype.format = function String$format()
	{
		return $string.format(this, arguments);
	};
};

/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the atom.js
 */

/**
 * Generates a random integer, optionally limited within a range.
 * @param {Number} [min] The minimum value of the random number to generate. Default is 0.
 * @param {Number} [max] The maximum value of the random number to generate. Default is <c>Number.MAX_VALUE</c>.
 * @param {Boolean} [noRounding] Prevents function from rounding the generated number to the closest integer. Default is false.
 * @returns {Number}
 */
Number.random = function Number$random(min, max, noRounding)
{
	var _min = 0;
	var _max = Number.MAX_VALUE;
	var _round = true;

	if (arguments.length == 1)
	{
		if ($type.isBoolean(arguments[0]))
			_round = !arguments[0];
		else if ($type.isNumber(arguments[0]))
			_max = arguments[0];
	}

	if (arguments.length == 2)
	{
		if ($type.isBoolean(arguments[1]))
		{
			_max = parseInt(arguments[0]) || 0;
			_round = !arguments[1];
		}
		else if ($type.isNumber(arguments[1]))
		{
			_min = parseInt(arguments[0]) || 0;
			_max = arguments[1];
		}
	}

	if (arguments.length == 3)
	{
		_min = parseInt(arguments[0]) || 0;
		_max = parseInt(arguments[1]) || 0;
		_round = !arguments[2];
	}

	var random = _min + ((_max - _min) * Math.random());
	return _round ? Math.round(random) : random;
};


/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the atom.js
 */

/**
 * Provides several array utilities and extensions.
 */
var $array = new function()
{
	/**
	 * Returns an array of arguments.
	 * The first argument is the array of arguments that was passed to the calling function. The second argument is the start index
	 * at which to check for an element that is itself an array, or from which to start copying elements into the resulting list. If the
	 * <c>start</c> argument is omitted, it will default to zero. Therefore, if the Nth element in the list is itself an array, it will
	 * be that element that will be returned.
	 * @example var args = $array.fromArguments(arguments);
	 * @example var args = $array.fromArguments(arguments, 2);
	 * @param {Array} list The array of arguments from which to return a list
	 * @param {Number} [start] The index from which to start copying. Optional (Default is 0).
	 * @param {Number} [stop] The index at which to stop copying. Optional (Default is list.length).
	 * @return {Array} The array of arguments.
	 */
	this.fromArguments = function fromArguments(list, start, stop)
	{
		if (list == null || undefined)
			return [];

		if (!$type.isArray(list))
			list = [list];

		if (list.length == 1 && $type.isArray(list[0]))
			list = list[0];

		if (isNaN(parseInt(start)))
			start = 0;

		if (isNaN(parseInt(stop)))
			stop = list.length - 1;

		var result = [];
		for (var i = start; i <= stop; i++)
			result.push(list[i]);

		if (result.length == 1 && $type.isArray(result[0]))
			result = result[0];

		return result;
	};

	/**
	 * Returns all supplied arguments flattened into a single array.
	 * @returns {Array}
	 */
	this.flatten = function flatten()
	{
		var result = [];
		for (var i = 0; i < arguments.length; i++)
		{
			var current = arguments[i];
			if (!$type.isArray(current))
			{
				result.push(current);
				continue;
			}

			for (var j = 0; j < current.length; j++)
			{
				if (!$type.isArray(current[j]))
				{
					result.push(current[j]);
				}
				else
				{
					result.push.apply(result, this.flatten(current[j]));
				}
			}
		}

		return result;
	};


	/**
	 * Provides a shim for older browsers that do not implement array extras.
	 */
	var extras =
	{
		/**
		 * Checks whether the array contains the specified element.
		 * @returns {Boolean} <c>true</c> if the array contains the specified element; otherwise <c>false</c>.
		 */
		contains: function contains(element)
		{
			return this.indexOf(element) >= 0;
		},

		/**
		 * Returns the first index at which a given element can be found in the array, or -1 if it is not present.
		 * @param {Object} element Element to locate in the array.
		 * @param {Number} fromIndex The index at which to begin the search. Defaults to 0, i.e.
		 * the whole array will be searched. If the index is greater than or equal to the length of the
		 * array, -1 is returned, i.e. the array will not be searched.
		 * @returns {Number} The index of the specified <c>element</c>, or -1 if it ws not found.
		 */
		indexOf: function indexOf(element, fromIndex)
		{
			var start = isNaN(fromIndex) ? 0 : fromIndex;
			if (start < 0 || start > this.length - 1)
				return -1;

			for (var i = start; i < this.length; i++)
				if (this[i] == element)
					return i;

			return -1;
		},

		/**
		 * Returns the last index at which a given element can be found in the array, or -1 if it is not present..
		 * @param {Object} element Element to locate in the array.
		 * @param {Number} fromIndex The index at which to start searching backwards. Defaults to the array's
		 * length, i.e. the whole array will be searched. If the index is greater than or equal to the length
		 * of the array, the whole array will be searched.
		 * @returns {Number} The index of the specified <c>element</c>, or -1 if it ws not found.
		 */
		lastIndexOf: function lastIndexOf(element, fromIndex)
		{
			var end = isNaN(fromIndex) ? this.length - 1 : fromIndex;
			if (end < 0 || end > this.length - 1)
				return -1;

			for (var i = end; i >= 0; i--)
				if (this[i] == element)
					return i;

			return -1;
		},

		/**
		 * Tests whether all elements in the array pass the test implemented by the provided function.
		 * <p><c>every</c> executes the provided callback function once for each element present in the array
		 * until it finds one where callback returns a false value. If such an element is found, the every
		 * method immediately returns false. Otherwise, if callback returned a true value for all elements,
		 * every will return true. callback is invoked only for indexes of the array which have assigned values;
		 * it is not invoked for indexes which have been deleted or which have never been assigned values.</p>
		 * <c>callback</c> is invoked with three arguments: the value of the element, the index of the element,
		 * and the Array object being traversed.
		 * @param {Function} callback Function to test for each element.
		 * @param {Object} context Object to use as <c>this</c> when executing <c>callback</c>.
		 * @returns {Boolean} <c>true</c> if all elements in the array pass the test; otherwise <c>false</c>.
		 */
		every: function every(callback, context)
		{
			if (typeof callback != "function")
				throw new Error("The callback argument is not a function!");

			context = context || this;

			for (var i = 0; i < this.length; i++)
			{
				var result = callback.call(context, this[i], i, this);
				if (!result)
					return false;
			}

			return true;
		},

		/**
		 * Tests whether some element in the array passes the test implemented by the provided function.
		 * <p>some executes the callback function once for each element present in the array until it finds one where callback returns a true value. If such an element is found, some immediately returns true. Otherwise, some returns false. callback is invoked only for indexes of the array which have assigned values; it is not invoked for indexes which have been deleted or which have never been assigned values.</p>
		 * <c>callback</c> is invoked with three arguments: the value of the element, the index of the element,
		 * and the Array object being traversed.
		 * @param {Function} callback Function to test for each element.
		 * @param {Object} context Object to use as <c>this</c> when executing <c>callback</c>.
		 * @returns {Boolean} <c>true</c> if all elements in the array pass the test; otherwise <c>false</c>.
		 */
		some: function some(callback, context)
		{
			if (typeof callback != "function")
				throw new Error("The callback argument is not a function!");

			context = context || this;

			for (var i = 0; i < this.length; i++)
			{
				var result = callback.call(context, this[i], i, this);
				if (result)
					return true;
			}

			return false;
		},

		/**
		 * Creates a new array with all elements that pass the test implemented by the provided function.
		 * <p><c>filter</c> calls a provided <c>callback</c> function once for each element in an array, and constructs a new array of all
		 * the values for which <c>callback</c> returns a true value. <c>callback</c> is invoked only for indexes of the
		 * array which have assigned values; it is not invoked for indexes which have been deleted or which
		 * have never been assigned values. Array elements which do not pass the <c>callback</c> test are simply
		 * skipped, and are not included in the new array.</p>
		 * <p><c>callback</c> is invoked with three arguments: the value of the element, the index of the
		 * element, and the Array object being traversed.
		 * @param {Function} callback Function to test for each element.
		 * @param {Object} context Object to use as <c>this</c> when executing <c>callback</c>.
		 * @returns {Array} A new array, consisting of elements that passes the <c>callback</c> filtering.
		 */
		filter: function filter(callback, context)
		{
			if (typeof callback != "function")
				throw new Error("The callback argument is not a function!");

			var result = [];
			context = context || this;

			for (var i = 0; i < this.length; i++)
			{
				var pass = callback.call(context, this[i], i, this);
				if (pass)
					result.push(this[i]);
			}

			return result;
		},

		/**
		 * Executes the provided function once on each array element.
		 * <p><c>callback</c> is invoked only for indexes of the array which have assigned values;
		 * it is not invoked for indexes which have been deleted or which have never been assigned values.</p>
		 * <p><c>callback</c> is invoked with three arguments: the value of the element, the index of the
		 * element, and the Array object being traversed.
		 * @param {Function} callback Function to execute on each element.
		 * @param {Object} context Object to use as <c>this</c> when executing <c>callback</c>.
		 */
		forEach: function forEach(callback, context)
		{
			if (typeof callback != "function")
				throw new Error("The callback argument is not a function!");

			context = context || this;
			for (var i = 0; i < this.length; i++)
			{
				if (this[i] === undefined)
					continue;

				callback.call(context, this[i], i, this);
			}
		},

		/**
		 * Creates a new array with the results of calling a provided function on every element in this array.
		 * <p><c>map</c> calls the provided <c>callback</c> function once for each element in an array,
		 * in order, and constructs a new array from the results. <c>callback</c> is invoked only for indexes
		 * of the array which have assigned values; it is not invoked for indexes which have been deleted or
		 * which have never been assigned values.</p>
		 * <p><c>callback</c> is invoked with three arguments: the value of the element, the index of the
		 * element, and the Array object being traversed.
		 * @param {Function} callback Function to execute on each element.
		 * @param {Object} context Object to use as <c>this</c> when executing <c>callback</c>.
		 */
		map: function map(callback, context)
		{
			if (typeof callback != "function")
				throw new Error("The callback argument is not a function!");

			var result = [];
			context = context || this;

			for (var i = 0; i < this.length; i++)
			{
				if (this[i] === undefined)
					continue;

				result.push(callback.call(context, this[i], i, this));
			}

			return result;
		}
	};

	$type.extend(Array.prototype, extras);
};

/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the atom.js
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

/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the atom.js
 */

/**
 * Provides several useful date manipulation utilities.
 * @return {Date}
 */
var $date = new function()
{
	var MONTH_NAMES = ["January", "February", "Match", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
	var DAY_NAMES = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
	var OFFSET_KEYS = { s: "Seconds", m: "Minutes", h: "Hours", d: "Date", M: "Month", y: "FullYear" };

	var CONVERSIONS =
	{
		dayAndMonth1:
		{
			regexp: new RegExp("\\b(\\d{1,2})(-|/|\\.)(\\d{1,2})(?:\\2(\\d{2}))?\\b"),
			handler: function setDayAndMonth1(date, matches)
			{
				var x = parseInt(matches[1]);
				var y = parseInt(matches[3]);
				var z = matches[2];

				if (y > 12 || z == "/") // consider the first digit as the month
				{
					date.setDate(y);
					date.setMonth(x - 1);
				}
				else // consider the second digit as the month
				{
					date.setDate(x);
					date.setMonth(y - 1);
				}

				if (!isNaN(z))
					date.setYear(Number(String(new Date().getFullYear()).substring(0, 2) + String(z)));

				return date;
			}
		},

		dayAndMonth2:
		{
			regexp: new RegExp("(?:\\b(\\d{1,2})(?:[a-z]{2}|.|-)?\\s*)?\\b(jan|feb|mar|jun|jul|aug|sep|oct|nov|dec)(?:[a-z]+)?(?:(?:.|-)?\\s*\\b(\\d{1,2})(?:[a-z]{2}|.|-)?\\b)?", "i"),
			handler: function setDayAndMonth2(date, matches)
			{
				var name = matches[2].toLowerCase();
				var index = $.inArray(name, "jan|feb|mar|jun|jul|aug|sep|oct|nov|dec".split("|"));

				date.setMonth(index);

				var date1 = parseInt(matches[1]);
				var date2 = parseInt(matches[3]);

				if (!isNaN(date1)) date.setDate(date1);
				if (!isNaN(date2)) date.setDate(date2);

				return date;
			}
		},

		time:
		{
			regexp: new RegExp("(?:\\b|T)\\b(\\dd{2}):(\\d{2}):(\\d{2})\\b"),
			handler: function setTime(date, matches)
			{
				date.setHours(parseInt(matches[1]));
				date.setMinutes(parseInt(matches[1]));
				date.setSeconds(parseInt(matches[1]));
				return date;
			}
		},

		year:
		{
			regexp: new RegExp("\\b(\\d{4})(?:\\b|T)"),
			handler: function setYear(date, matches)
			{
				date.setYear(parseInt(matches[1]));
				return date;
			}
		},

		today:
		{
			regexp: new RegExp("^today$"),
			handler: function setToday(date)
			{
				date = new Date(date.getFullYear(), date.getMonth(), date.getDate());
				return date;
			}
		}
	};

	var dateToString = Date.prototype.toString;

	/**
	 * Creates dates based on specified arguments.
	 *
	 * The <c>expression</c> argument specifies what kind of date to return.
	 * This can be either:
	 * <ul>
	 * <li>A number - this is interpreted as date milliseonds with which the resulting date is initialized.</li>
	 * <li>An offset expression - the amount by which to offset either the current date or, if specified the <c>fromDate</c></li>
	 * <li>A date/time string - see below for valid examples</li>
	 * <li>The literal string 'today' - This will return the current date with time set to 00:00:00.</li>
	 * </ul>
	 * <p>An <i>offset expression</i> specifies whether to increase or decrease (+/-), the amount of offset (number), and the
	 * unit to offset by, for example: <code>+45m</code> or <code>-1h</code>. Valid units are
	 * any of <code>s, m, h, d, M, y</code>.</p>
	 * <p>A date/time string expression attempts to parse various string formats with maximum flexibility. Rather than
	 * specifying a limited number of valid formats, the function looks for specific patterns as parts, making it possible
	 * to combine different formats. Any of the following examples are valid:</p>
	 * <ul>
	 * <li>29-11-1943</li>
	 * <li>29.11.1943 12:44:11</li>
	 * <li>29/11/1943</li>
	 * <li>1943/29/11</li>
	 * <li>1943-nov-29 12:44</li>
	 * <li>1943/11/29T12:44</li>
	 * </ul>
	 * <p>If called without arguments, the result will simply be a new date.</p>
	 * @param {String} expression The expression that specifies the date to return. Optional
	 * @param {Date} fromDate The date to use when using an offset <c>expression</c>. Optional.
	 * @return {Date} A new date instance.
	 */
	function makedate(expression, fromDate)
	{
		var now = new Date();
		var today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
		var date = $type.isDate(fromDate)
				? new Date(fromDate)
				: expression
						? new Date(today)
						: new Date(now);

		if (expression)
		{
			if (/^\d+$/.test(expression))
			{
				date = new Date(parseInt(expression));
			}
			else if (/^([+-])(\d+|\d*\.\d*)([smhdMy])$/.test(expression))
			{
				var prefix = RegExp.$1;
				var value = parseFloat(RegExp.$2);
				var unit = RegExp.$3;

				var getter = $string.concat("get", OFFSET_KEYS[unit]);
				var setter = $string.concat("set", OFFSET_KEYS[unit]);

				var current = Date.prototype[getter].apply(date);
				var updated = prefix == "-"
					? current - value
					: current + value;

				Date.prototype[setter].call(date, updated);
			}
			else
			{
				for (var name in CONVERSIONS)
				{
					if (!CONVERSIONS.hasOwnProperty(name))
						continue;

					// additional closure introduced to escape inspection warnings.
					new function inner()
					{
						var conversion = CONVERSIONS[name];
						expression = expression.replace(conversion.regexp, function processMatch()
						{
							date = conversion.handler(date, arguments);
							return $string.EMPTY;
						});
					};
				}
			}
		}

		return date;
	}

	/**
	 * Returns the value of the current date, formatted as specified by the <c>format</c> argument.
	 *
	 * The supported formatting values are:
	 * <ol>
	 * <li>yy: The year without the century.</li>
	 * <li>yyyy: The full year.</li>
	 * <li>M: The month.</li>
	 * <li>MM: The zero-padded month.</li>
	 * <li>MMM: The three-letter abbreviation of the month name.</li>
	 * <li>MMMM: The full month name.</li>
	 * <li>d: The date.</li>
	 * <li>dd: The zero-padded date.</li>
	 * <li>h: The hours.</li>
	 * <li>hh: The zero-padded hours.</li>
	 * <li>m: The minutes.</li>
	 * <li>mm: The zero-padded minutes.</li>
	 * <li>s: The seconds.</li>
	 * <li>s: The zero-padded seconds.</li>
	 * <li>www: The three-letter abbreviation of the week day.</li>
	 * <li>wwww: The full week day.</li>
	 * </ul>
	 * The month and week days are always in English.
	 * @param {Date} date The date to format.
	 * @param {String} expression String specifying how the resulting value should be formatted.
	 * @returns {String} The value of the current date, formatted as specified by the <c>format</c> argument.
	 */
	function formatDate(date, expression)
	{
		if (!$type.isDate(date))
				return "NaN";

		if (!expression)
			return dateToString.apply(date);

		return String(expression).replace(
			/yyyy/g, date.getFullYear().toString()).replace(
			/yy/g, date.getFullYear().toString().substring(2, 4)).replace(
			/MMMM/g, MONTH_NAMES[date.getMonth()]).replace(
			/MMM/g, MONTH_NAMES[date.getMonth()].substring(0, 3)).replace(
			/MM/g, $string.padLeft(date.getMonth() + 1, 2, 0)).replace(
			/M/g, (date.getMonth() + 1).toString()).replace(
			/dd/g, $string.padLeft(date.getDate(), 2, 0)).replace(
			/d/g, date.getDate().toString()).replace(
			/hh/g, $string.padLeft(date.getHours(), 2, 0)).replace(
			/h/g, date.getHours().toString()).replace(
			/mm/g, $string.padLeft(date.getMinutes(), 2, 0)).replace(
			/m/g, date.getMinutes().toString()).replace(
			/ss/g, $string.padLeft(date.getSeconds(), 2, 0)).replace(
			/s/g, date.getSeconds().toString()).replace(
			/wwww/g, DAY_NAMES[date.getDay()]).replace(
			/www/g, DAY_NAMES[date.getDay()].substr(0, 3));
	}

	function getDate(date)
	{
		return $type.isDate(date) ? date : new Date();
	}


	// modify prototype
	Date.prototype.toString = function Date$toString(expression)
	{
		return formatDate(this, expression);
	};

	// create and return the interface
	makedate.format = formatDate;

	makedate.year = function getYear(date)
	{
		return getDate(date).getFullYear();
	};

	makedate.month = function getMonth(date)
	{
		return getDate(date).getMonth();
	};

	makedate.day = function getDate(date)
	{
		return getDate(date).getDate();
	};

	makedate.weekday = function getDay(date)
	{
		return getDate(date).getDay();
	};

	makedate.hours = function getHours(date)
	{
		return getDate(date).getHours();
	};

	makedate.minutes = function getMinutes(date)
	{
		return getDate(date).getMinutes();
	};

	makedate.seconds = function getSeconds(date)
	{
		return getDate(date).getSeconds();
	};

	makedate.time = function getTime(date)
	{
		return getDate(date).getTime();
	};

	return makedate;
};





/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the atom.js
 */

/**
 * An empty function constant.
 * This constant serves to be used wherever an empty function is needed, without needing to create a new one each time.
 * @type {Function}
 */
Function.EMPTY = new Function;

Function.NAME_EXPR =
    /^[\s\S$]*?function[\s\n]+(([\w.$]*)[\s\n]*\((.*)\))\W[\s\S]*$/;

/**
 * Gets the name of the specified function, optionally including the function arguments.
 * @param {Function} fx The function whose name to get. If omitted, the function used will be the caller of this function.
 * @param {Boolean} [includeArguments] If <c>true</c>, the resulting name will be appended the function
 * arguments in function parentheses.
 * @param {Boolean} [argumentsOnly] If <c>true</c>, the resulting value will only contain the arguments.
 * @returns {String} The name of the function.
 */
Function.getName = function Function$getName(fx, includeArguments, argumentsOnly)
{
	if (!$type.isFunction(fx))
	{
		fx = arguments.callee.caller;
		if (!$type.isFunction(fx))
			return null;
	}

	var name = String(fx).replace(Function.NAME_EXPR, function ()
	{
		var fxName = arguments[2] ? arguments[2] : "anonymous";
		var functionDefinition = arguments[2] ? arguments[1] : [fxName, "(", arguments[3], ")"].join($string.EMPTY);

		if (includeArguments && !argumentsOnly)
			return functionDefinition;

		if (includeArguments && argumentsOnly)
			return arguments[3];

		return fxName;
	});

	return name.replace(/([\w\$])\$([\w\$])/g, "$1.$2");
};

/**
 * Gets the name of function that called the function that invokes this function.
 * @returns {String} The name of the function that called the function that invokes this function.
 */
Function.callerName = function function$callerName()
{
	return Function.getName(arguments.callee.caller.caller);
};

/**
 * Gets the stacktrace starting at the specified <c>point</c>.
 * @param {Function} point The function from which to start building the stack trace.
 * @param {Number} maxIterations The maximum number of iterations to go through.
 * @returns {String} The string representing the function call stack built from the specified <c>point</c>.
 */
Function.stackTrace = function function$stackTrace(point, maxIterations)
{
	// point = arguments.callee.caller
	var fxCaller = point || arguments.callee.caller;
	var callStack = [];
	var functions = [];

	var currentCount = 0;
	while (fxCaller != null)
	{
		if (maxIterations && currentCount >= maxIterations)
			break;

		// prevent infinite loop with recursive calls
		else if (functions.contains(fxCaller))
			break;

		var fxName = Function.getName(fxCaller);
		var fxArgs = [];
		var argValues = fxCaller.arguments;

		for (var i = 0; i < argValues.length; i++)
		{
			if ($type.isFunction(argValues[i]) && $type.isFunction(argValues[i].getName))
				fxArgs.push(argValues[i].getName(true));

			else if ($type.isArray(argValues[i]))
				fxArgs.push("array[{0}]".format(argValues[i].length));

			else if ($type.isObject(argValues[i]))
				fxArgs.push("object{..}");

			else if ($type.isNull(argValues[i]))
				fxArgs.push("null");

			else if ($type.isString(argValues[i]))
				fxArgs.push(argValues[i].length < 200
					? "\"{0}\"".format(argValues[i])
					: "[string({0})]".format(argValues[i].length));

			else if (argValues[i] == undefined)
				fxArgs.push("undefined");

			else
				fxArgs.push(argValues[i]);
		}

		functions.push(fxCaller);
		callStack.unshift(Function.getName(fxCaller) + "(" + fxArgs.join(", ") + ")");

		try
		{
			fxCaller = fxCaller.caller;
		}
		catch(e)
		{
			break;
		}
		currentCount++;
	}
	callStack.toString = function (joinString)
	{
		return this.join(joinString || "\n");
	};
	return callStack;
};


/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the atom.js
 */

/**
 * Provides base type access to function prototypes.
 * @class
 */
function Prototype()
{
}

/**
 * Extends the specified <c>object</c>'s prototype with this function's prototype.
 * This function will also be copied as a static function of the specified <c>object</c>,
 * making this object capable of extending other objects in the same way.
 * @param {Object} object The Object or Function instance to extend.
 * @return {Object} The specified <c>object</c>.
 */
Prototype.extend = function prototype$extend(object)
{
	if (object == null || object == this || object.prototype == null)
		return object;

	var ancestor = this;
	var constructor = $type.isFunction(object) ? object : object.constructor;
	object.prototype.__type = { base: ancestor.prototype, constructor: constructor };

	for (var name in ancestor.prototype)
		if (!object.prototype.hasOwnProperty(name))
			object.prototype[name] = ancestor.prototype[name];

	object.extend = object.extend || Prototype.extend;
	return object;
};

/**
 * Calls the base constructor.
 * @arguments {Object[]} 0-n Any arguments that should be passed to the base constructor.
 */
Prototype.prototype.construct = function prototype$construct()
{
	if (this.__type == null)
		return;

	this.$this = this.$this || this;
	var constructor = this.$this.__type.base.constructor;
	this.$this = this.$this.__type.base;

	constructor.apply(this, $array.fromArguments(arguments));
	delete this.$this;
};

/**
 * Calls the base method with the specified name.
 * @param {String} name The name of the base method to call.
 * @arguments {Object[]} 1-n The arguments that should be passed to the base method.
 * @returns {Object} The result of invoking the base method.
 */
Prototype.prototype.base = function prototype$base(name)
{
	if (this.__type == null)
		return null;

	var fx = null;
	var current = this;
	var caller = arguments.callee.caller;

	while (current.__type && !fx)
	{
		if (current.__type.base[name] != caller)
			fx = current.__type.base[name];

		current = current.__type.base;
	}

	if (typeof(fx) == "function")
		return fx.apply(this, Array.prototype.splice(arguments, 1));

	return null;
};


/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the atom.js
 */

/**
 * Dispatches (custom) events to event listeners.
 * All arguments to the constructor will be interpreted as names of events to register.
 * @constructor
 */
var Dispatcher = Prototype.extend(function Dispatcher()
{
	this.__events = [];
	this.__listeners = {};

	for (var i = 0; i < arguments.length; i++)
		this.registerEvent(arguments[i]);

});

/**
 * Registers an event with the specified name.
 * @arguments {String} [0-n] The names of the events to register.
 */
Dispatcher.prototype.registerEvent = function()
{
	for (var i = 0; i < arguments.length; i++)
	{
		var eventName = arguments[i];
		if (!this.hasEvent(eventName))
		{
			this.__events.push(eventName);
			this.__listeners[eventName] = [];
		}
	}
};

/**
 * Returns true if the specified event already exists.
 * @param {String} eventName The name of the event.
 * @return {Boolean} True if the specified event type exists.
 */
Dispatcher.prototype.hasEvent = function(eventName)
{
	return this.__events.contains(eventName);
};

/**
 * Returns true if the supplied eventType event listener exists.
 * @param {String} eventName The name of the event.
 * @param {Function} eventListener The event listener.
 * @return {Boolean} True if the specified listener exists.
 */
Dispatcher.prototype.hasListener = function(eventName, eventListener)
{
	if (this.hasEvent(eventName))
	{
		for (var i = 0; i < this.__listeners[eventName].length; i++)
		{
			if (this.__listeners[eventName][i] == eventListener)
				return true;
		}
	}
	return false;
};

/**
 * Adds an event listener for the specified event $type.
 * @param {String} eventName The name of the event.
 * @param {Function} eventListener The event listener.
 * @returns {Dispatcher} The current object.
 */
Dispatcher.prototype.addListener = function(eventName, eventListener)
{
	if (this.hasEvent(eventName))
	{
		if (!this.hasListener(eventName, eventListener))
			this.__listeners[eventName].push(eventListener);
	}
	else
	{
		$log.error("Did not add event '{0}' because the current object only supports '{1}'",
			eventName, this.__events.join(", "));
	}

	return this;
};

Dispatcher.prototype.on = Dispatcher.prototype.addListener;

/**
 * Removes the specified event listener.
 * @param {String} eventName The name of the event.
 * @param {Function} eventListener The event listener.
 * @returns {Dispatcher} The current object (this).
 */
Dispatcher.prototype.removeListener = function(eventName, eventListener)
{
	if (this.hasEvent(eventName))
	{
		if ($type.isFunction(eventListener))
		{
			for (var i = 0; i < this.__listeners[eventName].length; i++)
			{
				if (this.__listeners[eventName][i] == eventListener)
				{
					this.__listeners[eventName].splice(i, 1);
					return this;
				}
			}
		}
		else
			this.__listeners[eventName] = [];
	}

	return this;
};

Dispatcher.prototype.off = Dispatcher.prototype.removeListener;

/**
 * Fires the specified event.
 * @param {Object} event The event or the name of the event to fire.
 * @param {Object} eventData The event data.
 * @returns {Event} The event that was fired.
 */
Dispatcher.prototype.fireEvent = function(event, eventData)
{
	var eventObj = null;
	var eventType = null;

	if ($type.instanceOf(event, $evt.Event))
	{
		eventType = event.type;
		eventObj = event;
	}
	else
	{
		if (eventData == null)
			eventData = this.createEventData();

		eventType = event;
		eventObj = new $evt.Event(this, eventType, eventData);
	}

	var listeners = this.__listeners[eventType];
	if (listeners != null && listeners.length != 0)
	{
		for (var i = 0; i < listeners.length; i++)
		{
			if ($type.isFunction(listeners[i]))
			{
				listeners[i](eventObj);
				if (eventObj.cancel)
					break;
			}
		}

		return eventObj;
	}

	return null;
};

Dispatcher.prototype.fire = Dispatcher.prototype.fireEvent;

/**
 * Create an object with information about the raised event.
 *
 * This is the default method, it returns an empty object. Override this method in child
 * dispatcher to add actual information.
 * @param {String} eventName The name of the event that occurred.
 * @return {Object} Object with information about the raised event.
 */
Dispatcher.prototype.createEventData = function(eventName)
{
	return {};
};

Dispatcher.prototype.toString = function()
{
	var output = [];
	for (var eventType in this.__listeners)
	{
		if (!this.__listeners.hasOwnProperty(eventType))
			continue;

		output.push("{0}: {1} listeners".format(eventType, this.__listeners[eventType].length));
	}
	return "Dispatcher: {{0}}".format(output.join(", "));
};


/**
 * Implements a class that helps with creating variable number of argument objects in a structured way.
 * <p>When a function accepts a large number of arguments, where each can have a default value and can also be set
 * by the function itself, using, chaning and maintaining this function and its usages can be daunting. This objects
 * helps do it in a structured way.</p>
 * <p>By using the class as the container for arguments, the function can than have only one formal argument, because
 * all parameters it operates on are contained within an instance of this class.</p>
 * @example
 * atom.controls.SliderSettings = function SliderSettings(data, override)
 * {
 *	this.orientation = this.getString("orientation", data, override, "h");
 * 	this.roundValues = this.getBoolean("roundValues", data, override, true);
 * 	this.minValue = this.getNumber("minValue", data, override, 0);
 * 	this.maxValue = this.getNumber("maxValue", data, override, 100);
 * 	this.value = this.getNumber("value", data, override, 0);
 * 	this.defaultValue = this.getNumber("defaultValue", data, override, 0);
 * 	this.formControl = this.getString("formControl", data, override, null);
 * 	this.textControl = this.getString("textControl", data, override, null);
 * };
 *
 * var elem = document.getElementById("slider1");
 * var settings = new atom.controls.SliderSettings({ minValue: 20, maxValue: 50 });
 * var slider = new atom.controls.Slider(elem, settings);
 * @class
 */
var Settings = Prototype.extend(function Settings()
{
});

/**
 * Looks up a value with the specified name from one of the specified sources, and returns it.
 * When retrieving the value the lookup goes through the following steps:
 * <ol><li>The <c>data</c> object, if it contains a property with the specified name</li>
 * <li>The <c>override</c> object, if it is an HTML element
 * <ol><li>if it has an attribute with the specified name</li>
 * <li>if it has an attribute with the specified name and an 'data-' prefix</li></ol>
 * <li>The <c>override</c> object, if it's an object
 * <ol><li>if it has a property with the specified name</li></ol></li>
 * <li>The default value, if any</li></ol>
 * @param {String} propName The name of the value to retrieve.
 * @param {Object} data The object or element with properties in which to look for property with <c>propName</c>.
 * @param {Object} [override] An object or element properties (or attributes) in which to look for property (or attribute)
 * with <c>propName</c>.
 * @param {Object} [defaultValue] The value to return if property or attribute with <c>propName</c> wasn't found in either
 * <c>data</c> or <c>override</c> arguments.
 * @return {Object} A value as discovered in the supplied arguments <c>data</c> or <c>override</c>, or the
 * <c>defaultValue</c> if the value has not been discovered.
 */
Settings.prototype.getValue = function Settings$getValue(propName, data, override, defaultValue)
{
	data = data && data.jquery ? data[0] : data;
	override = override && override.jquery ? override[0] : override;

	if ($type.isHtmlElement(override))
	{
		if (override.getAttribute("data-" + propName))
			return override.getAttribute("data-" + propName);

		else if (override.getAttribute(propName))
			return override.getAttribute(propName);
	}
	else if (override && override[propName] != undefined)
	{
		return override[propName];
	}
	else if ($type.isHtmlElement(data))
	{
		if (data.getAttribute("data-" + propName))
			return data.getAttribute("data-" + propName);

		else if (data.getAttribute(propName))
			return data.getAttribute(propName);
	}
	else if (data && data[propName] != undefined)
	{
		return data[propName];
	}

	return defaultValue;
};

/**
 * Looks up a value with the specified name from one of the specified sources, and returns a <c>Boolean</c>.
 * @param {String} propName The name of the value to retrieve.
 * @param {Object} data The object with properties in which to look for property with <c>propName</c>.
 * @param {Object} [override] An element with attributes or object with properties in which to look for attribute or
 * property with <c>propName</c>.
 * @param {Object} [defaultValue] The value to return if property or attribute with <c>propName</c> wasn't found in either
 * <c>data</c> or <c>override</c> arguments.
 * @return {Boolean} A value as discovered in the supplied arguments <c>data</c> or <c>override</c>, or the
 * <c>defaultValue</c> if the value has not been discovered.
 * @see getValue
 */
Settings.prototype.getBoolean = function Settings$getBoolean(propName, data, override, defaultValue)
{
	var value = String(this.getValue(propName, data, override, defaultValue)).toLowerCase();
	return value == "1" || value == "true" || value == "yes";
};

/**
 * Looks up a value with the specified name from one of the specified sources, and returns a <c>Number</c>.
 * @param {String} propName The name of the value to retrieve.
 * @param {Object} data The object with properties in which to look for property with <c>propName</c>.
 * @param {Object} [override] An element with attributes or object with properties in which to look for attribute or
 * property with <c>propName</c>.
 * @param {Object} [defaultValue] The value to return if property or attribute with <c>propName</c> wasn't found in either
 * <c>data</c> or <c>override</c> arguments.
 * @return {Number} A value as discovered in the supplied arguments <c>data</c> or <c>override</c>, or the
 * <c>defaultValue</c> if the value has not been discovered.
 * @see getValue
 */
Settings.prototype.getNumber = function Settings$getNumber(propName, data, override, defaultValue)
{
	var value = this.getValue(propName, data, override, defaultValue);
	if (!$type.isNumeric(value))
		return 0;

	return parseInt(value);
};

/**
 * Looks up a value with the specified name from one of the specified sources, and returns a <c>String</c>.
 * @param {String} propName The name of the value to retrieve.
 * @param {Object} data The object with properties in which to look for property with <c>propName</c>.
 * @param {Object} [override] An element with attributes or object with properties in which to look for attribute or
 * property with <c>propName</c>.
 * @param {Object} [defaultValue] The value to return if property or attribute with <c>propName</c> wasn't found in either
 * <c>data</c> or <c>override</c> arguments.
 * @return {String} A value as discovered in the supplied arguments <c>data</c> or <c>override</c>, or the
 * <c>defaultValue</c> if the value has not been discovered.
 * @see getValue
 */
Settings.prototype.getString = function Settings$getString(propName, data, override, defaultValue)
{
	return String(this.getValue(propName, data, override, defaultValue));
};

/**
 * Looks up a value with the specified name from one of the specified sources, and returns a <c>Function</c>.
 * @param {String} propName The name of the value to retrieve.
 * @param {Object} data The object with properties in which to look for property with <c>propName</c>.
 * @param {Object} [override] An element with attributes or object with properties in which to look for attribute or
 * property with <c>propName</c>.
 * @param {Object} [defaultValue] The value to return if property or attribute with <c>propName</c> wasn't found in either
 * <c>data</c> or <c>override</c> arguments.
 * @return {Function} A value as discovered in the supplied arguments <c>data</c> or <c>override</c>, or the
 * <c>defaultValue</c> if the value has not been discovered.
 * @see getValue
 */
Settings.prototype.getFunction = function Settings$getFunction(propName, data, override, defaultValue)
{
	var result = this.getValue(propName, data, override, defaultValue);
	if ($type.isFunction(result))
		return result;

	return null;
};



/**
 * Provides information about a registered control.
 * @param {Function} type The function that implements the control.
 */
var ControlTypeInfo = function ControlTypeInfo(type)
{
	//// Initialize the control
	type.create = type.create || createInstance;
	type.createElement = type.createElement || createElement;
	type.get = type.get || getInstance;
	type.register = type.register || registerConstructor;
	type.dispose = type.dispose || Function.EMPTY;
	type.instances = [];
	type.registeredConstructors = {};
	type.registerInstance = registerInstance;

	var info = this;
	info.type = type;
	info.defaultConstructor = type.Control || type;
	info.typeName = Function.getName(type);
	info.expression = type.expression;
	info.async = type.async === true;
	info.getInstance = getInstance;
	info.createInstance = createInstance;

	/**
	 * Checks whether the specified function is either the default constructor or one of the registered constructors of this type.
	 * @param {Function} typeFunction The function to check.
	 * @returns {Boolean} <c>true</c> if the specified function is either the default constructor or one of the
	 * registered constructors of this type; otherwise <c>false</c>.
	 */
	this.checkType = function (typeFunction)
	{
		if (!$type.isFunction(typeFunction))
			return false;

		if (info.defaultConstructor == typeFunction)
			return true;

		for (var expression in type.registeredConstructors)
		{
			if (!type.registeredConstructors.hasOwnProperty(expression))
				continue;

			if (type.registeredConstructors[expression] == typeFunction)
				return true;
		}

		return false;
	};

	/**
	 * Calls static redraw method of the registered control, if the control implements it.
	 */
	this.redraw = function ()
	{
		if ($type.isFunction(type.redraw))
		{
			type.redraw();
		}
	};

	/**
	 * Sets up the control associated with this instance.
	 * @param {Function} onTypeInitialized The callback function for asynchronous initialization.
	 */
	this.setup = function (onTypeInitialized)
	{
		if (type.setup && type.async)
		{
			type.setup(onTypeInitialized);
		}
		else if (type.setup)
		{
			type.setup();
			onTypeInitialized();
		}
		else
		{
			onTypeInitialized();
		}
	};

	/**
	 * Call the <code>dispose</code> method of the associated control.
	 */
	this.dispose = function()
	{
		if (type.dispose)
		{
			type.dispose();
		}
	};

	/**
	 * Creates a new HTML element for use with the registered control.
	 * @returns {HTMLElement} A new HTML element for use with the registered control.
	 */
	function createElement()
	{
		var result = document.createElement("div");
		result.setAttribute("id", atom.dom.uniqueID(result));
		if (String(type.expression).trim().replace(/\./g, " ").match(/([\w\- ]+)/))
			result.setAttribute("className", RegExp.$1);

		return result;
	}

	/**
	 * Creates a new control.
	 * @example Create a new button
	 * Button.create($("button.big"));
	 * @example Create a new tab control
	 * TabControl.create(myElement);
	 * @param {HTMLElement} element The element that represents the control to create. Optional.
	 * @param {Object} settings The object that holds the settings for the control to create. Optional.
	 * @return {Object} The control that was created.
	 */
	function createInstance(element, settings)
	{
		var depth = arguments[2] || 0;
		var prefix = $string.repeat("  ", depth);

		if (element == null)
		{
			if ($type.isFunction(this.createElement))
			{
				element = this.createElement(settings);
			}
			else
			{
				throw Error($string.format(
					"No element has been provided and the control '{0}' doesn't have 'createElement' method, " +
					"therefore a new element can't be created", Function.getName(this)));
			}
		}

		var control = getInstance(element);
		if (control == null)
		{
			var ctor = getConstructor(element);
			control = new ctor(element, settings);
			if ($type.isFunction(control.init))
				control.init();

			registerInstance(control);
			$log.debug("{0}Created control {1}", prefix, control);
		}
		else
		{
			$log.warn("{0}Not creating another instance of {1} for {2}.", prefix, control, element);
		}

		return control;
	}

	function registerInstance(control)
	{
		if (!$type.instanceOf(control, HtmlControl))
			return;

		control.$element.attr("id", $dom.uniqueID(control.$element));

		if (!type.instances.contains(control))
			type.instances.push(control);
	}

	/**
	 * Gets the contructor function registered for the specified <c>element</c>.
	 * @param {HTMLElement|jQuery} element Either the HTML element of jQuery selection of it.
	 * @returns {Function} The function registered and the control constructor for the specified <c>element</c>.
	 */
	function getConstructor(element)
	{
		var constructor = info.defaultConstructor;
		var $element = $(element);

		for (var expression in type.registeredConstructors)
		{
			if (!type.registeredConstructors.hasOwnProperty(expression))
				continue;

			if ($element.is(expression))
			{
				constructor = type.registeredConstructors[expression];
				break;
			}
		}

		return constructor;
	}

	/**
	 * Gets the control instance associated with the specified <c>element</c>.
	 * @param {String|Element} element Either the HTML element or ID of the element for
	 * which to get the control instance.
	 * @returns {HtmlControl} The control instance associated with the specified <c>element</c>.
	 */
	function getInstance(element)
	{
		if (element == null)
			return null;

		if (element.jquery)
			element = element[0];

		if ($type.isString(element))
			element = $(element)[0];

		if (element == null)
			return null;

		var checkString = $type.isString(element);
		var checkElement = $type.isElement(element);

		for (var i = 0; i < type.instances.length; i++)
		{
			var instance = type.instances[i];
			if (checkElement && instance.element()[0] == element)
				return instance;

			if (checkString && instance.id() == element)
				return instance;
		}

		return null;
	}

	/**
	 * Registers a constructor to use (instead of the default constructor) when
	 * creating control instances for elements that match the specified css expression.
	 *
	 * @example
	 * Button.registerConstructor(MyConstructor, "#mybutton");
	 * @example
	 * Button.registerConstructor(RedButton, "div.buttons .button.red");
	 * @param {String} expression The css expression that specifies for which elements
	 * this constructor applies.
	 * @param {Function} constructor The constructor function to use.
	 */
	function registerConstructor(expression, constructor)
	{
		if (!$type.isString(expression) && !$type.isFunction(constructor))
			return $log.warn("Arguments expression:String and constructor:Function both need to be used");

		type.registeredConstructors[expression] = constructor;
		return null;
	}
};

/**
 * Implements a global control registry.
 *
 * The control registry provides a centralized and streamlined way for controls to be discovered, initialized
 * and made available to the whole window.
 *
 * When a control is registered, it provides information about how it needs to be initialized; if the control need to
 * load resources asynchronously, it will wait on it to complete before signaling readiness.
 *
 * A registered control will be appended several methods that provide it with functionality to create get and create
 * instances, as well as to register custom constructors for specific elements.
 */
var ControlRegistry = function ControlRegistry()
{
	var types = [];
	var expressions = [];

	var depth = -1;

	/**
	 * Calls static redraw method on all control types that implement it.
	 *
	 * This provides a single point for redraw synchronization when arbitrary code element change the screen layout in
	 * ways that require controls to be redrawn.
	 */
	this.redraw = function ControlRegistry$redraw()
	{
		for (var i = 0; i < types.length; i++)
		{
			types[i].redraw();
		}
	};

	/**
	 * Registers a control.
	 * @param {Object} control An object that describes the control.
	 */
	this.register = function ControlRegistry$register(control)
	{
		$log.assert($type.isObject(control), "Argument 'control' should be an object that describes the control or the control itself");

		var typeInfo = new ControlTypeInfo(control);
		types.push(typeInfo);
		expressions.push(typeInfo.expression);

		$log.info("Registered control {0}", $type.isFunction(control) ?
			Function.getName(control) :
			Function.getName(control.constructor));

		return control;
	};

	/**
	 * Searches through the registered controls and find an instance that matches the specification.
	 * @param {Object} element Either the id of an HTML element or the HTML element itself.
	 * @param {Object} type Either the class name associated with a control type, the name of the type or
	 * the control type itself.
	 * @returns {Control} The control associated with the specified element, and optionally $type.
	 */
	this.get = function ControlRegistry$get(element, type)
	{
		if (element == null)
			return null;

		if (element.jquery)
			element = element[0];

		if ($type.isString(element))
			element = $(element)[0];

		if (element == null)
			return null;

		var result = null;
		for (var i = 0; i < types.length; i++)
		{
			var typeInfo = types[i];
			if (type != null)
			{
				if (typeInfo.checkType(type) || typeInfo.typeName == type || typeInfo.expression.indexOf(type) != -1)
				{
					result = typeInfo.getInstance(element);
					break;
				}
			}
			else
			{
				// if no type has been specified, return the first matching control.
				result = typeInfo.getInstance(element);
				if (result != null)
				{
					break;
				}
			}
		}

		return result;
	};

	/**
	 * Sets up all registered controls.
	 * @param {Function} onSetupReady The function to call when the setup completes.
	 */
	this.setup = function ControlRegistry$setup(onSetupReady)
	{
		if (types.length == 0)
		{
			if ($type.isFunction(onSetupReady))
				onSetupReady();

			return;
		}

		var self = this;
		var typesReady = 0;
		for (var i = 0; i < types.length; i++)
		{
			var typeInfo = types[i];
			typeInfo.setup(function onTypeInitialized()
			{
				if (++typesReady == types.length)
				{
					self.update();
					if ($type.isFunction(onSetupReady))
						onSetupReady();
				}
			});
		}
	};

	/**
	 * Call <code>dispose</code> method on all registered controls.
	 */
	this.dispose = function ControlRegistry$dispose()
	{
		for (var i = 0; i < types.length; i++)
			types[i].dispose();
	};

	/**
	 * Discovers and initializes registered controls within the specified <c>parent</c>.
	 *
	 * @param {Object} parent The parent element in which to discover and update controls. If omitted it defaults to
	 * whole document.
	 */
	this.update = function ControlRegistry$update(parent)
	{
		depth++;

		if (depth < 10)
		{
			var elements = $(expressions.join(", "), parent || document);

			if (elements.length > 0)
			{
				if (depth == 0)
					$log.groupCollapsed("Creating element instances for registered controls");

				for (var i = 0; i < types.length; i++)
				{
					var typeInfo = types[i];
					var matches = elements.filter(typeInfo.expression);

					for (var j = 0; j < matches.length; j++)
					{
						typeInfo.createInstance(matches[j], null, depth);
					}
				}

				if (depth == 0)
					$log.groupEnd();
			}
		}
		else
		{
			atom.log.warn("An attempt was made to recurse into ControlRegistry.update deeper than 10 levels. Check the code for infinite loops");
		}

		depth--;
	};
};

/**
 * Provides a base class for HTML controls.
 * @param {HTMLElement} element The HTML element that this control wraps.
 * @arguments {String} [1-n] Any events that this control dispatches.
 */
var HtmlControl = Dispatcher.extend(function HtmlControl(element)
{
	this.construct($array.fromArguments(arguments, 1));

	/**
	 * @type {jQuery}
	 */
	this.$element = $(element);
});

/**
 * Gets or sets the id of the element that this control uses.
 * @param {String} [id] The new id top to set on the element.
 * @returns {String} The current id of the element.
 */
HtmlControl.prototype.id = function HtmlControl$id(id)
{
	if ($type.isString(id))
		this.$element.attr("id", id);

	return this.$element.attr("id");
};

/**
 * Gets the HTML element that this control uses.
 * @returns {jQuery} The element that this control uses.
 */
HtmlControl.prototype.element = function HtmlControl$element()
{
	return this.$element;
};

/**
 * Gets a string that represents this element.
 * @returns {String} A string that represents this element.
 */
HtmlControl.prototype.toString = function HtmlControl$toString()
{
	var name = Function.getName(this.constructor);
	if (this.$element.length == 0)
		return $string.format("{0}(null)", name);

	var attrId = this.id();
	var attrClass = this.$element.attr("class");
	var tagName = String(this.$element.prop("tagName")).toLowerCase();
	return $string.format("{0}(\"{1}{2}{3}\")", name, tagName,
		attrClass ? "." + attrClass.replace(/\s+/, ".") : $string.EMPTY,
		attrId ? "#" + attrId : $string.EMPTY);
};

	/*include: controls/PageControl.js  */

/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the atom.js
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

	css.reset = function css$reset()
	{
		parsedCss = null;
	};

	css.parseDocument = function css$parseDocument(doc)
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

	css.parseStylesheet = function css$parseStylesheet(stylesheet)
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

	css.computed = function css$computed(element, prop)
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

	css.findRules = function css$findRules(expression)
	{
		var rules = css();
		var result = [];
		if (expression)
		{
			var re = new RegExp(expression);
			for (var selectorText in rules)
			{
				if (!rules.hasOwnProperty(selectorText))
					continue;

				if (selectorText.match(re))
					result.push({ selector: selectorText, css: rules[selectorText] });
			}
		}

		return result;
	};

	css.getRule = function css$getRule(selectorText)
	{
		return parsedCss[selectorText];
	};

	css.getStyle = function css$getStyle(selectorText)
	{
		var rule = css.getRule(selectorText);
		return rule ? rule.style.cssText : $string.EMPTY;
	};

	css.getProperty = function css$getProperty(selectorText, property)
	{
		var rule = css.getRule(selectorText);
		return rule ? rule.style[property] : $string.EMPTY;
	};

	css.copyStyles = function css$copyStyles(from, to, group)
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

				atom.log("{0}: {1}", prop, computed[prop]);
			}
		}
	};

	return css;

};

/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the atom.js
 */

/**
 * Provides several dom-related utilities.
 * @type {Object}
 */
var $dom = new function dom()
{
	var generatedIds = {};
	var hideSpecs = [];
	var listeningToChange = false;

	var dom = new Dispatcher("selectionchange");

	dom.registerAutoHide = function dom$registerAudoHide(specs)
	{
		if (!specs)
			return;

		var s = { element: null, delay: 0, hide: autoHideElement };
		if (specs.jquery)
			s.element = $(specs);
		else
		{
			s.element = specs.element;
			if ($type.isNumber(specs.delay))
				s.delay = specs.delay;
			if ($type.isFunction(specs.hide))
				s.hide = specs.hide;
		}

		hideSpecs.push(s);
		if (!listeningToChange)
		{
			$(document).bind("mousedown mouseup click keydown keyup", onDocumentEvent);
			listeningToChange = true;
		}
	};

	dom.isChrome = window.navigator.userAgent.indexOf("Chrome") != -1;

	dom.isTouchDevice =
		!!('ontouchstart' in window) ||
		!!('onmsgesturechange' in window);

	dom.uniqueID = function dom$uniqueID(element)
	{
		if (element && element.jquery)
			element = element[0];

		if (element == null)
			return null;

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

	dom.unselectable = function dom$unselectable(off)
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
	};

	$.fn.unselectable = function unselectable(value)
	{
		return this.each(function unselectable()
		{
			dom.selectable(value);
    		});
	};

	function autoHideElement(element)
	{
		$(element).hide();
	}

	function onDocumentEvent(e)
	{
		for (var i = 0; i < hideSpecs.length; i++)
		{
			for (var j = 0; j < hideSpecs[i].element.length; j++)
			{
				var $el = hideSpecs[i].element.eq(j);
				if (!$el.is(":visible"))
					continue;

				var delay = hideSpecs[i].delay;
				var hideFx = $.proxy(hideSpecs[i].hide, this, $el);

				var isMenuRelated =
					$el.is(e.target) || $el.has(e.target).length != 0;

				if (!isMenuRelated)
				{
					if (delay)
						hideSpecs[i].delayId = setTimeout(hideFx, delay);
					else
						hideFx();
				}
				else if (delay)
				{
					clearTimeout(hideSpecs[i].delayId);
				}
			}
		}
	}

	return dom;
};


/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the atom.js
 */

/**
 * Provides a proxy object that can be used across different browsers to log to console.
 * @type {Function}
 */
var $log = new function()
{
	var timers = {};
	var listeners = [];
	var standardMethods = ["log", "debug", "info", "warn", "error", "group", "groupCollapsed", "groupEnd"];
	var customMethods = [];

	/**
	 * Logs the supplied <c>message</c> to console.
	 * The severity argument is read from the last supplied argument, and will be ignored unless
	 * it's one of the allowed values. Any arguments between message and severity are considered
	 * formatting variables.
	 * @param {String} message The message to log.
	 * @param {String} [severity] The severity of the message. One of <c>debug, info, warn, error</c>.
	 * Default is <c>debug</c>.
	 * @arguments {Object} [1-n] The variables to insert into the format string.
	 */
	function log(message, severity)
	{
		var method = "log";

		var count = arguments.length;
		var first = 1;
		var last = count - 1;

		var testName = String(arguments[last]);
		if (standardMethods.contains(testName) || customMethods.contains(testName))
		{
			method = testName;
			last = count - 2;
		}

		if (method != "dir")
		{
			if (last >= first)
			{
				var formatArgs = $array.fromArguments(arguments, first, last);
				message = $string.format(message, formatArgs);
			}
		}

		for (var i = 0; i < listeners.length; i++)
		{
			var listener = listeners[i];
			if ($type.isFunction(listener[method]))
				listener[method](message);
		}
	}

	log.trace = function Log$trace(message)
	{
		var stackTrace = Function.stackTrace(arguments.callee.caller);
		if (arguments.length > 0)
		{
			var args = $array.fromArguments(arguments, 1);
			log.group(message, args);
			for (var i = 0; i < stackTrace.length; i++)
				log.debug(stackTrace[i]);

			log.groupEnd();
		}
		else
			log(stackTrace.toString());
	};

	log.assert = function Log$assert(condition, message)
	{
		var args = $array.fromArguments(arguments, 2);
		if (condition === false)
			log(message, args, "error");
	};

	log.time = function Log$time(timerName)
	{
		if (console.time)
		{
			console.time.apply(console, arguments);
			return;
		}

		if (timerName)
			timers[timerName] = new Date().getTime();
	};

	log.timeEnd = function Log$timeEnd(timerName)
	{
		if (console.timeEnd)
		{
			console.timeEnd.apply(console, arguments);
			return;
		}

		if (timers[timerName])
		{
			log.info("{0}: {1}.ms", timerName, new Date().getTime() - timers[timerName]);
		}
	};

	log.clear = function Log$clear()
	{
		console.clear();
	};

	log.log = function Log$debug(value)
	{
		log(value, $array.fromArguments(arguments, 1), "log");
	};

	log.debug = function Log$debug(value)
	{
		log(value, $array.fromArguments(arguments, 1), "debug");
	};

	log.groupCollapsed = function Log$groupCollapsed(value)
	{
		log(value, $array.fromArguments(arguments, 1), "groupCollapsed");
	};

	log.group = function Log$group(value)
	{
		log(value, $array.fromArguments(arguments, 1), "group");
	};

	log.groupEnd = function Log$groupEnd()
	{
		log($string.EMPTY, "groupEnd");
	};

	log.info = function Log$info(value)
	{
		log(value, $array.fromArguments(arguments, 1), "info");
	};

	log.warn = function Log$warn(value)
	{
		log(value, $array.fromArguments(arguments, 1), "warn");
	};

	log.error = function Log$error(value)
	{
		log(value, $array.fromArguments(arguments, 1), "error");
	};

	log.dir = function Log$dir(value)
	{
		log(value, $array.fromArguments(arguments, 1), "dir");
	};

	log.register = function register(listener, skipMethods)
	{
		if (listener != null && !listeners.contains(listener))
			listeners.push(listener);

		if (skipMethods !== true)
		{
			for (var name in listener)
			{
				if (!listener.hasOwnProperty(name))
					continue;

				if ($type.isFunction(listener[name]))
				{
					if (!customMethods.contains(name))
						customMethods.push(name);
				}
			}
		}
	};

	log.unregister = function unregister(listener)
	{
		var index = listeners.indexOf(listener);
		if (listener != null && index != -1)
		{
			listeners.splice(index, 1);
			for (var i = customMethods.length - 1; i >= 0; i--)
			{
				var name = customMethods[i];
				var remove = true;
				for (var j = 0; j < listeners.length; j++)
				{
					if ($type.isFunction(listeners[j][name]))
					{
						remove = false;
						break;
					}
				}

				if (remove)
					customMethods.splice(i, 1);
			}
		}
	};

	log.register(window.console, true);

	return log;
};

/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the atom.js
 */

/**
 * Provides a proxy object that can be used across different browsers to log to console.
 * @type {Object}
 */
var $url = new function url()
{
	var url = function makeUrl(value)
	{
		var result = new Url();
		result.parse($type.isString(value) ? value : location.href);
		return result;
	};

	/**
	 * Defines the regular expression that matches different parts of a URL.
	 * The different parts of the URL that this expression matches are:
	 * <ul>
	 * <li>protocol</li>
	 * <li>authority<ul>
	 *    <li>userInfo<ul>
	 *       <li>user</li>
	 *       <li>password</li>
	 *    </ul></li>
	 *    <li>host</li>
	 *    <li>port</li></ul></li>
	 * <li>relative<ul>
	 *    <li>path<ul>
	 *        <li>directory</li>
	 *        <li>file</li></ul></li>
	 *     <li>query</li>
	 *     <li>anchor (fragment)</li>
	 * </ul></li></ul>
	 * @type {RegExp}
	 */
	url.URL_EXPRESSION =
		/^(?:(?![^:@]+:[^:@\/]*@)([^:\/?#.]+):)?(?:\/\/)?((?:(([^:@]*)(?::([^:@]*))?)?@)?([^:\/?#]*)(?::(\d*))?)(((\/(?:[^?#](?![^?#\/]*\.[^?#\/.]+(?:[?#]|$)))*\/?)?([^?#\/]*))(?:\?([^#]*))?(?:#(.*))?)/;

	/**
	 * The url component names corresponding to the sub-match indexes of the regular expression <c>URL_EXPRESSION</c>.
	 * @type {Array.<String>}
	 */
	url.COMPONENT_NAMES =
		["source", "protocol", "authority", "userInfo", "user", "password", "host", "port", "relative", "path", "directory", "file", "query", "hash"];

	/**
	 * Specifies the protocol separator string ("://").
	 * @type {String}
	 */
	url.PROTOCOL_SEPARATOR = "://";

	/**
	 * Specifies the string to use to indicate an empty hash string and to interpret a hash string as empty.
	 * When manipulating hash parameters of the browser's location object by using <c>Url.setHash</c>, <c>Url.setHashParam</c>
	 * and <c>Url.removeHashParam</c> methods, and the resulting hash is an empty string, the browser's location will contain
	 * a hash character ("#") without any value following. Setting the location hash to '#' will cause the browser to
	 * scroll to the top of the page, which is most of the time not the desired effect. To prevent that, this class uses
	 * the string specified with this constant to indicate an empty string. In other words, if the hash contains just this
	 * character it is considered empty, and similarly, if the resulting hash to set is an empty string, the value that
	 * will be set will be this character.
	 * @type {String}
	 * @constant
	 */
	url.EMPTY_HASH = "!";

	/**
	 * The default character (&) to use as the name/value pair separator within url query strings and hashes.
	 * @type {string}
	 */
	url.DEFAULT_SEPARATOR = "&";

	/**
	 * The default character (=) to use as the names/value separator within url query strings and hashes.
	 * @type {string}
	 */
	url.DEFAULT_EQUALS = "=";

	/**
	 * Provides a wrapper around a url parsed from a string, and a set of static methods for working with the
	 * browser's location object.
	 * @param {String} [value] The url string to use to initialize a new instance with. If omitted, the url of the
	 * browser's current location is used.
	 * @param {Object} [options] The object that specifies the options of this instance.
	 * <dl>
	 *   <dt>separator</dt>
	 *     <dd>the character to use as the name/value pair separator within url query strings and hashes</dd>
	 *   <dt>equals</dt>
	 *     <dd>the character to use as the names/value separator within url query strings and hashes.</dd>
	 * </dl>
	 */
	function Url(value, options)
	{
		options = options || {};

		this.separator = options.separator || url.DEFAULT_SEPARATOR;
		this.equals = options.equals || url.DEFAULT_EQUALS;

		this.init();

		if (value)
			this.parse(value);
	}

	/**
	 * Initializes all url components to blank values.
	 * @return {Url} The current instance.
	 */
	Url.prototype.init = function ()
	{
		this.components = {
			source: $string.EMPTY,
			protocol: $string.EMPTY,
			authority: $string.EMPTY,
			userInfo: $string.EMPTY,
			user: $string.EMPTY,
			password: $string.EMPTY,
			host: $string.EMPTY,
			port: $string.EMPTY,
			relative: $string.EMPTY,
			path: $string.EMPTY,
			directory: $string.EMPTY,
			file: $string.EMPTY,
			query: $string.EMPTY,
			hash: $string.EMPTY
		};

		this.hashParam = {};
		this.queryParam = {};

		return this;
	};

	Url.prototype.qualify = function (value)
	{
		if (!value)
			return $string.EMPTY;

		if (value.indexOf("//") != -1)
			return value;

		if (value.indexOf("/") == 0)
			return [
				this.components.protocol, url.PROTOCOL_SEPARATOR,
				this.components.authority,
				value].join($string.EMPTY);

		return [
			this.components.protocol, url.PROTOCOL_SEPARATOR,
			this.components.authority,
			this.components.directory,
			value].join($string.EMPTY);
	};


	/**
	 * Gets all hash parameters from this <c>Url</c> instance, optionally prefixed with the specified <c>prefix</c>.
	 * @param {String} prefix The character to prefix the resulting  string with
	 * @returns {String}
	 */
	Url.prototype.getHash = function (prefix)
	{
		var result = [];
		for (var name in this.hashParam)
		{
			if (!this.hashParam.hasOwnProperty(name))
				continue;

			result.push(name);
			result.push(this.equals);
			result.push(this.hashParam[name]);
		}

		var value = result.join(this.separator);
		if (value != $string.EMPTY && prefix != $string.EMPTY)
			return prefix + value;

		return value;
	};

	/**
	 * Gets all query string parameters from this <c>Url</c> instance, optionally prefixed with the specified <c>prefix</c>.
	 * @param {String} prefix The character to prefix the resulting  string with
	 * @returns {String}
	 */
	Url.prototype.getQuery = function (prefix)
	{
		var result = [];
		for (var name in this.queryParam)
		{
			if (!this.queryParam.hasOwnProperty(name))
				continue;

			result.push(name);
			result.push(this.equals);
			result.push(this.queryParam[name]);
		}

		var value = result.join(this.separator);
		if (value != $string.EMPTY && prefix != $string.EMPTY)
			return prefix + value;

		return value;
	};

	/**
	 * Gets the query string parameter with the specified <c>name</c>.
	 * @param {String} name The name of the parameter to get.
	 * @return {String} The value of the parameter.
	 */
	Url.prototype.getQueryParam = function (name)
	{
		return this.queryParam[name];
	};

	/**
	 * Gets the hash parameter with the specified <c>name</c>.
	 * @param {String} name The name of the parameter to get.
	 * @return {String} The value of the parameter.
	 */
	Url.prototype.getHashParam = function (name)
	{
		return this.hashParam[name];
	};

	/**
	 * Sets the query string parameter with the specified <c>name</c> the value of the specified <c>value</c>.
	 * @param {String} name The name of the parameter to set.
	 * @param {String} value The value to set.
	 * @return {Url} The current instance
	 */
	Url.prototype.setQueryParam = function (name, value)
	{
		this.queryParam[name] = value;
		return this;
	};

	/**
	 * Sets the hash parameter with the specified <c>name</c> the value of the specified <c>value</c>.
	 * @param {String} name The name of the parameter to set.
	 * @param {String} value The value to set.
	 * @return {Url} The current instance
	 */
	Url.prototype.setHashParam = function (name, value)
	{
		this.hashParam[name] = value;
		return this;
	};

	/**
	 * Removes the parameter with the specified <c>name</c> from the query string component of the current object.
	 * @param {String} name The name of the parameter to remove.
	 * @return {Url} The current instance
	 */
	Url.prototype.removeQueryParam = function (name)
	{
		delete this.queryParam[name];
		return this;
	};

	/**
	 * Removes the parameter with the specified <c>name</c> from the hash component of the current object.
	 * @param {String} name The name of the parameter to remove.
	 * @return {Url} The current instance
	 */
	Url.prototype.removeHashParam = function (name)
	{
		delete this.hashParam[name];
		return this;
	};

	/**
	 * Parses the specified <c>value</c> as url components and stores them into the current instance.
	 * @param {String} value
	 * @return {Url} The current instance
	 */
	Url.prototype.parse = function (value)
	{
		if (value == null)
			return this;

		var matches = url.URL_EXPRESSION.exec(value);

		for (var i = url.COMPONENT_NAMES.length - 1; i > 0; i--)
			this.components[url.COMPONENT_NAMES[i]] = matches[i] || $string.EMPTY;

		var hash = this.components.hash;
		var query = this.components.query;

		if (hash == url.EMPTY_HASH)
			hash = $string.EMPTY;

		this.hashParam = url.parseQuery(hash);
		this.queryParam = url.parseQuery(query);

		return this;
	};

	/**
	 * Creates a complete url string made of all components of this <c>Url</u> instance.
	 * @returns {String}
	 */
	Url.prototype.toString = function ()
	{
		var prefix = this.components.protocol
			? $string.concat(this.components.protocol, url.PROTOCOL_SEPARATOR)
			: $string.EMPTY;

		return $string.concat(
			prefix,
			this.components.authority,
			this.components.path,
			url.serializeParams(this.queryParam, { prefix: "?", encode: true }),
			url.serializeParams(this.hashParam, { prefix: "#", encode: false })
		);
	};

	/**
	 * Combines the parent <c>folderUrl</c> with the child <c>fileUrl</c> into a single URL.
	 * @param {String} folderUrl The parent URL to combine, e.g. <c>my/directory/to/files</c>.
	 * @param {String} fileUrl The child URL to combine, e.g. <c>../file2.txt</c>
	 * @return {String} The combined URL.
	 * @example
	 * // the following returns "my/directory/to/file2.txt":
	 * var combined = combine("my/directory/to/files", "../file2.txt");
	 */
	url.combine = function (folderUrl, fileUrl)
	{
		var filePath = $string.concat(folderUrl, "/", fileUrl);
		while (filePath.match(/[^\/]+\/\.\.\//))
		{
			filePath = filePath.replace(/[^\/]+\/\.\.\//, "");
		}
		return filePath.replace(/\/{2,}/g, "/");
	};

	/**
	 * Gets the parameter with the specified <c>name</c> from the query string component of the current <c>window.location</c>.
	 * @param {String} name The name of the parameter to get
	 * @param {String} defaultValue The value to return in case the current query string doesn't contain the part with the
	 * specified <c>name</c> or if that value is blank.
	 * @returns {String}
	 */
	url.getQueryParam = function (name, defaultValue)
	{
		return new Url().getQueryParam(name) || defaultValue;
	};

	/**
	 * Gets the parameter with the specified <c>name</c> from the hash component of the current <c>window.location</c>.
	 * @param {String} name The name of the parameter to get
	 * @param {String} defaultValue The value to return in case the current hash doesn't contain the part with the
	 * specified <c>name</c> or if that value is blank.
	 * @returns {String}
	 */
	url.getHashParam = function (name, defaultValue)
	{
		return new Url().getHashParam(name) || defaultValue;
	};

	/**
	 * Gets the query string component (without the initial hash character) from current <c>window.location</c>.
	 * @returns {String}
	 */
	url.getHash = function ()
	{
		return String(location.hash).substring(1);
	};

	/**
	 * Gets the query string component (without the initial question character) from current <c>window.location</c>.
	 * @returns {String}
	 */
	url.getQuery = function ()
	{
		return String(location.search).substring(1);
	};

	/**
	 * Sets the value of the specified hash parameter of the current <c>window.location</c>.
	 *
	 * If a single argument is passed as an object, it will be treated as a name/value collection. If two values are
	 * passed, they will be treated a single key/value pair.
	 * @example setHashParam("color", "red");
	 * @example setHashParam({ color: "red", size: "xlarge" });
	 */
	url.setHashParam = function ()
	{
		var current = url(location.href);
		if (arguments.length == 2)
		{
			current.setHashParam(arguments[0], arguments[1]);
		}
		else if (arguments.length == 1)
		{
			if ($type.isObject(arguments[0]))
			{
				for (var name in arguments[0])
					current.setHashParam(name, arguments[0][name]);
			}
			else
			{
				current.setHashParam(arguments[0], $string.EMPTY);
			}
		}
		else
		{
			return;
		}

		url.setHash(current.getHash());
	};

	/**
	 * Sets the complete hash of the current <c>window.location</c>.
	 * @param {String} hash The value to set.
	 */
	url.setHash = function (hash)
	{
		// ensure that setting the location hash to empty doesn't cause the page to scroll to the top
		if (hash == $string.EMPTY)
			hash = url.EMPTY_HASH;

		window.location.hash = hash;
	};

	/**
	 * Removes the parameter with the specified <c>name</c> from the hash component of the current <c>window.location</c>.
	 * @param {String} name The name of the parameter to remove.
	 */
	url.removeHashParam = function (name)
	{
		var current = new Url(location);
		current.removeHashParam(name);

		url.setHash(url.getHash());
	};

	/**
	 * Gets the file extension from the specified <c>path</c>.
	 * @param {String} path
	 * @returns {String}
	 */
	url.getFileExtension = function (path)
	{
		var dotIndex = path ? path.lastIndexOf(".") : -1;
		return (dotIndex > -1 ? path.slice(dotIndex + 1) : "");
	};

	/**
	 * Gets the file name part of the specified <c>path</c>.
	 * @param {String} path
	 * @returns {String}
	 */
	url.getFileName = function (path)
	{
		return String(path).match(/[^\\\/]*$/)[0];
	};

	/**
	 * Gets an object with properties initialized with values as parsed from name/value pairs in the
	 * specified <c>queryString</c>.
	 * @param {String} queryString The string to parse.
	 * @param {Object} options Optional object that specifies the parse options.
	 * <dl>
	 *  <dt>separator</dt>
	 *    <dd>the character to use as the name/value pair separator within url query strings and hashes</dd>
	 *  <dt>equals</dt>
	 *    <dd>the character to use as the names/value separator within url query strings and hashes.</dd>
	 *  <dt>equals</dt>
	 *    <dd>the character to use as the names/value separator within url query strings and hashes.</dd>
	 * </dl>
	 * @returns {Object}
	 */
	url.parseQuery = function (queryString, options)
	{
		if (!queryString)
			return {};

		var opt = $.extend({
				separator: url.DEFAULT_SEPARATOR,
				equals: url.DEFAULT_EQUALS,
			},
			options || {});

		var param = String(queryString).split(opt.separator);
		var query = {};
		for (var i = 0; i < param.length; i++)
		{
			if (param[i].length == 0)
				continue;

			var pair = param[i].split(opt.equals);
			var key = pair[0];
			var itemValue = pair[1] || $string.EMPTY;

			if (query[key] != null)
			{
				if (!$type.isArray(query[key]))
					query[key] = [query[key]];

				query[key].push(itemValue);
			}
			else
			{
				query[key] = itemValue;
			}
		}

		return query;
	};

	/**
	 * Serializes the names and values of properties of <c>params</c> into a string of name/value pairs.
	 * @param {Object} params The object to serialize.
	 * @param {Object} [options] Object that specifies the serialization options.
	 * <dl>
	 *  <dt>separator</dt>
	 *    <dd>the character to use as the pair separator</dd>
	 *  <dt>equals</dt>
	 *    <dd>the character to use as the names/value separator</dd>
	 *  <dt>encode</dt>
	 *    <dd>if <c>true</c> the resulting string will be URL encoded.</dd>
	 * </dl>
	 * @return {String}
	 */
	url.serializeParams = function (params, options)
	{
		if (params == null)
			return null;

		var opt = $.extend({
			separator: url.DEFAULT_SEPARATOR,
			equals: url.DEFAULT_EQUALS,
			encode: false
		},
			options || {});

		var result = [];
		for (var name in params)
		{
			if (!params.hasOwnProperty(name))
				continue;

			var itemName = opt.encode ? encodeURIComponent(name) : name;
			var itemValue = opt.encode ? encodeURIComponent(params[name] || $string.EMPTY) : params[name];
			if (itemValue)
				result.push(itemName + opt.equals + itemValue);
			else
				result.push(itemName);
		}

		var value = result.join(opt.separator);
		if (opt.prefix && value.length)
			return $string.concat(opt.prefix, value);

		return value;
	};

	url.qualify = function (value)
	{
		var url = $("base[href]").attr("href") || document.location.href;
		return new Url(url).qualify(value);
	};

	return url;
};


/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the atom.js
 */

/**
 * Gets or sets a cookie, or gets a raw cookie string, depending on the number of arguments.
 * If no arguments are provided, it returns the raw browser cookie string. If one argument
 * is provided, it is considered the name of the cookie to get. The value returned is the value
 * of that cookie (@see {atom.cookie.get}). If more arguments are provided, the cookie is set (@see {atom.cookie.set})
 */
var $cookie = new function cookie()
{
	/**
	 * Sets/adds a cookie to the document.cookies collection
	 * @param {String} name The name of the cookie.
	 * @param {String} value The value of the cookie.
	 * @param {String} [expires] An expression, signifying the expires property of the cookie. See <code>MakeDate</code>.
	 * @param {String} [domain] The domain property of the cookie.
	 * @param {String} [path] The path property of the cookie.
	 * @param {Boolean} [secure] The secure property of the cookie.
	 * @return {String} The value of the cookie that has just been set.
	 */
	function set(name, value, expires, domain, path, secure)
	{
		var cookie = name + "=" + encodeURIComponent(value);

		if (expires) cookie += "; expires=" + $date(expires).toGMTString();
		if (domain)  cookie += "; domain="  + domain;
		if (path)    cookie += "; path="    + path;
		if (secure)  cookie += "; secure=true";

		document.cookie = cookie;

		return get(name);
	}

	/**
	 * Returns a cookie with the specified name from the document.cookies collection, if present,
	 * and a <code>null</code> if not.
	 * @param {String} name The name of the cookie. Required.
	 * @return {Object} The value of the cookie, if found, and a <code>null</code> otherwise.
	 */
	function get(name)
	{
		var cookies = document.cookie.split("; ");
		for (var i = 0; i < cookies.length; i++)
		{
			var cookieName  = cookies[i].substring(0, cookies[i].indexOf("="));
			var cookieValue = cookies[i].substring(cookies[i].indexOf("=") + 1, cookies[i].length);
			if (cookieName == name)
			{
				if (cookieValue.indexOf("&") != -1)
				{
					var pairs  = cookieValue.split("&");
					var cookie = {};
					for (var j = 0; j < pairs.length; j++)
					{
						var arrTemp = pairs[j].split("=");
						cookie[arrTemp[0]] = arrTemp[1];
					}
					return cookie;
				}
				else
					return decodeURIComponent(cookieValue);
			}
		}
		return null;
	};

	/**
	 * Deletes a cookie with the specified name from the document.cookies collection.
	 * @param {String} name The name of the cookie. Required.
	 */
	function remove(name)
	{
		var cookieValue = name ? name + "=null;" : "null;";
		document.cookie = cookieValue + "expires=" + new Date().toGMTString();
	};

	function cookie()
	{
		if (arguments.length == 0)
			return String(document.cookie);

		if (arguments.length == 1)
			return Cookie.get.apply(this, arguments);

		return set.apply(this, arguments);
	}

	cookie.get = get;
	cookie.set = set;
	cookie.remove = remove;

	return cookie;
};


/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the atom.js
 */

/**
 * Provides xml utilities
 * @type {Function}
 */
var $xml = new function xml()
{
	var parser;
	var parseErrorNs;
	var parserThrows = false;
	var implementation = document.implementation;
	var domcompatible = window.XMLSerializer != null;
	var xhtmlNamespace = "http://www.w3.org/1999/xhtml";
	var ORDERED_NODE_ITERATOR_TYPE = 5;

	var nodeType = {
		ELEMENT: 1,
		ATTRIBUTE: 2,
		TEXT: 3,
		CDATA_SECTION: 4,
		ENTITY_REFERENCE: 5,
		ENTITY: 6,
		PROCESSING_INSTRUCTION: 7,
		COMMENT: 8,
		DOCUMENT: 9,
		DOCUMENT_TYPE: 10,
		DOCUMENT_FRAGMENT: 11,
		NOTATION: 12
	};

	/**
	 * Returns an XML node or XML document, depending on the type of <c>value</c>.
	 * <p>If the specified <c>value</c> is a string, a new document will be created and initialized using the <c>value</c>
	 * as it's XML content. The string <c>value</c> should not be blank and it should start with a '&lt;' to be
	 * considered xml.</p>
	 * <p>If the specified <c>value</c> is an XML node, it will be returned unchanged.</p>
	 * @param {Node|String} value Either the XML node that will be returned, or the string to create a new document with.
	 * @returns {Node} Either the XML node that was specified or an XML document initialized with the specified string.
	 */
	var xml = function xml(value)
	{
		if ($type.isNode(value))
			return value;

		if ($type.isString(value))
		{
			if (value.trim().indexOf("<") == 0)
			{
				return xml.document(value);
			}
		}

		return null;
	};

	/**
	 * Selects the text content from an xml node.
	 * This function can be called in several ways and the position of arguments is flexible.
	 * If the function is called with a single argument <c>subject</c>, the function will simply return the its text content.
	 * If the <c>xpath</c> argument function is used, the xml node(s) specified with the <c>xpath</c> expression are
	 * selected first and their (concatenated) text content is returned. If the function was called without the <c>subject</c>
	 * argument specified the subject against which the <c>xpath</c> expression will be executed will be the current HTML
	 * document.
	 * @param {String} [xpath] Optional xpath to use to select the node(s) whose text content to return.
	 * @param {Node} [subject] Optional node whose text to return, or within which to search for the node(s) specified
	 * with the <c>xpath</c> argument.
	 * @param {Object} [namespaces] Optional object that specifies namespaces and their prefixes. If the xpath argument
	 * uses qualified names in its expression this argument is required (and it must define the namespace prefixes used).
	 * @returns {String} The selected text content
	 */
	xml.text = function (xpath, subject, namespaces)
	{
		var xpath_ = $string.EMPTY;
		var subject_ = null;
		var namespaces_ = {};

		for (var i = 0; i < 3; i++)
		{
			if (arguments.length < i + 1)
				break;

			var arg = arguments[i];
			if ($type.isNode(arg))
				subject_ = arg;
			else if ($type.isString(arg))
				xpath_ = arg;
			else if ($type.isObject(arg))
				namespaces_ = arg;
		}

		var selection = [];

		if (xpath_ == $string.EMPTY && subject_ == null)
			return null;

		if (xpath_ == $string.EMPTY && subject_ != null)
			selection = [subject_];

		else
			selection = xml.select(xpath_, subject_ || document, namespaces_);

		var result = [];
		for (i = 0; i < selection.length; i++)
		{
			var selected = selection[i];
			if (selected.nodeType == nodeType.DOCUMENT)
				selected = selected.documentElement;

			switch (selected.nodeType)
			{
				case nodeType.TEXT:
				case nodeType.COMMENT:
					result.push(selected.nodeValue);
					break;

				default:
					if (selected.textContent != null)
						result.push(selected.textContent);
					else if (selection.text != null)
						result.push(selected.text);
					else
						result.push(selection.innerText);
					break;
			}
		}

		return result.join($string.EMPTY);
	};

	/**
	 * Returns the xml text for the specified <c>node</c>.
	 *
	 * The node can be supplied directly or it can be specified as an xpath
	 * expression to the node to select (in which case the selection will happen within the current document).
	 * @param {string|Node} node The node (or xpath of the node) whose xml string to get.
	 * @returns {string} The xml text of the specified <c>node</c>.
	 */
	xml.toXml = function(node)
	{
		if ($type.isString(node))
			node = xml.select(node)[0];

		if (domcompatible)
		{
			if (node == document || node.ownerDocument == document)
				return xml.serialize(node);

			return new XMLSerializer().serializeToString(Node(node));
		}

		return node.xml;
	};

	xml.serialize = function (node)
	{
		if (node == null || node.nodeType == null)
			return $string.EMPTY;

		if (node.nodeType == nodeType.DOCUMENT)
			return xml.serialize(node.documentElement);

		var rslt = [];
		var skipNamespace = false;

		if (node.nodeType == nodeType.ELEMENT)
		{
			var nodeName = node.nodeName;
			if (node.namespaceURI == xhtmlNamespace)
			{
				nodeName = nodeName.toLowerCase();
				skipNamespace = true;
			}

			rslt.push("<");
			rslt.push(nodeName);
			for (var i = 0; i < node.attributes.length; i++)
			{
				var attrName = node.attributes[i].name;
				var attrValue = node.getAttribute(attrName);

				if (attrName == "xmlns" && skipNamespace)
					continue;

				rslt.push(" ");
				rslt.push(node.attributes[i].name);
				rslt.push("=\"");
				rslt.push(xml.escape(attrValue, true));
				rslt.push("\"");
			}

			if (node.childNodes.length == 0)
				rslt.push("/>");
			else
			{
				rslt.push(">");
				for (i = 0; i < node.childNodes.length; i++)
				{
					var child = node.childNodes[i];
					if (child.nodeType == nodeType.ELEMENT)
					{
						rslt.push(xml.serialize(child));
					}
					if (child.nodeType == nodeType.COMMENT)
					{
						rslt.push("<!--");
						rslt.push(xml.text(child));
						rslt.push("-->");
					}
					if (child.nodeType == nodeType.CDATA_SECTION)
					{
						rslt.push("<![CDATA[");
						rslt.push(xml.text(child));
						rslt.push("]]>");
					}
					if (child.nodeType == nodeType.TEXT)
					{
						rslt.push(xml.escape(xml.text(child)));
					}
				}
				rslt.push("</");
				rslt.push(nodeName);
				rslt.push(">");
			}
		}

		return rslt.join($string.EMPTY);
	};

	xml.toObject = function(node)
	{
		if (node == null || node.nodeType == null)
			return null;

		var result = {};
		if (node.nodeType == nodeType.DOCUMENT)
		{
			var rootName = node.documentElement.nodeName;
			result[rootName] = xml.toObject(node.documentElement);
		}
		else if (node.nodeType == nodeType.ELEMENT)
		{
			for (var i = 0; i < node.attributes.length; i++)
			{
				result[node.attributes[i].name] =
					node.getAttribute(node.attributes[i].name);
			}
			for (i = 0; i < node.childNodes.length; i++)
			{
				var child = node.childNodes[i];
				if (child.nodeType == nodeType.ELEMENT)
				{
					var parsed = xml.toObject(child);
					if (result[child.nodeName] != null)
					{
						if (!$type.isArray(result[child.nodeName]))
							result[child.nodeName] = [result[child.nodeName]];

						result[child.nodeName].push(parsed);
					}
					else
						result[child.nodeName] = parsed;
				}
				if (child.nodeType == nodeType.TEXT && child.nodeValue.trim() != $string.EMPTY)
					result.$text = child.nodeValue;
			}
		}
		return result;
	};

	xml.resolver = function(namespaces)
	{
		$log.assert($type.isObject(namespaces), "Argument 'namespaces' should be an object");

		return function NsResolver(prefix) /**/
		{
			return namespaces[prefix];
		};
	};

	/**
	 * Selects xml nodes using the specified from <c>xpath</c>, either from the current document or from the specified
	 * <c>subject</c>.
	 * @param {string} xpath The XPath selection string to use.
	 * @param {Node} [subject] Optional xml node from which to select.
	 * @param {object} [namespaces] Optional object that defines the namespaces used by the <c>xpath</c> expression.
	 * The property names should be the namespace prefixes and property values the actual namespace URI's.
	 * @returns {Node[]} An array of nodes that were selected by the <c>xpath</c> expression.
	 */
	xml.select = function(xpath, subject, namespaces)
	{
		if (arguments.length == 2 && $type.isString(arguments[1]))
		{
			xpath = arguments[0];
			namespaces = arguments[1];
		}

		if (!$type.isNode(subject))
			subject = document;

		var ownerDocument = subject.nodeType == nodeType.DOCUMENT ? subject : subject.ownerDocument;
		if (window.ActiveXObject)
		{
			var nslist = [];
			for (var prefix in namespaces)
				if (namespaces.hasOwnProperty(prefix))
					nslist.push('xmlns:{0}="{1}"'.format(prefix, namespaces[prefix]));

			ownerDocument.setProperty("SelectionNamespaces", nslist.join(" "));
			return subject.selectNodes(xpath);
		}
		else
		{
			var nsResolver = namespaces
				? xml.resolver(namespaces)
				: null;

			var result = ownerDocument.evaluate(xpath, subject, nsResolver, ORDERED_NODE_ITERATOR_TYPE, null);
			var node = result.iterateNext();
			var nodeList = [];
			while (node)
			{
				nodeList.push(node);
				node = result.iterateNext();
			}
			return nodeList;
		}
	};

	/**
	 * Loads the specified <c>url</c>, parses the response as an xml document and calls the <c>onload</c> callback when
	 * the document is loaded and parsed.
	 * @param {string} url The url of the document to load
	 * @param {function(Document)} [onload] The function to call then the document has loaded.
	 * @param {function(Error|string)} [onerror] The function to call if an error occurs during loading or parsing the
	 * specified <c>url</c>.
	 * @returns {XMLHttpRequest} The jquery XMLHttpRequest created for the specified <c>url</c>.
	 */
	xml.load = function(url, onload, onerror)
	{
		$log.assert($type.isString(url), "Argument 'url' is required");

		return $.ajax(url,
		{
			complete: function (request)
			{
				var document;
				try
				{
					document = xml.document(request.responseText);
					if ($type.isFunction(onload))
						onload(document);
				}
				catch(e)
				{
					if ($type.isFunction(onerror))
						onerror(e);
					else
						throw e;
				}
			},
			error: function (request, status, errorThrown)
			{
				if ($type.isFunction(onerror))
					onerror(errorThrown);
			}
		});
	};

	/**
	 * Creates a new Document, optionally initializing it with the specified <c>source</c>.
	 * @param {string|Document} [source] Optional string or document whose html to use to initialize the returned document with.
	 * @returns {Document} A new Document, optionally initialized with the specified <c>source</c>.
	 */
	xml.document = function(source)
	{
		if ($type.isDocument(source))
		{
			source = xml.serialize(source);
		}

		var document = null;
		if (domcompatible)
		{
			if ($type.isString(source))
			{
				document = getParser().parseFromString(source, "text/xml");
				if (!parserThrows)
					throwIfParseError(document);
			}
			else
			{
				document = implementation.createDocument($string.EMPTY, $string.EMPTY, null);
			}
		}
		else if (window.ActiveXObject)
		{
			document = new ActiveXObject("MSXML2.DomDocument");
			if (source)
			{
				document.loadXML(source);
				throwIfParseError(document);
			}
		}

		return document;
	};

	xml.processor = function(source)
	{
		source = xml(source);
		if (source == null)
			return null;

		var processor = null;
		if (domcompatible)
		{
			processor = new XSLTProcessor();
			processor.importStylesheet(source);
		}
		else
		{
			var ftDocument = xml.document(source.xml);
			var template = new ActiveXObject("MSXML2.XslTemplate");
			template.stylesheet = ftDocument;
			processor = template.createProcessor();
		}

		return processor;
	};

	xml.transform = function(document, processor)
	{
		$log.assert($type.isDocument(document), "Argument 'document' should be a document.");
		$log.assert(!$type.isNull(processor), "Argument 'xslProcessor' is required");

		var result = null;
		if (domcompatible)
		{
			result = processor.transformToDocument(document);
		}
		else
		{
			result = xml.document();
			processor.input = document;
			processor.output = result;
			processor.transform();
		}

		return result;
	};

	xml.escape = function(value, quotes)
	{
		var result = String(value)
			.replace(/&(?!amp;)/g, "&amp;")
			.replace(/</g, "&lt;")
			.replace(/>/g, "&gt;");

		if (quotes)
			result = result.replace(/\"/g, "&quot;");

		return result;
	};

	function throwIfParseError(document)
	{
		var info = { name: "XML Parse Error", error: false, data: { line: 1, column: 0 }};
		if (domcompatible)
		{
			var parseError = document.getElementsByTagNameNS(parseErrorNs, "parsererror")[0];
			if (parseError != null)
			{
				var message = xml.text(parseError).replace(/line(?: number)? (\d+)(?:,)?(?: at)? column (\d+):/i, function replace($0, $1, $2)
				{
					info.line = parseInt($1);
					info.column = parseInt($2);
					return $0 + "\n";
				});

				info.error = true;
				info.message = message;
			}
		}
		else
		{
			if (document.parseError != 0)
			{
				info.error = true;
				info.message = document.parseError.reason;

				if (document.parseError.line)
					info.data.line = document.parseError.line;

				if (document.parseError.column)
					info.data.column = document.parseError.column;
			}
		}

		if (info.error == true)
			throw error(info);
	}

	function getParser()
	{
		if (parser == null)
		{
			parser = new DOMParser();
			try
			{
				parseErrorNs = parser
					.parseFromString("INVALID XML", "text/xml")
					.getElementsByTagName("*")[0]
					.namespaceURI;
			}
			catch(e)
			{
				parserThrows = true;
			}
		}

		return parser;
	}

	xml.nodeType = nodeType;

	return xml;
};

/**
 * Provides an object that initializes modules in a controlled way.
 * @param {Function} onread A callback function to register as listener of the <c>done</c> event. Optional.
 * @event setup Fired after one of the modules has been setup. The event data contains two number properties;
 * <c>total</c>: the total number of modules registered with current instance, and <c>current</c>: the index of the
 * module that has just been setup.
 * @event done Fired when all of the modules have been setup.
 */
var Initializer = Dispatcher.extend(function Initializer(onready)
{
	this.construct("setup", "done");

	var init = this;
	var modules = [];
	var modulesReady = 0;

	/**
	 * Registers a module for initialization.
	 * @param {Function} module The object that will be initialized. It needs to have the <code>setup</code> method, which
	 * will be called to set it up. Optionally, if it has a <code>dispose</code> method, it will be called during
	 * the initializer's disposal.
	 * @param {Boolean} async A value indicating whether the object should be initialized asynchronously.
	 * @returns {Object} The <c>module</c> that was specified.
	 */
	this.register = function register(module, async)
	{
		$log.assert($type.isObject(module), "Argument 'module' is required");
		$log.assert($type.isFunction(module.setup), "The 'module' argument must have a 'setup' method");

		modules.push({ setup: $.proxy(module.setup, module), dispose: $.proxy(module.dispose, module), async: async || false });
		$log.info("Registered module {0}", $type.isFunction(module) ?
			Function.getName(module) :
			Function.getName(module.constructor));

		return module;
	};

	/**
	 * Starts the initialization.
	 */
	this.setup = function setup()
	{
		if (modules.length == 0)
		{
			done();
		}
		else
		{
			modulesReady = 0;
			for (var i = 0; i < modules.length; i++)
			{
				var module = modules[i];
				if (module.async)
				{
					module.setup(onModuleInitialized);
				}
				else
				{
					module.setup();
					onModuleInitialized();
				}
			}
		}
	};

	/**
	 * Calls dispose on any registed modules that implement their own <code>dispose</code> method.
	 */
	this.dispose = function dispose()
	{
		for (var i = 0; i < modules.length; i++)
		{
			var module = modules[i];
			if (module.dispose)
			{
				module.dispose();
			}
		}
	};

	function onModuleInitialized()
	{
		modulesReady += 1;
		init.fire("setup", { total: modules.length, current: modulesReady });

		if (modulesReady == modules.length)
			done();
	}

	function done()
	{
		init.fire("done");
		if ($type.isFunction(onready))
			onready();
	}
});

/**
 * Defines various constant collections
 * @type {Object}
 */
var $const = new function constants()
{
	/**
	 * Defines commonly used key code constants
	 * @enum
	 */
	this.Key =
	{
		TAB: 9,
		CONTROL: 17,
		SHIFT: 16,
		ALT: 18,
		ENTER: 13,
		UP: 38,
		DOWN: 40,
		LEFT: 37,
		RIGHT: 39,
		DELETE: 46,
		BACKSPACE: 8,
		ESCAPE: 27
	};
};

/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the atom.js
 */

/**
 * Provides event-related utilities.
 */
var $evt = new function ()
{
	/**
	 * Returns the cross-browser event object.
	 * @param {Event} e The event that was raised. In IE6 and lower this is null and is read from the window object.
	 * @returns {Event} The cross-browser event object.
	 */
	function evt(e)
	{
		return e || window.event;
	}

	/**
	 * Calls <c>preventDefault</c> method if the event object supports it.
	 * @param {Event} e The event that was raised. In IE6 and lower this is null and is read from the window object.
	 * @returns {Boolean} Returns false.
	 */
	evt.preventDefault = function preventDefault(e)
	{
		e = evt(e);
		if (e.preventDefault)
			e.preventDefault();

		return false;
	};

	/**
	 * Sets the event's <c>cancelBubble</c> to <c>true</c>, <c>returnValue</c> to <c>false</c> and calls it's
	 * <c>stopPropagation</c> event.
	 * @param {Event} e The event that was raised.
	 * @returns {Boolean} Returns false.
	 */
	evt.cancel = function cancel(e)
	{
		e = evt(e);
		if (e != null)
		{
			e.cancelBubble = true;
			e.returnValue = false;

			if (e.stopPropagation)
				e.stopPropagation();
		}

		return false;
	};

	/**
	 * Creates a new <c>Event</c> object.
	 * @param {Dispatcher} source The object that fired the event.
	 * @param {String} type The event type/name.
	 * @param {Object} [data] Optional object that contain additional event information.
	 * @returns {$evt.Event} A new <c>Event</c> object.
	 */
	evt.create = function createEvent(source, type, data)
	{
		return new evt.Event(source, type, data);
	};

	/**
	 * Implements an object that provides information about events raised by <c>Dispatcher</c> objects.
	 * @constructor
	 * @param {Dispatcher} source The object that fired the event.
	 * @param {String} type The event type/name.
	 * @param {Object} data Optional object that contain additional event information.
	 */
	evt.Event = function Event(source, type, data)
	{
		this.source = source;
		this.type = type;
		this.name = type;
		this.cancel = false;
		this.data = {};

		for (var prop in data)
			if (data.hasOwnProperty(prop))
				this.data[prop] = data[prop];
	};

	return evt;
};

/**
 * Provides an object that coordinates dragging.
 */
var $drag = new function Dragger()
{
	var specs = {};

	var clientX, clientY;
	var startX, startY;

	var $target = null;

	var OVERLAY_CSS = "position: absolute; background: #fff; opacity: .01; z-index:10000;";
	var OVERLAY_CLASS = "drag-overlay-" + $date.time();

	var overlays = [];

	/**
	 * Implements a <c>Dispatcher</c> that controls element dragging.
	 * @event start Fired at the start of the drag.
	 * @event beforemove Fired before right before the element is moved. The event data will contain coordinates to which
	 * the object will be moved; <code>targetX</code> and <code>targetY</code>. This provides listener control over movement
	 * bounds; they can then analyze the target coordinates and set them accordingly prior to object being moved.
	 * @event move Fired right after the element has been moved. The event data will contain coordinates to which the
	 * object has been moved.
	 * @event end Fired at the end of the drag.
	 */
	var dragger = new Dispatcher("start", "beforemove", "move", "stop");
	dragger.active = false;

	/**
	 * Starts the dragging of an element.
	 *
	 * The <c>specifications</c> parameter specifies details about the drag:
	 * <ul><li>horizontal: Boolean (Specifies whether horizontal dragging is allowed. Default is <c>true</c>.)</li>
	 * <li>vertical: Boolean (Specifies whether vertical dragging is allowed. Default is <c>true</c>.)</li>
	 * <li>minX: Number (Specifies the minimum allowed horizontal coordinate.)</li>
	 * <li>maxX: Number (Specifies the minimum allowed horizontal coordinate.)</li>
	 * <li>minY: Number (Specifies the minimum allowed vertical coordinate.)</li>
	 * <li>maxY: Number (Specifies the minimum allowed vertical coordinate.)</li>
	 * </ul>
	 * @param {Event} e The event from which dragging starts; typically <code>mousedown</code>.
	 * @param {HTMLElement} target The element to be dragged.
	 * @param {Object} specifications Object with drag specifications.
	 * @returns {Boolean} <c>false</c> if drag has started, to cancel the original event; or <c>true</c> if the drag is
	 * rejected due to missing or invalid arguments.
	 */
	dragger.start = function start(e, target, specifications)
	{
		var event = e || window.event;
		if (event == null)
			return true;

		if (event.originalEvent)
			event = event.originalEvent;
		$target = $(target);
		if ($target.length == 0)
			return true;

		if (specifications == null)
			specifications = {};

		specs.moveX = specifications.moveX !== false;
		specs.moveY = specifications.moveY !== false;

		specs.accelX = 0;
		specs.accelY = 0;

		if (!specs.moveX && !specs.moveY)
			return true;

		specs.minX = isNaN(specifications.minX) ? Number.NEGATIVE_INFINITY : specifications.minX;
		specs.maxX = isNaN(specifications.maxX) ? Number.POSITIVE_INFINITY : specifications.maxX;
		specs.minY = isNaN(specifications.minY) ? Number.NEGATIVE_INFINITY : specifications.minY;
		specs.maxY = isNaN(specifications.maxY) ? Number.POSITIVE_INFINITY : specifications.maxY;

		clientX = $dom.isTouchDevice ? event.pageX : event.clientX;
		clientY = $dom.isTouchDevice ? event.pageY : event.clientY;

		var position = $target.position();
		var offset = $target.offset();

		startX = isNaN(position.left) ? offset.left : position.left;
		startY = isNaN(position.top) ? offset.top : position.top;

		document.addEventListener("mouseup", stop, true);
		document.addEventListener("touchend", stop, true);
		document.addEventListener("mousemove", move, true);
		document.addEventListener("touchmove", move, true);

		overlayFrames();

		dragger.active = true;
		dragger.fire("start");

		return $evt.cancel(e);
	};

	function stop()
	{
		dragger.active = false;
		dragger.fire("stop", { specs: specs });

		document.removeEventListener("mousemove", move, true);
		document.removeEventListener("touchmove", move, true);
		document.removeEventListener("mouseup", stop, true);
		document.removeEventListener("touchend", stop, true);

		removeOverlays();

		$target = null;
	}

	function move(e)
	{
		var event = $evt(e);

		var eventX = $dom.isTouchDevice ? event.pageX : event.clientX;
		var eventY = $dom.isTouchDevice ? event.pageY : event.clientY;

		var diffX = eventX - clientX;
		var diffY = eventY - clientY;

		var targetX = Math.max(Math.min(specs.maxX, startX + diffX), specs.minX);
		var targetY = Math.max(Math.min(specs.maxY, startY + diffY), specs.minY);

		if (dragger.__listeners["beforemove"].length)
		{
			var moveEvent = $evt.create(dragger, "beforemove");

			if (specs.moveX)
				moveEvent.data.targetX = targetX;

			if (specs.moveY)
				moveEvent.data.targetY = targetY;

			dragger.fire(moveEvent);

			targetX = moveEvent.data.targetX;
			targetY = moveEvent.data.targetY;
		}

		var position1 = $target.position();
		if (specs.moveX)
			$target.css("left", targetX);
		if (specs.moveY)
			$target.css("top", targetY);

		var position2 = $target.position();
		if (position2.left != position1.left || position2.top != position1.top)
			dragger.fire("move", position2);

		specs.accelX = position2.left - position1.left;
		specs.accelY = position2.top - position1.top;

		sizeOverlays();
		return $evt.cancel(e);
	}

	function overlayFrames()
	{
		var iframes = $("iframe");
		if (iframes.length > 0)
		{
			removeOverlays();
			iframes.each(function overlayFrame(i, iframe)
			{
				if (iframe.offsetHeight > 0)
				{
					var overlay = document.createElement("div");
					overlay.iframe = iframe;
					overlay.style.cssText = OVERLAY_CSS;
					overlay.className = OVERLAY_CLASS;

					document.body.appendChild(overlay);

					overlays.push(overlay);
				}
			});

			sizeOverlays();
		}
	}

	function sizeOverlays()
	{
		for (var i = overlays.length - 1; i >= 0; i--)
		{
			var overlay = overlays[i];
			var iframeOffset = $(overlay.iframe).offset();

			overlay.style.left = iframeOffset.left + "px";
			overlay.style.top = iframeOffset.top + "px";
			overlay.style.width = overlay.iframe.offsetWidth + "px";
			overlay.style.height = overlay.iframe.offsetHeight + "px";
		}
	}

	function removeOverlays()
	{
		for (var i = overlays.length - 1; i >= 0; i--)
		{
			overlays[i].parentNode.removeChild(overlays[i]);
			overlays.pop();
		}
	}

	return dragger;
};

var $easing = new function Easing()
{
	/**
	 * Calculate the current value without any easing
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeNone = function easeNone(t, b, c, d)
	{
		return c * t/ d + b;
	};

	/**
	 * Calculate the current value without any easing
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeIn = function easeIn(t, b, c, d)
	{
		return c * t / d + b;
	};

	/**
	 * Calculate the current value without any easing
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeOut = function easeOut(t, b, c, d)
	{
		return c * t / d + b;
	};

	/**
	 * Calculate the current value without any easing
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInOut = function easeInOut(t, b, c, d)
	{
		return c * t / d + b;
	};

	/**
	 * Calculate the current value with quad (^2) easing in
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInQuad = function easeInQuad(t, b, c, d)
	{
		return c * (t /= d) * t + b;
	};

	/**
	 * Calculate the current value with quad (^2) easing out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeOutQuad = function easeOutQuad(t, b, c, d)
	{
		return -c * ( t/= d) * (t - 2) + b;
	};

	/**
	 * Calculate the current value with quad (^2) easing in and out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInOutQuad = function easeInOutQuad(t, b, c, d)
	{
		if ((t /= d / 2) < 1)
			return c / 2 * t * t + b;

		return -c / 2 * ((--t) * (t-2) - 1) + b;
	};

	/**
	 * Calculate the current value with cubic (^3) easing in
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInCubic = function easeInCubic(t, b, c, d)
	{
		return c * (t /= d) * t * t + b;
	};

	/**
	 * Calculate the current value with cubic (^3) easing out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeOutCubic = function easeOutCubic(t, b, c, d)
	{
		return c * ((t = t/d - 1) * t * t + 1) + b;
	};

	/**
	 * Calculate the current value with cubic (^3) easing in and out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInOutCubic = function easeInOutCubic(t, b, c, d)
	{
		if ((t /= d/2) < 1)
			return c/2 * t * t * t + b;

		return c/2 * ((t -= 2) * t * t + 2) + b;
	};

	/**
	 * Calculate the current value with quadruple (^4) easing in
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInQuart = function easeInQuart(t, b, c, d)
	{
		return c * (t /= d) * t * t * t + b;
	};

	/**
	 * Calculate the current value with quadruple (^4) easing out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeOutQuart = function easeOutQuart(t, b, c, d)
	{
		return -c * ((t = t/d - 1) * t * t * t - 1) + b;
	};

	/**
	 * Calculate the current value with quadruple (^4) easing in and out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInOutQuart = function easeInOutQuart(t, b, c, d)
	{
		if ((t /= d/2) < 1)
			return c/2 * t * t * t * t + b;

		return -c/2 * ((t -= 2) * t * t * t - 2) + b;
	};

	/**
	 * Calculate the current value with quntiple (^5) easing in
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInQuint = function easeInQuint(t, b, c, d)
	{
		return c * (t /= d) * t * t * t * t + b;
	};

	/**
	 * Calculate the current value with quntiple (^5) easing out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeOutQuint = function easeOutQuint(t, b, c, d)
	{
		return c * ((t = t / d - 1) * t * t * t * t + 1) + b;
	};

	/**
	 * Calculate the current value with quntiple (^5) easing in and out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInOutQuint = function easeInOutQuint(t, b, c, d)
	{
		if ((t /= d / 2) < 1)
			return c / 2 * t * t * t * t * t + b;

		return c/2 * ((t -= 2) * t * t * t * t + 2) + b;
	};

	/**
	 * Calculate the current value with exponential easing in
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInExpo = function easeInExpo(t, b, c, d)
	{
		return (t == 0) ? b :  c * Math.pow(2, 10 * (t/d - 1)) + b;
	};

	/**
	 * Calculate the current value with exponential easing out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeOutExpo = function easeOutExpo(t, b, c, d)
	{
		return (t == d) ? b+c :  c * (-Math.pow(2, -10 * t/d) + 1) + b;
	};

	/**
	 * Calculate the current value with exponential easing in and out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInOutExpo = function easeInOutExpo(t, b, c, d)
	{
		if (t == 0) return b;
		if (t == d) return b + c;
		if ((t /= d/2) < 1)
			return c/2 * Math.pow(2, 10 * (t - 1)) + b;

		return c/2 * (-Math.pow(2, -10 * --t) + 2) + b;
	};

	/**
	 * Calculate the current value with sinusoid easing in
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInSine = function easeInSine(t, b, c, d)
	{
		return -c * Math.cos(t/d * (Math.PI/2)) + c + b;
	};

	/**
	 * Calculate the current value with sinusoid easing out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeOutSine = function easeOutSine(t, b, c, d)
	{
		return c * Math.sin(t/d * (Math.PI/2)) + b;
	};

	/**
	 * Calculate the current value with sinusoid easing in and out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInOutSine = function easeInOutSine(t, b, c, d)
	{
		return -c/2 * (Math.cos(Math.PI*t/d) - 1) + b;
	};

	/**
	 * Calculate the current value with circular easing in
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInCirc = function easeInCirc(t, b, c, d)
	{
		return -c * (Math.sqrt(1 - (t /= d) * t) - 1) + b;
	};;

	/**
	 * Calculate the current value with circular easing out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeOutCirc = function easeOutCirc(t, b, c, d)
	{
		return c * Math.sqrt(1 - (t = t/d - 1) * t) + b;
	};

	/**
	 * Calculate the current value with circular easing in and out
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 */
	this.easeInOutCirc = function easeInOutCirc(t, b, c, d)
	{
		if ((t /= d/2) < 1)
			return -c/2 * (Math.sqrt(1 - t * t) - 1) + b;

		return c/2 * (Math.sqrt(1 - (t -= 2) * t) + 1) + b;
	};

	// BACK
	this.easeInBack = function easeInBack(t, b, c, d, s)
	{
		if (s == undefined)
			s = 1.70158;

		return c * (t /= d) * t * ((s + 1) * t - s) + b;
	};
	this.easeOutBack = function easeOutBack(t, b, c, d, s)
	{
		if (s == undefined)
			s = 1.70158;

		return c * ((t = t/d - 1) * t * ((s + 1) * t + s) + 1) + b;
	};
	this.easeInOutBack = function easeInOutBack(t, b, c, d, s)
	{
		if (s == undefined)
			s = 1.70158;
		if ((t /= d/2) < 1)
			return c/2 * (t * t * (((s *= (1.525)) + 1) * t - s)) + b;

		return c/2 * ((t -= 2) * t * (((s *= (1.525)) + 1) * t + s) + 2) + b;
	};

	// ELASTIC
	this.easeInElastic = function easeInElastic(t, b, c, d, a, p)
	{
		if (t==0) return b;  if ((t/=d)==1) return b+c;  if (!p) p=d*.3;
		if (!a || a < Math.abs(c)) { a=c; var s=p/4; }
		else var s = p/(2*Math.PI) * Math.asin (c/a);
		return -(a*Math.pow(2,10*(t-=1)) * Math.sin( (t*d-s)*(2*Math.PI)/p )) + b;
	};
	this.easeOutElastic = function easeOutElastic(t, b, c, d, a, p)
	{
		if (t==0) return b;  if ((t/=d)==1) return b+c;  if (!p) p=d*.3;
		if (!a || a < Math.abs(c)) { a=c; var s=p/4; }
		else var s = p/(2*Math.PI) * Math.asin (c/a);
		return (a*Math.pow(2,-10*t) * Math.sin( (t*d-s)*(2*Math.PI)/p ) + c + b);
	};
	this.easeInOutElastic = function easeInOutElastic(t, b, c, d, a, p)
	{
		if (t==0) return b;  if ((t/=d/2)==2) return b+c;  if (!p) p=d*(.3*1.5);
		if (!a || a < Math.abs(c)) { a=c; var s=p/4; }
		else var s = p/(2*Math.PI) * Math.asin (c/a);
		if (t < 1) return -.5*(a*Math.pow(2,10*(t-=1)) * Math.sin( (t*d-s)*(2*Math.PI)/p )) + b;
		return a*Math.pow(2,-10*(t-=1)) * Math.sin( (t*d-s)*(2*Math.PI)/p )*.5 + c + b;
	};

	// BOUNCE
	this.easeInBounce = function easeInBounc(t, b, c, d)
	{
		if ((t /= d) < (1/2.75))
			return c * (7.5625 * t * t) + b;
		else if (t < (2/2.75))
			return c * (7.5625 * (t -= (1.5/2.75)) * t + .75) + b;
		else if (t < (2.5/2.75))
			return c * (7.5625 * (t -= (2.25/2.75)) * t + .9375) + b;
		else
			return c * (7.5625 * (t-= (2.625/2.75)) * t + .984375) + b;
	};

	this.easeOutBounce = function easeOutBounce(t, b, c, d)
	{
		return c - this.easeOut(d-t, 0, c, d) + b;
	};

	this.easeInOutBounce = function easeInOutBounce(t, b, c, d)
	{
		if (t < d/2)
			return this.easeIn(t * 2, 0, c, d) * .5 + b;
		else
			return this.easeOut(t * 2 - d, 0, c, d) * .5 + c * .5 + b;
	};

	/**
	 * Calculate the current value using a sine wave
	 * @param {Number} t Elapsed time (in milliseconds)
	 * @param {Number} b Begin value
	 * @param {Number} c The current difference (between begin value and end value)
	 * @param {Number} d The duration of transition (in milliseconds)
	 * @param {Number} a The amplitude (in pixels) of the sinal curve
	 * @param {Number} p The phase (in pixels) of the sinal curve
	 */
	this.sineLinear = function sineLinear(t, b, c, d, a, p)
	{
		d = d / 1000;
		t = 1000 - t;

		var value = a * Math.sin(d * t);

		$log("Y: {0} * Math.sin(({1} * {2})) = {3}", a, d, t, value);
		return value;
	};

	var extension = {};
	for (var name in this)
	{
		if (!this.hasOwnProperty(name))
			continue;

		if ($type.isFunction(this[name]))
		{
			extension[name] = function proxy()
			{
				var targetFx = arguments.callee.fx;
				return targetFx.apply(this, $array.fromArguments(arguments, 1));
			};

			extension[name].fx = this[name];
		}
	}

	extension.def = "easeOutQuad";
	extension.swing = function (x, t, b, c, d)
	{
		return jQuery.easing[jQuery.easing.def](x, t, b, c, d);
	};

	jQuery.extend(jQuery.easing, extension);
};

/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the atom.js
 */

/**
 * Provides various image manipulation and processing utilities.
 * @type Object
 */
var $image = new function image()
{
	var XImage = HtmlControl.extend(function XImage(element)
	{
		this.construct(element, "start", "progress", "done");

		var canvasSupported = isCanvasSupported();
		if (canvasSupported)
		{
			var tagName = this.$element.prop("tagName");
			if (tagName == "CANVAS")
			{
				this.canvas = this.$element[0];
			}
			else
			{
				var canvas = this.$element.parent().find("canvas")[0];
				if (canvas == null)
				{
					canvas = document.createElement("canvas");
					canvas.width = this.$element.width();
					canvas.height = this.$element.height();

					this.$element.parent().append(canvas);
				}

				if (tagName == "IMG")
				{
					var img = new Image();
					var me = this;
					img.onload = function onload()
					{
						if (canvas.width == 0 || canvas.height == 0)
						{
							canvas.width = img.width;
							canvas.height = img.height;
						}

						var context = canvas.getContext("2d");
						context.drawImage(img, 0, 0, canvas.width, canvas.height);
					};

					img.src = this.$element.prop("src");
				}

				this.canvas = canvas;
				this.context = canvas.getContext("2d");
				this.$element.hide();
			}
		}
		else
		{
			this.$image2 = $("<img/>");
			for (var i = 0; i < this.$element[0].attributes.length; i++)
			{
				var attr = this.$element[0].attributes[i];
				this.$image2.attr(attr.name, attr.value);
			}

			this.$image2.hide();
			this.$image2.addClass("image2");
			this.$element.parent().append(this.$image2);
		}
	});

	XImage.prototype.drawImage = function XImage$drawImage(image)
	{
		this.clear();
		return this.context.drawImage(image, 0, 0,
			this.canvas.width,
			this.canvas.width * image.height / image.width);
	};

	XImage.prototype.cloneCurrent = function XImage$cloneCurrent()
	{
		var result = document.createElement("canvas");
		result.width = this.canvas.width;
		result.height = this.canvas.height;
		result.getContext("2d").drawImage(this.canvas, 0, 0, result.width, result.height);
		return result;
	};

	XImage.prototype.clear = function XImage$clear()
	{
		this.context.clearRect(0, 0, this.canvas.width, this.canvas.height);
	};

	XImage.prototype.transition = function XImage$transition(options)
	{
		var transition = this.__transition = $.extend(image.transitionDefaults, options);

		if (transition.start)
			this.on("start", transition.start);
		if (transition.progress)
			this.on("progress", transition.progress);
		if (transition.done)
			this.on("done", transition.done);

		transition.start = $date.time();
		transition.end = transition.start + transition.duration;

		if (this.canvas != null)
			transitionUsingCanvas.call(this, options);
		else
			transitionUsingOpacity.call(this, options);
	};

	function transitionUsingCanvas(options)
	{
		var me = this;
		var transition = this.__transition;

		transition.effect = image.Transitions[transition.effect] || image.Transitions.horizontalOpen;
		transition.easingFx = $easing[transition.easing] || $easing.easeOutQuad;
		transition.from = this.cloneCurrent();

		function beginTransition()
		{
			if (transition.effect.init)
				transition.effect.init(me, transition);

			me.fireEvent("start");
			me.animating = true;
			schedule(proxy(renderTransition, me));
		}

		if (transition.to instanceof Image)
			beginTransition();
		else
		{
			var src = transition.to;
			transition.to = new Image();
			transition.to.onload = beginTransition;
			transition.to.src = src;
		}
	}

	function transitionUsingOpacity(options)
	{
		var me = this;

		function beginTransition()
		{
			me.$image2.attr("src", options.to.src);
			me.$image2.css({ opacity: 0, display: "block" });
			me.$image2.animate({ opacity: 1 });
			me.$element.animate({ opacity: 0 },
			{
				duration: options.duration,
				easing: options.easing,
				progress: function progress(promise, progress, remaining) /**/
				{
					me.fireEvent("progress", { progress: progress });
				},
				complete: function complete() /**/
				{
					var img2 = me.$image2;
					var elem = me.$element;
					me.$element.hide().addClass("image2");
					me.$image2.removeClass("image2");
					me.$element = img2;
					me.$image2 = elem;
					me.fireEvent("done");
				}
			});

			me.fireEvent("start");
		}

		if (options.to instanceof Image)
		{
			beginTransition();
		}
		else
		{
			options.to = new Image();
			options.to.onload = beginTransition;
			options.to.src = src;
		}
	}

	function renderTransition()
	{
		var time = $date.time();
		var transition = this.__transition;
		var progress = transition.easingFx(time - transition.start, 0, 1, transition.duration);

		if (time >= transition.end)
		{
			this.clear();
			this.fireEvent("done");
			this.animating = false;
			return this.drawImage(transition.to);
		}
		else
		{
			transition.effect.render(this, transition, progress);
			this.fireEvent("progress", { progress: progress });
			schedule(proxy(renderTransition, this));
		}
	};

	function image(elem)
	{
		return new XImage(elem);
	}

	image.getData = function XImage$getData(source)
	{
		var temp = document.createElement("canvas");
		temp.width = source.width;
		temp.height = source.height;

		var context = temp.getContext("2d");
		context.drawImage(source, 0, 0, temp.width, temp.height);

		var result = context.getImageData(0, 0, temp.width, temp.height);
		temp = null;
		context = null;
		return result;
	};

	image.transitionDefaults =
	{
		duration: 1000,
		transition: "horizontalOpen",
		easing: "easeOutExpo"
	};

var Transitions = new function Transitions()
{
	var Transition = Prototype.extend(function Transition(object)
	{
		if (object && $type.isFunction(object.init))
			this.init = object.init;

		if (object && $type.isFunction(object.render))
			this.render = object.render;
	});

	Transition.prototype.init = function Transition$render(image, transition)
	{
		this.options = $.extend({}, transition.options);
	};

	Transition.prototype.render = function Transition$render(image, transition, progress)
	{
	};

	var ClippedTransition = Prototype.extend(function Transition(clipFunction)
	{
		this.clipFunction = clipFunction;
	});

	ClippedTransition.prototype.render = function ClippedTransition$render(image, transition, progress)
	{
		var canvas = image.canvas;
		var context = image.canvas.getContext("2d");
		image.drawImage(transition.from);

		context.save();
		context.beginPath();
		this.clipFunction(context, canvas.width, canvas.height, progress);
		context.clip();
		image.drawImage(transition.to);
		context.restore();
	};

	this.clock = new ClippedTransition(function render(ctx, w, h, p)
	{
		ctx.moveTo(w / 2, h / 2);
		return ctx.arc(w / 2, h / 2, Math.max(w, h), 0, Math.PI * 2 * p, false);
	});

	this.circle = new ClippedTransition(function render(ctx, w, h, p)
	{
		return ctx.arc(w / 2, h / 2, 0.6 * p * Math.max(w, h), 0, Math.PI * 2, false);
	});

	this.diamond = new ClippedTransition(function render(ctx, w, h, p)
	{
		var dh, dw, h2, w2;
		w2 = w / 2;
		h2 = h / 2;
		dh = p * h;
		dw = p * w;
		ctx.moveTo(w2, h2 - dh);
		ctx.lineTo(w2 + dw, h2);
		ctx.lineTo(w2, h2 + dh);
		return ctx.lineTo(w2 - dw, h2);
	});

	this.verticalOpen = new ClippedTransition(function render(ctx, w, h, p)
	{
		var h1, h2, hi, nbSpike, pw, spikeh, spikel, spiker, spikew, xl, xr, _results;
		nbSpike = 8;
		spikeh = h / (2 * nbSpike);
		spikew = spikeh;
		pw = p * w / 2;
		xl = w / 2 - pw;
		xr = w / 2 + pw;
		spikel = xl - spikew;
		spiker = xr + spikew;
		ctx.moveTo(xl, 0);
		for (hi = 0; 0 <= nbSpike ? hi <= nbSpike : hi >= nbSpike; 0 <= nbSpike ? hi++ : hi--)
		{
			h1 = (2 * hi) * spikeh;
			h2 = h1 + spikeh;
			ctx.lineTo(spikel, h1);
			ctx.lineTo(xl, h2);
		}
		ctx.lineTo(spiker, h);
		_results = [];
		for (hi = nbSpike; nbSpike <= 0 ? hi <= 0 : hi >= 0; nbSpike <= 0 ? hi++ : hi--)
		{
			h1 = (2 * hi) * spikeh;
			h2 = h1 - spikeh;
			ctx.lineTo(xr, h1);
			_results.push(ctx.lineTo(spiker, h2));
		}
		return _results;
	});

	this.horizontalOpen = new ClippedTransition(function render(ctx, w, h, p)
	{
		return context.rect(0, (1 - progress) * height / 2, width, height * progress);
	});

	this.horizontalSunblind = new ClippedTransition(function render(ctx, w, h, p)
	{
		var blind, blindHeight, blinds, _results;
		p = 1 - (1 - p) * (1 - p);
		blinds = 6;
		blindHeight = h / blinds;
		_results = [];
		for (blind = 0; 0 <= blinds ? blind <= blinds : blind >= blinds; 0 <= blinds ? blind++ : blind--)
		{
			_results.push(ctx.rect(0, blindHeight * blind, w, blindHeight * p));
		}
		return _results;
	});

	this.verticalSunblind = new ClippedTransition(function render(ctx, w, h, p)
	{
		var blind, blindWidth, blinds, prog, _results;
		p = 1 - (1 - p) * (1 - p);
		blinds = 10;
		blindWidth = w / blinds;
		_results = [];
		for (blind = 0; 0 <= blinds ? blind <= blinds : blind >= blinds; 0 <= blinds ? blind++ : blind--)
		{
			prog = Math.max(0, Math.min(2 * p - (blind + 1) / blinds, 1));
			_results.push(ctx.rect(blindWidth * blind, 0, blindWidth * prog, h));
		}
		return _results;
	});

	this.circles = new ClippedTransition(function render(ctx, w, h, p)
	{
		var circleH, circleW, circlesX, circlesY, cx, cy, maxRad, maxWH, r, x, y, _results;
		circlesY = 6;
		circlesX = Math.floor(circlesY * w / h);
		circleW = w / circlesX;
		circleH = h / circlesY;
		maxWH = Math.max(w, h);
		maxRad = 0.7 * Math.max(circleW, circleH);
		_results = [];
		for (x = 0; 0 <= circlesX ? x <= circlesX : x >= circlesX; 0 <= circlesX ? x++ : x--)
		{
			_results.push((function ()
			{
				var _results2;
				_results2 = [];
				for (y = 0; 0 <= circlesY ? y <= circlesY : y >= circlesY; 0 <= circlesY ? y++ : y--)
				{
					cx = (x + 0.5) * circleW;
					cy = (y + 0.5) * circleH;
					r = Math.max(0, Math.min(2 * p - cx / w, 1)) * maxRad;
					ctx.moveTo(cx, cy);
					_results2.push(ctx.arc(cx, cy, r, 0, Math.PI * 2, false));
				}
				return _results2;
			})());
		}
		return _results;
	});

	this.squares = new ClippedTransition(function render(ctx, w, h, p)
	{
		var blindHeight, blindWidth, blindsX, blindsY, prog, rh, rw, sx, sy, x, y, _results;
		p = 1 - (1 - p) * (1 - p);
		blindsY = 5;
		blindsX = Math.floor(blindsY * w / h);
		blindWidth = w / blindsX;
		blindHeight = h / blindsY;
		_results = [];
		for (x = 0; 0 <= blindsX ? x <= blindsX : x >= blindsX; 0 <= blindsX ? x++ : x--)
		{
			_results.push((function ()
			{
				var _results2;
				_results2 = [];
				for (y = 0; 0 <= blindsY ? y <= blindsY : y >= blindsY; 0 <= blindsY ? y++ : y--)
				{
					sx = blindWidth * x;
					sy = blindHeight * y;
					prog = Math.max(0, Math.min(3 * p - sx / w - sy / h, 1));
					rw = blindWidth * prog;
					rh = blindHeight * prog;
					_results2.push(ctx.rect(sx - rw / 2, sy - rh / 2, rw, rh));
				}
				return _results2;
			})());
		}
		return _results;
	});

	this.fadeLeft = new Transition(
	{
		init: function init(self, transition)
		{
			this.options = $.extend({ direction: "right" }, transition.options);

			transition.randomTrait = [];
			transition.fromData = image.getData(transition.from);
			transition.toData = image.getData(transition.to);
			transition.output = self.context.createImageData(self.canvas.width, self.canvas.height);

			var h = self.canvas.height;
			for (var i = 0; 0 <= h ? i <= h : i >= h; 0 <= h ? i++ : i--)
				transition.randomTrait[i] = Math.random();
		},

		render: function render(self, transition, progress, data)
		{
			var blur = 150;
			var height = self.canvas.height;
			var width = self.canvas.width;
			var fd = transition.fromData.data;
			var td = transition.toData.data;
			var out = transition.output.data;

			var wpdb = width * progress / blur;
			for (var x = 0; x < width; ++x)
			{
				var xdb = x / blur;
				for (var y = 0; y < self.canvas.height; ++y)
				{
					var b = (y * width + x) * 4;
					var p1 = Math.min(Math.max((xdb - wpdb * (1 + transition.randomTrait[y] / 10)), 0), 1);
					var p2 = 1 - p1;
					for (var c = 0; c < 3; ++c)
					{
						var i = b + c;
						out[i] = p1 * (fd[i]) + p2 * (td[i])
					}
					out[b + 3] = 255;
				}
			}

			return self.context.putImageData(transition.output, 0, 0);
		}
	});

};


	image.Transitions	= Transitions;

	var schedule =
		window.requestAnimationFrame ||
		window.webkitRequestAnimationFrame ||
		window.mozRequestAnimationFrame ||
		window.oRequestAnimationFrame ||
		window.msRequestAnimationFrame ||
			function schedule(callback, element) { window.setTimeout(callback, 1000 / 60); };

	var proxy = function (fn, me)
	{
		return function proxy()
		{
			return fn.apply(me, arguments);
		};
	};

	function isCanvasSupported()
	{
		var canvas = document.createElement("canvas");
		return !(canvas == null || canvas.getContext == null || canvas.getContext("2d") == null);
	}


	return image;
};


	atom.Prototype = Prototype;
	atom.Dispatcher = Dispatcher;
	atom.Settings = Settings;
	atom.Initializer = Initializer;
	atom.HtmlControl = HtmlControl;

	atom.type = $type;
	atom.string = $string;
	atom.array = $array;
	atom.date = $date;
	atom.error = $error;
	atom.log = $log;
	atom.css = $css;
	atom.dom = $dom;
	atom.url = $url;
	atom.cookie = $cookie;
	atom.image = $image;
	atom.xml = $xml;
	atom.const = $const;
	atom.event = $evt;
	atom.drag = $drag;
	atom.easing = $easing;

	atom.controls = new ControlRegistry;

	atom.init = new Initializer;
	atom.init.register(atom.controls, true);

	$(window)
		.on("ready", atom.init.setup)
		.on("unload", atom.init.dispose);

	return atom;
};

