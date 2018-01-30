function openKendoWindow(windowName) {
	var window = $("#" + windowName).data("kendoWindow");
	window.center();
	window.open();
}

function closeKendoWindow(windowName) {
	var window = $("#" + windowName).data("kendoWindow");
	window.close();
}

function refreshKendoGrid(gridName, readMethod) {
    var grid = $("#" + gridName).data("kendoGrid");
    if (typeof (grid.dataSource) != "undefined") {
        if (typeof (readMethod) != "undefined") {
            grid.dataSource.read();  //refreshing grid and keep current page wherever it was
        }
        else if (typeof (grid.dataSource._pageSize) == "undefined")   //when it return empty result
        {
            grid.dataSource.read();
        }
        else {
            grid.dataSource.page(1);    //default method call always go to the 1st page
        }
    }
    return grid;
}

function openTelerikWindow(windowName) {
	var windowElement = $("#" + windowName).data("tWindow");
	windowElement.center().open();
}

function closeTelerikWindow(windowName) {
	var windowElement = $("#" + windowName).data("tWindow");
	windowElement.center().close();
}

//check if input is number
function isNumber(evt, allowDecimal) {
    evt = (evt) ? evt : window.event;
    var charCode = (evt.which) ? evt.which : evt.keyCode;
    if (charCode > 31 && (charCode < 48 || charCode > 57)) {
        if (allowDecimal && charCode == 46) {
            return true;
        }
        return false;
    }
    return true;
}

//TextArea maxLength
function ismaxlength(obj) {
	var mlength = obj.getAttribute ? parseInt(obj.getAttribute("maxlength")) : "";
	if (obj.getAttribute && obj.value.length > mlength)
		obj.value = obj.value.substring(0, mlength);
}

