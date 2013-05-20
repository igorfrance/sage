/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the aeon.js
 */

/**
 * Provides a proxy object that can be used across different browsers to log to console.
 * @type {Function}
 */
var $log = new function()
{
	var timers = {};
	var listeners = [];
	var standardMethods = ["log", "debug", "info", "warn", "error", "group", "groupCollapsed", "groupEnd"];
	var customMethods = [];

	/**
	 * Logs the supplied <c>message</c> to console.
	 * The severity argument is read from the last supplied argument, and will be ignored unless
	 * it's one of the allowed values. Any arguments between message and severity are considered
	 * formatting variables.
	 * @param {String} message The message to log.
	 * @param {String} severity The severity of the message. One of <c>debug, info, warn, error</c>.
	 * Default is <c>debug</c>.
	 * @arguments {Object} [1-n] The variables to insert into the format string.
	 */
	function log(message, severity)
	{
		var method = "log";

		var count = arguments.length;
		var first = 1;
		var last = count - 1;

		var testName = String(arguments[last]);
		if (standardMethods.contains(testName) || customMethods.contains(testName))
		{
			method = testName;
			last = count - 2;
		}

		if (method != "dir")
		{
			if (last >= first)
			{
				var formatArgs = $array.fromArguments(arguments, first, last);
				message = $string.format(message, formatArgs);
			}
		}

		for (var i = 0; i < listeners.length; i++)
		{
			var listener = listeners[i];
			if ($type.isFunction(listener[method]))
				listener[method](message);
		}
	}

	log.assert = function Log$assert(condition, message)
	{
		var args = $array.fromArguments(arguments, 2);
		if (condition === false)
			log(message, args, "error");
	};

	log.time = function Log$time(timerName)
	{
		if (console.time)
			return console.time.apply(console, arguments);

		if (timerName)
			timers[timerName] = new Date().getTime();
	};

	log.timeEnd = function Log$timeEnd(timerName)
	{
		if (console.timeEnd)
			return console.timeEnd.apply(console, arguments);

		if (timers[timerName])
		{
			log.info("{0}: {1}.ms", timerName, new Date().getTime() - timers[timerName]);
		}
	};

	log.clear = function Log$clear()
	{
		console.clear();
	};

	log.log = function Log$debug(value)
	{
		log(value, $array.fromArguments(arguments, 1), "log");
	};

	log.debug = function Log$debug(value)
	{
		log(value, $array.fromArguments(arguments, 1), "debug");
	};

	log.groupCollapsed = function Log$groupCollapsed(value)
	{
		log(value, $array.fromArguments(arguments, 1), "groupCollapsed");
	};

	log.group = function Log$group(value)
	{
		log(value, $array.fromArguments(arguments, 1), "group");
	};

	log.groupEnd = function Log$groupEnd()
	{
		log($string.EMPTY, "groupEnd");
	};

	log.info = function Log$info(value)
	{
		log(value, $array.fromArguments(arguments, 1), "info");
	};

	log.warn = function Log$warn(value)
	{
		log(value, $array.fromArguments(arguments, 1), "warn");
	};

	log.error = function Log$error(value)
	{
		log(value, $array.fromArguments(arguments, 1), "error");
	};

	log.dir = function Log$dir(value)
	{
		log(value, $array.fromArguments(arguments, 1), "dir");
	};

	log.register = function register(listener, skipMethods)
	{
		if (listener != null && !listeners.contains(listener))
			listeners.push(listener);

		if (skipMethods !== true)
		{
			for (var name in listener)
			{
				if ($type.isFunction(listener[name]))
				{
					if (!customMethods.contains(name))
						customMethods.push(name);
				}
			}
		}
	};

	log.unregister = function unregister(listener)
	{
		var index = listeners.indexOf(listener);
		if (listener != null && index != -1)
		{
			listeners.splice(index, 1);
			for (var i = customMethods.length - 1; i >= 0; i--)
			{
				var name = customMethods[i];
				var remove = true;
				for (var j = 0; j < listeners.length; j++)
				{
					if ($type.isFunction(listeners[j][name]))
					{
						remove = false;
						break;
					}
				}

				if (remove)
					customMethods.splice(i, 1);
			}
		}
	};

	log.register(window.console, true);

	return log;
};
