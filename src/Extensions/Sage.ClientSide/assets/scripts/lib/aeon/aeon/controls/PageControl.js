Type.registerNamespace("aeon.controls");

/**
 * Implements a simple multi-page control.
 * @event pagechanged
 * @event pagecreated
 * @event pagedeleted
 */
aeon.controls.PageControl = function PageControl(elem, settings)
{
	$assert.isHtmlElement(elem);

	this.$super(elem);
	this.addEvent("pagechanged", "pagecreated", "beforepagedelete", "pagedeleted");

	this.settings = new aeon.controls.PageControlSettings(settings, this.element);
	this._setupSwitches(this.$headerSwitches());

	this.$newButton()
		.bind("click", Function.createDelegate(this, this._onPageNewClick));

	this.$selectButton()
		.bind("click", Function.createDelegate(this, this._onPageSelectButtonMouseDown))
		.bind("click", Function.createDelegate(this, this._onPageSelectButtonClick));

	this.$selectorElement()
		.bind("click", Function.createDelegate(this, this._onPageSelectorClick))
		.bind("focusout", Function.createDelegate(this, this.hidePageSelector));

	this.$switchContainer()
		.bind("click", Function.createDelegate(this, this._onPageSelectorClick));

	var _refresh = Function.createDelegate(this, this._refresh); /**/
	$(window).resize(_refresh);

	this._refresh();
};

aeon.controls.PageControl.inherits(aeon.controls.HtmlControl);
aeon.controls.ControlRegistry.registerControl(aeon.controls.PageControl, ".tabcontrol, .pagecontrol");

/**
 * Creates a HTML element for this control, without adding it to the DOM.
 * @returns {HTMLElement} The element that can be used as a page control, created using the c
 */
aeon.controls.PageControl.createElement = function PageControl$createElement(settings)
{
	settings = settings || {};

	var newSwitchText = this.settings.newSwitchText || "New page";
	var newPageText = this.settings.newPageText || "New page";
	var templates = aeon.controls.PageControl.HtmlTemplates;

	var sw = templates.SWITCH.format(newSwitchText);
	var page = templates.PAGE.format(newPageText);
	var selector = templates.SELECTOR;

	var result = $(templates.MAIN.format(sw, page, selector))[0];
	$dom.makeUnselectable(result);

	return result;
};

/**
 * Sets the HTML string for the control's template element associated with the specified <c>name</c>.
 * @param {String} name The name of the template element to set.
 * @param {String} template The HTML string that represents the template.
 */
aeon.controls.PageControl.setTemplate = function PageControl$setTemplate(name, template)
{
	$assert.isString(name, "name is a required argument");
	$assert.isString(template, "template is a required argument");

	var templates = aeon.controls.PageControl.HtmlTemplates;
	var keys = Enumerable.from(templates).select("$.key").toArray();
	if (keys.indexOf(name) == -1)
	{
		$log.warn("Argument value '{0}' is not a valid element name. It must be one of '{1}'.", name, keys.join(","));
	}

	templates[name] = template;
};

/**
 * Gets the HTML string for the control's template element associated with the specified <c>name</c>.
 * @returns {String} The HTML string that represents the template with the specified <c>name</c>.
 */
aeon.controls.PageControl.getTemplate = function PageControl$getTemplate(element)
{
	$assert.isString(element, "element is a required argument");

	return aeon.controls.PageControl.HtmlTemplates[element];
};

aeon.controls.PageControl.prototype.showPageSelector = function PageControl$showPageSelector()
{
	var switches = this.$headerSwitches();
	var selector = this.$selectorElement();
	var selectButton = this.$selectButton();
	selector.css({ display: "block", visibility: "hidden" });

	if (selector.find("li").length == 0)
	{
		var templates = aeon.controls.PageControl.HtmlTemplates;

		var html = [];
		for (var i = 0; i < switches.length; i++)
			html.push(templates.SELECTOR_SWITCH.format(switches.eq(i).text().trim()));

		selector.html(html.join(String.EMPTY));
		selector.find(".switch").removeClass("current").eq(this.pageIndex()).addClass("current");
	}

	var align = this.settings.selectorAlign || String.EMPTY;

	var top, left;
	var relativeTop = (selectButton.offset().top - this.element.offset().top);
	var relativeLeft = (selectButton.offset().left - this.element.offset().left);

	if (align.indexOf("T") != -1)
		top = relativeTop + selectButton.outerHeight();

	else if (align.indexOf("B") != -1)
		top = (relativeTop + selectButton.outerHeight()) - selector.outerHeight();

	if (align.indexOf("L") != -1)
		left = relativeLeft + selectButton.outerWidth();

	else if (align.indexOf("R") != -1)
		left = (relativeLeft + selectButton.outerWidth()) - selector.outerWidth();

	selector.css({ left: left, top: top, visibility: "visible" }).addClass("opened");
	selector.focus();
	selectButton.addClass("opened");
};