$(function () {
	$('.begin-collapsed').children("#CollapseUp").removeClass('hidden');
	$('.begin-collapsed').children("#CollapseDown").addClass('hidden');
	$('.begin-collapsed').parent().parent().parent().parent().children('.block-content').hide();

	if ($('.collapsible-panel').size() == $('.begin-collapsed').size()) {
		$('.collapse-all-button').children("#CollapseUp").removeClass('hidden');
		$('.collapse-all-button').children("#CollapseDown").addClass('hidden');
	}
	$('.collapse-button').click(function () {
		if ($(this).children('#CollapseUp').hasClass('hidden')) {
			$(this).children('#CollapseUp').removeClass('hidden');
			$(this).children('#CollapseDown').addClass('hidden');
		}
		else {
			$(this).children('#CollapseDown').removeClass('hidden');
			$(this).children('#CollapseUp').addClass('hidden');
		}
		$(this).parent().parent().parent().parent().children('.block-content').slideToggle('fast');

		var hiddenUpButtonCount = $('.collapse-button').filter(function () { return $(this).children('#CollapseUp').hasClass('hidden') }).size();
		var hiddenDownButtonCount = $('.collapse-button').filter(function () { return $(this).children('#CollapseDown').hasClass('hidden') }).size();

		if (hiddenUpButtonCount == 0) {
			$('.collapse-all-button').children('#CollapseUp').removeClass('hidden');
			$('.collapse-all-button').children('#CollapseDown').addClass('hidden');
		}
		if (hiddenDownButtonCount == 0) {
			$('.collapse-all-button').children('#CollapseUp').addClass('hidden');
			$('.collapse-all-button').children('#CollapseDown').removeClass('hidden');
		}
	});
	$('.collapse-all-button').click(function () {
		if ($(this).children('#CollapseUp').hasClass('hidden')) {
			$(this).children('#CollapseUp').removeClass('hidden');
			$(this).children('#CollapseDown').addClass('hidden');
			$('.collapsible-panel').parent().children('.block-content').slideUp('fast');
			$('.collapse-button').children('#CollapseUp').removeClass('hidden');
			$('.collapse-button').children('#CollapseDown').addClass('hidden');
		}
		else {
			$(this).children('#CollapseDown').removeClass('hidden');
			$(this).children('#CollapseUp').addClass('hidden');
			$('.collapsible-panel').parent().children('.block-content').slideDown('fast');
			$('.collapse-button').children('#CollapseDown').removeClass('hidden');
			$('.collapse-button').children('#CollapseUp').addClass('hidden');
		}
		return false;
	});

	// preventDefault on inactive-button
	$('a.inactive-button').click(function (e) {
		e.preventDefault();
	});

	$('div.document-ui-section').hide();
	$('input.document-source:checked').each(function () {
		$('#document-ui-section' + $(this).val()).show();
		$("#ActionSourceID").val($(this).val());
	});
	$('input.document-source').click(function () {
		$('div.document-ui-section').hide();
		$('#document-ui-section' + $(this).val()).show();
		$("#ActionSourceID").val($(this).val());
	});

	// Begin CERS Checkbox Dynamic Show/Hide Section Functionality
	// First, for each checked cers-checkbox having a showHideTarget attribute, show the target:
	$('input.cers-checkbox[showHideTarget]:checked').each(function () {
		$('#' + $(this).attr('showHideTarget')).show('fast');
	});
	// Second, bind show/hide logic to the click event for cers-checkboxes having a showHideTarget attribute:
	$('input.cers-checkbox[showHideTarget]').live("click", function () {
		if ($(this).is(':checked')) {
			$('#' + $(this).attr('showHideTarget')).show('fast');
		} else {
			$('#' + $(this).attr('showHideTarget')).hide('fast');
		}
	});
	// End CERS Checkbox Dynamic Show/Hide Section Functionality

	// Begin CERS Radiobox Dynamic Show/Hide Section Functionality
	// First, for each select cers-radio-button having a showHideTarget and showHideValue attribute,
	// compare the radio button's value with the showHideValue.  If they match, show the target:
	$('input.cers-radio-button[showHideTarget][showHideValue]:checked').each(function () {
		if ($(this).is(':checked') && $(this).val() == $(this).attr('showHideValue')) {
			$('#' + $(this).attr('showHideTarget')).show('fast');
		}
	});
	// Second, bind show/hide logic to the change event for the cers-radio-buttoni having a showHideTarget
	// and showHideValue attribute.  Compare the radio button's value with the showHideValue.  If they
	// match, show the target:
	$('input.cers-radio-button[showHideTarget][showHideValue]').live("click", function () {
		if ($(this).is(':checked') && $(this).val() == $(this).attr('showHideValue')) {
			$('#' + $(this).attr('showHideTarget')).show('fast');
		} else {
			$('#' + $(this).attr('showHideTarget')).hide('fast');
		}
	});
	// End CERS Radiobox Dynamic Show/Hide Section Functionality

	//NUMERIC TEXTBOX
	$('input.numeric-textbox-whole-number').live('keydown', function (event) {
		//dont allow ctrl, alt keys
		if (event.ctrlKey || event.altKey) {
			event.preventDefault();
		}
		if (event.shiftKey && (event.keyCode != 9)) {
			event.preventDefault();
		}
		//allow numbers, numbers from numpad, Backspace, delete, Tab, left arrow, right arrow, and enter.
		if (((event.keyCode >= 48 && event.keyCode <= 57) || (event.keyCode >= 96 && event.keyCode <= 105) ||
			 (event.keyCode == 8 || event.keyCode == 46 || event.keyCode == 9 || event.keyCode == 37 || event.keyCode == 39 || event.keyCode == 13)) == false) {
			event.preventDefault();
		}
	});
	$('input.numeric-textbox-floating-point-number').live('keydown', function (event) {
		//get textbox value.
		var value = $(this).val();
		//dont allow ctrl, alt keys
		if (event.ctrlKey || event.altKey) {
			event.preventDefault();
		}
		//prevent shift; except when tab(9) is pressed
		if (event.shiftKey && (event.keyCode != 9)) {
			event.preventDefault();
		}
		//allow numbers, numbers from numpad, Backspace, delete, Tab, left arrow, right arrow, and enter.
		if (((event.keyCode >= 48 && event.keyCode <= 57) || (event.keyCode >= 96 && event.keyCode <= 105) ||
			 (event.keyCode == 8 || event.keyCode == 46 || event.keyCode == 9 || event.keyCode == 37 || event.keyCode == 39 || event.keyCode == 13 || event.keyCode == 190 || event.keyCode == 110)) == false) {
			event.preventDefault();
		}
		//allow period only when there is no period already.
		if (value.indexOf(".") >= 0 && (event.keyCode == 190 || event.keyCode == 110)) {
			event.preventDefault();
		}
	});
	$('input.numeric-textbox-dollar-amount').live('keydown', function (event) {
		//get textbox value.
		var value = $(this).val();
		//dont allow ctrl, alt keys
		if (event.ctrlKey || event.altKey) {
			event.preventDefault();
		}
		//prevent shift; except when tab(9) is pressed
		if (event.shiftKey && (event.keyCode != 9)) {
			event.preventDefault();
		}
		//allow numbers, numbers from numpad, Backspace, delete, Tab, left arrow, right arrow, and enter.
		if (((event.keyCode >= 48 && event.keyCode <= 57) || (event.keyCode >= 96 && event.keyCode <= 105) ||
			 (event.keyCode == 8 || event.keyCode == 46 || event.keyCode == 9 || event.keyCode == 37 || event.keyCode == 39 || event.keyCode == 13 || event.keyCode == 190 || event.keyCode == 110)) == false) {
			event.preventDefault();
		}
		//allow period only when there is no period already.
		if (value.indexOf(".") > 0 && (event.keyCode == 190 || event.keyCode == 110)) {
			event.preventDefault();
		}
		if ((value.indexOf(".") > 0 && value.indexOf(".") + 3 == value.length) &&
			 (event.keyCode == 8 || event.keyCode == 46 || event.keyCode == 9 || event.keyCode == 37 || event.keyCode == 39 || event.keyCode == 13 || event.keyCode == 190 || event.keyCode == 110) == false) {
			event.preventDefault();
		}
	});
	//.nullable radio button script
	$(function () {
		var allRadios = $('.nullable-radio-button')
		var radioChecked;

		var setCurrent = function (e) {
			var obj = e.target;
			radioChecked = $(obj).attr('checked');
		}

		var setCheck = function (e) {
			if (e.type == 'keypress' && e.charCode != 32) {
				return false;
			}

			var obj = e.target;

			if (radioChecked) {
				$(obj).attr('checked', false);
			} else {
				$(obj).attr('checked', true);
			}
		}

		$.each(allRadios, function (i, val) {
			var label = $('label[for=' + $(this).attr("id") + ']');

			$(this).bind('mousedown keydown', function (e) {
				setCurrent(e);
			});

			label.bind('mousedown keydown', function (e) {
				e.target = $('#' + $(this).attr("for"));
				setCurrent(e);
			});

			$(this).bind('click', function (e) {
				setCheck(e);
			});
		});
	});

	// Trap both "click" and "mousedown" event for CERS Radio Button
	//    $('input.nullable-radio-button').bind('click mousedown', (function () {
	//        // Local variable "isChecked" is used to trap the status of radio button on mousedown
	//        var isChecked;
	//        return function (event) {
	//            if (event.type == 'click') {
	//                if (isChecked) {
	//                    // If already checked, clear radio button:
	//                    isChecked = this.checked = false;
	//                } else {
	//                    // If not checked, set value to true (browser will set radio button by itself):
	//                    isChecked = true;
	//                }
	//                // Trigger a 'change' event on this object; if a user 'de-selects' a radio button,
	//                // this call will help ensure that any logic bound to the 'change' event is called:
	//                $(this).change();
	//            } else { // "mousedown" event
	//                // Track value of radio button here; browser will call the function a second
	//                // time during the "click" event to set/clear the value:
	//                isChecked = this.checked;
	//            }
	//        }
	//    })());
});

