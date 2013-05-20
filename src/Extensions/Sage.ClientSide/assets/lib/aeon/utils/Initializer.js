/**
 * Provides an object that initializes modules in a controlled way.
 * @param {Function} onread A callback function to register as listener of the <c>done</c> event. Optional.
 * @event setup Fired after one of the modules has been setup. The event data contains two number properties;
 * <c>total</c>: the total number of modules registered with current instance, and <c>current</c>: the index of the
 * module that has just been setup.
 * @event done Fired when all of the modules have been setup.
 */
var Initializer = Dispatcher.extend(function Initializer(onready)
{
	this.construct("setup", "done");

	var init = this;
	var modules = [];
	var mainFunction = onready;
	var modulesReady = 0;

	/**
	 * Registers a module for initialization.
	 * @param {Object} module The object that will be initialized. It needs to have the <code>setup</code> method, which
	 * will be called to set it up. Optionally, if it has a <code>dispose</code> method, it will be called during
	 * the initializer's disposal.
	 * @param {Boolean} async A value indicating whether the object should be initialized asynchronously.
	 * @returns {Object} The <c>module</c> that was specified.
	 */
	this.register = function register(module, async)
	{
		$log.assert($type.isObject(module), "Argument 'module' is required");
		$log.assert($type.isFunction(module.setup), "The 'module' argument must have a 'setup' method");

		modules.push({ setup: $.proxy(module.setup, module), dispose: $.proxy(module.dispose, module), async: async || false });
		$log.info("Registered module {0}", $type.isFunction(module) ?
			Function.getName(module) :
			Function.getName(module.constructor));
	};

	/**
	 * Starts the initialization.
	 */
	this.setup = function setup()
	{
		if (modules.length == 0)
		{
			done();
		}
		else
		{
			modulesReady = 0;
			for (var i = 0; i < modules.length; i++)
			{
				var module = modules[i];
				if (module.async)
				{
					module.setup(onModuleInitialized);
				}
				else
				{
					module.setup();
					onModuleInitialized();
				}
			}
		}
	};

	/**
	 * Calls dispose on any registed modules that implement their own <code>dispose</code> method.
	 */
	this.dispose = function dispose()
	{
		for (var i = 0; i < modules.length; i++)
		{
			var module = modules[i];
			if (module.dispose)
			{
				module.dispose();
			}
		}
	}

	function onModuleInitialized()
	{
		modulesReady += 1;
		init.fire("setup", { total: modules.length, current: modulesReady });

		if (modulesReady == modules.length)
			done();
	}

	function done()
	{
		init.fire("done");
		if ($type.isFunction(mainFunction))
			mainFunction();
	};
});