aeon.controls.PageControl.prototype.hidePageSelector = function PageControl$hidePageSelector()
{
	var self = this;
	function hideIt() /**/
	{
		if (self.$selectorElement().prop("offsetHeight") != 0)
		{
			self.$selectorElement().hide();
			self.$selectButton().removeClass("opened");
		}
	}

	this.__hideTimeout = setTimeout(hideIt, 80);
};

aeon.controls.PageControl.prototype.activatePage = function PageControl$activatePage(pageIndex)
{
	if (!Type.isNumeric(pageIndex))
		return;

	if (pageIndex == this.pageIndex())
		return;

	var switches = this.$headerSwitches();
	var switches2 = this.$selectorSwitches();
	var pages = this.$pages();

	switches.removeClass("current").eq(pageIndex).addClass("current");
	switches2.removeClass("current").eq(pageIndex).addClass("current");
	pages.removeClass("current").eq(pageIndex).addClass("current");

	this._refresh();

	// now make sure the switch for the page that was created comes into view
	var container = this.$switchContainer();
	var scrollLeft = parseInt(container.css("left")) || 0;
	var boundsElem = container.parent();
	var boundsLeft = container.offset().left;
	var boundsRight = boundsLeft + boundsElem.innerWidth();

	var currSwitch = switches.eq(pageIndex);
	var switchLeft = currSwitch.offset().left + scrollLeft;
	var switchRight = switchLeft + currSwitch.outerWidth();

	if (switchLeft < boundsLeft)
	{
		container.css("left", scrollLeft + (boundsLeft - switchLeft));
	}
	else if (switchRight > boundsRight)
	{
		container.css("left", scrollLeft - (switchRight - boundsRight));
	}

	this.fireEvent("pagechanged");
};

aeon.controls.PageControl.prototype.pageIndex = function PageControl$pageIndex(value)
{
	if (Type.isNumeric(value))
		this.activatePage(value);

	return this.$pages().filter(".current").index();
};

aeon.controls.PageControl.prototype.createPage = function PageControl$createPage(pageName, pageHtml, activate)
{
	pageName = pageName || this.settings.newSwitchText;
	pageHtml = pageHtml || this.settings.newPageText;

	var templates = aeon.controls.PageControl.HtmlTemplates;
	var sw = $(templates.SWITCH.format(pageName));
	var sw2 = $(templates.SELECTOR_SWITCH.format(pageName));
	var page = $(templates.PAGE.format(pageHtml));

	this.$switchContainer().append(sw);
	this.$selectorElement().append(sw2);
	this.$pageContainer().append(page);
	this._setupSwitches(sw);

	var index = this.$headerSwitches().length - 1;
	if (activate)
		this.activatePage(index);

	$dom.makeUnselectable(sw);
	this.fireEvent("pagecreated", { index: index });
};

aeon.controls.PageControl.prototype.deletePage = function PageControl$deletePage(pageIndex)
{
	if (!Type.isNumeric(pageIndex))
		return;

	var pages = this.$pages();
	if (pageIndex < 0 || pageIndex > pages.length)
		return;

	var currentIndex = this.pageIndex();
	
	var evt = new aeon.Event(this, "beforepagedelete", { index: pageIndex, cancel: false });
	this.fireEvent(evt);
	
	if (evt.data.cancel === true)
		return false;

	this.$headerSwitches().eq(pageIndex).remove();
	this.$selectorSwitches().eq(pageIndex).remove();
	pages.eq(pageIndex).remove();

	this._refresh();
	this.fireEvent("pagedeleted", { index: pageIndex });

	if (currentIndex == pageIndex || currentIndex >= this.$headerSwitches().length)
	{
		this.activatePage(currentIndex - 1);
	}
};

aeon.controls.PageControl.prototype.toString = function PageControl$toString()
{
	return String.format("({0}) pages: {1}, current page: {2}", this.constructor.getName(), this.$pages().length, this.pageIndex());
};

/**
 * @private
 */
aeon.controls.PageControl.prototype._refresh = function PageControl$_refresh()
{
	var container = this.$switchContainer();
	var parent = container.parent();
	var switches = this.$headerSwitches();
	var totalWidth = 0;
	for (var i = 0; i < switches.length; i++)
	{
		var sw = switches.eq(i);
		totalWidth += sw.outerWidth(true);
		sw.css({ zIndex: i, width: sw.innerWidth() });
	}

	switches.filter(".current").css("z-index", switches.length + 10);
	container.css("width", totalWidth);
	if (totalWidth < parent.width())
		container.css("left", 0);
};

/**
 * @private
 */
aeon.controls.PageControl.prototype._setupSwitches = function PageControl$_setupSwitches(switches)
{
	switches.bind("click", Function.createDelegate(this, this._onPageSwitchClick));
	switches.find("del").bind("click", Function.createDelegate(this, this._onPageDeleteClick));
};

