Type.registerNamespace("aeon.animation");

aeon.animation.SizeSettings = function SizeSettings(data, override)
{
	this.$super(data, override);

	this.width = this.getString("width", data, override, String.EMPTY);
	this.height = this.getString("height", data, override, String.EMPTY);

	this.unit = "px";
}
aeon.animation.SizeSettings.inherits(aeon.animation.AnimationSettings);

