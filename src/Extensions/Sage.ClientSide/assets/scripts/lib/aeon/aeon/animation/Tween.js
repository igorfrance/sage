Type.registerNamespace("aeon.animation");

/**
 * Tweens an object's property
 * @constructor
 * @param {Object} element The element object whose property should be tweened.
 * @param {Animation} animation The parent animation that this tween is a part of.
 * @param {AnimationSettings} settings The settings of this tween.
 * @param {Boolean} isCssTween Indicates that this is a css tween.
 */
aeon.animation.Tween = function Tween(element, settings, animation, isCssTween)
{
	$assert.isNotNull(element);
	$assert.isNotNull(settings);

	this.$super("onstart", "onprogress", "oncomplete");

	this.element = element;
	this.$element = animation.$element;
	this.animation = animation;
	this.settings = new $anim.AnimationSettings(settings);

	this.isPaused = false;
	this.timeStart = null;

	this.$get = settings.getter ||
		(isCssTween ? this.$getCssProperty : this.$getProperty);

	this.$set = settings.setter ||
		(isCssTween ? this.$setCssProperty : this.$setProperty);
};
aeon.animation.Tween.inherits(aeon.Dispatcher);

/**
 * Assigns the value/property getting function for this tween
 * @param {Function} delegate The function that will be gsetting the value of this tween
 */
aeon.animation.Tween.prototype.setGetter = function Tween$setGetter(delegate)
{
	this.$get = delegate;
};

/**
 * Assigns the value/property setting function for this tween
 * @param {Function} delegate The function that will be setting the value of this tween
 */
aeon.animation.Tween.prototype.setSetter = function Tween$setSetter(delegate)
{
	this.$set = delegate;
};

/**
 * Returns the current property value of this tween's element.
 * @return {Number} The current property value of this tweens element.
 */
aeon.animation.Tween.prototype.$getProperty = function Tween$$getProperty()
{
	return parseInt(this.element[this.settings.property]);
};

/**
 * Returns the current css property value of this tweens element.
 * @return {Number} The current css property value of this tweens element.
 */
aeon.animation.Tween.prototype.$getCssProperty = function Tween$$getCssProperty()
{
	var value = this.$element.css(this.settings.property);
	var number = parseFloat(String(value).replace(/[^0-9.\-]/g, String.EMPTY));

	return isNaN(number) ? 0 : number;
};

/**
 * Sets the property of this tweens element.
 * @param {Number} value The value to set.
 */
aeon.animation.Tween.prototype.$setProperty = function Tween$$setProperty(value)
{
	this.element[this.settings.property] = value + (this.settings.unit || String.EMPTY);
};

/**
 * Sets the css property of this tweens element.
 * @param {Number} value The value to set.
 */
aeon.animation.Tween.prototype.$setCssProperty = function Tween$$setCssProperty(value)
{
	this.$element.css(this.settings.property, value + (this.settings.unit || String.EMPTY));
};

aeon.animation.Tween.prototype.getStartValue = function Tween$getStartValue()
{
	var startValue = this.getValue();
	return startValue;
};

aeon.animation.Tween.prototype.setEndValue = function Tween$setEndValue(value)
{
	this.settings.endValue = value;
};

aeon.animation.Tween.prototype.getEndValue = function Tween$getEndValue()
{
	var percent = false;
	var increase = false;
	var decrease = false;
	var endValue = this.settings.endValue;
	var calcValue = 0;

	if (Type.isString(endValue) && endValue.match(/^(?:(\+|\-))?(-?[\d\.]+)(?:(%))?$/))
	{
		if (RegExp.$1 == "+")
			increase = true;
		if (RegExp.$1 == "-")
			decrease = true;
		if (RegExp.$3 == "%")
			percent = true;

		calcValue = parseFloat(RegExp.$2);

		var current = this.getValue();

		if (percent)
			calcValue *= (current / 100);

		if (increase)
			endValue = current + calcValue;
		else if (decrease)
			endValue = current - calcValue;
		else
			endValue = calcValue;
	}
	else
	{
		var testValue = parseFloat(endValue);
		if (isNaN(testValue))
		{
			$log.warn("The tween {0}->{{1}} endValue '{2}' was not recognized as a valid number, defaulting to 0.",
				this.animation.getName(), this.toString(), endValue);

			endValue = 0;
		}
		else
			endValue = testValue;
	}

	return endValue;
};

