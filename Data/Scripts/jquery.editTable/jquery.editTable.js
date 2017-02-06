(function ($) {
    $.fn.editTable = function (option) {
        return this.each(function () {
            var $self = $(this);

            var opt = $.extend(true, {}, {
                "url": null,
                "className": null,
                "editMode": "inline",
                "columns": [],
                "header": { "controls": { "width": "90px" } },
                "footer": { "button": { "text": "ADD", "className": null } },
                "canEdit": function (i) { return true; },
                "canDelete": function (i) { return true; },
                "canAdd": function (data) { return true; },
                "complete": function (table, data) { },
                "reload": function (table, data) { },
                "nodata": null
            }, option);

            var table = $("<table/>").append($("<thead/>")).append($("<tbody/>")).append($("<tfoot/>")).appendTo($self);

            //this function allows properties to be either a value or a function
            var getValue = function (property, arg, callback) {
                if (property === null)
                    callback(null);

                if (typeof property == "function")
                    callback(property(arg));
                else
                    return callback(property);
            }

            var createEditDeleteControls = function (item) {
                var div = $("<div/>");

                getValue(opt.canEdit, item, function (v) {
                    if (v) div.append($("<a/>").attr("href", "#").attr("title", "Edit").addClass("row-edit"));
                });

                getValue(opt.canDelete, item, function (v) {
                    if (v) {
                        $(".row-edit", div).css('margin-right', '15px');
                        div.append($("<a/>").attr("href", "#").attr("title", "Delete").addClass("row-delete"));
                    }
                })

                return div;
            }

            var createSaveCancelControls = function (item) {
                if (opt.editMode == "inline") {
                    var div = $("<div/>");
                    div.append($("<a/>").attr({ "href": "#", "title": "Update" }).addClass("row-save").css('margin-right', '15px'));
                    div.append($("<a/>").attr({ "href": "#", "title": "Cancel" }).addClass("row-cancel"));
                    return div;
                }
                else
                    return "&nbsp;";
            }

            var createPopup = function (mode, row) {
                var div = $("<div/>").addClass("modal fade").attr({ "id": "basicModal", "tabindex": "-1", "role": "dialog", "aria-labelledby": "basicModel", "aria-hidden": "true" });
                var dialog = $("<div/>").addClass("modal-dialog").appendTo(div);
                var content = $("<div/>").addClass("modal-content").appendTo(dialog);
                var header = $("<div/>").addClass("modal-header").appendTo(content);
                header
                    .append($("<button/>").addClass("close").attr({ "type": "button", "data-dismiss": "modal", "aria-hidden": "true" }).html("&times;"))
                    .append($("<h4/>").addClass("modal-title").attr("id", "myModalTitle").html(mode == "edit" ? "Edit" : "Add"));
                var body = $("<div/>").addClass("modal-body").appendTo(content);
                var footer = $("<div/>").addClass("modal-footer").appendTo(content);
                footer
                    .append($("<button/>").addClass("btn btn-default").attr({ "type": "button", "data-dismiss": "modal" }).html("Close"))
                    .append($("<button/>").addClass("btn btn-primary").attr({ "type": "button", "data-dismiss": "modal" }).html("Save Changes").on("click", function (e) {

                        var item = null;

                        if (mode == "edit"){
                            item = row.data("item");
                            updateItem(body, item);
                        }
                        else //add
                            item = newItem(body);

                        saveItem(item);
                    }));


                $.each(opt.columns, function (index, col) {
                    col = $.extend(true, { "key": null, "type": "text", "select": { "items": null, "className": null }, "readonly": false, "edit": { "html": null }, "footer": { "html": null }, "data": null }, col);

                    if (col.key) {
                        if (mode == "edit") {
                            var item = row.data("item");
                            var td = findElementByKey(row, col.key);
                            var n = (td.data("name")) ? td.data("name") : col.key;
                            var formGroup = $("<div/>").addClass("form-group").attr("data-key", col.key);
                            var label = $("<label/>").addClass("control-label").html(n).appendTo(formGroup);

                            if (col.edit.html) {
                                getValue(col.edit.html, item, function (v) {
                                    formGroup.append(v);
                                });
                            } else {
                                var content = null;
                                switch (col.type) {
                                    case "select":
                                        var select = $("<select/>").css("width", "100%");

                                        if (col.readonly)
                                            select.prop("disabled", true);

                                        getValue(col.select.className, null, function (v) {
                                            if (v) select.addClass(v);
                                        });

                                        if (col.select.items) {
                                            getValue(col.select.items, function (items) { populateSelect(select, items); }, function (v) {
                                                if ($.isArray(v)) populateSelect(select, v);
                                            });
                                        }

                                        select.val(item[col.key]);

                                        content = select;
                                        break;
                                    default: //text
                                        content = $("<input/>").attr("type", "text").css("width", "100%").prop("disabled", col.readonly).val(item[col.key]);
                                        break;
                                }
                                formGroup.append(content);
                            }
                        } else { //add
                            var n = (col.data && col.data.name) ? col.data.name : col.key;
                            var formGroup = $("<div/>").addClass("form-group").attr("data-key", col.key);
                            var label = $("<label/>").addClass("control-label").html(n).appendTo(formGroup);

                            if (col.footer.html) {
                                getValue(col.footer.html, index, function (v) {
                                    formGroup.append(v);
                                });
                            } else {

                                var content = null;
                                switch (col.type) {
                                    case "select":
                                        var select = $("<select/>").css("width", "100%");

                                        getValue(col.select.className, null, function (v) {
                                            if (v) select.addClass(v);
                                        });

                                        getValue(col.select.items, function (items) { populateSelect(select, items); }, function (v) {
                                            if ($.isArray(v)) populateSelect(select, v);
                                        });

                                        content = select;
                                        break;
                                    default: //text
                                        content = $("<input/>").attr("type", "text").css("width", "100%");
                                        break;
                                }
                                formGroup.append(content);
                            }
                        }

                        body.append(formGroup);
                    }
                });


                div.modal().on("hide.bs.modal", function (e) {
                    if (mode == "edit")
                        cancelRow(row);
                });
            }

            var findElementByKey = function (container, key) {
                return $("[data-key='" + key + "']", container);
            }

            var populateSelect = function (select, items) {
                $.each(items, function (index, i) {
                    select.append($("<option/>").val(i.value).text(i.text));
                })
            }

            var source = function (callback) {
                if (!opt.url) {
                    $("thead", table).html("");
                    $("tfoot", table).html("");
                    $("tbody", table).append($("<tr/>").append($("<td/>").css({ "background-color": "#ff0000", "color": "#fff" }).html("Missing option: url")));
                    return;
                }

                $.ajax({
                    "url": opt.url,
                    "type": "GET",
                    "success": function (data) {
                        header(data, callback);
                    }
                });
            }

            var header = function (data, callback) {
                var row = $("<tr/>");

                $.each(opt.columns, function (index, col) {
                    col = $.extend(true, { "key": null, "className": null, "header": { "html": null, "width": null, "controls": { "width": null } }, "data": null }, col);

                    if (col.key) {
                        var th = $("<th/>");
                        th.attr("data-key", col.key);

                        getValue(col.data, index, function (data) {
                            if (data) {
                                for (var key in data)
                                    th.attr("data-" + key, data[key]);
                            }
                        });

                        getValue(col.className, null, function (v) {
                            if (v) th.addClass(v);
                        })

                        getValue(col.header.html, index, function (v) {
                            th.html(v);
                        });

                        getValue(col.header.width, null, function (v) {
                            if (v) th.css("width", v);
                        });

                        row.append(th);
                    }
                })

                //this is for the row controls
                getValue(opt.header.controls.width, null, function (width) {
                    getValue(opt.header.controls.html, null, function (html) {
                        row.append($("<th/>").addClass("row-controls").css({ "width": width || "90px;" }).html(html || "&nbsp;"));
                    })
                });

                $("thead", table).html(row);

                footer(data, callback);
            }

            var footer = function (data, callback) {

                getValue(opt.canAdd, data, function (canAdd) {
                    if (canAdd) {
                        $("tfoot", table).show();

                        var row = $("<tr/>");

                        if (opt.editMode == "inline") {

                            $.each(opt.columns, function (index, col) {
                                col = $.extend(true, { "key": null, "type": "text", "className": null, "select": { "items": null, "className": null }, "footer": { "html": null } }, col);

                                if (col.key) {
                                    var td = $("<td/>");
                                    td.attr("data-key", col.key);

                                    getValue(col.className, null, function (v) {
                                        if (v) td.addClass(v);
                                    });

                                    if (col.footer.html) {
                                        getValue(col.footer.html, index, function (v) {
                                            td.html(v);
                                        });
                                    } else {
                                        var content = null;
                                        switch (col.type) {
                                            case "select":
                                                var select = $("<select/>").css("width", "100%");

                                                getValue(col.select.className, null, function (v) {
                                                    if (v) select.addClass(v);
                                                });

                                                getValue(col.select.items, function (items) { populateSelect(select, items); }, function (v) {
                                                    if ($.isArray(v)) populateSelect(select, v);
                                                });

                                                content = $("<div/>").css("padding-right", "10px").html(select);

                                                break;
                                            default: //text
                                                content = $("<div/>").css("padding-right", "10px").html($("<input/>").attr("type", "text").css("width", "100%"));
                                                break;
                                        }
                                        td.html(content);
                                    }

                                    row.append(td);
                                }
                            });

                            //this is for the row controls
                            row.append($("<td/>").addClass("row-controls").css("text-align", "center").html(
                                $("<button/>")
                                    .attr("type", "button")
                                    .addClass("add-button")
                                    .addClass(opt.footer.button.className || null)
                                    .html(opt.footer.button.text || "ADD")
                            ));
                        } else {
                            var button = $("<button/>")
                                .attr("type", "button")
                                .addClass("add-button")
                                .addClass(opt.footer.button.className || null)
                                .html(opt.footer.button.text || "ADD");

                            row.append($("<td/>").attr("colspan", opt.columns.length + 1).append(button));
                        }

                        $("tfoot", table).html(row);
                    } else {
                        $("tfoot", table).html("").hide();
                    }
                });



                body(data, callback);
            }

            var body = function (data, callback) {

                $("tbody", table).html("");

                if (data.length > 0) {
                    $.each(data, function (index, item) {
                        var row = $("<tr/>");
                        row.data("item", item);

                        $.each(opt.columns, function (index, col) {
                            col = $.extend(true, { "key": null, "type": null, "className": null, "html": null, "data": null }, col);

                            if (col.key) {
                                var td = $("<td/>");
                                td.attr("data-key", col.key);

                                getValue(col.data, index, function (data) {
                                    if (data) {
                                        for (var key in data)
                                            td.attr("data-" + key, data[key]);
                                    }
                                });

                                getValue(col.className, null, function (v) {
                                    if (v) td.addClass(v);
                                });

                                if (col.html) {
                                    getValue(col.html, item, function (v) {
                                        td.html(v);
                                    });
                                } else {
                                    td.html(item[col.key]);
                                }

                                row.append(td);
                            }
                        })

                        row.append($("<td/>").addClass("row-controls").html(createEditDeleteControls(item)));

                        $("tbody", table).append(row);
                    });
                }
                else {
                    getValue(opt.nodata, null, function (v) {
                        if (v) {
                            var row = $("<tr/>").append($("<td/>").attr("colspan", opt.columns.length + 1).addClass("nodata").html(v));
                            $("tbody", table).append(row);
                        }
                    });
                }

                if ($("tfoot", table).is(":visible"))
                    $("tbody tr:last-child", table).addClass("last");
                else
                    $("tbody tr:last-child", table).removeClass("last");

                callback(table, data);
            }

            var saveItem = function (item) {
                $.ajax({
                    "url": opt.url,
                    "type": "POST",
                    "data": item,
                    "success": function (data) {
                        source(opt.reload);
                    }
                });
            }

            var addRow = function () {
                if (opt.editMode == "inline") {
                    var item = newItem($("tfoot tr", table));
                    saveItem(item);
                }
                else {
                    createPopup("add");
                }
            }

            var deleteRow = function (row) {
                var item = row.data("item");
                $.ajax({
                    "url": opt.url,
                    "type": "DELETE",
                    "data": item,
                    "success": function (data) {
                        source(opt.reload);
                    }
                });
            }

            var editRow = function (row) {
                if (opt.editMode == "popup") {
                    createPopup("edit", row);
                } else { //inline
                    var item = row.data("item");
                    $.each(opt.columns, function (index, col) {
                        col = $.extend(true, { "key": null, "type": "text", "select": { "items": null, "className": null }, "readonly": false, "edit": { "html": null } }, col);

                        if (col.key && !col.readonly) {
                            var td = findElementByKey(row, col.key);

                            if (col.edit.html) {
                                getValue(col.edit.html, item, function (v) {
                                    td.html(v);
                                });
                            } else {
                                var content = null;
                                switch (col.type) {
                                    case "select":
                                        var select = $("<select/>").css("width", "100%");

                                        getValue(col.select.className, null, function (v) {
                                            if (v) select.addClass(v);
                                        });

                                        if (col.select.items) {
                                            getValue(col.select.items, function (items) { populateSelect(select, items); }, function (v) {
                                                if ($.isArray(v)) populateSelect(select, v);
                                            });
                                        }

                                        select.val(item[col.key]);

                                        content = $("<div/>").css("padding-right", "10px").html(select);
                                        break;
                                    default: //text
                                        content = $("<div/>").css("padding-right", "10px").html($("<input/>").attr("type", "text").css("width", "100%").val(item[col.key]));
                                        break;
                                }
                                td.html(content);
                            }
                        }
                    });
                }

                $(".row-controls", row).html(createSaveCancelControls(item));
                row.addClass("editing");
            }

            var cancelRow = function (row) {
                var item = row.data("item");

                $.each(opt.columns, function (index, col) {
                    col = $.extend(true, { "key": null, "type": "text", "readonly": false, "html": null, "edit": { "html": null } }, col);

                    if (col.key && !col.readonly) {
                        var td = findElementByKey(row, col.key);

                        if (col.html) {
                            getValue(col.html, item, function (v) {
                                td.html(v);
                            });
                        } else {
                            td.html(item[col.key]);
                        }
                    }
                });

                $(".row-controls", row).html(createEditDeleteControls(item));
                row.removeClass("editing");
            }

            var saveRow = function (row) {
                var item = row.data("item");
                updateItem(row, item);
                saveItem(item);
            }

            var updateItem = function (container, item) {

                $.each(opt.columns, function (index, col) {
                    col = $.extend(true, { "key": null, "type": "text", "readonly": false, "html": null, "edit": { "html": null } }, col);

                    if (col.key && !col.readonly) {
                        var el = findElementByKey(container, col.key);
                        var v = null;
                        switch (col.type) {
                            case "select":
                                v = $("select", el).val();
                                break;
                            default: //text
                                v = $("input[type='text']", el).val();
                                break;
                        }

                        item[col.key] = v;
                    }
                });
            }

            var newItem = function (container) {
                var result = {};
                $.each(opt.columns, function (index, col) {
                    col = $.extend(true, { "key": null, "type": "text" }, col);
                    if (col.key) {
                        var el = findElementByKey(container, col.key);
                        var v = null;
                        switch (col.type) {
                            case "select":
                                v = $("select", el).val();
                                break;
                            default: //text
                                v = $("input[type='text']", el).val();
                                break;
                        }
                        result[col.key] = v;
                    }
                });
                return result;
            }

            getValue(opt.className, null, function (v) {
                if (v) table.addClass(v);
            });

            //this get data and fills the table
            source(opt.complete);

            table.on("click", ".add-button", function (e) {
                addRow();
            }).on("click", ".row-delete", function (e) {
                e.preventDefault();
                var row = $(this).closest('tr');
                deleteRow(row);
            }).on("click", ".row-edit", function (e) {
                e.preventDefault();
                var row = $(this).closest('tr');
                editRow(row);
            }).on("click", ".row-cancel", function (e) {
                e.preventDefault();
                var row = $(this).closest('tr');
                cancelRow(row);
            }).on("click", ".row-save", function (e) {
                e.preventDefault();
                var row = $(this).closest('tr');
                saveRow(row);
            });

            $self.on("editable_reload", function (e) {
                source(opt.reload);
            });

        });
    }
}(jQuery));