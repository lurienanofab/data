(function ($) {
    $.fn.autodd = function (a, b, c) {
        var o;
        var init = true;
        var set_opt_name = '';
        var set_opt_value = null;

        if (a == 'option') {
            init = false;
            set_opt_name = b;
            set_opt_value = c;
        }
        else {
            var defopt = { 'selected_value': '', 'disabled': false, 'close': function (event) { } };
            o = $.extend({}, defopt, a);
        }

        if (!init && typeof set_opt_value == 'undefined') {
            var temp = [];

            this.each(function () {
                var wrapper = $(this).parent();
                if (wrapper.hasClass('autodd-wrapper')) {
                    switch (set_opt_name) {
                        case 'disabled':
                            temp.push((wrapper.find('.search-text').attr('disabled') == 'disabled'));
                            break;
                    }
                }
                else {
                    temp.push(null);
                }
            });

            if (temp.length == 0) result = null;
            else if (temp.length == 1) result = temp[0];
            else result = temp;

            return result;
        }
        else {
            return this.each(function () {
                var $this = $(this);
                if (init) {
                    if ($this.is('select')) {
                        if ($this.parent().hasClass('autodd-wrapper')) {
                            $this.unwrap().siblings().remove();
                        }
                        $this.hide().wrap('<div class="autodd-wrapper"></div>');
                        var wrapper = $this.parent();
                        wrapper.append('<table class="autodd-table"><tbody><tr class="controls"><td><input type="text" class="search-text" /></td><td><div class="expand-button off"></div></td></tr><tr class="container"><td colspan="2"><div class="item-list"></div></td></tr></table>');
                        var autodd_table = wrapper.find('.autodd-table');
                        var controls = wrapper.find('.controls');
                        var search_text = controls.find('.search-text');
                        var expand_button = controls.find('.expand-button');
                        var container = wrapper.find('.container');
                        var item_list = container.find('.item-list');
                        search_text.css({ 'width': $this.outerWidth() + 'px' });
                        var pad = item_list.outerWidth(true) - item_list.width();
                        item_list.css({ 'width': (autodd_table.width() - pad) + 'px' });
                        if (o.disabled) search_text.attr('disabled', true);

                        expand_button.hover(
function () {
    if (search_text.attr('disabled') == 'disabled') return;
    $(this).removeClass('off').addClass('on');
},
function () {
    $(this).removeClass('on').addClass('off');
}
).click(function (event) {
    event.stopPropagation();
    if (item_list.is(':visible')) {
        handle_close($this, wrapper, o);
    }
    else {
        if (search_text.attr('disabled') != 'disabled') {
            item_list.find('.autodd-item').show();
            item_list.show(0, function () {
                resize_item_list(item_list);
                var i = find_selected_item($this, item_list);
                item_list.scrollTop(i.position().top);
            });
        }
    }
});

                        $this.find('option').each(function () {
                            var value = $(this).val();
                            var text = $(this).text();
                            item_list.append('<div class="autodd-item off">' + text + '<input type="hidden" value="' + value + '" /></div>');
                            if (value == o.selected_value) {
                                $this.val(value);
                                search_text.val(value);
                            }
                        });
                        item_list.append('<div class="nodata-container"><span class="nodata">No items were found.</span></div>');

                        item_list.find('.autodd-item').removeClass('last');
                        item_list.find('.autodd-item:visible:last').addClass('last');

                        item_list.find('.autodd-item').hover(
function () {
    $(this).removeClass('off').addClass('on');
},
function () {
    $(this).removeClass('on').addClass('off');
}
).click(function (event) {
    event.stopPropagation();
    var text = $(this).text();
    var value = $(this).find('input[type="hidden"]').val();
    var old_value = $this.val();
    $this.val(value);
    handle_close($this, wrapper, o, old_value);
});

                        wrapper.on('keydown', '.search-text', function (event) {
                            if (event.which == $.ui.keyCode.TAB) {
                                if (item_list.find('.autodd-item:visible').length > 0) {
                                    var top_item = item_list.find('.autodd-item:visible').eq(0);
                                    var text = top_item.text();
                                    var value = top_item.find('input[type="hidden"]').val();
                                    var old_value = $this.val();
                                    $this.val(value);
                                    handle_close($this, wrapper, o, old_value);
                                }
                                else {
                                    handle_close($this, wrapper, o);
                                }
                            }
                            else if (event.which == $.ui.keyCode.ESCAPE) {
                                search_text.blur();
                                handle_close($this, wrapper, o);
                            }
                        }).on('keyup', '.search-text', function (event) {
                            var text = $(this).val().toUpperCase();
                            item_list.find('.autodd-item').each(function () {
                                if ($(this).text().toUpperCase().substring(0, text.length) == text) $(this).show();
                                else $(this).hide();
                            });

                            item_list.show(0, function () {
                                resize_item_list(item_list);
                            });
                        }).on('focus', '.search-text', function (event) {
                            event.stopPropagation();
                            $(this).select();
                        }).on('click', '.search-text', function (event) {
                            event.stopPropagation();
                        }).on('mouseup', '.search-text', function (event) {
                            return false;
                        }); ;

                        var index = $this[0].selectedIndex;
                        var value = $this.val();
                        var text = $('option:selected', $this).text();
                        search_text.val(text);

                        $(document).click(function () {
                            if (item_list.is(':visible')) handle_close($this, wrapper, o);
                        });
                    }
                }
                else {
                    var wrapper = $this.parent();
                    if (wrapper.hasClass('autodd-wrapper')) {
                        switch (set_opt_name) {
                            case 'disabled':
                                if (set_opt_value) wrapper.find('.search-text').attr('disabled', true);
                                else wrapper.find('.search-text').removeAttr('disabled');
                                break;
                        }
                    }
                }
            });
        }
    }

    function resize_item_list(i) {
        if (i.height() > 300)
            i.css({ 'height': '300px' });
        else
            i.css({ 'height': 'auto' });
        i.find('.autodd-item').removeClass('last');
        i.find('.autodd-item:visible:last').addClass('last');
        if (i.find('.autodd-item:visible').length == 0)
            i.find('.nodata-container').show();
        else
            i.find('.nodata-container').hide();
    }

    function handle_close(select, wrapper, opt, old_value) {
        var index = select[0].selectedIndex;
        var value = select.val();
        var text = $('option:selected', select).text()
        var ov = (typeof old_value == 'undefined') ? value : old_value;
        var changed = (ov != value);

        wrapper.find('.search-text').val(text);
        wrapper.find('.item-list').css({ 'height': 'auto' }).hide(0, function () {
            if (typeof opt.close == 'function')
                opt.close({ 'wrapper': wrapper, 'changed': changed, 'old_value': ov, 'selected': { 'index': index, 'value': value, 'text': text} });
        });
    }

    function find_selected_item(select, item_list) {
        var selected_value = select.val();
        var result = item_list.find('.autodd-item input[type="hidden"][value="' + selected_value + '"]').parent();
        return result;
    }
} (jQuery));