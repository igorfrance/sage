Type.registerNamespace("aeon.animation");

/**
 * Animates an element's scrollLeft and scrollTop properties.
 * @param {HTMLElement} element The DOM element to animate.
 * @param {AnimationSettings} settings The settings of this animation.
 * @param {Function} callback The callback function to call when this animation completes.
 * @constructor
 */
aeon.animation.Scroll = function Scroll(element, settings, callback)
{
	this.$super(element, new $anim.ScrollSettings(settings), callback);

	if (this.settings.left)
		this.tweens.left =
			this.addTween(new $anim.AnimationSettings(this.settings, { property: "scrollLeft", endValue: this.settings.left }));

	if (this.settings.top)
		this.tweens.top =
			this.addTween(new $anim.AnimationSettings(this.settings, { property: "scrollTop", endValue: this.settings.top }));
}
aeon.animation.Scroll.inherits(aeon.animation.Animation);

