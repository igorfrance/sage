/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the aeon.js
 */
var $string = new function()
{
	var $string = this;

	/**
	 * Defines an empty string as a constant value.
	 * @type {String}
	 */
	$string.EMPTY = "";

	var RX_FORMAT;

	function applyPadding(str, count, character, direction)
	{
		var value = String(str);
		var diff = count - value.length;
		var output = new String();
		while (output.length < diff)
			output += character;

		return (direction == 2 ? value + output : output + value);
	}

	/**
	 * Replaces the specified format string with supplied arguments.
	 * @param {String} value The format string.
	 * @arguments {String} 1-n The values to replace the string with.
	 * @return {String} The formatted version of the supplied string.
	 */
	this.format = function format(value)
	{
		var args = $array.fromArguments(arguments, 1);

		if (RX_FORMAT == null)
			RX_FORMAT = /\{(\d+)([$#])?(\d+)?([+-])?\}/g;

		return String(value).replace(RX_FORMAT, function applyFormat()
		{
			try
			{
				var value = new String(args[arguments[1]]);
			}
			catch(e) { value = $string.EMPTY; }

			if (arguments[2] != $string.EMPTY && arguments[3] != $string.EMPTY)
			{
				var direction = arguments[4] == "+" ? 2 : 1;
				value = applyPadding(value, arguments[3], arguments[2] == "#" ? "0" : " ", direction);
			}
			return value;
		});
	};

	/**
	 * Indicates whether the specified <c>value</c> is null or an empty string.
	 * @param {String} value The string to check.
	 * @returns {Boolean} <c>true</c> if the value parameter is null or an empty string ("");
	 * otherwise, <c>false</c>.
	 */
	this.isEmpty = function isEmpty(value)
	{
		return value == null || $string.trim(value) == $string.EMPTY;
	};

	/**
	 * Removes characters from the beginning and the end of the supplied string value.
	 * @param {String} value The string to trim.
	 * @param {String} expression The string specifying the regular expression that selects the characters to remove.
	 * The default expression selects and removes any whitespace characters ([\s]*). Optional.
	 * @return {String} The trimmed value.
	 */
	this.trim = function trim(value, expression)
	{
		if (expression == null)
			expression = "\\s";

		var re = new RegExp("^[" + expression + "]*([\\s\\S]*?)[" + expression + "]*$");
		return String(value).replace(re, "$1");
	};

	/**
	 * Prepends characters to the specified <c>value</c> until it reaches the specified length.
	 * @param {Number} length The final length that the string should be.
	 * @param {String} padChar The character to pad the string with, default is ' '. Optional.
	 * @return {String} The padded version of the specified <c>value</c>.
	 */
	this.padLeft = function padLeft(value, length, padChar)
	{
		if (padChar == undefined)
			padChar = " ";

		var result = String(value);
		while(result.length < length)
			result = padChar + result;

		return result;
	};

	/**
	 * Appends characters to the specified <c>value</c> until it reaches the specified length.
	 * @param {Number} length The final length that the string should be.
	 * @param {String} padChar The character to pad the string with, default is ' '. Optional.
	 * @return {String} The padded version of the specified <c>value</c>.
	 */
	this.padRight = function padRight(value, length, padChar)
	{
		if (padChar == undefined)
			padChar = " ";

		var result = String(value);
		while(result.length < length)
			result = result + padChar;

		return result;
	};

	/**
	 * Returns a string that repeats the specified <c>value</c> the specified <c>count</c> of times.
	 * @param {String} value The string to repeat.
	 * @param {Number} count The count of times to repeat the string.
	 * @return {String} The specified <c>value</c>, repeated the specified <c>count</c> of times.
	 */
	this.repeat = function repeat(value, count)
	{
		var result = [];
		for (var i = 0; i < count; i++)
			result.push(value);

		return result.join($string.EMPTY);
	};

	/**
	 * Combines the text of all arguments into a single string.
	 * @arguments {String} 0-n The values to concatenate.
	 * @return {String} The text of all arguments combined into a single string.
	 */
	this.concat = function concat()
	{
		var result = [];
		for (var i = 0; i < arguments.length; i++)
		{
			result.push(arguments[i]);
		}

		return result.join($string.EMPTY);
	};


	/**
	 * Removes characters from the beginning and the end of the current string.
	 * @param {String} expression The string specifying the regular expression that selects the characters to remove. The default expression
	 * selected any whitespace characters. Optional.
	 * @return {String} The trimmed version of the current string
	 */
	String.prototype.trim = function String$trim(expression)
	{
		return $string.trim(this, expression);
	};

	/**
	 * Replaces the current string with supplied arguments.
	 * @arguments {String} [0-n] The values to replace the string with.
	 * @return {String} The formatted version of the current string.
	 */
	String.prototype.format = function String$format()
	{
		return $string.format(this, arguments);
	};
};
