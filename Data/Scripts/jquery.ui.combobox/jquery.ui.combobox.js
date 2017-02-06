(function ($) {
    $.widget("ui.combobox", {
        _create: function () {

            var onfocus = null; //function (a) { $(a).select(); };
            var useRegexMatching = false;

            var isMatch = function (text, term, prefix, suffix) {
                if (useRegexMatching) {
                    var temp = $.ui.autocomplete.escapeRegex(term);
                    if (prefix) temp = prefix + temp;
                    if (suffix) temp = temp + suffix;
                    var matcher = new RegExp(temp, "i");
                    return matcher.test(text);
                }
                else {
                    return text.indexOf(term) == 0;
                }
            };

            var input,
					self = this,
					select = this.element.hide(),
					selected = select.children(":selected"),
					value = selected.val() ? selected.text() : "",
					wrapper = this.wrapper = $("<span>")
						.addClass("ui-combobox")
						.insertAfter(select);

            input = $("<input>")
					.appendTo(wrapper)
					.val(value)
					.addClass("ui-state-default ui-combobox-input")
					.autocomplete({
					    delay: 0,
					    minLength: 0,
					    source: function (request, response) {
					        response(select.children("option").map(function () {
					            var text = $(this).text();
					            var term = request.term;
					            if (this.value && (!request.term || isMatch(text, term)))
					                return {
					                    label: text.replace(
											new RegExp(
												"(?![^&;]+;)(?!<[^<>]*)(" +
												$.ui.autocomplete.escapeRegex(request.term) +
												")(?![^<>]*>)(?![^&;]+;)", "gi"
											), "<strong>$1</strong>"),
					                    value: text,
					                    option: this
					                };
					        }));
					    },
					    select: function (event, ui) {
					        ui.item.option.selected = true;
					        self._trigger("selected", event, {
					            item: ui.item.option
					        });
					        select.trigger("change");
					    },
					    change: function (event, ui) {
					        if (!ui.item) {
					            var text = $(this).text();
					            var term = $(this).val();
					            var valid = false;
					            select.children("option").each(function () {
					                if (isMatch(text, term, '^', '$')) {
					                    this.selected = valid = true;
					                    return false;
					                }
					            });
					            if (!valid) {
					                // remove invalid value, as it didn't match anything
					                $(this).val("");
					                select.val("");
					                input.data("autocomplete").term = "";
					                return false;
					            }
					        }
					    }
					})
					.addClass("ui-widget ui-widget-content ui-corner-left")
                    .focus(function () {
                        if (typeof (onfocus) == 'function')
                            onfocus($(this));
                    });

            input.data("autocomplete")._renderItem = function (ul, item) {
                return $("<li></li>")
						.data("item.autocomplete", item)
						.append("<a>" + item.label + "</a>")
						.appendTo(ul);
            };

            $("<a>")
					.attr("tabIndex", -1)
					.attr("title", "Show All Items")
					.appendTo(wrapper)
					.button({
					    icons: {
					        primary: "ui-icon-triangle-1-s"
					    },
					    text: false
					})
					.removeClass("ui-corner-all")
					.addClass("ui-corner-right ui-combobox-toggle")
					.click(function () {
					    // close if already visible
					    if (input.autocomplete("widget").is(":visible")) {
					        input.autocomplete("close");
					        return;
					    }

					    // work around a bug (likely same cause as #5265)
					    $(this).blur();

					    // pass empty string as value to search for, displaying all results
					    input.autocomplete("search", "");
					    input.focus();
					});
        },

        destroy: function () {
            this.wrapper.remove();
            this.element.show();
            $.Widget.prototype.destroy.call(this);
        }
    });
})(jQuery);