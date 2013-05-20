/*//
Code is being refactored to make more use of jQuery, before being moved to wl.utils.js
//*/

//$(document).ready(function() {
    //wl.handleEvents('#expandFlash', 'click', function() {
        //wl.Page.Home.expandFlash(500, 10);
    //});
//});

var rAnimate = {
	isBusy: false,
	Move: {
		iObj: this,
		easeTo: function(iObj, iRule, iParams, iStep) {
			this.iObj.isBusy = true;
			var rootObj = this;
			var ease = {
				CSS: [],
				cssRule: function(fRule) {
					switch(fRule) {
						case 'position': this.CSS[0] = 'top'; this.CSS[1] = 'left'; break;
						case 'margin': this.CSS[0] = 'marginTop'; this.CSS[1] = 'marginLeft'; break;
						case 'padding': this.CSS[0] = 'paddingTop'; this.CSS[1] = 'paddingLeft';
					}
				},
				endX: null,
				endY: null,
				xDone: false,
				yDone: false,
				obj: null,
				action: function(iDiv) {
					var endX = this.endX;
					var endY = this.endY;
					var obj = this;
					var myMove = this.obj;
						var posTop = parseInt(myMove.css(this.CSS[0]));
						var posLeft = parseInt(myMove.css(this.CSS[1]));
						var topDiff = (endY - posTop) / iDiv;
						var leftDiff = (endX - posLeft) / iDiv;
						var newTop = Math.round(posTop + topDiff);
						var newLeft = Math.round(posLeft + leftDiff);
						if (obj.yDone == false && topDiff < -0.8 || topDiff > 0.8) {
							myMove.css(this.CSS[0], newTop + 'px');
						}
						else {
							myMove.css(this.CSS[0], endY + 'px');
							topDiff = 0;
							obj.yDone = true;
						}
						if (obj.xDone == false && leftDiff < -0.8 || leftDiff > 0.8) {
							myMove.css(this.CSS[1], newLeft + 'px');
						}
						else {
							myMove.css(this.CSS[1], endX + 'px');
							leftDiff = 0;
							obj.xDone = true;
						}
						if (obj.yDone == true && obj.xDone == true) {
							rootObj.iObj.isBusy = false;
							clearInterval(rootObj.action);
						}
				}
			}
			ease.cssRule(iRule);
			ease.obj = $(iObj);
			ease.endX = iParams[1];
			ease.endY = iParams[0];
			this.action = setInterval(function() { ease.action(iStep) }, 30);
			return this;
		}
	}
}