//  Simply Buttons, version 2.0
//  (c) 2007-2009 Kevin Miller
//
//  This script is freely distributable under the terms of an MIT-style license.
//
/*-----------------------------------------------------------------------------------------------*/
//
// * Adjusts the buttons so that they will not have an outline when they are pressed.
// * If the browser is mobile then we replace the buttons with inputs for compatibility.
// * Disables the text in the buttons from being selected.
// * The default styles here are meant for use with the Sliding Doors technique http://alistapart.com/articles/slidingdoors/
//     to be used for IE so we can have nice states with a horrid browser too!
//
/*-----------------------------------------------------------------------------------------------*/

var SimplyButtons = {
	options: {
		hyperlinkClass: 'button', activeButtonClass: 'button_active',
		states: {
			outer: {
				active: { backgroundPosition: 'bottom left' },
				inactive: { backgroundPosition: 'top left' }
			},
			inner: {
				active: { backgroundPosition: 'bottom right' },
				inactive: { backgroundPosition: 'top right' }
			}
		},
		iphone: { replaceButtons: true }
	},
	buttons: [],
	iphone: false,
	init: function (options) {
		for (var property in options) {
			this.options[property] = options[property];
		}

		this.iphone = (navigator.userAgent.match(/iPhone/i)) || (navigator.userAgent.match(/iPod/i));

		this.process(document.getElementsByTagName('button'), false);
		this.process(document.getElementsByTagName('a'), true);

		if (this.iphone && this.options.iphone.replaceButtons) {
			this.remove();
		}
	},
	process: function (elements, links) {
		var linkTest = new RegExp('\\b' + this.options.hyperlinkClass + '\\b');
		for (var a = 0; a < elements.length; a++) {
			if ((links && linkTest.test(elements[a].className)) || !links) {
				if (this.iphone && !links) {
					this.mobile(elements[a]);
				}
				else {
					this.disable(elements[a]);
					this.setup(elements[a]);
				}

				if (!links) {
					this.buttons.push(elements[a]);
				}
			}
		}
	},
	mobile: function (element) {
		var input = document.createElement('input');
		input.setAttribute('type', element.getAttribute('type') == 'submit' ? 'submit' : 'button');

		var attributes = new Array('id', 'name', 'value', 'class', 'onclick', 'onmouseover', 'onmouseout', 'onpress', 'onfocus', 'onblur', 'onmouseup', 'onmousedown');
		for (var a = 0; a < attributes.length; a++) {
			if (element.getAttribute(attributes[a])) {
				input.setAttribute(attributes[a], element.getAttribute(attributes[a]));
			}
		}

		input.style.marginLeft = element.style.marginLeft;
		input.style.marginRight = element.style.marginRight;

		element.parentNode.insertBefore(input, element);
	},
	remove: function () {
		for (var a = 0; a < this.buttons.length; a++) {
			this.buttons[a].parentNode.removeChild(this.buttons[a]);
		}
	},
	disable: function (element) {
		element.onselectstart = function () { return false; };
		element.style.MozUserSelect = 'none';
		element.style.KhtmlUserSelect = 'none';
		element.style.UserSelect = 'none';
		element.style.cursor = 'default';
	},

	setup: function (element) {
		if (document.all) {
			if (element.tagName == 'BUTTON') {
				element.attachEvent('onfocus', this.bind(this.toggle, this, element));
			}
			else {
				element.attachEvent('onmousedown', this.bind(this.toggle, this, element));
			}
			element.attachEvent('onmouseup', this.bind(this.toggle, this, element));
		}
		else {
			element.onfocus = function () { this.blur(); };
		}
	},
	toggle: function (o, element) {
		if (element.tagName != 'BUTTON' && element.tagName != 'A') {
			while (element.tagName != 'A') {
				element = element.parentNode;
			}
		}
		if (event.type == 'focus' || event.type == 'mousedown') {
			element.className += ' ' + o.options.activeButtonClass;
			o.style(element.childNodes[0], o.options.states.inner.active);
			o.style(element.childNodes[0].childNodes[0], o.options.states.outer.active);
			element.blur();
		}
		else {
			element.className = element.className.replace(o.options.activeButtonClass, '');
			o.style(element.childNodes[0], o.options.states.inner.inactive);
			o.style(element.childNodes[0].childNodes[0], o.options.states.outer.inactive);
		}
	},
	style: function (element, styles) {
		for (var property in styles) {
			element.style[property] = styles[property];
		}
	},
	bind: function (func) {
		var args = [];
		for (var a = 1; a < arguments.length; a++) {
			args.push(arguments[a]);
		}
		return function () { return func.apply(this, args); };
	}
};
/* END SimplyButtons */

