Type.registerNamespace("aeon");

/**
 * Provides cross-browser helper for working with and tracking changes of the document location hash value.
 * @class
 * @event onhashchange Fires when the location hash changes.
 */
aeon.HashHandler = function HashHandler()
{
	this.$super("onhashchange");

	/**
	 * Maintains the current hash value, and is a reflection of the main document location's hash.
	 * @type {String}
	 */
	this.outerHash = $url.getHash();

	/**
	 * Maintains the current hash value within the hidden IFRAME, used for Internet Explorer < 8.
	 * @type {String}
	 */
	this.innerHash = null;

	/**
	 * Helps Internet Explorer < 8 keep track of hash changes.
	 * @type {HTMLIFrameElement}
	 */
	this.iframe = null;

	var checkLocation = Function.createDelegate(this, this.checkLocation); /**/
	if (window.ActiveXObject)
	{
		if (!document.documentMode || document.documentMode < 8)
		{
			this.setup();
		}
		else
		{
			window.attachEvent("onhashchange", checkLocation);
		}
	}
	else
	{
		if (window.history.navigationMode)
			window.history.navigationMode = "compatible";

		window.setInterval(checkLocation, 50);
	}
};
aeon.HashHandler.inherits(aeon.Dispatcher);

/**
 * @private
 */
aeon.HashHandler.prototype.setup = function HashHandler$setup()
{
	if (document.body == null)
	{
		window.setTimeout(Function.createDelegate(this, this.setup), 10);
		return;
	}

	this.iframe = document.body.appendChild(document.createElement("iframe"));
	this.iframe.style.cssText = "position: absolute; width: 1px; height: 1px; top: -1000px; left: -1000px; display: none";

	this.setInnerHash(this.outerHash);
	this.innerHash = this.outerHash;

	window.setInterval(Function.createDelegate(this, this.checkInnerLocation), 50); /**/
};

/**
 * Sets the location hash value, triggering the <c>onhashchange</c> event.
 */
aeon.HashHandler.prototype.setHash = function HashHandler$setHash(newHash)
{
	if (newHash == this.outerHash)
		return;

	if (this.iframe)
		this.setInnerHash(newHash);

	else
	{
		location.hash = this.outerHash = newHash;
		this.onHashChange();
	}
};

/**
 * @private
 */
aeon.HashHandler.prototype.setInnerHash = function HashHandler$setInnerHash(newHash)
{
	var doc = this.iframe.contentWindow.document;
	doc.open();
	doc.write(String.format("<html><body>{0}</body></html>", newHash));
	doc.close();
	this.outerHash = newHash;
};

/**
 * @private
 */
aeon.HashHandler.prototype.checkLocation = function HashHandler$checkLocation()
{
	var currentHash = $url.getHash();
	if (currentHash != this.outerHash)
	{
		this.outerHash = currentHash;
		this.onHashChange();
	}
};

/**
 * @private
 */
aeon.HashHandler.prototype.checkInnerLocation = function HashHandler$checkInnerLocation()
{
	var curData, curHash;

	try
	{
		curData = this.iframe.contentWindow.document.body.innerText;
		if (curData != this.innerHash)
		{
			this.innerHash = curData;
			location.hash = this.outerHash = curData;
			this.onHashChange();
		}
		else
		{
			curHash = $url.getHash();
			if (curHash != this.outerHash)
				this.setInnerHash(curHash);
		}
	}
	catch (e) {}
};

/**
 * @private
 */
aeon.HashHandler.prototype.onHashChange = function HashHandler$onHashChange()
{
	this.fireEvent("onhashchange", { hash: this.outerHash });
};

var $hash = new aeon.HashHandler();
