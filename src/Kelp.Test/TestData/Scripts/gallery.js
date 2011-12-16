Type.registerNamespace("hrvatska.skola");

hrvatska.skola.Gallery = function Gallery(element)
{
	$assert.isHtmlElement(element);

	this.$super(element);
	this.images = [];
	this.currentImage = 0;

	var self = this;
	var slider = this.element.find(".thumbs .slider");
	var thumbs = this.element.find(".thumbs .thumb[data-src]");

	thumbs.each(function processElement(i, element)
	{
		self.images.push({ src: element.getAttribute("data-src") });
	});

	var loaded = 0;
	var totalWidth = 0;
	function onImageLoaded()
	{
		self.images[loaded].width = this.width;
		self.images[loaded].height = this.height;

		var thumb = thumbs.eq(loaded);
		var img = thumb.find("img");
		img.attr("src", this.src);
		img.attr("class", "loaded");
		img.bind("click", Function.createDelegate(self, self.onThumbClicked));
		loaded++;

		totalWidth += thumb.outerWidth(true);
		slider.css({ width: totalWidth });

		loadNext();
	}

	function onImageError()
	{
		this.images.splice(loaded, 1);
		thumbs.eq(loaded).remove();
		thumbs = self.element.find(".thumbs .thumb[data-src]");
	}

	function loadNext()
	{
		if (loaded == self.images.length)
			return;

		var target = thumbs.eq(loaded);
		var img = $("<img src='about:blank' class='loading'/>");
		target.append(img);

		var loader = new Image();
		loader.onload = onImageLoaded;
		loader.onerror = onImageError;
		loader.src = self.images[loaded].src;
	}

	loadNext();
};
hrvatska.skola.Gallery.inherits(aeon.controls.HtmlControl);

hrvatska.skola.Gallery.prototype.setImage = function Gallery$setImage(index)
{
	var thumbs = $(this.element).find(".thumb");
	var imageContainer = $(this.element).find(".image");
	var image1 = imageContainer.find("img");
	var image2 = image1.clone();
	image1.parent().append(image2);

	thumbs.removeClass("current");
	thumbs.eq(index).addClass("current");

	image1.attr("src", this.images[index].src);
	image1.attr("width", this.images[index].width);
	image1.attr("height", this.images[index].height);

	var containerWidth = imageContainer.innerWidth();
	var containerHeight = imageContainer.innerHeight();

	image1.css({
		top: (containerHeight - this.images[index].height) / 2,
		left: (containerWidth - this.images[index].width) / 2
	});

	image1.fadeIn(500);
	image2.fadeOut(500, function onFadeOutComplete()
	{
		image2.remove();
	});
};

hrvatska.skola.Gallery.prototype.onThumbClicked = function Gallery$onThumbClicked(e)
{
	var index = $(e.target).closest(".thumb").index();
	this.setImage(index);
};

hrvatska.skola.Gallery.setup = function Gallery$setup()
{
	hrvatska.skola.Gallery.registry = new Object;

	$(".image-gallery").each(function initializeInstance(i, element)
	{
		var uniqueId = $dom.uniqueID(element);
		hrvatska.skola.Gallery.registry[uniqueId] = new hrvatska.skola.Gallery(element);
	});

};


$(window).ready(hrvatska.skola.Gallery.setup);
