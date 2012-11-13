Type.registerNamespace("aeon.utils");

aeon.utils.Assert = new function Assert()
{
};

/**
 * Checks that value is <code>true</code>.
 * @param {Object} value The value to check.
 * @param {String} description Optional description of this assertion.
 * @return {Boolean} The result of assertion.
 */
aeon.utils.Assert.isTrue = function Assert$isTrue(value, description)
{
	var result = (value == true);
	if (result == false)
		aeon.utils.Assert.raiseError(description || ("Value should be true."));

	return result;
};

/**
 * Checks that value is <code>false</code>.
 * @param {Object} value The value to check.
 * @param {String} description Optional description of this assertion.
 * @return {Boolean} The result of assertion.
 */
aeon.utils.Assert.isFalse = function Assert$isFalse(value, description)
{
	var result = (value == false);
	if (result == false)
		aeon.utils.Assert.raiseError(description || ("Value should be false."));

	return result;
};

/**
 * Checks that value is defined.
 * @param {Object} value The value to check.
 * @param {String} description Optional description of this assertion.
 * @return {Boolean} The result of assertion.
 */
aeon.utils.Assert.isDefined = function Assert$isDefined(value, description)
{
	var result = (typeof(value) != 'undefined');
	if (result == false)
		aeon.utils.Assert.raiseError(description || ("Value should be defined."));

	return result;
};

/**
 * Checks that value is <code>null</code>.
 * @param {Object} value The value to check.
 * @param {String} description Optional description of this assertion.
 * @return {Boolean} The result of assertion.
 */
aeon.utils.Assert.isNull = function Assert$isNull(value, description)
{
	var result = (value == null);
	if (result == false)
		aeon.utils.Assert.raiseError(description || ("Value should be null."));

	return result;
};

/**
 * Checks that value is not <code>null</code>.
 * @param {Object} value The value to check.
 * @param {String} description Optional description of this assertion.
 * @return {Boolean} The result of assertion.
 */
aeon.utils.Assert.isNotNull = function Assert$isNotNull(value, description)
{
	var result = (value != null);
	if (result == false)
		aeon.utils.Assert.raiseError(description || ("Value should not be null."));

	return result;
};

/**
 * Checks that value1 and value2 are equal.
 * @param {Object} value1 First value to check.
 * @param {Object} value2 Second value to check.
 * @param {String} description Optional description of this assertion.
 * @return {Boolean} The result of assertion.
 */
aeon.utils.Assert.areEqual = function Assert$areEqual(value1, value2, description)
{
	var result = (value1 == value2);
	if (result == false)
		aeon.utils.Assert.raiseError(description || ("Value should be equal."), value1, value2);

	return result;
};

/**
 * Checks that value1 and value2 are not equal.
 * @param {Object} value1 First value to check.
 * @param {Object} value2 Second value to check.
 * @param {String} description Optional description of this assertion.
 * @return {Boolean} The result of assertion.
 */
aeon.utils.Assert.areNotEqual = function Assert$areNotEqual(value1, value2, description)
{
	var result = (value1 != value2);
	if (result == false)
		aeon.utils.Assert.raiseError(description || ("Value should not be equal."), value1, value2);

	return result;
};

/**
 * Checks that value is a string.
 * @param {Object} value A value to check.
 * @param {String} description Optional description of this assertion.
 * @return {Boolean} The result of assertion.
 */
aeon.utils.Assert.isString = function Assert$isString(value, description)
{
	var result = Type.isString(value);
	if (result == false)
		aeon.utils.Assert.raiseError(description || ("Value should be a string."));

	return result;
};

/**
 * Checks that value is a number.
 * @param {Object} value A value to check.
 * @param {String} description Optional description of this assertion.
 * @return {Boolean} The result of assertion.
 */
aeon.utils.Assert.isNumber = function Assert$isNumber(value, description)
{
	var result = Type.isNumber(value);
	if (result == false)
		aeon.utils.Assert.raiseError(description || ("Value should be a number."));

	return result;
};

/**
 * Checks that value is a object.
 * @param {Object} value A value to check.
 * @param {String} description Optional description of this assertion.
 * @return {Boolean} The result of assertion.
 */
