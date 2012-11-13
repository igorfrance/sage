Type.registerNamespace("aeon.animation");

/**
 * Combines multiple tweens of an element into an animation.
 * @param {HTMLElement} element The DOM element to animate.
 * @param {AnimationSettings} settings The settings of this animation.
 * @param {Function} callback The callback function to call when this animation completes.
 * @abstract
 * @constructor
 */
aeon.animation.Animation = function Animation(element, settings, callback)
{
	$assert.isHtmlElement(element);
	$assert.isObject(settings);

	this.$super("onstart", "onprogress", "oncomplete");

	this.tweens = new Array;
	this.tweensComplete = 0;
	this.element = element;
	this.$element = $(element);
	this.settings = settings;
	this.sequence = null;
	this.running = false;

	/**
	 * One-time-only callback function to call when the animation completes.
	 * @type {Function}
	 */
	this.afterRunCallback = null;

	this.startValues = [];

	if (Type.isFunction(callback))
		this.addListener("oncomplete", callback);
};
aeon.animation.Animation.inherits(aeon.Dispatcher);

/**
 * Specifies the animation interface.
 * @type {Object}
 */
aeon.animation.Animation.interface = { run: 1, stop: 1, reset: 1, getName: 1 };

/**
 * Sets the end value of all tweens in this animation
 * @param {Number} value The end value to set.
 */
aeon.animation.Animation.prototype.setEndValue = function Animation$setEndValue(value)
{
	for (var i = 0; i < this.tweens.length; i++)
		this.tweens[i].setEndValue(value);
};

/**
 * Create an object with information about the raised event.
 * @return {Object} Object with information about the raised event.
 */
aeon.animation.Animation.prototype.createEventData = function Animation$createEventData()
{
	return { element: this.element };
};

/**
 * Returns true if the current sequence contains the specified animation.
 * @param {String} animation The animation to check for.
 * @return {Boolean} <c>true></c> if this sequence contains the supplied animation.
 */
aeon.animation.Animation.prototype.containsTween = function Animation$containsAnimation(tween)
{
	for (var i = 0; i < this.tweens.length; i++)
	{
		if (this.tweens[i] == tween)
			return true;
	}

	return false;
};

aeon.animation.Animation.prototype.getTween = function Animation$getTween(index)
{
	return this.tweens[index];
};

aeon.animation.Animation.prototype.getTweenIndex = function Animation$getTweenIndex(tween)
{
	for (var i = 0; i < this.tweens.length; i++)
	{
		if (this.tweens[i] == tween)
			return i;
	}

	return -1;
};

/**
 * Creates and adds a tween to this animation
 * @param {AnimationSettings} settings The settings of this tween.
 * @param {Boolean} isCssTween Indicates that the tween to add should be a css tween
 * @return {Tween} The tween that was added.
 */
aeon.animation.Animation.prototype.addTween = function Animation$addTween(settings, isCssTween)
{
	var tween = new $anim.Tween(this.element, settings, this, isCssTween);
	tween.addListener("onprogress", Function.createDelegate(this, this.onTweenProgress)); /**/
	tween.addListener("oncomplete", Function.createDelegate(this, this.onTweenComplete)); /**/
	this.tweens.push(tween);

	return tween;
};

/**
 * Creates and adds a css tween to this animation
 * @param {AnimationSettings} settings The settings of this tween.
 * @return {Tween} The tween that was added.
 */
aeon.animation.Animation.prototype.addCssTween = function Animation$addCssTween(settings)
{
	return this.addTween(settings, true);
};

/**
 * Called continously as the tween onprogress events fire
 */
aeon.animation.Animation.prototype.onTweenProgress = function Animation$onTweenProgress()
{
	this.fireEvent("onprogress");
};

/**
 * Called when the animation is complete.
 */
aeon.animation.Animation.prototype.onTweenComplete = function Animation$onTweenComplete(eventObj)
{
	var tween = eventObj.source;
	var tweenIndex = this.getTweenIndex(tween);
	if (++this.tweensComplete == this.tweens.length)
	{
		this.stop();
		this.complete();
	}
};

/**
 * Resets all values that this animation manipilates.
 */
aeon.animation.Animation.prototype.reset = function Animation$reset()
{
	this.tweensComplete = 0;
	for (var i = 0; i < this.tweens.length; i++)
		this.tweens[i].reset();
};

/**
 * Runs the animation.
 * @param {Function} callback A callback function to call when the animation completes. Optional.
 */
aeon.animation.Animation.prototype.run = function Animation$run(callback)
{
	if (this.running)
	{
		$log.info("{0} will not start because it is already running", this.getName());
		return;
	}

	this.afterRunCallback = callback;
	this.tweensComplete = 0;
	if (this.tweens.length)
	{
		this.running = true;
		for (var i = 0; i < this.tweens.length; i++)
			this.tweens[i].run();
	}
};

/**
 * Stops the animation
 */
aeon.animation.Animation.prototype.stop = function Animation$stop()
{
	for (var i = 0; i < this.tweens.length; i++)
		this.tweens[i].stop();

	this.running = false;
};

aeon.animation.Animation.prototype.complete = function Animation$complete()
{
	if (Type.isFunction(this.afterRunCallback))
		this.afterRunCallback(this);

	this.fireEvent("oncomplete");
};

aeon.animation.Animation.prototype.getName = function Animation$getName()
{
	var elemInfo = undefined;
	if (Type.isHtmlElement(this.element))
	{
		var e = this.element;
		elemInfo = String.format("({0}{1})", e.tagName, e.id
			? "#" + e.id
			: e.className
				? "." + e.className.replace(" ", ".")
				: String.EMPTY);
	}

	var result = [];
	result.push(this.constructor.getName());
	if (this.settings.name)
		result.push("[" + this.settings.name + "]");

	if (elemInfo)
		result.push(elemInfo);

	return result.join(String.EMPTY);
};

/**
 * Returns a string representation of this <c>Animation</c>.
 */
aeon.animation.Animation.prototype.toString = function Animation$toString()
{
	var result = [];
	result.push(this.getName());

	var tweens = [];
	for (var i = 0; i < this.tweens.length; i++)
		tweens.push(this.tweens[i].toString());

	result.push(String.format(" { {0} }", tweens.join(", ")));
	return result.join(String.EMPTY);
};
