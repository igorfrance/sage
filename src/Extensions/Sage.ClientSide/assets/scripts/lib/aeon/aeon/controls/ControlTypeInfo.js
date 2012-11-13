Type.registerNamespace("aeon.controls");

/**
 * Provides information about types of controls.
 * @class
 * @param {Function} type The constructor function of the control type.
 * @param {String} expression The sizzle expression that selects the elements of this control type (e.g. '.button').
 * @param {Boolean} isAsynchronous If <c>true</c>, indicates that the control type initializes asynchronously
 */
aeon.controls.ControlTypeInfo = function ControlTypeInfo(type, expression, isAsynchronous)
{
	$assert.isFunction(type);
	$assert.isString(expression);

	this.type = type;
	this.typeName = type.getName();
	this.typeInitialized = false;
	this.expression = expression;
	this.isAsynchronous = isAsynchronous;
	this.constructors = {};
	this.instances = {};

	type.info = this;
	for (var name in aeon.controls.ControlTypeInfo.StaticControlInterface)
	{
		type[name] = aeon.controls.ControlTypeInfo.StaticControlInterface[name];
	}
};

aeon.controls.ControlTypeInfo.prototype.setup = function ControlTypeInfo$setup(onSetupReady)
{
	$assert.isFunction(onSetupReady);

	if (this.isAsynchronous)
	{
		if (!Type.isFunction(this.type.setup))
		{
			throw Error(String.format(
				"For asynchronous initalization, the control type {0} needs its own static setup method", this.type.getName()
			));
		}

		this.type.setup(onSetupReady);
	}
	else
	{
		if (Type.isFunction(this.type.setup))
			this.type.setup();

		onSetupReady();
	}

	this.typeInitialized = true;
};

/**
 * Registers a constructor for creating control instances on elements that match the specified css expression.
 * @example
 * Button.registerClass(MyConstructor, "#mybutton");
 * @example
 * Button.registerClass(RedButton, "div.buttons .button.red");
 * @param {Function} constructor The constructor function to use.
 * @param {String} expression The css expression that specifies for which elements this constructor applies.
 */
aeon.controls.ControlTypeInfo.prototype.registerClass = function ControlTypeInfo$registerClass(constructor, expression)
{
	$assert.isFunction(constructor);
	$assert.isString(expression);

	this.constructors[expression] = constructor;
};

aeon.controls.ControlTypeInfo.prototype.getType = function ControlTypeInfo$getType()
{
	return this.type;
};

aeon.controls.ControlTypeInfo.prototype.getTypeName = function ControlTypeInfo$getTypeName()
{
	return this.typeName;
};

aeon.controls.ControlTypeInfo.prototype.getConstructor = function ControlTypeInfo$getConstructor(element)
{
	$assert.isHtmlElement(element);

	var constructor = this.type;
	for (var expression in this.constructors)
	{
		if (Sizzle.matches(expression, [element]).length != 0)
		{
			constructor = this.constructors[expression];
			break;
		}
	}

	return constructor;
};

aeon.controls.ControlTypeInfo.prototype.getControl = function ControlTypeInfo$getControl(control)
{
	$assert.isNotNull(control);

	if (Type.isElement(control))
	{
		return this.getControlByElement(control);
	}

	return this.getControlByID(control);
};

aeon.controls.ControlTypeInfo.prototype.getControlByID = function ControlTypeInfo$getControlByID(controlID)
{
	$assert.isString(controlID);

	return this.instances[controlID];
};

aeon.controls.ControlTypeInfo.prototype.getControlByElement = function ControlTypeInfo$getControlByElement(element)
{
	$assert.isElement(element);

	var control = null;
	var instances = this.instances;
	for (var controlID in instances)
	{
		if (instances[controlID].getElement == null)
		{
			throw Error(String.format("The control '{0}' doesn't implement getElement() method.", controlID));
		}

		if (instances[controlID].getElement()[0] == element)
		{
			control = instances[controlID];
			break;
		}
	}

	return control;
};

aeon.controls.ControlTypeInfo.prototype.getControls = function ControlTypeInfo$getControls()
{
	return this.instances;
};

aeon.controls.ControlTypeInfo.prototype.createInstance = function ControlTypeInfo$createInstance(element, settings)
{
	$assert.isHtmlElement(element);

	var control = this.getControl(element);

	if (!control)
	{
		var id = element.id || $dom.uniqueID(element);
		var ctor = this.getConstructor(element);
		var control = new ctor(element, settings);
		if (Type.isFunction(control.initialize))
		{
			control.initialize();
		}
		control.setId(id);

		this.instances[id] = control;
	}

	return control;
};

/**
 * Defines the set of methods that will be copied onto the type being registered as a control.
 * @see {aeon.controls.ControlRegistry.registerControl}
 */
aeon.controls.ControlTypeInfo.StaticControlInterface =
{
	/**
	 * Creates a new control of current type.
	 * When attached to existing types, this method can than be called as a static method of that type to create
	 * instances of the type that it is attached to.
	 * @example Create a new button
	 * Button.create($("button.big"));
	 * @example Create a new tab control
	 * TabControl.create(myElement);
	 * @param {HTMLElement} element The element that represents the control to create. Optional.
	 * @param {Object} settings The object that holds the settings for the control to create. Optional.
	 * @return {Object} The control that was created.
	 */
	create: function StaticControlInterface$create(element, settings)
	{
		if (element == null)
		{
			if (Type.isFunction(this.createElement))
			{
				element = this.createElement(settings);
			}
			else
			{
				throw Error(String.format(
					"No element has been provided and the control '{0}' doesn't have 'createElement' method, therefore no new element can be created", this.getName()));
			}
		}
		return this.info.createInstance(element, settings);
	},

	/**
	 * Gets the javascript control instance for the specified element.
	 * @param {Object} element Either a <c>String</c> (id of the HTML element) or the <c>HTMLElement</c> itself for which
	 * to get the control instance.
	 * @return {Object} The control instance associated with the specified element.
	 */
	getControl: function StaticControlInterface$getControl(element)
	{
		return this.info.getControl(element);
	},

	/**
	 * Registers a constructor for creating control instances on elements that match the specified css expression.
	 * @param {Function} type The constructor function to use.
	 * @param {String} expression The css expression that specifies for which elements this constructor applies.
	 */
	registerClass: function StaticControlInterface$registerClass(type, expression)
	{
		this.info.registerClass(type, expression);
	}
};

