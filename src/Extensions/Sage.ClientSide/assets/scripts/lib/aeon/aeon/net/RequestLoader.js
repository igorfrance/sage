Type.registerNamespace("aeon.net");

aeon.net.RequestLoader = function RequestLoader()
{
	this.$super("onload", "oncomplete");

	this.$requests = [];
	this.$loadCount = 0;
};
aeon.net.RequestLoader.inherits(aeon.Dispatcher);

aeon.net.RequestLoader.prototype.addRequest = function RequestLoader$addRequest(requestUrl)
{
	var request = new aeon.net.WebRequest(requestUrl);
	request.addListener("oncomplete", Function.createDelegate(this, this.requestLoaded)); /**/
	this.$requests.push(request);

	return request;
};

aeon.net.RequestLoader.prototype.requestLoaded = function RequestLoader$requestLoaded(event)
{
	var request = event.source;
	this.fireEvent("onload", { request: request });

	if (++this.$loadCount == this.$requests.length)
		this.fireEvent("oncomplete");
};

aeon.net.RequestLoader.prototype.load = function RequestLoader$load()
{
	for (var i = 0; i < this.$requests.length; i++)
		this.$requests[i].execute();
};

