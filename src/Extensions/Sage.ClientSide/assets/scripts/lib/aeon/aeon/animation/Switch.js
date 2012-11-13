Type.registerNamespace("aeon.animation");

/**
 * Represents a simple switch to some value.
 * @constructor
 * @param {Object} object The object whose value to change.
 * @param {String} property The object's property to change.
 * @param {Object} value The value to change to.
 * @param {Function} callback The callback to call when state changes. Optional.
 */
aeon.animation.Switch = function Switch(object, settings)
{
	$assert.isObject(object);
	$assert.isObject(settings);

	this.$super("oncomplete");

	this.object = object;
	this.settings = new $anim.SwitchSettings(settings);

	if (this.settings.isStyleProperty && this.object.style == null)
		throw new Error(
			"This Switch animation's settings indicate that the object's style property " +
			"should be manipulated, but the object doesn't have the style property");

	this.originalValue = this.getValue();
};
aeon.animation.Switch.inherits(aeon.Dispatcher);

/**
 * Runs this animation.
 * @param {Function} callback A callback function to call when the animation completes. Optional.
 */
aeon.animation.Switch.prototype.run = function Switch$run(callback)
{
	this.setValue(this.settings.value);
	this.fireEvent("oncomplete", { object: this.object, value: this.settings.value });
	if (Type.isFunction(callback))
		callback(this);
};

/**
 * Resets this animation object's property value to its original value.
 */
aeon.animation.Switch.prototype.reset = function Switch$reset()
{
	this.setValue(this.originalValue);
};

/**
 * Stops the animation (not applicable for <c>Switch</c>).
 */
aeon.animation.Switch.prototype.stop = function Switch$stop()
{
};

/**
 * Gets the name of this <c>Switch</c> instance.
 */
aeon.animation.Switch.prototype.getName = function Switch$getName()
{
	return this.settings.name || "Switch";
};

/**
 * Gets this animation object's current property value.
 * @return {Object} The current value of this animation object's property.
 */
aeon.animation.Switch.prototype.getValue = function Switch$getValue()
{
	if (this.settings.isStyleProperty)
		return this.object.style[this.settings.property];

	return this.object[this.settings.property];
};

/**
 * Sets this animation object's property to the specified value.
 * @param {Object} value The value t set.
 */
aeon.animation.Switch.prototype.setValue = function Switch$setValue(value)
{
	if (this.settings.isStyleProperty)
		this.object.style[this.settings.property] = value;
	else
		this.object[this.settings.property] = value;
};

/**
 * Returns the string representation of this object.
 */
aeon.animation.Switch.prototype.toString = function Switch$toString()
{
	var result = [];
	result.push(this.getName());

	var propName = this.settings.isStyleProperty
		? "style." + this.settings.property
		: this.settings.property;

	result.push(String.format("({0}: {1})", propName, this.settings.value));
	return result.join(String.EMPTY);
};
