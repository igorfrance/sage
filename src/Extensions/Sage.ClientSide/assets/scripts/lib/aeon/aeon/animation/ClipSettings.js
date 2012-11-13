Type.registerNamespace("aeon.animation");

/**
 * Defines the settings of a clip animation.
 * @constructor
 * @param {Object} data The object with initial settings.
 * @param {Object} override If a settings with equivalent name is found in this object it overrides the setting from
 * the <c>data</c> object.
 * @param {HTMLElement} element The element this animation applies to.
 */
aeon.animation.ClipSettings = function ClipSettings(data, override, element)
{
	this.$super(data, override);

	this.left = this.getString("left", data, override, String.EMPTY);
	this.right = this.getString("right", data, override, String.EMPTY);
	this.top = this.getString("top", data, override, String.EMPTY);
	this.bottom = this.getString("bottom", data, override, String.EMPTY);

	this.clip = this.getString("clip", data, override, String.EMPTY);

	if (this.clip)
	{
		var rect = $anim.ClipRect.parse(this.clip, element);
		this.left = rect.left;
		this.right = rect.right;
		this.top = rect.top;
		this.bottom = rect.bottom;
	}

	this.property = "clip";
}
aeon.animation.ClipSettings.inherits(aeon.animation.AnimationSettings);

