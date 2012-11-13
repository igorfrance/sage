Type.registerNamespace("aeon.utils");

if (window.console == null)
	window.console = new Object;

window.console.log = Type.isFunction(window.console.log) ? window.console.log : Function.EMPTY;
window.console.group = Type.isFunction(window.console.group) ? window.console.group : window.console.log;
window.console.groupEnd = Type.isFunction(window.console.groupEnd) ? window.console.groupEnd : Function.EMPTY;
window.console.info = Type.isFunction(window.console.info) ? window.console.info : window.console.log;
window.console.warn = Type.isFunction(window.console.warn) ? window.console.warn : window.console.log;
window.console.error = Type.isFunction(window.console.error) ? window.console.error : window.console.log;
window.console.dir = Type.isFunction(window.console.dir) ? window.console.dir : window.console.log;

/**
 * Logs the specified <c>message</c> to the console, provides static logging methods.
 * @param {String} message The message to log.
 * @arguments {Object} [1-n] Optional formatting arguments.
 */
aeon.utils.Log = function Log(message)
{
	$log.message.apply(this, arguments);
};

aeon.utils.Log.Level = {};
aeon.utils.Log.Level.ALL = 0;
aeon.utils.Log.Level.INFO = 1;
aeon.utils.Log.Level.WARN = 2;
aeon.utils.Log.Level.ERROR = 4;
aeon.utils.Log.Level.DEBUG = 5;
aeon.utils.Log.Level.NONE = 9;

aeon.utils.Log.level = aeon.utils.Log.Level.INFO;

aeon.utils.Log.group = function Log$group(message, level)
{
	if ($log.level > level)
		return;

	console.group(message);
};

aeon.utils.Log.groupEnd = function Log$groupEnd(level)
{
	if ($log.level > level)
		return;

	console.groupEnd();
};

aeon.utils.Log.message = function Log$message(message)
{
	console.log($log.getMessage(arguments));
};

aeon.utils.Log.info = function Log$info()
{
	if ($log.level > $log.Level.INFO)
		return;

	console.info($log.getMessage(arguments));
};

aeon.utils.Log.warn = function Log$warn()
{
	if ($log.level > $log.Level.WARN)
		return;

	console.warn($log.getMessage(arguments));
};

aeon.utils.Log.error = function Log$error()
{
	if ($log.level > $log.Level.ERROR)
		return;

	console.error($log.getMessage(arguments));
};

aeon.utils.Log.errortrace = function Log$errortrace()
{
	if ($log.level > $log.Level.ERROR)
		return;

	$log.error.apply(this, arguments);
	$log.trace.apply(this, arguments);
};

aeon.utils.Log.debug = function Log$debug()
{
	if ($log.level > $log.Level.DEBUG)
		return;

	console.log($log.getMessage(arguments));
};

aeon.utils.Log.dir = function Log$dir()
{
	console.dir(arguments[0]);
};

aeon.utils.Log.trace = function Log$trace()
{
	var caller = arguments.callee.caller;
	var callee = arguments.callee;
	var stackTrace = callee.getStackTrace(caller).toString();

	console.group($log.getMessage(arguments));
	console.log(stackTrace);
	console.groupEnd();
};

aeon.utils.Log.getMessage = function Log$getMessage(args)
{
	if (args.length == 1)
		return args[0];

	return String.format.apply(this, args);
};

/**
 * Global alias to <c>aeon.utils.Log</c>
 * @type {aeon.utils.Log}
 */
var $log = aeon.utils.Log;
