Type.registerNamespace("aeon.animation");

aeon.animation.CssSettings = function CssSettings(data, override)
{
	this.$super(data, override);

	this.css = {};
	this.unit = "px";

	for (var prop in data)
		this.css[prop] = this.getString(prop, data, override, String.EMPTY);
}

aeon.animation.CssSettings.inherits(aeon.animation.AnimationSettings);
