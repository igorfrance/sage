Type.registerNamespace("aeon.animation");

/**
 * Defines the settings of the <c>Circle</c> animation.
 * @constructor
 * @param {Object} data The object with initial settings.
 * @param {Object} override The object with overriding settings. If a setting exist both in <c>data</c> and in <c>override</c>
 * objects, the setting from <c>override</c> takes precedence.
 */
aeon.animation.CircleSettings = function CircleSettings(data, override)
{
	this.$super(data, override);

	/**
	 * The start radius of the animation. Default is 100.
	 * @type {Number}
	 */
	this.radius = this.getNumber("radius", data, override, 100);

	/**
	 * The amount by which to stretch the circle's radius. Optional.
	 * @type {TweenExpression}
	 */
	this.stretch = this.getString("stretch", data, override, String.EMPTY);

	/**
	 * The start degree of the animation. Default is 90.
	 * @type {Number}
	 */
	this.degree = this.getNumber("degree", data, override, 90);

	/**
	 * The number of degrees by which to turn the animated element.
	 * @type {TweenExpression}
	 */
	this.turn = this.getValue("turn", data, override, "+180");
}
aeon.animation.CircleSettings.inherits(aeon.animation.AnimationSettings);

