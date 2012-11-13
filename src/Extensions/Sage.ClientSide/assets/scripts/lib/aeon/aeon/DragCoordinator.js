Type.registerNamespace("aeon");

/**
 * Utility object that helps with handling drag & drop operations.
 * Drag & drop can only be done one object at a time.
 * @type {aeon.Dispatcher}
 */
aeon.DragCoordinator = new aeon.Dispatcher("ondragstart", "ondragstop", "onbeforedragmove", "ondragmove");

/**
 * @type {Boolean}
 */
aeon.DragCoordinator.active = false;

/**
 * @type {Boolean}
 */
aeon.DragCoordinator.overlayCss = "position: absolute; background: #fff; opacity: .01; z-index:10000;";

aeon.DragCoordinator.start = function DragCoordinator$start(e, target, moveX, moveY)
{
	var event = $evt(e);
	if (event == null || !Type.isElement(target))
		return;

	$drag.moveX = moveX;
	$drag.moveY = moveY;

	$drag.clientX = event.clientX;
	$drag.clientY = event.clientY;

	var pixelTop = parseInt($css.pixelTop(target));
	var pixelLeft = parseInt($css.pixelLeft(target));

	var offsetTop = $dom.offsetTop(target);
	var offsetLeft = $dom.offsetLeft(target);

	$drag.startX = isNaN(pixelLeft) ? offsetLeft : pixelLeft;
	$drag.startY = isNaN(pixelTop) ? offsetTop : pixelTop;

	$drag.targetX = $drag.startX;
	$drag.targetY = $drag.startY;

	$drag.diffX = 0;
	$drag.diffY = 0;

	$drag.target = target;

	if (target.setCapture)
	{
		$drag.target.$onmouseup = $drag.target.onmouseup;
		$drag.target.$onmousemove = $drag.target.onmousemove;

		$drag.target.onmouseup = $drag.stop;
		$drag.target.onmousemove = $drag.move;

		$drag.target.setCapture();
	}
	else
	{
		document.addEventListener("mouseup", $drag.stop, true);
		document.addEventListener("mousemove", $drag.move, true);

		$drag.overlayFrames();
	}
	$drag.active = true;
	$drag.fireEvent("ondragstart");

	return $evt.cancel(e);
};

aeon.DragCoordinator.stop = function DragCoordinator$stop(e)
{
	$drag.active = false;
	$drag.fireEvent("ondragstop");

	$drag.clientX = 0;
	$drag.clientY = 0;

	$drag.startX = 0;
	$drag.startY = 0;

	$drag.diffX = 0;
	$drag.diffY = 0;

	$drag.targetX = 0;
	$drag.targetY = 0;

	if ($drag.target.releaseCapture)
	{
		$drag.target.onmouseup = $drag.target.$onmouseup;
		$drag.target.onmousemove = $drag.target.$onmousemove;

		$drag.target.releaseCapture();
	}
	else
	{
		document.removeEventListener("mouseup", $drag.stop, true);
		document.removeEventListener("mousemove", $drag.move, true);

		$drag.removeOverlays();
	}

	$drag.target = null;
	return $evt.cancel(e);
};

aeon.DragCoordinator.move = function DragCoordinator$move(e)
{
	$drag.eventX = $evt(e).clientX;
	$drag.eventY = $evt(e).clientY;

	$drag.diffX = $drag.eventX - $drag.clientX;
	$drag.diffY = $drag.eventY - $drag.clientY;

	$drag.targetX = $drag.startX + $drag.diffX;
	$drag.targetY = $drag.startY + $drag.diffY;

	var onDragMove = new aeon.Event($drag, "onbeforedragmove");

	if ($drag.moveX)
		onDragMove.data.targetX = $drag.targetX;

	if ($drag.moveY)
		onDragMove.data.targetY = $drag.targetY;

	$drag.fireEvent(onDragMove);

	var x1 = $css.pixelLeft($drag.target);
	var y1 = $css.pixelTop($drag.target);
	var x2 = x1;
	var y2 = y1;

	if ($drag.moveX)
		x2 = $css.pixelLeft($drag.target, onDragMove.data.targetX);

	if ($drag.moveY)
		y2 = $css.pixelTop($drag.target, onDragMove.data.targetY);

	if (x2 != x1 || y2 != y1)
		$drag.fireEvent("ondragmove");

	$drag.sizeOverlays();

	return $evt.cancel(e);
};

aeon.DragCoordinator.overlayFrames = function DragCoordinator$overlayFrames()
{
	var iframes = document.getElementsByTagName("iframe");
	if (iframes.length > 0)
	{
		if (!$drag.$overlays)
			$drag.$overlays = [];
		else
			$drag.removeOverlays();

		for (var i = 0; i < iframes.length; i++)
		{
			var iframe = iframes[i];
			if (iframe.offsetHeight > 0)
			{
				var overlay = document.createElement("div");
				overlay.iframe = iframe;
				overlay.style.cssText = $drag.overlayCss;

				document.body.appendChild(overlay);
				$drag.$overlays.push(overlay);
			}
		}

		$drag.sizeOverlays();
	}
};

aeon.DragCoordinator.sizeOverlays = function DragCoordinator$sizeOverlays()
{
	if ($drag.$overlays)
	{
		for (var i = 0; i < $drag.$overlays.length; i++)
		{
			var overlay = $drag.$overlays[i];
			$css.pixelLeft(overlay, $dom.offsetLeft(overlay.iframe));
			$css.pixelTop(overlay, $dom.offsetTop(overlay.iframe));
			$css.pixelWidth(overlay, overlay.iframe.offsetWidth);
			$css.pixelHeight(overlay, overlay.iframe.offsetHeight);
		}
	}
};

aeon.DragCoordinator.removeOverlays = function DragCoordinator$removeOverlays()
{
	if ($drag.$overlays == null)
		return;

	while ($drag.$overlays.length)
	{
		var oly = $drag.$overlays.shift();
		oly.parentNode.removeChild(oly);
	}
};

/**
 * Global alias to <c>aeon.DragCoordinator</c>
 * @type {aeon.DragCoordinator}
 */
var $drag = aeon.DragCoordinator;
