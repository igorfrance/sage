/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the aeon.js
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
	};

	/**
	 * Registers the supplied namespace.
	 * @param {String} namespace The string that specifies the namespace to register, eg: Aeon.Utils.Types
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

		var result = false;
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
	 * @return {Boolean} <c>true</c> if the specified object implements the specified interface, otherwise <c>false</c>.
	 */
	type.implements = function type$implements(object, interface)
	{
		var result = true;
		if (object == null || interface == null)
		{
			result = false;
		}
		else
		{
			for (var name in interface)
			{
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
				if (target[name] == undefined || overwrite)
					target[name] = source[name];
			}
		}

		return target;
	};

	return type;

};