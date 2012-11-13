Type.registerNamespace("aeon.animation");

/**
 * Animates an elements's css alpha.
 * @param {HTMLElement} element The DOM element to animate.
 * @param {AnimationSettings} settings The settings of this animation.
 * @param {Function} callback The callback function to call when this animation completes.
 * @constructor
 */
aeon.animation.Alpha = function Alpha(element, settings, callback)
{
	this.$super(element, new $anim.AlphaSettings(settings), callback);

	this.tweens.alpha =
		this.addTween(new $anim.AnimationSettings(this.settings, { getter: this.getAlpha, setter: this.setAlpha, property: "opacity" }));
};
aeon.animation.Alpha.inherits(aeon.animation.Animation);

aeon.animation.Alpha.prototype.getAlpha = function Alpha$getAlpha()
{
	return this.$element.css("opacity");
};

aeon.animation.Alpha.prototype.setAlpha = function Alpha$setAlpha(value)
{
	return this.$element.css("opacity", value);
};

