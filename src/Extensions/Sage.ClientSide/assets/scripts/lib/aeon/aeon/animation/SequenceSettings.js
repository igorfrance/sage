Type.registerNamespace("aeon.animation");

/**
 * Specifies the settings for an animation sequence.
 * The data object passed to the constructor will be used to initialize this settings.
 * @constructor
 * @param {Object} data The object with initial settings.
 * @param {Object} override If a settings with equivalent name is found in this object it overrides the setting from
 * the <c>data</c> object.
 */
aeon.animation.SequenceSettings = function SequenceSettings(data, override)
{
	/**
	 * The name of this sequence, useful while debugging. Optional.
	 * @type {String}
	 */
	this.name = this.getString("name", data, override, String.EMPTY);

	/**
	 * The type of this sequence. Default is <c>DEFAULT_SEQUENCE_TYPE</c>.
	 * @type {SequenceType}
	 */
	this.type = this.getString("type", data, override, $anim.DEFAULT_SEQUENCE_TYPE);

	/**
	 * Indicates that the sequence should repeat its steps in a loop.
	 * @type {Number}
	 */
	this.loop = this.getBoolean("loop", data, override, false);

	/**
	 * Specifies the number of times this sequence should be looped.
	 * Default is <c>0</c>, meaning that (if the <c>loop</c> property is <c>true</c>) the sequence should loop indefinitely.
	 * @type {Number}
	 */
	this.loopRounds = this.getNumber("loopRounds", data, override, 0);

	/**
	 * If <c>true</c>, and the sequence is set to loop, the sequence will reset all contituent elements after each loop cycle.
	 * @type {Boolean}
	 */
	this.loopReset = this.getBoolean("loopReset", data, override, false);

	/**
	 * Specifies the time (in milliseconds) to wait between looping the sequence (if applicable). Default is <c>0</c>.
	 * @type {Number}
	 */
	this.loopPause = this.getNumber("loopPause", data, override, 0);
}
aeon.animation.SequenceSettings.inherits(aeon.Settings);

