Type.registerNamespace("aeon");

aeon.QueryString = function QueryString(url)
{
	this.scriptName = String.EMPTY;
	this.scriptPath = String.EMPTY;
	this.scriptNameFull = String.EMPTY;
	this.serverNameFull = String.EMPTY;
	this.protocol = String.EMPTY;
	this.serverName = String.EMPTY;
	this.query = String.EMPTY;
	this.hash = String.EMPTY;

	this.hashParams = {};
	this.queryParams = {};

	if (url != null)
		this.parse(url);
	else
		this.parse(location.href);
};

aeon.QueryString.prototype.getItem = function QueryString$getItem(itemKey)
{
	return this.param[itemKey];
};

aeon.QueryString.prototype.parse = function QueryString$parse(url)
{
	if (url == null)
		return;

	this.url = new String(url);
	if (this.url.match(/^(((https?|ftp):\/\/([a-z-A-Z0-9.]+))([^#\?]+))?(?:\?([^#]*))?(?:\#(.*))?$/))
	{
		this.scriptNameFull = RegExp.$1;
		this.scriptPath = RegExp.$5;
		this.serverName = RegExp.$4;
		this.serverNameFull = RegExp.$2;
		this.protocol = RegExp.$3;
		this.query = RegExp.$6;
		this.hash = RegExp.$7;

		if (!this.scriptPath.match(/\//))
		{
			this.scriptName = this.scriptPath;
			this.scriptPath = String.EMPTY;
		}
		else if (this.scriptPath.match(/^(.*\/)(.*)$/))
		{
			this.scriptPath = RegExp.$1;
			this.scriptName = RegExp.$2;
		}
	}
	else
	{
		this.query = this.url;
	}

	this.hashParams = aeon.QueryString.parseQueryString(this.hash);
	this.queryParams = aeon.QueryString.parseQueryString(this.query);
};

aeon.QueryString.parseQueryString = function Uri$parseQueryString(query)
{
	var result = {};
	if (!query)
		return result;

	var param = String(query).split("&");
	for (var i = 0; i < param.length; i++)
	{
		if (param[i].length == 0)
			continue;

		var pair = param[i].split("=");
		var itemKey = pair[0];
		var itemValue = pair[1];

		if (result[itemKey] != null)
		{
			if (!Type.isArray(result[itemKey]))
				result[itemKey] = [result[itemKey]];

			result[itemKey].push(itemValue);
		}
		else
			result[itemKey] = itemValue;
	}

	return result;
};

/**
 * Creates a query string initialized around properties of the supplied object.
 * @param {Object} obj The object to parse
 * @returns {aeon.QueryString} The resulting query string object.
 */
aeon.QueryString.fromObject = function QueryString$fromObject(obj)
{
	var result = new aeon.QueryString(String.EMPTY);
	for (var name in obj)
	{
		result.queryParams[name] = obj[name];
	}

	return result;
};

aeon.QueryString.prototype.toString = function QueryString$toString(full)
{
	var param = new Array();
	for (var name in this.param)
	{
		if (this.param[name] && this.param[name].constructor == Array)
		{
			for (var i = 0; i < this.param[name].length; i++)
				param.push(escape(name) + "=" + escape(this.param[name][i]));
		}
		else
		{
			var itemValue = String(this.param[name]).match(/\{.*\}/) ? this.param[name] : escape(this.param[name]);
			param.push(escape(name) + "=" + itemValue);
		}
	}

	var query = param.join("&");
	if (full == true)
		var result = (param.length ? this.scriptNameFull + "?" + query : this.scriptNameFull) + this.hash;
	else
		var result = query;

	return result;
};

