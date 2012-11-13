Type.registerNamespace("aeon.animation");

/**
 * Moves the specified element in a circular path.
 * @param {HTMLElement} element The DOM element to animate.
 * @param {CircleSettings} settings The settings of this animation.
 * @param {Function} callback The callback function to call when this animation completes.
 * @constructor
 * @example var c = new $anim.Circle($("#elem1"), { radius: 150, degree: 0, turn: -90 });
 */
aeon.animation.Circle = function Circle(element, settings, callback)
{
	this.$super(element, new $anim.CircleSettings(settings), callback);

	/**
	 * The x coordinate of the circle's center (relative to the radius and the initial degree and position of the animated element).
	 * This value is (re)calculated before each run of this animation.
	 * @type {Number}
	 */
	this.centerX;
	/**
	 * The y coordinate of the circle's center (relative to the radius and the initial degree and position of the animated element).
	 * This value is (re)calculated before each run of this animation.
	 * @type {Number}
	 */
	this.centerY;

	this.tweens.degree =
		this.addTween(new $anim.AnimationSettings(this.settings, {
			getter: Function.createDelegate(this, this.getDegree), /**/
			setter: Function.createDelegate(this, this.setDegree), /**/
			endValue: this.settings.turn,
			property: "degree"
		}));

	if (this.settings.stretch && this.settings.stretch != "0")
	{
		this.tweens.radius =
			this.addTween(new $anim.AnimationSettings(this.settings, {
				getter: Function.createDelegate(this, this.getRadius), /**/
				setter: Function.createDelegate(this, this.setRadius), /**/
				endValue: this.settings.stretch,
				property: "radius"
			}));
	}
}
aeon.animation.Circle.inherits(aeon.animation.Animation);

/**
 * Calculates the center coordinates.
 */
aeon.animation.Circle.prototype.init = function Circle$init()
{
	var x = parseInt(this.$element.css("left"));
	var y = parseInt(this.$element.css("top"));

	var coords = this.getDegreeCoords(this.settings.radius, this.settings.degree, x, y);

	this.centerX = x - (coords.x - x);
	this.centerY = y - (coords.y - y);
};

/**
 * Calculates the center coordinates, and runs this animation.
 */
aeon.animation.Circle.prototype.run = function Circle$run()
{
	this.init();
	this.$method("run");
};

/**
 * Gets the circle coordinates for the specified <c>radius</c>, <c>degree</c>, and start coordinates.
 * @param {Number} radius The radius of the circle.
 * @param {Number} degree The current circle degree.
 * @param {Number} startX The start X coordinate of the animated object.
 * @param {Number} startY The start Y coordinate of the animated object.
 * @returns {Object} An object with x & y properties specifying the circle coordinates for the current degree.
 */
aeon.animation.Circle.prototype.getDegreeCoords = function Circle$getDegreeCoords(radius, degree, startX, startY)
{
	var radian = (degree / 180) * Math.PI;

	var x = startX + Math.cos(radian) * radius;
	var y = startY - Math.sin(radian) * radius;

	return { x: x, y: y };
};

/**
 * Gets this animations current circle degree.
 * @returns {Number} This animations current circle degree.
 */
aeon.animation.Circle.prototype.getDegree = function Circle$getDegree(s)
{
	return this.settings.degree;
};

/**
 * Sets this animations current circle degree.
 * @param {Number} value This animations current circle degree.
 */
aeon.animation.Circle.prototype.setDegree = function Circle$setDegree(value)
{
	this.settings.degree = value;

	if (!this.centerX || !this.centerY)
		this.init();

	var coords = this.getDegreeCoords(this.settings.radius, this.settings.degree, this.centerX, this.centerY);
	console.log(coords.x, "x", coords.y, " r:", this.settings.radius,
		" d:", this.settings.degree, " cx:", this.centerX, " cy:", this.centerY);
	this.$element.css({ left: coords.x, top: coords.y });
};

/**
 * Gets this animations current circle radius.
 * @returns {Number} This animations current circle radius.
 */
aeon.animation.Circle.prototype.getRadius = function Circle$getRadius()
{
	return this.settings.radius;
};

/**
 * Sets this animations current circle radius.
 * @param {Number} value This animations current circle radius.
 */
aeon.animation.Circle.prototype.setRadius = function Circle$setRadius(value)
{
	this.settings.radius = value;
};
