Type.registerNamespace("aeon");

/**
 * Implements a class that helps with creating variable number of argument objects in a structured way.
 * <p>When a function accepts a large number of arguments, where each can have a default value and can also be set
 * by the function itself, using, chaning and maintaining this function and its usages can be daunting. This objects
 * helps do it in a structured way.</p>
 * <p>By using the class as the container for arguments, the function can than have only one formal argument, because
 * all parameters it operates on are contained within an instance of this class.</p>
 * @example
 * aeon.controls.SliderSettings = function SliderSettings(data, override)
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
 * var settings = new aeon.controls.SliderSettings({ minValue: 20, maxValue: 50 });
 * var slider = new aeon.controls.Slider(elem, settings);
 * @class
 */
aeon.Settings = function Settings()
{
};

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
 * @param {Object} data The object with properties in which to look for property with <c>propName</c>.
 * @param {Object} override An element with attributes or object with properties in which to look for attribute or
 * property with <c>propName</c>.
 * @param {Object} defaultValue The value to return if property or attribute with <c>propName</c> wasn't found in either
 * <c>data</c> or <c>override</c> arguments.
 * @return {Object} A value as discovered in the supplied arguments <c>data</c> or <c>override</c>, or the
 * <c>defaultValue</c> if the value has not been discovered.
 */
aeon.Settings.prototype.getValue = function Settings$getValue(propName, data, override, defaultValue)
{
	if (Type.isHtmlElement(override))
	{
		if (override.getAttribute("data-" + propName))
			return override.getAttribute("data-" + propName);
		else if (override.getAttribute(propName))
			return override.getAttribute(propName);
	}
	else if (override && override[propName] != null)
		return override[propName];

	if (data && data[propName] != null)
		return data[propName];

	return defaultValue;
};

/**
 * Looks up a value with the specified name from one of the specified sources, and returns a <c>Boolean</c>.
 * @param {String} propName The name of the value to retrieve.
 * @param {Object} data The object with properties in which to look for property with <c>propName</c>.
 * @param {Object} override An element with attributes or object with properties in which to look for attribute or
 * property with <c>propName</c>.
 * @param {Object} defaultValue The value to return if property or attribute with <c>propName</c> wasn't found in either
 * <c>data</c> or <c>override</c> arguments.
 * @return {Boolean} A value as discovered in the supplied arguments <c>data</c> or <c>override</c>, or the
 * <c>defaultValue</c> if the value has not been discovered.
 * @see getValue
 */
aeon.Settings.prototype.getBoolean = function Settings$getBoolean(propName, data, override, defaultValue)
{
	var value = String(this.getValue(propName, data, override, defaultValue)).toLowerCase();
	if (value == "1" || value == "true" || value == "yes")
		return true;

	return false;
};

/**
 * Looks up a value with the specified name from one of the specified sources, and returns a <c>Number</c>.
 * @param {String} propName The name of the value to retrieve.
 * @param {Object} data The object with properties in which to look for property with <c>propName</c>.
 * @param {Object} override An element with attributes or object with properties in which to look for attribute or
 * property with <c>propName</c>.
 * @param {Object} defaultValue The value to return if property or attribute with <c>propName</c> wasn't found in either
 * <c>data</c> or <c>override</c> arguments.
 * @return {Number} A value as discovered in the supplied arguments <c>data</c> or <c>override</c>, or the
 * <c>defaultValue</c> if the value has not been discovered.
 * @see getValue
 */
aeon.Settings.prototype.getNumber = function Settings$getNumber(propName, data, override, defaultValue)
{
	var value = this.getValue(propName, data, override, defaultValue);
	if (!Type.isNumeric(value))
		return 0;

	return parseInt(value);
};

/**
 * Looks up a value with the specified name from one of the specified sources, and returns a <c>String</c>.
 * @param {String} propName The name of the value to retrieve.
 * @param {Object} data The object with properties in which to look for property with <c>propName</c>.
 * @param {Object} override An element with attributes or object with properties in which to look for attribute or
 * property with <c>propName</c>.
 * @param {Object} defaultValue The value to return if property or attribute with <c>propName</c> wasn't found in either
 * <c>data</c> or <c>override</c> arguments.
 * @return {String} A value as discovered in the supplied arguments <c>data</c> or <c>override</c>, or the
 * <c>defaultValue</c> if the value has not been discovered.
 * @see getValue
 */
aeon.Settings.prototype.getString = function Settings$getString(propName, data, override, defaultValue)
{
	return this.getValue(propName, data, override, defaultValue);
};

/**
 * Looks up a value with the specified name from one of the specified sources, and returns a <c>Function</c>.
 * @param {String} propName The name of the value to retrieve.
 * @param {Object} data The object with properties in which to look for property with <c>propName</c>.
 * @param {Object} override An element with attributes or object with properties in which to look for attribute or
 * property with <c>propName</c>.
 * @param {Object} defaultValue The value to return if property or attribute with <c>propName</c> wasn't found in either
 * <c>data</c> or <c>override</c> arguments.
 * @return {Function} A value as discovered in the supplied arguments <c>data</c> or <c>override</c>, or the
 * <c>defaultValue</c> if the value has not been discovered.
 * @see getValue
 */
aeon.Settings.prototype.getFunction = function Settings$getFunction(propName, data, override, defaultValue)
{
	var result = this.getValue(propName, data, override, defaultValue);
	if (Type.isFunction(result))
		return result;

	return null;
};

