Type.registerNamespace("aeon.animation");

aeon.animation.ScrollSettings = function ScrollSettings(data, override)
{
	this.$super(data, override);

	this.left = this.getString("left", data, override, String.EMPTY);

	this.top = this.getString("top", data, override, String.EMPTY);
}
aeon.animation.ScrollSettings.inherits(aeon.animation.AnimationSettings);
