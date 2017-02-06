(function ($) {
    $.fn.datagrid = function (options) {
        var opt = $.extend({
            'allowCopy': true,
            'allowEdit': true,
            'getFields': function () { alert('[ERROR:jquery.datagrid] getFields option must be specified.'); },
            'disableFooter': function (instance) { return false; },
            'onAddRow': function (instance, r) { },
            'onRemoveRow': function (instance, r) { }
        }, options);

        return this.each(function () {
            var $this = $(this);

            if (!$this.hasClass('datagrid'))
                $this.addClass('datagrid');

            var getFields = function () {
                var result = new Array();
                $.each(opt.getFields(), function (i, f) {
                    result.push($.extend({
                        'name': null,
                        'className': null,
                        'visible': true,
                        'required': false,
                        'getCell': function (r) {
                            return $('.' + this.className, r);
                        },
                        'getFooter': function () {
                            return $('tfoot .' + this.className, $this);
                        },
                        'getFooterValue': function () {
                            if (!this.visible) return 0;
                            else return this.getFooter().find('input[type="text"]').val();
                        },
                        'getValue': function (r) {
                            return $('.' + this.className, r).text();
                        },
                        'beginEdit': function (r) {
                            var c = this.getCell(r);
                            var v = this.getValue(r);
                            c.html('')
                                .append($('<input type="text" />').addClass('edit-' + this.className).val(v))
                                .append($('<input type="hidden" />').addClass('edit-' + this.className + '-original').val(v));
                        },
                        'cancelEdit': function (r) {
                            var c = this.getCell(r);
                            var v = c.find('.edit-' + this.className + '-original').val();
                            c.html(v);
                        },
                        'saveEdit': function (r) {
                            var c = this.getCell(r);
                            var v = c.find('.edit-' + this.className).val();
                            c.html(v);
                        },
                        'copy': function (r) {
                            var v = this.getValue(r);
                            var foot = this.getFooter();
                            $('input[type="text"]', foot).val(v);
                        }
                    }, f));
                });
                return result;
            }

            var getInstance = function () {

                $this.getFooter = function () {
                    return getRow($('tfoot', $this));
                }

                $this.disableFooter = function () {
                    $this.getFooter().find('input[type="text"]').prop('disabled', true);
                    $this.getFooter().find('input[type="button"]', $this).prop('disabled', true);
                    $this.getFooter().find('select', $this).prop('disabled', true);
                };

                $this.enableFooter = function () {
                    $this.getFooter().find('input[type="text"]').prop('disabled', false);
                    $this.getFooter().find('input[type="button"]', $this).prop('disabled', false);
                    $this.getFooter().find('select', $this).prop('disabled', false);
                }

                $this.clearFooter = function () {
                    $('tfoot input[type="text"]', $this).val('');
                }

                var newRowControls = function () {
                    var result = '';
                    if (opt.allowCopy) result += '<a href="#" class="copy-row"><img src="//ssel-apps.eecs.umich.edu/static/images/copy.png" alt="Copy" title="Copy" /></a>';
                    if (opt.allowEdit) result += '<a href="#" class="edit-row"><img src="//ssel-apps.eecs.umich.edu/static/images/edit.png" alt="Edit" title="Edit" /></a>';
                    result += '<a href="#" class="delete-row"><img src="//ssel-apps.eecs.umich.edu/static/images/delete.png" alt="Delete" title="Delete" /></a>';
                    return $(result);
                }

                $this.addRowControls = function () {
                    $('tbody tr .row-controls', $this).each(function () {
                        $(this).html('').append(newRowControls());
                    });
                }

                $this.sortSelect = function (select) {
                    var options = select.find('option');
                    if (options.length > 1) {
                        setTimeout(function () {
                            var arr = options.map(function (_, o) { return { t: $(o).text(), v: o.value }; }).get();
                            arr.sort(function (o1, o2) { return o1.t > o2.t ? 1 : o1.t < o2.t ? -1 : 0; });
                            options.each(function (i, o) {
                                o.value = arr[i].v;
                                $(o).text(arr[i].t);
                            });
                        }, 100);
                    }
                }

                return $this;
            }

            var getRow = function (r) {

                r.getCell = function(className){
                    return r.find('.'+className);
                }

                return r;
            }

            var instance = getInstance();
            var fields = getFields();

            var clearAlerts = function () {
                instance.getFooter().find('input[type="text"]').css({ 'background-color': '' });
            }

            var requiredAlert = function (f) {
                var textBox = f.getFooter().find('input[type="text"]');
                textBox.css({ 'background-color': '#ffcccc' });
                return false;
            }

            var editRow = function (r) {
                clearAlerts();
                $.each(fields, function (i, f) {
                    f.beginEdit(r);
                });
            }

            var saveRow = function (r) {
                clearAlerts();
                $.each(fields, function (i, f) {
                    f.saveEdit(r);
                });
            }

            var cancelRow = function (r) {
                clearAlerts();
                $.each(fields, function (i, f) {
                    f.cancelEdit(r);
                });
            }

            var copyRow = function (r) {
                clearAlerts();
                $.each(fields, function (i, f) {
                    f.copy(r);
                });
            }

            var removeRow = function (r) {
                clearAlerts();
                r.remove();
                opt.onRemoveRow(instance, getRow(r));
            }

            var toggleRowControls = function (k, r) {
                switch (k) {
                    case 'edit-delete':
                        $('.copy-row', r).show();
                        $('.save-row', r).addClass('edit-row').removeClass('save-row').find('img').attr('src', '//ssel-apps.eecs.umich.edu/static/images/edit.png').attr('alt', 'Edit').attr('title', 'Edit');
                        $('.cancel-row', r).addClass('delete-row').removeClass('cancel-row').find('img').attr('src', '//ssel-apps.eecs.umich.edu/static/images/delete.png').attr('alt', 'Delete').attr('title', 'Delete');
                        break;
                    case 'save-cancel':
                        $('.copy-row', r).hide();
                        $('.edit-row', r).addClass('save-row').removeClass('edit-row').find('img').attr('src', '//ssel-apps.eecs.umich.edu/static/images/save.png').attr('alt', 'Save').attr('title', 'Save');
                        $('.delete-row', r).addClass('cancel-row').removeClass('delete-row').find('img').attr('src', '//ssel-apps.eecs.umich.edu/static/images/cancel.png').attr('alt', 'Cancel').attr('title', 'Cancel');
                        break;
                }
            }

            var addRow = function () {
                clearAlerts();

                var row = $('<tr/>');
                var valid = true;
                for (var i = 0; i < fields.length; i++) {
                    var f = fields[i];
                    var v = f.getFooterValue();
                    if (f.required && v == '')
                        valid = requiredAlert(f);
                    else {
                        var cell = $('<td/>');
                        cell.html(v);
                        cell.addClass(f.className);
                        if (!f.visible) cell.hide();
                        row.append(cell);
                    }
                }
                if (!valid) return false;
                row.append($('<td/>').addClass('row-controls'));

                row.appendTo($('tbody', $this));
                instance.addRowControls();
                instance.clearFooter();
                opt.onAddRow(instance, getRow(row));
                disableFooterChecker();

                return true;
            }

            var disableFooterChecker = function () {
                if (opt.disableFooter(instance))
                    instance.disableFooter();
                else
                    instance.enableFooter();
            }

            var createPostData = function () {
                var form = $this.closest('form');
                var index = 0;
                var property = $this.data('property');
                $('.' + property + '-post-data', form).remove();
                $('tbody tr', $this).each(function () {
                    var row = $(this);

                    $.each(fields, function (i, f) {
                        var v = f.getValue(row);
                        $('<input type="hidden"/>')
                            .addClass(property + '-post-data')
                            .attr('name', property + '[' + index + '].' + f.name)
                            .val(v)
                            .appendTo(form);
                    });
                    index++;
                });
            }

            var initFields = function () {
                $.each(fields, function (i, f) {
                    if (!f.visible) {
                        $('thead', $this).find('.' + f.className).hide();
                        $('tbody', $this).find('.' + f.className).hide();
                        $('tfoot', $this).find('.' + f.className).hide();
                    }
                });
            }

            initFields();
            instance.addRowControls();
            disableFooterChecker();
            createPostData();

            $this.on('click', '.add-row', function (event) {
                if (addRow()) {
                    instance.addRowControls();
                    disableFooterChecker();
                    instance.clearFooter();
                    createPostData();
                }
            }).on('click', '.copy-row', function (event) {
                event.preventDefault();
                var row = $(this).closest('tr');
                instance.clearFooter();
                copyRow(row);
            }).on('click', '.edit-row', function (event) {
                event.preventDefault();
                var row = $(this).closest('tr');
                toggleRowControls('save-cancel', row);
                editRow(row);
                createPostData();
            }).on('click', '.save-row', function (event) {
                event.preventDefault();
                var row = $(this).closest('tr');
                toggleRowControls('edit-delete', row);
                saveRow(row);
                createPostData();
            }).on('click', '.cancel-row', function (event) {
                event.preventDefault();
                var row = $(this).closest('tr');
                toggleRowControls('edit-delete', row);
                cancelRow(row);
                createPostData();
            }).on('click', '.delete-row', function (event) {
                event.preventDefault();
                var row = $(this).closest('tr');
                removeRow(row);
                disableFooterChecker();
                instance.clearFooter();
                createPostData();
            });
        });
    };
}(jQuery));