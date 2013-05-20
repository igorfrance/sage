/**
 * Provides information about a registered control.
 * @param {Object} control The object that implements the control.
 */
var ControlTypeInfo = function ControlTypeInfo(type)
{
	//// Initialize the control
	type.create = type.create || createInstance;
	type.createElement = type.createElement || createElement;
	type.get = type.get || getInstance;
	type.register = type.register || registerConstructor;
	type.dispose = type.dispose || Function.EMPTY;
	type.instances = new Array;
	type.registeredConstructors = {};
	type.registerInstance = registerInstance;

	var info = this;
	info.type = type;
	info.defaultConstructor = type.Control || type;
	info.typeName = Function.getName(type);
	info.expression = type.expression;
	info.async = type.async === true;
	info.getInstance = getInstance;
	info.createInstance = createInstance;

	/**
	 * Cheks whether the specified function is either the defaul constructor or one of the registered constructors of this type.
	 * @param {Function} typeFunction The function to check.
	 * @returns {Boolean} <c>true</c> if the specified function is either the defaul constructor or one of the
	 * registered constructors of this type; otherwise <c>false</c>.
	 */
	this.checkType = function ControlType$checkType(typeFunction)
	{
		if (!$type.isFunction(typeFunction))
			return false;

		if (info.defaultConstructor == typeFunction)
			return true;

		for (var expression in type.registeredConstructors)
		{
			if (type.registeredConstructors[expression] == typeFunction)
				return true;
		}

		return false;
	};

	/**
	 * Calls static redraw method of the registered control, if the control implements it.
	 */
	this.redraw = function ControlType$redraw()
	{
		if ($type.isFunction(type.redraw))
		{
			type.redraw();
		}
	};

	/**
	 * Sets up the control associated with this instance.
	 * @param {Function} onTypeInitialized The callback function for asynchronous initialization.
	 */
	this.setup = function ControlType$setup(onTypeInitialized)
	{
		if (type.setup && type.async)
		{
			type.setup(onTypeInitialized);
		}
		else if (type.setup)
		{
			type.setup();
			onTypeInitialized();
		}
		else
		{
			onTypeInitialized();
		}
	};

	/**
	 * Call the <code>dispose</code> method of the associated control.
	 */
	this.dispose = function ControlType$dispose()
	{
		if (type.dispose)
		{
			type.dispose();
		}
	};

	/**
	 * Creates a new HTML element for use with the registered control.
	 * @returns {HTMLElement} A new HTML element for use with the registered control.
	 */
	function createElement()
	{
		var result = document.createElement("div");
		result.setAttribute("id", aeon.dom.uniqueID(result));
		if (String(type.expression).trim().replace(/\./g, " ").match(/([\w\- ]+)/))
			result.setAttribute("className", RegExp.$1);

		return result;
	};

	/**
	 * Creates a new control.
	 * @example Create a new button
	 * Button.create($("button.big"));
	 * @example Create a new tab control
	 * TabControl.create(myElement);
	 * @param {HTMLElement} element The element that represents the control to create. Optional.
	 * @param {Object} settings The object that holds the settings for the control to create. Optional.
	 * @return {Object} The control that was created.
	 */
	function createInstance(element, settings)
	{
		var depth = arguments[2] || 0;
		var prefix = $string.repeat("  ", depth);

		if (element == null)
		{
			if ($type.isFunction(this.createElement))
			{
				element = this.createElement(settings);
			}
			else
			{
				throw Error($string.format(
					"No element has been provided and the control '{0}' doesn't have 'createElement' method, " +
					"therefore a new element can't be created", Function.getName(this)));
			}
		}

		var control = getInstance(element);
		if (control == null)
		{
			var ctor = getConstructor(element);
			var control = new ctor(element, settings);
			if ($type.isFunction(control.init))
				control.init();

			registerInstance(control);
			$log.debug("{0}Created control {1}", prefix, control);
		}
		else
		{
			$log.warn("{0}Not creating another instance of {1} for {2}.", prefix, control, element);
		}

		return control;
	};

	function registerInstance(control)
	{
		if (!$type.instanceOf(control, HtmlControl))
			return;

		control.$element.attr("id", $dom.uniqueID(control.$element));

		if (!type.instances.contains(control))
			type.instances.push(control);
	}

	/**
	 * Gets the contructor function registered for the specified <c>element</c>.
	 * @param {HTMLElement|jQuery} element Either the HTML element of jQuery selection of it.
	 * @returns {Function} The function registered and the control constructor for the specified <c>element</c>.
	 */
	function getConstructor(element)
	{
		var constructor = info.defaultConstructor;
		var $element = $(element);

		for (var expression in type.registeredConstructors)
		{
			if ($element.is(expression))
			{
				constructor = type.registeredConstructors[expression];
				break;
			}
		}

		return constructor;
	};

	/**
	 * Gets the control instance associated with the specified <c>element</c>.
	 * @param {String|Element} element Either the HTML element or ID of the element for
	 * which to get the control instance.
	 * @returns {HtmlControl} The control instance associated with the specified <c>element</c>.
	 */
	function getInstance(element)
	{
		if (element == null)
			return null;

		if (element.jquery)
			element = element[0];

		if ($type.isString(element))
			element = $(element)[0];

		if (element == null)
			return null;

		var checkString = $type.isString(element);
		var checkElement = $type.isElement(element);

		var control = null;
		for (var i = 0; i < type.instances.length; i++)
		{
			var instance = type.instances[i];
			if (checkElement && instance.element()[0] == element)
				return instance;

			if (checkString && instance.id() == element)
				return instance;
		}

		return null;
	};

	/**
	 * Registers a constructor to use (instead of the default constructor) when
	 * creating control instances for elements that match the specified css expression.
	 *
	 * @example
	 * Button.registerConstructor(MyConstructor, "#mybutton");
	 * @example
	 * Button.registerConstructor(RedButton, "div.buttons .button.red");
	 * @param {String} expression The css expression that specifies for which elements
	 * this constructor applies.
	 * @param {Function} constructor The constructor function to use.
	 */
	function registerConstructor(expression, constructor)
	{
		if (!$type.isString(expression) && !$type.isFunction(constructor))
			return $log.warn("Arguments expression:String and constructor:Function both need to be used");

		type.registeredConstructors[expression] = constructor;
	};
};
