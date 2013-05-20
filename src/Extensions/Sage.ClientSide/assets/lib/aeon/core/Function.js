/**
 * @copyright 2012 Igor France
 * Licensed under the MIT License
 *
 * This file is a part of the aeon.js
 */

/**
 * An empty function constant.
 * This constant serves to be used wherever an empty function is needed, without needing to create a new one each time.
 * @type {Function}
 */
Function.EMPTY = new Function;

/**
 * Gets the name of the specified function, optionally including the function arguments.
 * @param {Function} fx The function whose name to get. If omitted, the function used will be the caller of this function.
 * @param {Boolean} includeArguments If <c>true</c>, the resulting name will be appended the function
 * arguments in function parantheses.
 * @param {Boolean} argumentsOnly If <c>true</c>, the resulting value will only contain the arguments.
 * @returns {String} The name of the function.
 */
Function.getName = function Function$getName(fx, includeArguments, argumentsOnly)
{
	if (!$type.isFunction(fx))
	{
		fx = arguments.callee.caller;
		if (!$type.isFunction(fx))
			return;
	}

	var name = String(fx).replace(/^[\s\S]*?function[\s\n]+(([\w.$]*)[\s\n]*\((.*)\))\W[\s\S]*$/, function () /* rxp */
	{
		var fxName = arguments[2] ? arguments[2] : "anonymous";
		var functionDefinition = arguments[2] ? arguments[1] : [fxName, "(", arguments[3], ")"].join($string.EMPTY);

		if (!includeArguments && !argumentsOnly)
			return fxName;

		if (includeArguments && !argumentsOnly)
			return functionDefinition;

		if (includeArguments && argumentsOnly)
			return arguments[3];
	});

	return name.replace(/([\w\$])\$([\w\$])/g, "$1.$2");
};

/**
 * Gets the name of function that called the funtion that invokes this function.
 * @returns {String} The name of the function that called the funtion that invokes this function.
 */
Function.callerName = function Function$callerName()
{
	return Function.getName(arguments.callee.caller.caller);
};

/**
 * Gets the stacktrace starting at the specified <c>point</c>.
 * @param {Function} point The function from which to start building the stack trace.
 * @param {Number} maxIterations The maximum number of iterations to go through.
 * @returns {String} The string representing the function call stack built from the specified <c>point</c>.
 */
Function.stackTrace = function Function$stackTrace(point, maxIterations)
{
	// point = arguments.callee.caller
	var fxCaller = point || arguments.callee.caller;
	var callStack = new Array();
	var functions = new Array();

	var currentCount = 0;
	while (fxCaller != null)
	{
		if (maxIterations && currentCount >= maxIterations)
			break;

		// unfortunatelly this script goes into an infinite loop
		// if there is a recursion going on in the function call stack :(
		else if (functions.contains(fxCaller)) /**/
			break;

		var fxName = Function.getName(fxCaller);
		var fxArgs = new Array();
		var argValues = fxCaller.arguments;

		for (var i = 0; i < argValues.length; i++)
		{
			if ($type.isFunction(argValues[i]) && $type.isFunction(argValues[i].getName)) /**/
				fxArgs.push(argValues[i].getName(true));

			else if ($type.isArray(argValues[i]))
				fxArgs.push("array[{0}]".format(argValues[i].length));

			else if ($type.isObject(argValues[i]))
				fxArgs.push("object{..}");

			else if ($type.isNull(argValues[i]))
				fxArgs.push("null");

			else if ($type.isString(argValues[i]))
				fxArgs.push(argValues[i].length < 200
					? "\"{0}\"".format(argValues[i])
					: "[string({0})]".format(argValues[i].length));

			else if (argValues[i] == undefined)
				fxArgs.push("undefined");

			else
				fxArgs.push(argValues[i]);
		}

		functions.push(fxCaller); /**/
		callStack.unshift(Function.getName(fxCaller) + "(" + fxArgs.join(", ") + ")");

		try
		{
			fxCaller = fxCaller.caller;
		}
		catch(e)
		{
			break;
		}
		currentCount++;
	}
	callStack.toString = function (joinString) /**/
	{
		return this.join(joinString || "\n");
	};
	return callStack;
};

