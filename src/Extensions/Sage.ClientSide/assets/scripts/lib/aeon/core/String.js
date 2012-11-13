/**
 * Defines an empty string as a constant value.
 * @type {String}
 */
String.EMPTY = "";

/**
 * Defines the linebreak constant.
 * @type {String}
 */
String.LINEBREAK = "\r\n";

/**
 * Defines the tab constant.
 * @type {String}
 */
String.TAB = "\t";

/**
 * Concatenates all supplied arguments into a single string value.
 * @example var fullName = String.concat(firstName, " ", lastName);
 * @arguments {Object} 0-n The values to concatenate.
 * @return {String} All supplied values, concatenated into a single string.
 */
String.concat = function String$concat()
{
	var result = [];
	for (var i = 0; i < arguments.length; i++)
		result.push(arguments[i]);

	return result.join(String.EMPTY);
};

/**
 * Replaces the specified format string with supplied arguments.
 * @param {String} value The format string.
 * @arguments {String} 1-n The values to replace the string with.
 * @return {String} The formatted version of the supplied string.
 */
String.format = function String$format(value)
{
	var args = Array.fromArguments(arguments, 1);

	function applyPadding(str, count, character, direction) /**/
	{
		var value = String(str);
		var diff = count - value.length;
		var output = new String();
		while (output.length < diff)
			output += character;

		return (direction == 2 ? value + output : output + value);
	}

	function applyFormat() /**/
	{
		try
		{
			var value = new String(args[arguments[1]]);
		}
		catch(e) { value = String.EMPTY; }

		if (arguments[2] != String.EMPTY && arguments[3] != String.EMPTY)
		{
			var direction = arguments[4] == "+" ? 2 : 1;
			value = applyPadding(value, arguments[3], arguments[2] == "#" ? "0" : " ", direction);
		}
		return value;
	}

	if (String.RX_FORMAT == null)
		String.RX_FORMAT = /\{(\d+)([$#])?(\d+)?([+-])?\}/g;

	return String(value).replace(String.RX_FORMAT, applyFormat);
};

/**
 * Removes characters from the beginning and the end of the supplied string value.
 * @param {String} value The string to trim.
 * @param {String} expression The string specifying the regular expression that selects the characters to remove.
 * The default expression selects and removes any whitespace characters ([\s]*). Optional.
 * @return {String} The trimmed value.
 */
String.trim = function String$trim(value, expression)
{
	if (expression == null)
		expression = "\\s";

	var re = new RegExp("^[" + expression + "]*([\\s\\S]*?)[" + expression + "]*$");
	return String(value).replace(re, "$1");
};

/**
 * Replaces the current string with supplied arguments.
 * @arguments {String} [0-n] The values to replace the string with.
 * @return {String} The formatted version of the current string.
 */
String.prototype.format = function String$format()
{
	return String.format(this, arguments);
};

/**
 * Prepends characters to the current string until it reaches the specified length.
 * @param {Number} length The final length that the string should be.
 * @param {String} padChar The character to pad the string with, default is ' '. Optional.
 * @return {String} The padded version of the current string.
 */
String.prototype.padLeft = function String$padLeft(length, padChar)
{
	if (padChar == undefined)
		padChar = " ";

	var string = this.toString();
	while(string.length < length)
		string = padChar + string;
	return string;
};

/**
 * Appends characters to the current string until it reaches the specified length.
 * @param {Number} length The final length that the string should be.
 * @param {String} padChar The character to pad the string with, default is ' '. Optional.
 * @return {String} The padded version of the current string.
 */
String.prototype.padRight = function String$padRight(length, padChar)
{
	if (padChar == undefined)
		padChar = " ";

	var string = this.toString();
	while(string.length < length)
		string = string + padChar;
	return string;
};

/**
 * Removes characters from the beginning and the end of the current string.
 * @param {String} expression The string specifying the regular expression that selects the characters to remove. The default expression
 * selected any whitespace characters. Optional.
 * @return {String} The trimmed version of the current string
 */
String.prototype.trim = function String$trim(expression)
{
	return String.trim(this, expression);
};

