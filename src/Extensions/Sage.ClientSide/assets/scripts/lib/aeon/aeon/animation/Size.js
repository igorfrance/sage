Type.registerNamespace("aeon.animation");

/**
 * Animates the dimensions of the specified element.
 * @param {HTMLElement} element The DOM element to animate.
 * @param {AnimationSettings} settings The settings of this animation.
 * @param {Function} callback The callback function to call when this animation completes.
 * @constructor
 */
aeon.animation.Size = function Size(element, settings, callback)
{
	this.$super(element, new $anim.SizeSettings(settings), callback);

	if (this.settings.width)
		this.tweens.width =
			this.addCssTween(new $anim.AnimationSettings(this.settings, { property: "width", endValue: this.settings.width }));

	if (this.settings.height)
		this.tweens.height =
			this.addCssTween(new $anim.AnimationSettings(this.settings, { property: "height", endValue: this.settings.height }));
}
aeon.animation.Size.inherits(aeon.animation.Animation);