/*Tooltips*/
function launchTooltip(targetControlId, title, content) {
	$("#ddToolTipTitle").html(title);
	$("#ddToolTipContent").html(content);
	$("#" + targetControlId).tooltip({ effect: 'slide' });
}

function setCookie(name, value, days) {
	if (days) {
		var date = new Date();
		date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
		var expires = "; expires=" + date.toGMTString();
	}
	else var expires = "";
	document.cookie = name + "=" + value + expires + "; path=/";
}

function getCookie(name) {
	var nameEQ = name + "=";
	var ca = document.cookie.split(';');
	for (var i = 0; i < ca.length; i++) {
		var c = ca[i];
		while (c.charAt(0) == ' ') c = c.substring(1, c.length);
		if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
	}
	return null;
}

function deleteCookie(name) {
	setCookie(name, "", -1);
}

/* Browser Detection functions */
function get_browser() {
    var N = navigator.appName, ua = navigator.userAgent, tem;
    var M = ua.match(/(opera|chrome|safari|firefox|msie)\/?\s*(\.?\d+(\.\d+)*)/i);
    if (M && (tem = ua.match(/version\/([\.\d]+)/i)) != null) M[2] = tem[1];
    M = M ? [M[1], M[2]] : [N, navigator.appVersion, '-?'];
    return M[0];
}

function get_browser_version() {
    var N = navigator.appName, ua = navigator.userAgent, tem;
    var M = ua.match(/(opera|chrome|safari|firefox|msie)\/?\s*(\.?\d+(\.\d+)*)/i);
    if (M && (tem = ua.match(/version\/([\.\d]+)/i)) != null) M[2] = tem[1];
    M = M ? [M[1], M[2]] : [N, navigator.appVersion, '-?'];
    return M[1];
}
/* END Browser Detection functions*/
