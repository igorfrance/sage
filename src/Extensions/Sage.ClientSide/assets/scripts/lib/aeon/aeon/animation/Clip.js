Type.registerNamespace("aeon.animation");

/**
 * Animates the clip rectangle of the specified element.
 * @param {HTMLElement} element The DOM element to animate.
 * @param {AnimationSettings} settings The settings of this animation.
 * @param {Function} callback The callback function to call when this animation completes.
 * @constructor
 */
aeon.animation.Clip = function Clip(element, settings, callback)
{
	this.$super(element, new $anim.ClipSettings(settings, null, element), callback);

	if (this.settings.top !== String.EMPTY)
		this.addComponentTween("top", this.settings.top);

	if (this.settings.right !== String.EMPTY)
		this.addComponentTween("right", this.settings.right);

	if (this.settings.bottom !== String.EMPTY)
		this.addComponentTween("bottom", this.settings.bottom);

	if (this.settings.left !== String.EMPTY)
		this.addComponentTween("left", this.settings.left);

	var currentClip = this.$element.css("clip");
	this.setClip($anim.ClipRect.parse(currentClip, this.element));
}
aeon.animation.Clip.inherits(aeon.animation.Animation);

aeon.animation.Clip.prototype.addComponentTween = function Clip$addComponentTween(componentName, componentValue)
{
	var getter = Function.createDelegate(this, this.getClipComponent, [componentName]); /**/
	var setter = Function.createDelegate(this, this.setClipComponent, [componentName]); /**/

	var settings = new $anim.AnimationSettings(this.settings,
		{ component: componentName, endValue: componentValue, getter: getter,  setter: setter });

	this.addTween(settings);
}

/**
 * Sets the clip rectangle of the specified element to the specified value.
 * @param {ClipRect} rect The clip rectangle value to set.
 */
aeon.animation.Clip.prototype.setClip = function Clip$setClip(rect)
{
	this.$element.css("clip", rect.toString());
}

/**
 * Gets the clip rectangle of the specified element.
 * @returns {ClipRect} The clip rectangle of this animation's element.
 */
aeon.animation.Clip.prototype.getClip = function Clip$getClip()
{
	var clip = this.$element.css("clip");
	return $anim.ClipRect.parse(clip, this.element);
}

aeon.animation.Clip.prototype.getClipComponent = function Clip$getClipComponent(component)
{
	var clip = this.getClip();
	return clip[component];
}

aeon.animation.Clip.prototype.setClipComponent = function Clip$setClipComponent(component, value)
{
	var clip = this.getClip();
	clip[component] = value;

	this.setClip(clip);

	console.log(clip.toString());
}

