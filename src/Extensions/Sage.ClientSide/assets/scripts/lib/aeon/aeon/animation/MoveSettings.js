Type.registerNamespace("aeon.animation");

/**
 * Defines the settings of the Move.
 * @constructor
 * @param {Object} data The object with initial settings.
 * @param {Object} override If a settings with equivalent name is found in this object it overrides the setting from
 * the <c>data</c> object.
 */
aeon.animation.MoveSettings = function MoveSettings(data, override)
{
	this.$super(data, override);

	this.left = this.getString("left", data, override, String.EMPTY);
	this.right = this.getString("right", data, override, String.EMPTY);
	this.top = this.getString("top", data, override, String.EMPTY);
	this.bottom = this.getString("bottom", data, override, String.EMPTY);

	this.unit = "px";
};
aeon.animation.MoveSettings.inherits(aeon.animation.AnimationSettings);

