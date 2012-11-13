Type.registerNamespace("aeon.animation");

/**
 * Animates the specified color of the supplied element.
 * @param {HTMLElement} element The DOM element to animate.
 * @param {ColorSettings} settings The settings of this animation.
 * @param {Function} callback The callback function to call when this animation completes.
 * @constructor
 */
aeon.animation.Color = function Color(element, settings, callback)
{
	this.$super(element, new $anim.ColorSettings(settings), callback);

	if (this.settings.background)
		this.addColorTween("background");

	if (this.settings.foreground)
		this.addColorTween("foreground");
}
aeon.animation.Color.inherits(aeon.animation.Animation);

aeon.animation.Color.prototype.addColorTween = function Color$addColorTween(color)
{
	$assert.isString(color);

	var colorName = color == "foreground" ? "color" : "backgroundColor";
	var colorValue = $anim.Rgb.parse(this.settings[color]);

	this.addComponentTween(colorName, "r", colorValue.r);
	this.addComponentTween(colorName, "g", colorValue.g);
	this.addComponentTween(colorName, "b", colorValue.b);

	this.setColor(colorName, this.getColor(colorName));
}

aeon.animation.Color.prototype.addComponentTween = function Color$addComponentTween(colorName, componentName, componentValue)
{
	var self = this;
	var getter = Function.createDelegate(this, this.getColorComponent, [colorName, componentName]); /**/
	var setter = Function.createDelegate(this, this.setColorComponent, [colorName, componentName]); /**/

	var settings = new $anim.AnimationSettings(this.settings,
		{ property: colorName, component: componentName, endValue: componentValue, getter: getter,  setter: setter });

	this.addTween(settings);
}

aeon.animation.Color.prototype.getColor = function Color$getColor(color)
{
	var value = this.$element.css(color);
	if (value == null)
		return new aeon.animation.Rgb(255, 255, 255);

	if (value.match(/auto|transparent/))
	{
		var parent = element;
		while (parent = parent.parentNode)
		{
			value = $(parent).css(color);
			if (!value.match(/auto|transparent/))
				break;
		}
	}

	return aeon.animation.Rgb.parse(value);
}

aeon.animation.Color.prototype.setColor = function Color$setColor(color, value)
{
	this.$element.css(color, value.toString());
}

aeon.animation.Color.prototype.getColorComponent = function Color$getColorComponent(colorName, component)
{
	var color = this.getColor(colorName);
	return color[component];
}

aeon.animation.Color.prototype.setColorComponent = function Color$setColorComponent(colorName, component, value)
{
	var color = this.getColor(colorName);
	color[component] = value;

	this.setColor(colorName, color);
}

