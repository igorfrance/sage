Type.registerNamespace("aeon.animation");

/**
 * Defines the settings of the <c>Alpha</c>.
 * @constructor
 * @param {Object} data The object with initial settings.
 * @param {Object} override If a settings with equivalent name is found in this object it overrides the setting from
 * the <c>data</c> object.
 */
aeon.animation.AlphaSettings = function AlphaSettings(data, override)
{
	this.$super(data, override);

	this.alpha = this.getString("alpha", data, override, "50");

	this.endValue = this.alpha;
}
aeon.animation.AlphaSettings.inherits(aeon.animation.AnimationSettings);

