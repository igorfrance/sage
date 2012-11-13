Type.registerNamespace("aeon");

aeon.Initializer = new function Initializer()
{
	this.modules = [];
	this.mainMethod = null;
};

aeon.Initializer.registerMain = function Initializer$registerModule(mainMethod)
{
	$assert.isFunction(mainMethod);

	this.mainMethod = mainMethod;
};

aeon.Initializer.registerModule = function Initializer$registerModule(moduleType, isAsync)
{
	$assert.isObject(moduleType);
	$assert.isFunction(moduleType.setup, "The module being registered must have the static setup method");

	this.modules.push({ type: moduleType, isAsync: isAsync });

	$log.info("Registered module {0}", Type.isFunction(moduleType)
		? moduleType.getName()
		: moduleType.constructor.getName()
	);
};

aeon.Initializer.setup = function Initializer$setup()
{
	var modulesReady = 0;

	function aeon$setup$onModuleInitialized() /**/
	{
		if (++modulesReady == aeon.Initializer.modules.length)
		{
			aeon.Initializer.start();
		}
	}

	if (aeon.Initializer.modules.length == 0)
	{
		aeon.Initializer.start();
	}
	else for (var i = 0; i < aeon.Initializer.modules.length; i++)
	{
		var module = aeon.Initializer.modules[i];
		if (module.isAsync)
		{
			module.type.setup(aeon$setup$onModuleInitialized);
		}
		else
		{
			module.type.setup();
			aeon$setup$onModuleInitialized();
		}
	}
};

aeon.Initializer.dispose = function Initializer$dispose()
{
	for (var i = 0; i < aeon.Initializer.modules.length; i++)
	{
		var module = aeon.Initializer.modules[i];
		if (Type.isFunction(module.type.dispose))
			module.type.dispose();
	}
};

aeon.Initializer.start = function Initializer$start()
{
	if (Type.isFunction(this.mainMethod))
	{
		this.mainMethod();
	}
};

$evt.addListener(window, "onready", aeon.Initializer.setup);
$evt.addListener(window, "onunload", aeon.Initializer.dispose);

/**
 * Global alias to <c>aeon.Initializer</c>.
 * @type {aeon.Initializer}
 */
var $init = aeon.Initializer;
