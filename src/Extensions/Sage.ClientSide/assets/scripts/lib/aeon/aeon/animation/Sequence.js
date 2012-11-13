Type.registerNamespace("aeon.animation");

/**
 * Combines multiple Animation or Sequence instances.
 * When sequence is run, it's components are run either successivelly or simultaneously.
 * @param {SequenceSettings} settings The settings of this sequence.
 * @param {Function} callback The callback function to call when this sequence completes.
 * @event onstart Fired when the sequence first starts.
 * @event oncomplete Fired when the sequence completes.
 * @event onstepcomplete Fired when one of the sequence components completes.
 * @constructor
 */
aeon.animation.Sequence = function Sequence(settings)
{
	this.$super("onstart", "onstepcomplete", "onroundcomplete", "oncomplete");

	this.settings = new $anim.SequenceSettings(settings);
	this.segments = new Array;
	this.animationsComplete = 0;
	this.roundsComplete = 0;
	this.loopTimeoutId = null;
	this.startTime = null;
	this.running = false;

	/**
	 * One-time-only callback function to call when the sequence completes.
	 * @type {Function}
	 */
	this.afterRunCallback = null;
};
aeon.animation.Sequence.inherits(aeon.Dispatcher);

/**
 * Adds an animation to this sequence.
 * The animation can be either be an instance of Animation or of another Sequence.
 * @param {Animation} animation The animation to add
 * @param {Number} delay The time (in milliseconds) to wait before running the specified animation as a part of the
 * sequence.
 * @param {Number} index The index at this to add the specified animation. If this value is omitted, the animation
 * is added at the end of the current sequence.
 * @returns {Animation} The animation that was added.
 */
aeon.animation.Sequence.prototype.add = function Sequence$add(animation, delay, index)
{
	$assert.implements(animation, $anim.Animation.interface);
	$assert.isFalse(animation == this, "Cannot add the sequence unto itself");

	animation.addListener("oncomplete", Function.createDelegate(this, this.onAnimationCompleted)); /**/
	var valueToAdd = { animation: animation, delay: delay || 0 };

	if (Type.isNumeric(index) && index >= 0 && index < this.segments.length)
		this.segments.splice(index, 0, valueToAdd);
	else
		this.segments.push(valueToAdd);

	return animation;
};

/**
 * Returns true if the current sequence contains the specified animation.
 * @param {String} animation The animation to check for.
 * @return {Boolean} <c>true></c> if this sequence contains the supplied animation.
 */
aeon.animation.Sequence.prototype.containsAnimation = function Sequence$containsAnimation(animation)
{
	for (var i = 0; i < this.segments.length; i++)
	{
		if (this.segments[i].animation == animation)
			return true;
	}

	return false;
};

aeon.animation.Sequence.prototype.getAnimation = function Sequence$getAnimation(index)
{
	if (this.segments[index] != null)
		return this.segments[index].animation;

	return null;
};

aeon.animation.Sequence.prototype.getAnimationIndex = function Sequence$getAnimationIndex(animation)
{
	for (var i = 0; i < this.segments.length; i++)
	{
		if (this.segments[i].animation == animation)
			return i;
	}

	return -1;
};

/**
 * Gets the root parent sequence, or self if this sequence has no parent.
 * @return {Sequence} The root parent sequence.
 */
aeon.animation.Sequence.prototype.getRoot = function Sequence$getRoot()
{
	var root = this;
	while (root.parent != null)
		root = root.parent;

	return parent;
};

/**
 * Resets all constituent animations.
 */
aeon.animation.Sequence.prototype.reset = function Sequence$reset()
{
	this.roundsComplete = 0;
	this.animationsComplete = 0;

	for (var i = this.segments.length - 1; i >= 0; i--)
	{
		var animation = this.segments[i].animation;
		animation.reset();
	}
};

/**
 * Runs the animation sequence
 * @param {Function} callback A callback function to call when the animation completes. Optional.
 */
aeon.animation.Sequence.prototype.run = function Sequence$run(callback)
{
	if (this.running)
	{
		$log.info("{0} will not start because it is already running", this.getName());
		return;
	}

	this.afterRunCallback = callback;
	this.startTime = new Date();
	this.fireEvent("onstart");
	this.running = true;

	if (this.segments.length == 0)
	{
		this.running = false;
		this.fireEvent("oncomplete");
		return;
	}

	this.roundsComplete = 0;
	this.runSequence();
};

