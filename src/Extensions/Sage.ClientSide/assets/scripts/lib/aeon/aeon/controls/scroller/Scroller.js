Type.registerNamespace("aeon.controls");

/**
 * Implements a simple scroller control.
 */
aeon.controls.Scroller = function Scroller(elem, settings)
{
	$assert.isHtmlElement(elem);

	this.$super(elem, "onchange", "onreset", "ondragcomplete");

	this.settings = new aeon.controls.ScrollerSettings(settings, this.element);

	if (this.settings.scrollTarget == null)
		return $log.warn("Scroller could not be initialized because the scrollTarget specification is missing");

	var targetElem = $(this.settings.scrollTarget);
	if (this.settings.scrollTarget == null)
		return $log.warn("Scroller could not be initialized because the scrollTarget element '{0}' could not be found", this.settings.scrollTarget);

	this.elements = {};
	this.elements.target = targetElem;
	this.elements.content = $("*", targetElem);
	this.elements.track = $(".track", this.element);
	this.elements.grip = $(".gripper", this.element);

	$css(this.elements.content, "position", "relative");

	$evt.addListener(this.elements.grip, "onmousedown", Function.createDelegate(this, this.onDragStart)); /**/
	$evt.addListener(this.elements.target, "onscroll", Function.createDelegate(this, this.onScroll)); /**/
	$evt.addListener(window, "onresize", Function.createDelegate(this, this.redraw)); /**/

	this.redraw();
};
aeon.controls.Scroller.inherits(aeon.controls.HtmlControl);

aeon.controls.Scroller.setup = function Scroller$setup()
{
	aeon.controls.Scroller.$scrollers = [];
	aeon.controls.Scroller.$elements = [];

	aeon.controls.Scroller.setupElement(document);

	$evt.addListener(window, "onresize", aeon.controls.Scroller.redraw);
};

aeon.controls.Scroller.redraw = function Scroller$redraw()
{
	if (Type.isArray(aeon.controls.Scroller.$scrollers))
	{
		for (var i = 0; i < aeon.controls.Scroller.$scrollers.length; i++)
			aeon.controls.Scroller.$scrollers[i].redraw();
	}
};

aeon.controls.Scroller.dispose = function Scroller$dispose()
{
	if (Type.isArray(aeon.controls.Scroller.$scrollers))
		while (aeon.controls.Scroller.$scrollers.length)
			aeon.controls.Scroller.$scrollers.shift().dispose();

	if (Type.isArray(aeon.controls.Scroller.$elements))
		while (aeon.controls.Scroller.$elements.length)
			aeon.controls.Scroller.$elements.shift();
};

aeon.controls.Scroller.prototype.dispose = function Scroller$dispose()
{
	this.$element = null;
};

aeon.controls.Scroller.prototype.redraw = function Scroller$redraw()
{
	var target = this.elements.target;
	var content = this.elements.content;
	var track = this.elements.track;
	var grip = this.elements.grip;

	var availableHeight = target.offsetHeight;
	var totalHeight = target.scrollHeight;

	if (totalHeight > availableHeight)
	{
		$css.show(track);

		var scrollGripAreaHeight = grip.parentNode.offsetHeight;
		var ratio = scrollGripAreaHeight / totalHeight;
		var scrollGripHeight = Math.round(availableHeight * ratio);

		this.scrollRatio = ratio; // 0.416
		$log.message("{0}/{1}={2}", scrollGripAreaHeight, totalHeight, this.scrollRatio);

		$css.pixelHeight(grip, scrollGripHeight);
	}
	else
	{
		$css.hide(track);
	}
};

aeon.controls.Scroller.prototype.positionContentFromScroller = function Scroller$positionContentFromScroller()
{
	var gripTop = $css.pixelTop(this.elements.grip);
	var targetTop = Math.round(gripTop / this.scrollRatio);

	this.elements.target.scrollTop = targetTop;
};

aeon.controls.Scroller.prototype.positionScrollerFromContent = function Scroller$positionScrollerFromContent()
{
	var scrollTop = this.elements.target.scrollTop;
	var targetTop = Math.round(scrollTop * this.scrollRatio);

	$css.pixelTop(this.elements.grip, targetTop);
};

aeon.controls.Scroller.prototype.onDragStart = function Scroller$onDragStart(e)
{
	$drag.addListener("onbeforedragmove", Function.createDelegate(this, this.onBeforeDragMove)); /**/
	$drag.addListener("ondragmove", Function.createDelegate(this, this.onDragMove)); /**/
	$drag.addListener("ondragstop", Function.createDelegate(this, this.onDragEnd)); /**/

	this.minScroll = 0;
	this.maxScroll =
		this.elements.track.offsetHeight -
		this.elements.grip.offsetHeight;

	$drag.start(e, $evt.srcElement(e), false, true);
	return false;
};

aeon.controls.Scroller.prototype.onBeforeDragMove = function Scroller$onBeforeDragMove(event)
{
	if (event.data.targetY < this.minScroll)
		event.data.targetY = this.minScroll;

	if (event.data.targetY > this.maxScroll)
		event.data.targetY = this.maxScroll;
};

aeon.controls.Scroller.prototype.onDragMove = function Scroller$onDragMove(event)
{
	this.positionContentFromScroller();
};

aeon.controls.Scroller.prototype.onDragEnd = function Scroller$onDragEnd(event)
{
	$drag.removeListener("onbeforedragmove", Function.createDelegate(this, this.onBeforeDragMove)); /**/
	$drag.removeListener("ondragmove", Function.createDelegate(this, this.onDragMove)); /**/
	$drag.removeListener("ondragstop", Function.createDelegate(this, this.onDragEnd)); /**/
};

aeon.controls.Scroller.prototype.onScroll = function Scroller$onScroll(e)
{
	this.positionScrollerFromContent();
};

/**
 * @class
 */
aeon.controls.ScrollerSettings = function ScrollerSettings(data, override)
{
	this.scrollTarget = this.getString("scrollTarget", data, override);
};
aeon.controls.ScrollerSettings.inherits(aeon.Settings);

aeon.controls.ControlRegistry.registerControl(aeon.controls.Scroller, "scroller");
