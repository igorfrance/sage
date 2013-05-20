/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the aeon.js
 */

/**
 * Returns the inner makedate function.
 */
var $date = new function()
{
	var MONTH_NAMES = ["January", "February", "Match", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
	var DAY_NAMES = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
	var OFFSET_KEYS = { s: "Seconds", m: "Minutes", h: "Hours", d: "Date", M: "Month", y: "FullYear" };

	var CONVERSIONS =
	{
		dayAndMonth1:
		{
			regexp: new RegExp("\\b(\\d{1,2})(-|/|\\.)(\\d{1,2})(?:\\2(\\d{2}))?\\b"),
			handler: function setDayAndMonth1(date, matches)
			{
				var x = parseInt(matches[1]);
				var y = parseInt(matches[3]);
				var z = matches[2];

				if (y > 12 || z == "/") // consider the first digit as the month
				{
					date.setDate(y);
					date.setMonth(x - 1);
				}
				else // consider the second digit as the month
				{
					date.setDate(x);
					date.setMonth(y - 1);
				}

				if (!isNaN(z))
					date.setYear(Number(String(new Date().getFullYear()).substring(0, 2) + String(z)));

				return date;
			},
		},

		dayAndMonth2:
		{
			regexp: new RegExp("(?:\\b(\\d{1,2})(?:[a-z]{2}|.|-)?\\s*)?\\b(jan|feb|mar|jun|jul|aug|sep|oct|nov|dec)(?:[a-z]+)?(?:(?:.|-)?\\s*\\b(\\d{1,2})(?:[a-z]{2}|.|-)?\\b)?", "i"),
			handler: function setDayAndMonth2(date, matches)
			{
				var name = matches[2].toLowerCase();
				var index = $.inArray(name, "jan|feb|mar|jun|jul|aug|sep|oct|nov|dec".split("|"));

				date.setMonth(index);

				var date1 = parseInt(matches[1]);
				var date2 = parseInt(matches[3]);

				if (!isNaN(date1)) date.setDate(date1);
				if (!isNaN(date2)) date.setDate(date2);

				return date;
			}
		},

		time:
		{
			regexp: new RegExp("(?:\\b|T)\\b(\\dd{2}):(\\d{2}):(\\d{2})\\b"),
			handler: function setTime(date, matches)
			{
				date.setHours(parseInt(matches[1]));
				date.setMinutes(parseInt(matches[1]));
				date.setSeconds(parseInt(matches[1]));
				return date;
			}
		},

		year:
		{
			regexp: new RegExp("\\b(\\d{4})(?:\\b|T)"),
			handler: function setYear(date, matches)
			{
				date.setYear(parseInt(matches[1]));
				return date;
			}
		},

		today:
		{
			regexp: new RegExp("^today$"),
			handler: function setToday(date, matches)
			{
				date = new Date(date.getFullYear(), date.getMonth(), date.getDate());
				return date;
			}
		}
	};

	var dateToString = Date.prototype.toString;

	/**
	 * Creates dates based on specified arguments.
	 *
	 * The <c>expression</c> argument specifies what kind of date to return.
	 * This can be either:
	 * <ul>
	 * <li>A number - this is interpreted as date milliseonds with which the resulting date is initialized.</li>
	 * <li>An offset expression - the amount by which to offset either the current date or, if specified the <c>fromDate</c></li>
	 * <li>A date/time string - see below for valid examples</li>
	 * <li>The literal string 'today' - This will return the current date with time set to 00:00:00.</li>
	 * </ul>
	 * <p>An <i>offset expression</i> specifies whether to increase or decrease (+/-), the amount of offset (number), and the
	 * unit to offset by, for example: <code>+45m</code> or <code>-1h</code>. Valid units are
	 * any of <code>s, m, h, d, M, y</code>.</p>
	 * <p>A date/time string expression attempts to parse various string formats with maximum flexibility. Rather than
	 * specifying a limited number of valid formats, the function looks for specific patterns as parts, making it possible
	 * to combine different formats. Any of the following examples are valid:</p>
	 * <ul>
	 * <li>29-11-1943</li>
	 * <li>29.11.1943 12:44:11</li>
	 * <li>29/11/1943</li>
	 * <li>1943/29/11</li>
	 * <li>1943-nov-29 12:44</li>
	 * <li>1943/11/29T12:44</li>
	 * </ul>
	 * <p>If called without arguments, the result will simply be a new date.</p>
	 * @param {String} expression The expression that specifies the date to return. Optional
	 * @param {Date} fromDate The date to use when using an offset <c>expression</c>. Optional.
	 * @returns {Date} A new date instance.
	 */
	function makedate(expression, fromDate)
	{
		var now = new Date();
		var today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
		var date = $type.isDate(fromDate)
				? new Date(fromDate)
				: expression
						? new Date(today)
						: new Date(now);

		if (expression)
		{
			if (/^\d+$/.test(expression))
			{
				date = new Date(parseInt(expression));
			}
			else if (/^([+-])(\d+|\d*\.\d*)([smhdMy])$/.test(expression))
			{
				var prefix = RegExp.$1;
				var value = parseFloat(RegExp.$2);
				var unit = RegExp.$3;

				var getter = $string.concat("get", OFFSET_KEYS[unit])
				var setter = $string.concat("set", OFFSET_KEYS[unit])

				var current = Date.prototype[getter].apply(date);
				var updated = prefix == "-"
					? current - value
					: current + value;

				Date.prototype[setter].call(date, updated);
			}
			else
			{
				for (var name in CONVERSIONS)
				{
					var conversion = CONVERSIONS[name];

					expression = expression.replace(conversion.regexp, function processMatch()
					{
						date = conversion.handler(date, arguments);
						return $string.EMPTY;
					});
				}
			}
		}

		return date;
	}

	/**
	 * Returns the value of the current date, formatted as specified by the <c>format</c> argument.
	 *
	 * The supported formatting values are:
	 * <ol>
	 * <li>yy: The year without the century.</li>
	 * <li>yyyy: The full year.</li>
	 * <li>M: The month.</li>
	 * <li>MM: The zero-padded month.</li>
	 * <li>MMM: The three-letter abbreviation of the month name.</li>
	 * <li>MMMM: The full month name.</li>
	 * <li>d: The date.</li>
	 * <li>dd: The zero-padded date.</li>
	 * <li>h: The hours.</li>
	 * <li>hh: The zero-padded hours.</li>
	 * <li>m: The minutes.</li>
	 * <li>mm: The zero-padded minutes.</li>
	 * <li>s: The seconds.</li>
	 * <li>s: The zero-padded seconds.</li>
	 * <li>www: The three-letter abbreviation of the week day.</li>
	 * <li>wwww: The full week day.</li>
	 * </ul>
	 * The month and week days are always in English.
	 * @param {String} format String specifying how the resulting value should be formatted.
	 * @returns {String} The value of the current date, formatted as specified by the <c>format</c> argument.
	 */
	function formatDate(date, expression)
	{
		if (!$type.isDate(date))
				return "NaN";

		if (!expression)
			return dateToString.apply(date);

		return String(expression).replace(
			/yyyy/g, date.getFullYear()).replace(
			/yy/g, date.getFullYear().toString().substring(2, 4)).replace(
			/MMMM/g, MONTH_NAMES[date.getMonth()]).replace(
			/MMM/g, MONTH_NAMES[date.getMonth()].substring(0, 3)).replace(
			/MM/g, $string.padLeft(date.getMonth() + 1, 2, 0)).replace(
			/M/g, (date.getMonth() + 1)).replace(
			/dd/g, $string.padLeft(date.getDate(), 2, 0)).replace(
			/d/g, date.getDate()).replace(
			/hh/g, $string.padLeft(date.getHours(), 2, 0)).replace(
			/h/g, date.getHours()).replace(
			/mm/g, $string.padLeft(date.getMinutes(), 2, 0)).replace(
			/m/g, date.getMinutes()).replace(
			/ss/g, $string.padLeft(date.getSeconds(), 2, 0)).replace(
			/s/g, date.getSeconds()).replace(
			/wwww/g, DAY_NAMES[date.getDay()]).replace(
			/www/g, DAY_NAMES[date.getDay()].substr(0, 3));
	};

	function getDate(date)
	{
		return $type.isDate(date) ? date : new Date();
	}


	// modify prototype
	Date.prototype.toString = function Date$toString(expression)
	{
		return formatDate(this, expression);
	};

	// create and return the interface
	makedate.format = formatDate;

	makedate.year = function getYear(date)
	{
		return getDate(date).getFullYear();
	};

	makedate.month = function getMonth(date)
	{
		return getDate(date).getMonth();
	};

	makedate.day = function getDate(date)
	{
		return getDate(date).getDate();
	};

	makedate.weekday = function getDay(date)
	{
		return getDate(date).getDay();
	};

	makedate.hours = function getHours(date)
	{
		return getDate(date).getHours();
	};

	makedate.minutes = function getMinutes(date)
	{
		return getDate(date).getMinutes();
	};

	makedate.seconds = function getSeconds(date)
	{
		return getDate(date).getSeconds();
	};

	makedate.time = function getTime(date)
	{
		return getDate(date).getTime();
	};

	return makedate;
};




