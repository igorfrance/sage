Type.registerNamespace("aeon.animation");

/**
 * Defines the settings of a color animation.
 * @constructor
 * @param {Object} data The object with initial settings.
 * @param {Object} override If a settings with equivalent name is found in this object it overrides the setting from
 * the <c>data</c> object.
 */
aeon.animation.ColorSettings = function ColorSettings(data, override)
{
	this.$super(data, override);

	/**
	 * The background color to animate to.
	 * @type {String}
	 */
	this.background = this.getString("background", data, override, String.EMPTY);
	/**
	 * The foreground color to animate to.
	 * @type {String}
	 */
	this.foreground = this.getString("foreground", data, override, String.EMPTY);

	this.resolution = 50;
}
aeon.animation.ColorSettings.inherits(aeon.animation.AnimationSettings);