aeon.utils.Assert.isObject = function Assert$isObject(value, description)
{
	var result = Type.isObject(value);
	if (result == false)
		aeon.utils.Assert.raiseError(description || ("Value should be an object."));

	return result;
};

/**
 * Asserts that the value is an HTML element or XML node.
 * @param {Object} value The value to check.
 * @param {String} description Optional description of this assertion.
 * @return {Boolean} The result of assertion.
 */
aeon.utils.Assert.isNode = function Assert$isNode(value, description)
{
	var result = Type.isNode(value);
	if (result == false)
		aeon.utils.Assert.raiseError(description || ("Value should be a node."));

	return result;
};

/**
 * Asserts that the value is an HTML or XML element.
 * @param {Object} value The value to check.
 * @param {String} description Optional description of this assertion.
 * @return {Boolean} The result of assertion.
 */
aeon.utils.Assert.isElement = function Assert$isElement(value, description)
{
	var result = Type.isElement(value);
	if (result == false)
	{
		aeon.utils.Assert.raiseError(description || ("Value should be an element."));
	}
	return result;
};

/**
 * Asserts that the value is an HTML element.
 * @param {Object} value The value to check.
 * @param {String} description Optional description of this assertion.
 * @return {Boolean} The result of assertion.
 */
aeon.utils.Assert.isHtmlElement = function Assert$isHtmlElement(value, description)
{
	var result = Type.isHtmlElement(value);
	if (result == false)
		aeon.utils.Assert.raiseError(description || ("Value should be an html element."));

	return result;
};

/**
 * Asserts that the value is an XML document.
 * @param {Object} value The value to check.
 * @param {String} description Optional description of this assertion.
 * @return {Boolean} The result of assertion.
 */
aeon.utils.Assert.isDocument = function Assert$isDocument(value, description)
{
	var result = Type.isDocument(value);
	if (result == false)
		aeon.utils.Assert.raiseError(description || ("Value should be an xml document."));

	return result;
};

/**
 * Checks that value is a array.
 * @param {Object} value A value to check.
 * @param {String} description Optional description of this assertion.
 * @return {Boolean} The result of assertion.
 */
aeon.utils.Assert.isArray = function Assert$isArray(value, description)
{
	var result = Type.isArray(value);
	if (result == false)
		aeon.utils.Assert.raiseError(description || ("Value should be an array."));

	return result;
};

/**
 * Checks that value is a function.
 * @param {Object} value A value to check.
 * @param {String} description Optional description of this assertion.
 * @return {Boolean} The result of assertion.
 */
aeon.utils.Assert.isFunction = function Assert$isFunction(value, description)
{
	var result = Type.isFunction(value);
	if (result == false)
		aeon.utils.Assert.raiseError(description || ("Value should be a function."));

	return result;
};

/**
 * Checks that the specified object implements the specified interface.
 * @param {Object} object The object to check.
 * @param {Object} interface The interface to check against.
 * @param {String} description Optional description of this assertion.
 * @return {Boolean} The result of assertion.
 */
aeon.utils.Assert.implements = function Assert$implements(object, interface, description)
{
	var result = Type.implements(object, interface);
	if (result == false)
		aeon.utils.Assert.raiseError(description || ("Value should implement the specified interface."));

	return result;
};

/**
 * Checks that an object is instance of a class.
 * @param {Object} object An object to check.
 * @param {Object} objectClass The class that the object should be an instance of
 * @param {String} description Optional description of this assertion.
 * @return {Boolean} The result of assertion.
 */
aeon.utils.Assert.instanceOf = function Assert$instanceOf(object, objectClass, description)
{
	var result = (object instanceof objectClass);
	if (result == false)
		aeon.utils.Assert.raiseError(description || ("Object should be an instance of " + objectClass.getName()), object, objectClass);

	return result;
};

aeon.utils.Assert.raiseError = function Assert$raiseError(errorText)
{
	$log.error(errorText);
	throw new Error(errorText);
};

/**
 * Global alias to <c>aeon.utils.Assert</c>
 * @type {aeon.utils.Assert}
 */
var $assert = aeon.utils.Assert;
