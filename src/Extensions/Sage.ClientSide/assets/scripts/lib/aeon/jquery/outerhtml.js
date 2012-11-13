jQuery.fn.outerHtml = function jQuery$outerHtml()
{
	return $(this).clone().wrap('<p>').parent().html();
};