/**
 * Returns the value of the call to getter.
 * @return {Number} The current property value.
 */
aeon.animation.Tween.prototype.getValue = function Tween$getValue()
{
	return this.$get();
};

/**
 * Calls setter.
 * @param {Number} value The value to set.
 */
aeon.animation.Tween.prototype.setValue = function Tween$setValue(value)
{
	this.$set(value);
};

/**
 * Resets the tween's value to the last saved start value.
 */
aeon.animation.Tween.prototype.reset = function Tween$reset()
{
	if (this.startValue != null)
	{
		this.setValue(this.startValue);
	}
};

/**
 * Runs the tween
 */
aeon.animation.Tween.prototype.run = function Tween$run()
{
	var settings = this.settings;
	this.timeStart = new Date().getTime();

	this.startValue = this.getStartValue();
	this.endValue = this.getEndValue();

	if (!isNaN(this.startValue))
	{
		this.intervalCallback = Function.createDelegate(this, this.step); /**/
		this.interval = Function.setInterval(settings.resolution, this.intervalCallback); /**/
	}
	else
	{
		$log.warn("The tween {0}->{{1}} won't start because the startValue '{2}' was not recognized as a valid number",
			this.animation.getName(), this.toString(), this.startValue);

		this.complete();
	}
};

/**
 * Stops the tween
 */
aeon.animation.Tween.prototype.stop = function Tween$stop()
{
	if (this.interval)
		window.clearInterval(this.interval);

	this.interval = null;
};

/**
 * Called continuously until the tween is complete.
 */
aeon.animation.Tween.prototype.step = function Tween$step()
{
	var settings = this.settings;
	var timeElapsed = new Date().getTime() - this.timeStart;
	var easingParams = [timeElapsed, this.startValue, this.endValue - this.startValue, settings.duration];

	if (settings.easingParams)
		for (var i = 0; i < settings.easingParams.length; i++)
			easingParams.push(settings.easingParams[i]);

	this.value = settings.easingFx.apply(this, easingParams);
	if (settings.roundValues)
		this.value = Math.round(this.value);

	this.setValue(this.value);
	this.value = this.getValue();

	this.progress();
	if (this.value == this.endValue || timeElapsed >= settings.duration)
	{
		this.complete(true);
	}
};

/**
 * Completes the tween and fires the <c>oncomplete</c> event.
 * @param {Boolean} setEndValue If <c>true</c>, the target property will be set to the tween's end value.
 * @private
 */
aeon.animation.Tween.prototype.complete = function Tween$complete(setEndValue)
{
	this.stop();

	var settings = this.settings;
	if (settings.setEndValue)
	{
		this.setValue(this.endValue);
		this.value = this.endValue;
		this.progress();
	}

	this.fireEvent("oncomplete", { tween: this, value: this.value });
	this.intervalCallback = null;
};

/**
 * Fires the <c>oncomplete</c> event.
 * @private
 */
aeon.animation.Tween.prototype.progress = function Tween$progress()
{
	this.fireEvent("onprogress", { tween: this, value: this.value });
};

aeon.animation.Tween.prototype.toString = function Tween$toString()
{
	var endValue = this.settings.endValue;
	var property = this.settings.property;

	if (endValue != null && property != null)
		return String.format("{0}: {1}", property, endValue);

	else if (endValue != null)
		return endValue;

	return "<tween>";
};
