Type.registerNamespace("sage.dev");

sage.dev.LogViewer = new function LogViewer()
{
	var header, content;

	function setup()
	{
		header = $(".logviewer .header");
		content = $(".logviewer .content");

		content.bind("scroll", synchronizeScrolling);
	}

	function synchronizeScrolling()
	{
		header.css("left", -content.prop("scrollLeft"));
	}

	$(window).load(setup);
};
