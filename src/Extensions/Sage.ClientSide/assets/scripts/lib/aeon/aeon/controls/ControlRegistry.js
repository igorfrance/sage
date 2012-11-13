Type.registerNamespace("aeon.controls");

/**
 * Maintains a registry of initialized controls.
 * All controls are initialized and registered through static methods of <c>ControlBase</c>. This class helps maintain
 * a registry of these controls and provides access to them.
 */
aeon.controls.ControlRegistry = new function aeon$ControlRegistry()
{
	this.types = [];
	this.typesReady = 0;
};

/**
 * Registers a function that represents a control; the control's constructor.
 * The supplied function should have a <c>setup</c> method, that <c>DisplayController</c> will call when the
 * page loads. If the control need to load some files before it is fully initialized, you can use the <c>isAsynchronous</c>
 * argument to indicate that the <c>DisplayController</c> should wait for the control to call it back when the loading has
 * completed and the control has been fully initialized.
 * @param {Function} controlType The function that creates the control.
 * @param {String} expression The sizzle expression that selects the elements of this control type (e.g. '.button').
 * @param {Boolean} isAsynchronous A value indicating that the specified control needs to setup asynchronously
 * before it is fully initialized.
 */
aeon.controls.ControlRegistry.registerControl = function ControlRegistry$registerControl(controlType, expression, isAsynchronous)
{
	$assert.isFunction(controlType);
	$assert.isString(expression);

	isAsynchronous = isAsynchronous == true;
	var typeInfo = new aeon.controls.ControlTypeInfo(controlType, expression, isAsynchronous);
	this.types.push(typeInfo);

	$log.info("Registered control {0} for expression '{1}' (async: {2}).", controlType.getName(), expression, isAsynchronous);
};

/**
 * Sets up all registered controls and calls the <c>onSetupReady</c> function when completed.
 * The setup works in the following way:
 * <ul>
 * <li>All elements that match the control expressions are selected</li>
 * <li>The setup method is called for all control types, waiting for any async controls to complete their setup.</li>
 * <li>The <c>createControls</c> method is called with the elements that have been selected.</li>
 * </ul>
 * @type {Function} onSetupReady The function to call when all controls have been setup.
 */
aeon.controls.ControlRegistry.setup = function ControlRegistry$setup(onSetupReady)
{
	if (this.types.length == 0)
	{
		if (Type.isFunction(onSetupReady))
			onSetupReady();

		return;
	}

	var expressions = [];
	var typesReady = 0;

	for (var i = 0; i < this.types.length; i++)
	{
		expressions.push(this.types[i].expression);
	}

	var self = this;
	var elements = $(expressions.join(", "));

	function ControlRegistry$setup$onTypeInitialized() /**/
	{
		if (++typesReady == self.types.length)
		{
			self.createControls(elements);
			if (Type.isFunction(onSetupReady))
				onSetupReady();
		}
	}

	for (var i = 0; i < this.types.length; i++)
	{
		var typeInfo = this.types[i];
		if (!typeInfo.typeInitialized)
			typeInfo.setup(ControlRegistry$setup$onTypeInitialized);
		else
			ControlRegistry$setup$onTypeInitialized();
	}
};

aeon.controls.ControlRegistry.dispose = function ControlRegistry$dispose()
{
	while (this.types.length)
	{
		var typeInfo = this.types.shift();
		try
		{
			if (Type.isFunction(typeInfo.dispose))
				typeInfo.dispose();
		}
		catch(e)
		{
			continue;
		}
	}
};

aeon.controls.ControlRegistry.createControls = function ControlRegistry$createControls(elements)
{
	$assert.isArray(elements);

	for (var i = 0; i < this.types.length; i++)
	{
		var typeInfo = this.types[i];
		var matches = elements.filter(typeInfo.expression);

		for (var j = 0; j < matches.length; j++)
		{
			typeInfo.createInstance(matches[j]);
		}
	}
};

/**
 * Searches through the registered controls and find an instance that matches the specification.
 * @param {Object} control Either the id of an HTML element or the HTML element itself.
 * @param {Object} type Either the class name associated with a control type, the name of the type or the control type itself.
 * @returns {Control} The control associated with the specified element, and optionally type.
 */
aeon.controls.ControlRegistry.getControl = function ControlRegistry$getControl(control, type)
{
	if (control == null)
		return null;

	var result = null;
	for (var i = 0; i < this.types.length; i++)
	{
		var controlType = this.types[i];
		if (type != null)
		{
			if (controlType.expression.indexOf(type) != -1 || controlType.getType() == type || controlType.getTypeName() == type)
			{
				result = controlType.getControl(control);
				break;
			}
		}
		else
		{
			// if no type has been specified, return the first matching control.
			result = controlType.getControl(control);
			if (result != null)
			{
				break;
			}
		}
	}

	return result;
};

aeon.Initializer.registerModule(aeon.controls.ControlRegistry, true);

/**
 * Global alias to <c>aeon.controls.ControlRegistry</c>
 * @type {aeon.controls.ControlRegistry}
 */
var $ctrl = aeon.controls.ControlRegistry;
