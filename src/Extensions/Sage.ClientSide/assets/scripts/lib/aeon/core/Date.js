/**
 * Adds a boolean <c>true</c> to each date instance, thereby making it easier and fastr to identify date objects.
 * @type {Boolean}
 * @see Type.isDate
 */
Date.prototype.isDate = true;

Date.monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];

Date.dayNames = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];

/**
 * Returns a date set to midnight of the current date (today).
 * @return {Date} A date set to midnight of the current date (today).
 */
Date.today = function Date$today()
{
	var now = new Date();
	var today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
	return today;
};

/**
 * Offsets the specified <c>dateInstance</c> by the specified amount of time, expressed with <c>timeFormat</c>.
 * The time format is:
 * <p><code>(+|-)(integer)(smhdMy)</p>
 * @example Date.offset(today, "+1M")
 * @param dateInstance The date to offset.
 * @param timeFormat The expression that specifies by how many seconds, minutes, hours, days, months or years to
 * offset the date.
 * @return The specified <c>dateInstance</c>, offset by the specified amount of time, expressed with <c>timeFormat</c>.
 */
Date.offset = function Date$offset(dateInstance, timeFormat)
{
	var timeMultiplier = { s: 1000, m: 1000*60, h: 1000*60*60, d: 1000*60*60*24, M: 1000*60*60*24*30, y: 1000*60*60*24*365 };

	var timeOffset = 0;

	if (timeFormat != undefined)
		if (String(timeFormat).match( /^([+-]?(?:\d+|\d*\.\d*))([mhdMy]?)$/ ))
			timeOffset = parseInt(timeMultiplier[RegExp.$2] * RegExp.$1);

	dateInstance.setTime(dateInstance.getTime() + timeOffset);
	return dateInstance;
};

Date.prototype.__toString = Date.prototype.toString;

/**
 * Returns the value of the current date, formatted as specified by the <c>format</c> argument.
 * @param {String} format String specifying how the resulting value should be formatted.
 * @returns {String} The value of the current date, formatted as specified by the <c>format</c> argument.
 */
Date.prototype.toString = function Date$toString(format)
{
	if (isNaN(this))
			return "NaN";

	if (!format)
		return this.__toString();

	return String(format).replace(
		/\byyyy\b/g, this.getFullYear()).replace(
		/\byy\b/g, this.getFullYear().toString().substring(2, 4)).replace(
		/\bMMMM\b/g, Date.monthNames[this.getMonth()]).replace(
		/\bMMM\b/g, Date.monthNames[this.getMonth()].substring(0, 3)).replace(
		/\bMM\b/g, (this.getMonth() + 1).toString().padLeft(2, 0)).replace(
		/\bM\b/g, (this.getMonth() + 1)).replace(
		/\bdd\b/g, this.getDate().toString().padLeft(2, 0)).replace(
		/\bd\b/g, this.getDate()).replace(
		/\bhh\b/g, this.getHours().toString().padLeft(2, 0)).replace(
		/\bh\b/g, this.getHours()).replace(
		/\bmm\b/g, this.getMinutes().toString().padLeft(2, 0)).replace(
		/\bm\b/g, this.getMinutes()).replace(
		/\bss\b/g, this.getSeconds().toString().padLeft(2, 0)).replace(
		/\bs\b/g, this.getSeconds()).replace(
		/\bwwww\b/g, Date.dayNames[this.getDay()]).replace(
		/\bwww\b/g, Date.dayNames[this.getDay()].substr(0, 3));
};
