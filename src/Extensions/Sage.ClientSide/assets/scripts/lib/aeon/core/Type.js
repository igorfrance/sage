/**
 * Provides utility methods for working with javascript types.
 */
function Type()
{
}

/**
 * Registers the supplied namespace.
 * @param {String} namespace The string that specifies the namespace to register, eg: Aeon.Utils.Types
 */
Type.registerNamespace = function Type$registerNamespace(namespace)
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
Type.isArray = function Type$isArray(value)
{
	if (value == null)
		return false;
	if (value.isArray)
		return true;

	return (
		(value.constructor == Array) ||
		(value != window && !Type.isString(value) && !Type.isFunction(value) && Type.isNumber(value.length))
	);
};

/**
 * Returns <c>true</c> if the specified object is a boolean.
 * @param {Object} value The value to test.
 * @return {Boolean} <c>true</c> if the specified object is a boolean, otherwise <c>false</c>.
 */
Type.isBoolean = function Type$isBoolean(value)
{
	if (value == null)
		return false;

	return (
		(typeof value == 'boolean') ||
		(value.constructor == Boolean)
	);
};

/**
 * Returns <c>true</c> if the specified object is a function.
 * @param {Object} value The value to test.
 * @return {Boolean} <c>true</c> if the specified object is a function, otherwise <c>false</c>.
 */
Type.isFunction = function Type$isFunction(value)
{
	if (value == null)
		return false;

	return (
		(typeof value == 'function') ||
		(value.constructor == Function) ||
		(!Type.isString(value) && Boolean(String(value).match(/\bfunction\b/)))
	);
};

/**
 * Returns <c>true</c> if the specified object is a null.
 * @param {Object} value The value to test.
 * @return {Boolean} <c>true</c> if the specified object is a null, otherwise <c>false</c>.
 */
Type.isNull = function Type$isNull(value)
{
	return (value == null || value == undefined);
};

/**
 * Returns <c>true</c> if the specified object is a number.
 * @param {Object} value The value to test.
 * @return {Boolean} <c>true</c> if the specified object is a number, otherwise <c>false</c>.
 */
Type.isNumber = function Type$isNumber(value)
{
	if (value == null)
		return false;

	return (
		(typeof value == 'number' && isFinite(value)) ||
		(value.constructor == Number)
	);
};

/**
 * Returns <c>true</c> if the specified object can be converted to a number.
 * @param {Object} value The value to test.
 * @return {Boolean} <c>true</c> if the specified object can be converted to a number, otherwise <c>false</c>.
 */
Type.isNumeric = function Type$isNumeric(value)
{
	return !isNaN(parseFloat(value));
};

/**
 * Returns <c>true</c> if the specified object is a date.
 * @param {Object} value The value to test.
 * @return {Boolean} <c>true</c> if the specified object is a date, otherwise <c>false</c>.
 */
Type.isDate = function Type$isDate(value)
{
	if (value == null)
		return false;
	if (value.isDate)
		return true;

	return (
		(Type.isFunction(value.getDate) && Type.isFunction(value.getTime))
	);
};

/**
 * Returns <c>true</c> if the specified object is an object.
 * @param {Object} value The value to test.
 * @return {Boolean} <c>true</c> if the specified object is an object, otherwise <c>false</c>.
 */
Type.isObject = function Type$isObject(value)
{
	if (value == null)
		return false;

	return (
		typeof value == 'object' &&
		value.constructor != String &&
		value.constructor != Number) || Type.isFunction(value); /**/
};

/**
 * Returns <c>true</c> if the specified object is a string.
 * @param {Object} value The value to test.
 * @return {Boolean} <c>true</c> if the specified object is a string, otherwise <c>false</c>.
 */
Type.isString = function Type$isString(value)
{
	if (value == null)
		return false;

	return (
		typeof value == 'string' ||
		(value != null && value.constructor == String)
	);
};

/**
 * Returns <c>true</c> if the specified object is undefined.
 * @param {Object} value The value to test.
 * @return {Boolean} <c>true</c> if the specified object is undefined, otherwise <c>false</c>.
 */
Type.isUndefined = function Type$isUndefined(value)
{
	return (typeof value == 'undefined');
};

/**
 * Returns <c>true</c> if the specified value is an HTML or XML node.
 * @param {Object} value The value to test.
 * @return {Boolean} <c>true</c> if the specified object is an XML or HTML node, otherwise <c>false</c>.
 */
Type.isNode = function Type$isNode(value)
{
	return (Type.isObject(value) && Type.isNumber(value.nodeType));
};

/**
 * Returns <c>true</c> if the specified value is an HTML or XML document.
 * @param {Object} value The value to test.
 * @return {Boolean} <c>true</c> if the specified object is an HTML or XML document, otherwise <c>false</c>.
 */
Type.isDocument = function Type$isDocument(value)
{
	return (Type.isNode(value) && value.nodeType == 9);
};

/**
 * Returns <c>true</c> if the specified value is an XML or HTML element.
 * @param {Object} value The object to test.
 * @return {Boolean} <c>true</c> if the specified object is an XML or HTML element, otherwise <c>false</c>.
 */
Type.isElement = function Type$isElement(value)
{
	return (Type.isNode(value) && value.nodeType == 1);
};

/**
 * Returns <c>true</c> if the specified object is an HTML element.
 * @param {Object} object The object to test.
 * @return {Boolean} <c>true</c> if the specified object is an HTML element, otherwise <c>false</c>.
 */
Type.isHtmlElement = function Type$isHtmlElement(object)
{
	return (Type.isElement(object) && Type.isString(object.className));
};

/**
 * Returns <c>true</c> if the specified object is an instance of the specified type.
 * @param {Object} object The object to test.
 * @param {Function} type The type to test against.
 * @return {Boolean} <c>true</c> if the specified object is an instance of the specified type, otherwise <c>false</c>.
 */
Type.instanceOf = function Type$instanceOf(object, type)
{
	if (!Type.isObject(object) || !Type.isFunction(type))
		return false;

	if (object instanceof type)
		return true;

	var result = false;
	if (object.___ && object.___.constructor == type)
		return true;

	if (object.___ && object.___.base != null)
	{
		var parent = object;
		while (parent = parent.___.base)
		{
			if (parent.___.constructor == type)
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
Type.implements = function Type$implements(object, interface)
{
	var result = true;
	if (object == null || interface == null)
		result = false;

	else
	{
		for (var name in interface)
		{
			if (!Type.isFunction(object[name]))
			{
				result = false;
				break;
			}
		}
	}
	return result;
};

