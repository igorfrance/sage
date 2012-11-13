Type.registerNamespace("aeon.animation");

aeon.animation.Css = function Css(element, settings, callback)
{
	this.$super(element, new $anim.CssSettings(settings), callback);

	for (var property in this.settings.css)
	{
		this.tweens[property] =
			this.addCssTween(new aeon.animation.AnimationSettings(this.settings, { property: property, endValue: this.settings.css[property] }));
	}
};
aeon.animation.Css.inherits(aeon.animation.Animation);
