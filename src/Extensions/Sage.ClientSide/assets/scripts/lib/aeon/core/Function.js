/**
 * An empty function constant.
 * This constant serves to be used wherever an empty function is needed, without needing to create a new one each time.
 * @type {Function}
 */
Function.EMPTY = new Function;

/**
 * Utility property, that helps with identifying variables as functions (see <see type="Type.isFunction"/>).
 * @type {Boolean}
 */
Function.prototype.isFunction = true;

/**
 * Returns a previously created delegate.
 * @param {Object} object The target of the delegate.
 * @param {Object} method The function of the delegate.
 * @param {Array} fxargs The arguments that should be used when applying the delegate function. Optional.
 * @return {Function} The delegate function.
 */
Function.getDelegate = function Function$getDelegate(object, method, fxargs)
{
	if (Function.callbacks == null)
		Function.callbacks = [];

	for (var i = 0; i < Function.callbacks.length; i++)
	{
		var cb = Function.callbacks[i];
		var fx = true;
		if (cb.fxargs && fxargs)
			for (var j = 0; j < cb.fxargs.length; j++)
				if (cb.fxargs[j] != fxargs[j])
					fx = false;

		if (cb.object == object && cb.method == method && fx == true)
			return cb;
	}

	return null;
};

/**
 * Creates a function that will be execute the specifed method in the context of the specified object.
 * @param {Object} object The object on which the method will be applied.
 * @param {Object} method The method that should be applied on the object. This can be either a direct reference to the method or
 * a string name of the method of the specified object.
 * @param {Array} fxargs The arguments that should be used when applying the method. Optional.
 * @return {Function} The delegate function.
 */
Function.createDelegate = function Function$createDelegate(object, method, fxargs)
{
	var fx = method;
	if (Type.isString(method))
	{
		fx = object[method];
	}

	if (!Type.isFunction(fx))
	{
		$log.error("Method not found: " + method);
		return null;
	}

	var callbackFx = Function.getDelegate(object, fx, fxargs); /**/
	if (callbackFx != null)
		return callbackFx;

	callbackFx = function delegate() /**/
	{
		var object = arguments.callee.object;
		var method = arguments.callee.method;
		var fxargs = arguments.callee.fxargs.append(arguments);

		if (fxargs.length)
			return method.apply(object, fxargs);
		else
			return method.apply(object);
	};
	callbackFx.object = object;
	callbackFx.method = fx;
	callbackFx.fxargs = Type.isArray(fxargs) ? fxargs : [];

	Function.callbacks.push(callbackFx); /**/

	return callbackFx;
};

/**
 * Proxy function to <c>window.setInterval</c>.
 * The reason this has been created is to change the interface of <c>window.setInterval</c> so that the
 * function being assigned comes as the last argument, not the first.
 * <p>Documentation for <c>window.setInterval</c>: Calls a function repeatedly, with a fixed time delay between
 * each call to that function</p>
 * @param {Number} interval The time (in milliseconds) to wait between calling the supplied function.
 * @param {Function} callback The function to call periodically.
 * @return {Number} A unique interval ID you can pass to <c>window.clearInterval(intervalID)</c>.
 */
Function.setInterval = function Function$setInterval(interval, callback)
{
	return window.setInterval(callback, interval);
};

/**
 * Proxy function to <c>window.setTimeout</c>.
 * The reason this has been created is to change the interface of <c>window.setTimeout</c> so that the
 * function being assigned comes as the last argument, not the first.
 * <p>Documentation for <c>window.setTimeout</c>: Executes a code snippet or a function after specified delay.</p>
 * @param {Number} timeout The time (in milliseconds) to wait before calling the supplied function.
 * @param {Function} callback The function to call.
 * @return {Number} A unique timeout ID you can pass to <c>window.clearTimeout(timeoutID)</c>.
 */
Function.setTimeout = function Function$setTimeout(timeout, callback)
{
	return window.setTimeout(callback, timeout);
};