aeon.controls.PageControl.prototype._togglePageSelector = function PageControl$_togglePageSelector()
{
	if (this.$selectorElement().prop("offsetHeight") == 0)
		this.showPageSelector();
	else
		this.hidePageSelector();
};

/**
 * @private
 */
aeon.controls.PageControl.prototype._onPageSelectorClick = function PageControl$_onPageSelectorClick(e)
{
	var selector = $(e.target).closest(".selector");
	var sw = $(e.target).closest(".switch");
	var del = $(e.target).closest("del");

	if (del.length)
	{
		this.deletePage(sw.index());
		if (selector.length)
		{
			if (this.$headerSwitches().length)
				this.showPageSelector();
			else
				this.hidePageSelector();
		}
	}
	else
	{
		this.activatePage(sw.index());
		if (selector.length)
			this.hidePageSelector();
	}
};

/**
 * @private
 */
aeon.controls.PageControl.prototype._onPageSwitchClick = function PageControl$_onPageSwitchClick(e)
{
	var sw = $(e.currentTarget).closest(".switch");
	this.activatePage(sw.index());
};

/**
 * @private
 */
aeon.controls.PageControl.prototype._onPageDeleteClick = function PageControl$_onPageDeleteClick(e)
{
	var sw = $(e.currentTarget).closest(".switch");
	this.deletePage(sw.index());
};

/**
 * @private
 */
aeon.controls.PageControl.prototype._onPageNewClick = function PageControl$_onPageNewClick(e)
{
	this.createPage(null, null, true);
};

aeon.controls.PageControl.prototype._onPageSelectButtonMouseDown = function PageControl$_onPageSelectButtonMouseDown(e)
{
	clearTimeout(this.__hideTimeout);
};

/**
 * @private
 */
aeon.controls.PageControl.prototype._onPageSelectButtonClick = function PageControl$_onPageSelectButtonClick(e)
{
	this._togglePageSelector();
};

/**
 * @private
 */
aeon.controls.PageControl.prototype.$header = function PageControl$$header()
{
	return this.element.find("> .header");
};

/**
 * @private
 */
aeon.controls.PageControl.prototype.$switchContainer = function PageControl$$switchContainer()
{
	return this.$header().find(".wrapper");
};

/**
 * @private
 */
aeon.controls.PageControl.prototype.$pageContainer = function PageControl$$pageContainer()
{
	return this.element.find("> .pages");
};

/**
 * @private
 */
aeon.controls.PageControl.prototype.$newButton = function PageControl$$newButton()
{
	return this.$header().find(".new");
};

/**
 * @private
 */
aeon.controls.PageControl.prototype.$selectButton = function PageControl$$selectButton()
{
	return this.$header().find(".select");
};

/**
 * @private
 */
aeon.controls.PageControl.prototype.$selectorElement = function PageControl$$selectorElement()
{
	var result = this.element.find(".selector");
	result.attr("tabIndex", "1");

	if (result.length == 0)
	{
		result = $(aeon.controls.PageControl.HtmlTemplates.SELECTOR);
		this.element.append(result);
	}

	return result;
};

/**
 * @private
 */
aeon.controls.PageControl.prototype.$headerSwitches = function PageControl$$headerSwitches()
{
	return this.$switchContainer().find(".switch");
};

/**
 * @private
 */
aeon.controls.PageControl.prototype.$selectorSwitches = function PageControl$$selectorSwitches()
{
	return this.$selectorElement().find("li");
};

/**
 * @private
 */
aeon.controls.PageControl.prototype.$pages = function PageControl$$pages()
{
	return this.$pageContainer().find(".page");
};

/**
 * @class
 */
aeon.controls.PageControlSettings = function PageControlSettings(data, override)
{
	this.newSwitchText = this.getString("newSwitchText", data, override, "New page");
	this.newPageText = this.getString("newPageText", data, override, "New page");
	this.selectorAlign = this.getString("selectorAlign", data, override, "TR");
};
aeon.controls.PageControlSettings.inherits(aeon.Settings);

aeon.controls.PageControl.HtmlTemplates =
{
	MAIN:
		'<div class="pagecontrol">' +
			'<div class="switches">' +
				'<div class="wrapper">{0}</div>' +
				'<div class="controls">' +
					'<div class="new" title="Adds a new page"><span>+</span></div>' +
					'<div class="select" title="Toggles the page select control"><span>^</span></div>' +
				'</div>' +
			'</div>' +
			'{2}' +
			'<div class="pages">{1}</div>' +
		'</div>',

	PAGE:
		'<div class="page">{0}</div>',

	SWITCH:
		'<div class="switch"><span>{0}</span><del title="Close"></del></div>',

	SELECTOR:
		'<ul class="selector" tabIndex="1" hideFocus="true"></ul>',

	SELECTOR_SWITCH:
		'<li class="switch"><span>{0}</span><del title="Close"></del></li>'
};
