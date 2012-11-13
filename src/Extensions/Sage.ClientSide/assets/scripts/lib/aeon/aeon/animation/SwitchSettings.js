Type.registerNamespace("aeon.animation");

/**
 * Defines the settings of the <c>Switch</c>.
 * @constructor
 * @param {Object} data The object with initial settings.
 * @param {Object} override If a settings with equivalent name is found in this object it overrides the setting from
 * the <c>data</c> object.
 */
aeon.animation.SwitchSettings = function SwitchSettings(data, override)
{
	this.$super(data, override);

	this.property = this.getString("property", data, override, null);
	this.value = this.getValue("value", data, override, null);
	this.isStyleProperty = this.getBoolean("isStyleProperty", data, override, true);
};
aeon.animation.SwitchSettings.inherits(aeon.animation.AnimationSettings);

