Type.registerNamespace("aeon.net");

aeon.net.WebRequest = function WebRequest(url)
{
	this.$super("oncomplete", "ontimeout", "onabort");

	this.$url = url || String.EMPTY;
	this.$headers = { };
	this.$body = null;
	this.$httpVerb = null;
	this.$invokeCalled = false;
	this.$timeout = 0;
	this.$timer = null;
	this.$timedOut = false;
	this.$responseAvailable = false;

	this.responseText = null;
	this.responseXML = null;
	this.status = null;
	this.statusText = null;
};
aeon.net.WebRequest.inherits(aeon.Dispatcher);

aeon.net.WebRequest.resolveUrl = function WebRequest$resolveUrl(url, baseUrl)
{
	if (url && String(url).indexOf('://') !== -1)
		return String(url);

	if (!baseUrl || baseUrl.length === 0)
	{
		var baseElement = $("base");
		if (baseElement && baseElement.href && baseElement.href.length > 0)
			baseUrl = baseElement.href;
		else
			baseUrl = document.URL;
	}
	var qsStart = baseUrl.indexOf('?');
	if (qsStart !== -1)
		baseUrl = baseUrl.substr(0, qsStart);

	qsStart = baseUrl.indexOf('#');
	if (qsStart !== -1)
		baseUrl = baseUrl.substr(0, qsStart);

	baseUrl = baseUrl.substr(0, baseUrl.lastIndexOf('/') + 1);
	if (!url || url.length === 0)
		return baseUrl;

	if (url.charAt(0) === '/')
	{
		var slashslash = baseUrl.indexOf('://');
		if (slashslash === -1)
			throw Error("Base URL does not contain ://.");

		var nextSlash = baseUrl.indexOf('/', slashslash + 3);
		if (nextSlash === -1)
			throw Error.argument("Base URL does not contain another /.");

		return baseUrl.substr(0, nextSlash) + url;
	}
	else
	{
		var lastSlash = baseUrl.lastIndexOf('/');
		if (lastSlash === -1)
			throw Error.argument("Cannot find last / in base URL.");

		return baseUrl.substr(0, lastSlash+1) + url;
	}
};

aeon.net.WebRequest.createQueryString = function WebRequest$createQueryString(queryString, encodeMethod)
{
	if (!encodeMethod)
		encodeMethod = encodeURIComponent;

	var result = [];
	for (var arg in queryString)
	{
		if (Type.isFunction(queryString[arg]))
			continue;

		result.push(arg + "=" + Object.serialize(queryString[arg]));
	}
	return result.join("&");
};

aeon.net.WebRequest.createUrl = function WebRequest$createUrl(url, queryString)
{
	if (!queryString)
		return url;

	var qs = aeon.net.WebRequest.createQueryString(queryString);
	if (qs.length > 0)
	{
		var sep = "?";
		if (url && url.indexOf("?") !== -1)
			sep = "&";

		return url + sep + qs;
	}
	else
	{
		return url;
	}
};

aeon.net.WebRequest.prototype.getUrl = function WebRequest$getUrl()
{
	return this.$url;
};

aeon.net.WebRequest.prototype.setUrl = function WebRequest$setUrl(value)
{
	this.$url = value;
};

aeon.net.WebRequest.prototype.getHeaders = function WebRequest$getHeaders()
{
	return this.$headers;
};

aeon.net.WebRequest.prototype.getHttpVerb = function WebRequest$getHttpVerb()
{
	if (this.$httpVerb === null)
	{
		if (this.$body === null)
			return "GET";

		return "POST";
	}
	return this.$httpVerb;
};

aeon.net.WebRequest.prototype.setHttpVerb = function WebRequest$setHttpVerb(value)
{
	this.$httpVerb = value;
};

aeon.net.WebRequest.prototype.getBody = function WebRequest$getBody()
{
	return this.$body;
};

aeon.net.WebRequest.prototype.setBody = function WebRequest$setBody(value)
{
	this.$body = value;
};

aeon.net.WebRequest.prototype.getTimeout = function WebRequest$getTimeout()
{
	return this.$timeout;
};

aeon.net.WebRequest.prototype.setTimeout = function WebRequest$setTimeout(value)
{
	this.$timeout = value;
};

aeon.net.WebRequest.prototype.getResolvedUrl = function WebRequest$getResolvedUrl()
{
	return aeon.net.WebRequest.resolveUrl(this.$url);
};

aeon.net.WebRequest.prototype.execute = function WebRequest$execute()
{
	var body = this.getBody();
	var headers = this.getHeaders();
	var verb = this.getHttpVerb();
	var url = this.getResolvedUrl();

	this.$httpRequest = new XMLHttpRequest();
	this.$httpRequest.onreadystatechange = Function.createDelegate(this, this.onReadyStateChange); /**/
	this.$httpRequest.open(verb, url, true);

	if (headers)
		for (var header in headers)
			this.$httpRequest.setRequestHeader(header, headers[header]);

	if (verb.toLowerCase() === "post")
	{
		if ((headers === null) || !headers["Content-Type"])
			this.$httpRequest.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded; charset=utf-8');

		if (!body)
			body = String.EMPTY;
	}

	var timeout = this.getTimeout();
	if (timeout > 0)
		this.$timer = window.setTimeout(Function.createDelegate(this, this.onTimeout), timeout);

	this.$httpRequest.send(body);
};

aeon.net.WebRequest.prototype.abort = function WebRequest$abort()
{
	if (this.$httpRequest != null)
	{
		this.$httpRequest.abort();
		this.fireEvent("onabort");
	}
};

aeon.net.WebRequest.prototype.onReadyStateChange = function WebRequest$onReadyStateChange()
{
	if (this.$httpRequest.readyState === 4)
	{
		try
		{
			if (typeof(this.$httpRequest.status) === "undefined")
			{
				$log.error("Http request status is undefined!");
				this.fireEvent("onerror");
				return;
			}
			else if (!(this.$httpRequest.status == 0 || this.$httpRequest.status == 200))
			{
				$log.error("The request failed to load, the status is: " + this.$httpRequest.status);
				this.fireEvent("onerror");
				return;
			}
		}
		catch(ex)
		{
			$log.error(ex.message);
			return;
		}

		this.clearTimer();
		this.$responseAvailable = true;

		this.responseText = this.$httpRequest.responseText;
		this.responseXML = this.$httpRequest.responseXML;
		this.status = this.$httpRequest.status;
		this.statusText = this.$httpRequest.statusText;

		this.fireEvent("oncomplete");

		this.$xmlHttpRequest = null;
	}
};

aeon.net.WebRequest.prototype.onTimeout = function WebRequest$onTimeout()
{
	if (!this.$responseAvailable)
	{
		this.clearTimer();
		this.$timedOut = true;
		this.$xmlHttpRequest.onreadystatechange = new Function;
		this.$xmlHttpRequest.abort();

		this.fireEvent("ontimeout");

		this.$xmlHttpRequest = null;
	}
};

aeon.net.WebRequest.prototype.clearTimer = function WebRequest$clearTimer()
{
	if (this.$timer != null)
	{
		window.clearTimeout(this.$timer);
		this.$timer = null;
	}
};

