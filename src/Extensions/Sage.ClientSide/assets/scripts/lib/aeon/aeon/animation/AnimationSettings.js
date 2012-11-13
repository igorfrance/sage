Type.registerNamespace("aeon.animation");

/**
 * Defines the settings of an animation.
 * The data object passed to the constructor will be used to initialize this settings.
 * @constructor
 * @param {Object} data The object with initial settings.
 * @param {Object} override If a settings with equivalent name is found in this object it overrides the setting from
 * the <c>data</c> object.
 */
aeon.animation.AnimationSettings = function AnimationSettings(data, override)
{
	/**
	 * The name of this animation, useful while debugging. Optional.
	 * @type {String}
	 */
	this.name = this.getString("name", data, override, String.EMPTY);

	/**
	 * The name of the property being animated (eg: 'width'). Required.
	 * @type {String}
	 */
	this.property = this.getString("property", data, override, String.EMPTY);

	/**
	 * The end value to which to animate. Required.
	 * @type {String}
	 */
	this.endValue = this.getString("endValue", data, override, String.EMPTY);

	/**
	 * The unit of the property being animated (eg: 'px'). Default is empty string.
	 * @type {String}
	 */
	this.unit = this.getString("unit", data, override, String.EMPTY);

	/**
	 * Duration of this animation (in milliseconds). Default is <c>DEFAULT_DURATION</c>.
	 * @type {Number}
	 */
	this.duration = this.getNumber("duration", data, override, aeon.animation.DEFAULT_DURATION);

	/**
	 * The function to use to calculate the value for each animation step. Default is <c>DEFAULT_EASING</c>.
	 * The function call will be:
	 * <example>var value = easingFx(t, b, c, d [, [easingParams] ]);<example>
	 * <ul><li>t: The ellapsed time, in milliseconds, since the start of this animation</li>
	 * <li>The property value in as it was at the beginning of this animation</li>
	 * <li>The difference between the property start and end values of this animation (the total amount)</li>
	 * <li>The duration (in milliseconds) of this animation</li>
	 * <li><c>[easingParams]</c> are any additional arguments that should be passed to the <c>easingFx</c></li></ul>
	 * @type {Function}
	 */
	this.easingFx = this.getFunction("easingFx", data, override, aeon.animation.DEFAULT_EASING);

	/**
	 * Any additional argument to pass to the function specified with <c>easingFx</c>. Optional.
	 * @type {Array}
	 */
	this.easingParams = this.getValue("easingParams", data, override, null);

	/**
	 * If <c>true</c>, the value being set with each animation step will be rounded first. Default is <c>true</c>.
	 * @type {Boolean}
	 */
	this.roundValues = this.getBoolean("roundValues", data, override, true);

	/**
	 * The begin value from which to start animating. Default is <c>null</c>.
	 * If <c>null</c>, this value will be calculated from the target before animation starts.
	 * @type {Number}
	 */
	this.startValue = this.getValue("startValue", data, override, null);

	/**
	 * Specifies the resolution (precision) of this animation. Default is <c>DEFAULT_RESOLUTION</c>.
	 * This the time (in milliseconds) to wait between animation steps. For example, to get 25 'fps', specify 40.
	 * Smaller values will be more precise, but that will require more CPU power.
	 * @type {Number}
	 */
	this.resolution = this.getNumber("resolution", data, override, $anim.DEFAULT_RESOLUTION);

	/**
	 * Specifes the function that gets the animated property for each animation step. Optional.
	 * @type {Function}
	 */
	this.getter = this.getFunction("getter", data, override, null);

	/**
	 * Specifes the function that sets the animated property with each animation step. Optional.
	 * @type {Function}
	 */
	this.setter = this.getFunction("setter", data, override, null);

	/**
	 * Specifes whether to set the value to the end value when the animation 'expires'. Default is <c>true</c>.
	 * @type {Boolean}
	 */
	this.setEndValue = this.getBoolean("setEndValue", data, override, true);

	for (var name in data)
			this[name] = data[name];

	for (var name in override)
			this[name] = override[name];
};

aeon.animation.AnimationSettings.inherits(aeon.Settings);

/**
 * Either an absolute value of a relative expression of the amount of change that a tween should go through between its start and end values.
 * If this value is a number, it is considered as the absolute end value of the tween. If the value is a <c>String</c> and
 * is expressed with the following format: <code>[+/-]<numeric_value>[%]</code> than it is considered a relative expression
 * and will be calculated at the start of each run of the tween. Therefore, using such an expression, it is possible to specify a
 * value relative to the current value of the object, without having to use a specific value, such as a coordinate or dimension.
 * @example +200
 * @example -360
 * @example +50%
 * @example -20%
 * @type {Object}
 */
aeon.animation.TweenExpression;
