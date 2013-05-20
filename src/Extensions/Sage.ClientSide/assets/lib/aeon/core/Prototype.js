/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the aeon.js
 */

/**
 * Provides base type access to function prototypes.
 * @class
 */
function Prototype()
{
};

/**
 * Extends the specified <c>object</c>'s prototype with this function's prototype.
 * This function will also be copied as a static function of the specified <c>object</c>,
 * making this object capable of extending other objects in the same way.
 * @param {Object} object The Object or Function instance to extend.
 * @return {Object} The specified <c>object</c>.
 */
Prototype.extend = function Prototype$extend(object)
{
	if (object == null || object == this || object.prototype == null)
		return;

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
Prototype.prototype.construct = function Prototype$construct()
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
Prototype.prototype.base = function Prototype$base(name)
{
	if (this.__type == null)
		return;

	var fx = null;
	var current = this;

	while (current.__type && !fx)
	{
		fx = current.__type.base[name];
		current = current.__type.base;
	}

	if (typeof(fx) == "function")
		return fx.apply(this, Array.prototype.splice(arguments, 1));
};