Function.prototype.getName = function Function$getName(includeArguments, argumentsOnly)
{
	var name = this.toString().replace(/^[\s\S]*?function[\s\n]+(([\w.$]*)[\s\n]*\((.*)\))\W[\s\S]*$/, function () /* rxp */
	{
		var fxName = arguments[2] ? arguments[2] : "anonymous";
		var functionDefinition = arguments[2] ? arguments[1] : [fxName, "(", arguments[3], ")"].join(String.EMPTY);

		if (!includeArguments && !argumentsOnly)
			return fxName;

		if (includeArguments && !argumentsOnly)
			return functionDefinition;

		if (includeArguments && argumentsOnly)
			return arguments[3];
	});

	return name.replace(/([\w\$])\$([\w\$])/g, "$1.$2");
};

Function.prototype.getStackTrace = function Function$getStackTrace(point, maxIterations)
{
	var fxCaller = point || arguments.callee.caller;
	var callStack = new Array();
	var functions = new Array();

	var currentCount = 0;
	while (fxCaller != null && fxCaller.getName)
	{
		if (maxIterations && currentCount >= maxIterations)
			break;

		// unfortunatelly this script goes into an infinite loop
		// if there is a recursion going on in the function call stack :(
		else if (functions.contains(fxCaller)) /**/
			break;

		var fxName = fxCaller.getName();
		var fxArgs = new Array();
		var argValues = fxCaller.arguments;

		for (var i = 0; i < argValues.length; i++)
		{
			if (Type.isFunction(argValues[i]) && Type.isFunction(argValues[i].getName)) /**/
				fxArgs.push(argValues[i].getName(true));

			else if (Type.isArray(argValues[i]))
				fxArgs.push("array[{0}]".format(argValues[i].length));

			else if (Type.isObject(argValues[i]))
				fxArgs.push("object{..}");

			else if (Type.isNull(argValues[i]))
				fxArgs.push("null");

			else if (Type.isString(argValues[i]))
				fxArgs.push(argValues[i].length < 200
					? "\"{0}\"".format(argValues[i])
					: "[string({0})]".format(argValues[i].length));

			else if (Type.isUndefined(argValues[i]))
				fxArgs.push("undefined");

			else
				fxArgs.push(argValues[i]);
		}

		functions.push(fxCaller); /**/
		callStack.unshift(fxCaller.getName() + "(" + fxArgs.join(", ") + ")");

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

/**
 * Provides a simple inheritance mechanism that enables calling base methods from the derived types
 * @param {Function} ancestor The ancestor function that this function extends.
 */
Function.prototype.inherits = function Function$inherits(ancestor)
{
	if (ancestor == null || ancestor == this || ancestor.prototype == null)
		return;

	this.prototype.___ = { base: ancestor.prototype, constructor: ancestor };

	for (var name in ancestor.prototype)
	{
		if (this.prototype[name] == null || Object.prototype[name] != null)
			this.prototype[name] = ancestor.prototype[name];
	}
	this.prototype.toString = ancestor.prototype.toString;

	this.prototype.$method = Function.$method;
	this.prototype.$super = Function.$super;

	if (arguments.length > 1)
	{
		var ctorArgs = Array.fromArguments(arguments, 1);
		this.prototype.$super(ctorArgs);
	}
};

/**
 * Applies the constructor of the base type to the current type.
 * @arguments {Object} [0-n] The arguments that should be supplied to the base constructor.
 */
Function.$super = function Function$$super()
{
	if (this.$$ == null)
		this.$$ = this;

	var constructor = this.$$.___.constructor;
	try
	{
		if (constructor)
		{
			var callArgs = Array.fromArguments(arguments);
			this.$$ = this.$$.___.base;
			constructor.apply(this, arguments);
		}
	}
	finally
	{
		this.$$ = null;
	}
};

/**
 * Applies the specified method of the base type to the current type.
 * @param {String} method The name of the method to apply.
 * @arguments {Object} 1-n The arguments that should be supplied to the base method.
 * @return {Object} The value returned by the base method.
 */
Function.$method = function Function$$method(method)
{
	if (this.$$ == null)
		this.$$ = this;

	var base = this.$$.___.base;
	var result = undefined;
	try
	{
		if (base && Type.isFunction(base[method]))
		{
			var callArgs = Array.fromArguments(arguments, 1);
			this.$$ = this.$$.___.base;
			result = base[method].apply(this, callArgs);
		}
	}
	finally
	{
		this.$$ = null;
	}
	return result;
};

