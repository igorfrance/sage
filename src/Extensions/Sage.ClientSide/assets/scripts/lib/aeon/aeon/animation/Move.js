Type.registerNamespace("aeon.animation");

/**
 * Animates the position of the specified element.
 * @param {HTMLElement} element The DOM element to animate.
 * @param {AnimationSettings} settings The settings of this animation.
 * @param {Function} callback The callback function to call when this animation completes.
 * @constructor
 */
aeon.animation.Move = function Move(element, settings, callback)
{
	this.$super(element, new $anim.MoveSettings(settings), callback);

	if (Type.isNumeric(this.settings.left))
		this.addComponentTween("left", this.settings.left);

	if (Type.isNumeric(this.settings.top))
		this.addComponentTween("top", this.settings.top);

	if (Type.isNumeric(this.settings.right))
		this.addComponentTween("right", this.settings.right);

	if (Type.isNumeric(this.settings.bottom))
		this.addComponentTween("bottom", this.settings.bottom);
};
aeon.animation.Move.inherits(aeon.animation.Animation);

aeon.animation.Move.prototype.addComponentTween = function Move$addComponentTween(side, value)
{
	this.tweens[side] =
		this.addCssTween(new $anim.AnimationSettings(this.settings, { property: side, endValue: value }));
};

/**
 * Sets the end value of this animation
 * @param {Rect} coord The end coordinates to set.
 */
aeon.animation.Move.prototype.setEndValue = function Move$setEndValue(coord)
{
	$assert.isObject(coord);

	if (Type.isNumeric(coord.left) && this.tweens.left)
		this.tweens.left.setEndValue(coord.left);

	if (Type.isNumeric(coord.right) && this.tweens.right)
		this.tweens.right.setEndValue(coord.right);

	if (Type.isNumeric(coord.top) && this.tweens.top)
		this.top.left.setEndValue(coord.top);

	if (Type.isNumeric(coord.bottom) && this.tweens.bottom)
		this.tweens.bottom.setEndValue(coord.bottom);
};

