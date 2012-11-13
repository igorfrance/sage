/*# Include: HtmlControl.js */
Type.registerNamespace("aeon.controls");

aeon.controls.ThumbStrip = function ThumbStrip(element)
{
	this.$super(element);

	this.container = this.element.parent();

	/**
	 * Indicates that the element is currently reacting to the mouse movement
	 */
	this.moveEngaged = false;
	/**
	 * Indicates that the element is currently being animated
	 */
	this.animEngaged = false;

	this.widthStrip = 0;
	this.trackX = 0;

	this.minMouseX = 0;
	this.minMouseY = 0;
	this.maxMouseX = this.element.outerWidth();
	this.maxMouseY = this.element.outerHeight();

	this.startX = this.element.position().left;

	var callback = Function.createDelegate(this, this.stopAnimation); /**/
	this.move = new $anim.Move(this.element[0], { left: this.startX, duration: 400, easingFx: Math.easeOutExpo }, callback);

	var calculateValues = Function.createDelegate(this, this.calculateValues);

	$(document).bind("mousemove", Function.createDelegate(this, this.onDocumentMouseMove)); /**/
	$(window).bind("resize", calculateValues); /**/

	window.setInterval(calculateValues, 500);
	this.calculateValues();
}
aeon.controls.ThumbStrip.inherits(aeon.controls.HtmlControl);

aeon.controls.ThumbStrip.setup = function ThumbStrip$setup()
{
	aeon.controls.ThumbStrip.instances = [];
	$(".thumbstrip").each(function setupInstance(i, element)
	{
		aeon.controls.ThumbStrip.instances.push(new aeon.controls.ThumbStrip(element));
	});
}

aeon.controls.ThumbStrip.prototype.onDocumentMouseMove = function ThumbStrip$onDocumentMouseMove(e)
{
	if ((window.$drag && $drag.active) || this.container.outerWidth() == 0)
		return false;

	var html = $("html")[0];
	var mouseX = (e.clientX - this.container.offset().left) + html.scrollLeft;
	var mouseY = (e.clientY - this.container.offset().top) + html.scrollTop;

	this.moveEngaged = (this.widthStrip > this.container.outerWidth()) &&
		(mouseX >= this.minMouseX && mouseX <= this.maxMouseX &&
		 mouseY >= this.minMouseY && mouseY <= this.maxMouseY);

	if (!this.moveEngaged)
		return;

	var targetLeft = this.getTargetLeft(mouseX, aeon.controls.ThumbStrip.XTRA_WIDTH);
	var stripX = this.element.position().left || 0;
	var distanceX = stripX - targetLeft;

	var animEngaged = Math.abs(distanceX) > 10;

	this.move.setEndValue({ left: targetLeft });

	if (animEngaged && !this.animEngaged)
	{
		this.animEngaged = true;
		this.move.run();
	}

	if (!this.animEngaged)
		this.element.css({ left: targetLeft });

}

aeon.controls.ThumbStrip.prototype.stopAnimation = function ThumbStrip$stopAnimation()
{
	this.animEngaged = false;
}

aeon.controls.ThumbStrip.prototype.calculateValues = function ThumbStrip$calculateValues()
{
	this.widthStrip = this.element[0].scrollWidth + aeon.controls.ThumbStrip.XTRA_WIDTH;
	this.trackX = (this.widthStrip - this.container.outerWidth()) + this.startX;

	this.minMouseX = 0;
	this.minMouseY = 0;
	this.maxMouseX = this.container.outerWidth();
	this.maxMouseY = this.container.outerHeight();

	this.minStripX = -(this.element[0].scrollWidth - this.container.outerWidth());
	this.maxStripX = 0;
}

aeon.controls.ThumbStrip.prototype.getTargetLeft = function ThumbStrip$getTargetLeft(mouseX, offsetEdge)
{
	offsetEdge = offsetEdge || 0;

	var trackX = this.trackX + (offsetEdge);
	var ratio = trackX / this.container.outerWidth();
	var point = Math.round( -(mouseX * ratio) + offsetEdge );

	if (point > this.maxStripX)
		point = this.maxStripX;
	if (point < this.minStripX)
		point = this.minStripX;

	return this.startX + point;
}

aeon.controls.ThumbStrip.XTRA_WIDTH = 355;

$(window).ready(aeon.controls.ThumbStrip.setup);
