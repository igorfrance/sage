/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the aeon.js
 */

/**
 * Generates a random integer between 0 and <c>max</c>.
 * @param {Number} max The maximum value of the random number to generate.
 * @returns {Number} A random integer between 0 and <c>max</c>.
 */
Number.random = function Number$random(max)
{
	var top = Math.abs(max);
	var random = Math.round((top) * Math.random());
	return random;
};

/**
 * Generates a random number between <c>min</c> and <c>max</c>.
 * @param {Number} min The minimum value of the random number to generate.
 * @param {Number} max The maximum value of the random number to generate.
 * @param {Boolean} noRounding If true, the random number will not be rounded to the nearest integer.
 * @returns {Number} A random integer between 0 and <c>max</c>.
 */
Number.randomInRange = function Number$randomInRange(min, max, noRounding)
{
	var random = min + ((max - min) * Math.random());
	return noRounding ? random : Math.round(random);
};