aeon.animation.Sequence.prototype.runSequence = function Sequence$runSequence()
{
	this.animationsComplete = 0;
	if (this.settings.type == $anim.SequenceType.SIMULTANEOUS)
	{
		for (var i = 0; i < this.segments.length; i++)
			this.runAnimation(i);
	}
	else
	{
		this.runAnimation(0);
	}
};

/**
 * Runs the animation with the specified index.
 * @param {Number} animationIndex The index of the animation to run.
 */
aeon.animation.Sequence.prototype.runAnimation = function Sequence$runAnimation(animationIndex)
{
	var animationInfo = this.segments[animationIndex];
	if (animationInfo == null)
		return;

	var animation = animationInfo.animation;
	var animationDelay = animationInfo.delay;

	if (animationDelay != 0)
	{
		var sequenceStep = Function.createDelegate(animation, animation.run); /**/
		animationInfo.timeout = Function.setTimeout(animationDelay, sequenceStep); /**/
	}
	else
	{
		animation.run();
	}
};

/**
 * Clears all animations from the current Sequence.
 */
aeon.animation.Sequence.prototype.clear = function Sequence$clear()
{
	this.stop();
	while (this.segments.length)
	{
		this.segments[0] = null;
		this.segments.shift();
	}
};

/**
 * Stops the animation sequence.
 */
aeon.animation.Sequence.prototype.stop = function Sequence$stop()
{
	if (this.loopTimeoutId != null)
		window.clearTimeout(this.loopTimeoutId);

	for (var i = 0; i < this.segments.length; i++)
		this.stopAnimation(i);

	this.running = false;
};

/**
 * @private
 */
aeon.animation.Sequence.prototype.complete = function Sequence$complete()
{
	this.fireEvent("oncomplete");

	if (Type.isFunction(this.afterRunCallback))
		this.afterRunCallback(this);
};

/**
 * Stops the animation with the specified index.
 * @param {Number} animationIndex The index of the animation to stop.
 */
aeon.animation.Sequence.prototype.stopAnimation = function Sequence$stopAnimation(animationIndex)
{
	var animationInfo = this.segments[animationIndex];
	if (animationInfo == null)
		return;

	var animation = animationInfo.animation;
	if (animationInfo.timeout != null)
		window.clearTimeout(animationInfo.timeout);

	animation.stop();
};

/**
 * Called after an animation's segment completes.
 */
aeon.animation.Sequence.prototype.onAnimationCompleted = function Sequence$onAnimationCompleted(evt)
{
	this.fireEvent("onstepcomplete");

	this.animationsComplete++;

	var animation = evt.source;
	var animationIndex = this.getAnimationIndex(animation);

	var sequenceComplete = false;
	if (this.settings.type == $anim.SequenceType.SUCCESSIVE)
	{
		if (animationIndex == this.segments.length - 1)
			sequenceComplete = true;
		else
			this.runAnimation(animationIndex + 1);
	}
	else
	{
		if (this.animationsComplete == this.segments.length)
			sequenceComplete = true;
	}

	if (sequenceComplete)
	{
		this.roundsComplete++;
		var s = this.settings;
		if (s.loop)
		{
			this.fireEvent("onroundcomplete");

			if (s.loopRounds == 0 || s.loopRounds > this.roundsComplete)
			{
				if (s.loopPause > 0)
				{
					var self = this;
					this.loopTimeoutId = Function.setTimeout(s.loopPause, function Seqeunce$runSequenceAfterDelay() /**/
					{
						if (s.loopReset)
							self.reset();

						self.runSequence();
					});
				}
				else
				{
					if (s.loopReset)
						this.reset();

					this.runSequence();
				}
			}
			else
			{
				this.stop();
				this.complete();
			}
		}
		else
		{
			this.stop();
			this.complete();
		}
	}
};

aeon.animation.Sequence.prototype.getName = function Sequence$getName()
{
	if (this.settings.name)
		return String.format("{0}.{1}", this.settings.name, this.settings.type);

	return String.format("Sequence.{0}", this.settings.type);
};

/**
 * Returns a string representation of this <c>Sequence</c>.
 */
aeon.animation.Sequence.prototype.toString = function Sequence$toString()
{
	var result = [];
	result.push(this.getName());
	if (this.segments.length)
	{
		result.push("\n");
		for (var i = 0; i < this.segments.length; i++)
		{
			var segment = this.segments[i];
			var animation = segment.animation;
			var animString = animation.toString();

			result.push(animString.split("\n").each(function (index, value)
			{
				return ("    " + value);

			}).join("\n"));
			if (segment.delay)
				result.push(": delayed by {0}ms".format(segment.delay));

			result.push("\n");
		}
	}
	return result.join(String.EMPTY).trim();
};

