(function ($) {
    $.fn.restgrid = function (options) {
        return this.each(function () {
            var $this = $(this);

            var table = $("table", $this);

            var currentRow = null;

            var items = null;

            var opt = $.extend({}, {
                "apiurl": function () { return "api"; },
                "onload": function (e) { },
                "onsave": function (e) { /*return true to stop processing*/ },
                "ondelete": function (e) { /*return true to stop processing*/ },
                "columns": [],
                "canEdit": true,
                "canAdd": true
            }, $this.data(), options);

            var getApiUrl = function (item) {
                if (typeof opt.apiurl == "function")
                    return opt.apiurl(item);
                else {
                    var result = opt.apiurl || "api";
                    if (item) {
                        var delim = (result.indexOf("?") >= 0) ? "&" : "?";
                        $.each(item, function (k, v) {
                            if (result.toLowerCase().indexOf(k.toLowerCase()) == -1) {
                                result += delim + k + "=" + v;
                                delim = "&";
                            }
                        });
                    }
                    return result;
                }
            }

            var onload = function () {
                if (typeof opt.onload == "function")
                    opt.onload({ "table": table });
            }

            var onsave = function (row) {
                //return true to stop processing
                if (typeof opt.onsave == "function")
                    return opt.onsave({ "item": row.data("item"), "table": table, "row": row, "load": loadTable, "items": items });
                else
                    return false;
            }

            var ondelete = function (row) {
                //return true to stop processing
                if (typeof opt.ondelete == "function")
                    return opt.ondelete({ "item": row.data("item"), "table": table, "row": row, "load": loadTable, "items": items });
                else
                    return false;
            }

            var loadTable = function () {
                var rows = [];

                $.ajax({ "url": getApiUrl(), "type": "GET", "dataType": "json" }).done(function (data) {
                    items = data;

                    if (opt.canAdd)
                        rows.push(getAddRow());

                    $.each(items, function (index, item) {
                        rows.push(getRow(item));
                    });

                    $("tbody", table).html(rows);

                    onload();
                });
            };

            var getValue = function (row, col) {
                if (isAddRow(row)) {
                    return col.default || "";
                } else {
                    var item = row.data("item");
                    var prop = col.property;
                    return item[prop];
                }
            }

            var getEditValue = function (row, cell) {
                if (cell.data("readonly"))
                    return getValue(row, getColumn(cell));

                if (cell.data("type") == "checkbox")
                    return $("input[type='checkbox']", cell).prop("checked");
                else if (cell.data("type") == "select")
                    return $("select", cell).val();
                else
                    return $("input[type='text']", cell).val();
            }

            var setValue = function (row, cell, value) {
                var item = row.data("item");
                var prop = cell.data("property");
                item[prop] = value;
            }

            var updateItem = function (item) {
                return $.ajax({ "url": getApiUrl(), "type": "POST", "data": item });
            }

            var deleteItem = function (item) {
                return $.ajax({ "url": getApiUrl(item), "type": "DELETE" });
            }

            var getEditDeleteControls = function () {
                var result = $("<div/>");
                if (opt.canEdit) {
                    result.append($("<a/>", { "href": "#", "data-command": "edit", "class": "command-link" }).html("edit"));
                    result.append(" | ");
                    result.append($("<a/>", { "href": "#", "data-command": "delete", "class": "command-link" }).html("delete"));
                } else {
                    result.append($("<span/>").css({ "color": "#808080" }).html("edit"));
                    result.append(" | ");
                    result.append($("<span/>").css({ "color": "#808080" }).html("delete"));
                }
                return result;
            };

            var getSaveCancelControls = function () {
                var result = $("<div/>");
                if (opt.canEdit) {
                    result.append($("<a/>", { "href": "#", "data-command": "save", "class": "command-link" }).html("save"));
                    result.append(" | ");
                    result.append($("<a/>", { "href": "#", "data-command": "cancel", "class": "command-link" }).html("cancel"));
                } else {
                    result.append($("<span/>").css({ "color": "#808080" }).html("save"));
                    result.append(" | ");
                    result.append($("<span/>").css({ "color": "#808080" }).html("cancel"));
                }
                return result;
            };

            var getAddControls = function () {
                var result = $("<div/>");
                result.append($("<a/>", { "href": "#", "data-command": "add", "class": "command-link" }).html("add"));
                return result;
            }

            var addRow = function (row) {
                var item = {};
                var errors = false;

                $("td", row).each(function () {
                    var cell = $(this);
                    cell.removeClass("has-error");
                    if (!isControlsCell(cell)) {
                        var prop = cell.data("property");
                        var val = getEditValue(row, cell);
                        if (cell.data("required") && (typeof val === "undefined" || val === null || val === "")) {
                            cell.addClass("has-error");
                            errors = true;
                        } else {
                            item[prop] = val;
                        }
                    }
                });

                if (errors) return;

                updateItem(item).done(function (data) {
                    var row = getRow(data);
                    if (!onsave(row))
                        $("tbody > tr.add-row", table).after(row);
                });
            }

            var getColumn = function (cell) {
                var result = null;
                $.each(opt.columns, function (c, col) {
                    col = $.extend({ "property": null, "visible": true, "controls": false, "type": "text", "readonly": false }, col);
                    if (col.property == cell.data("property")) {
                        result = col;
                        return false; //break
                    }
                });
                return result;
            }

            var isAddRow = function (row) {
                return row.hasClass("add-row");
            }

            var isControlsCell = function (cell) {
                return cell.hasClass("controls");
            }

            var setEditControl = function (row, cell) {
                var col = getColumn(cell);
                if (col.type == "checkbox") {
                    if (isAddRow(row) || !col.readonly)
                        $("input[type='checkbox']", cell).prop("disabled", false);
                } else if (col.type == "select") {
                    if (isAddRow(row) || !col.readonly) {
                        var val = getValue(row, col);
                        var duplicates = ((typeof col.duplicates == "boolean") ? col.duplicates : true) || !isAddRow(row);
                        var control = getSelectControl(col, val, duplicates);
                        cell.html(control);
                    }
                } else {
                    if (isAddRow(row) || !col.readonly) {
                        var val = getValue(row, col);
                        var control = getTextControl(col, val);
                        cell.html(control);
                    }
                }
            }

            var editRow = function (row) {
                if (currentRow)
                    cancelRow(currentRow);

                currentRow = row;

                $("td", row).each(function () {
                    var cell = $(this);
                    if (isControlsCell(cell))
                        cell.html(getSaveCancelControls());
                    else
                        setEditControl(row, cell);
                });
            }

            var deleteRow = function (row) {
                if (currentRow)
                    cancelRow(currentRow);

                var item = row.data("item");
                deleteItem(item).done(function () {
                    if (!ondelete(row))
                        row.remove();
                });
            }

            var saveRow = function (row) {
                var item = row.data("item");
                var errors = false;
                $("td", row).each(function () {
                    var cell = $(this);
                    cell.removeClass("has-error");
                    if (!isControlsCell(cell)) {
                        var prop = cell.data("property");
                        var val = getEditValue(row, cell);
                        if (cell.data("required") && (typeof val == "undefined" || val === null || val === "")) {
                            cell.addClass("has-error")
                            errors = true;
                        } else {
                            item[prop] = val;
                        }
                    }
                });

                if (errors) return;

                updateItem(item).done(function (data) {
                    if (!onsave(row)) {
                        row.data("item", data);
                        cancelRow(row);
                    } else {
                        currentRow = null;
                    }
                });
            }

            var cancelRow = function (row) {
                currentRow = null;
                $("td", row).each(function () {
                    var cell = $(this);
                    var col = getColumn(cell);
                    if (isControlsCell(cell)) {
                        cell.html(getEditDeleteControls());
                    } else {
                        if (col.type == "checkbox")
                            $("input[type='checkbox']", cell).prop("disabled", true).prop("checked", getValue(row, cell));
                        else
                            cell.html(getValue(row, col));
                    }
                });
            }

            var exists = function (prop, value) {
                var result = false;

                if (items == null || items.length == 0)
                    return result;

                $.each(items, function (index, item) {
                    if (item[prop] == value) {
                        result = true;
                        return false;
                    }
                });

                return result;
            }

            var getAddRow = function () {
                var result = $("<tr/>", { "class": "add-row" });
                $.each(opt.columns, function (c, col) {
                    col = $.extend({ "property": null, "visible": true, "controls": false, "type": "text", "readonly": false, "required": false }, col);
                    if (col.visible) {
                        if (col.controls) {
                            result.append($("<td/>", { "class": "controls" }).css("vertical-align", "middle").html(getAddControls()));
                        } else {
                            if (col.property) {
                                var cell = $("<td/>", { "data-property": col.property, "data-type": col.type, "data-required": col.required });
                                setEditControl(result, cell);
                                result.append(cell);
                            }
                        }
                    }
                });
                return result;
            }

            var getTextControl = function (col, val) {
                var edit = $.extend(true, {
                    "attr": { "type": "text", "class": "form-control" },
                    "css": { "width": "95%" }
                }, col.edit);

                var result = $("<input/>", edit.attr).val(val);
                if (edit.css) result.css(edit.css);

                return result;
            }

            var getSelectControl = function (col, val, duplicates) {
                var edit = $.extend(true, {
                    "attr": { "class": "form-control" },
                    "css": { "width": "95%" }
                }, col.edit);

                var selectItems = col.items || [];

                var options = [];
                $.each(selectItems, function (index, item) {
                    item = $.extend({ "value": null, "text": null }, item);
                    var val = item.value || item.text;
                    if (duplicates || !exists(col.property, val)) {
                        options.push($("<option/>", { "value": val }).html(item.text));
                    }
                });

                var result = $("<select/>", edit.attr).html(options).val(val);
                if (edit.css) result.css(edit.css);

                return result;
            }

            var getRow = function (item) {
                var result = $("<tr/>").data("item", item);
                $.each(opt.columns, function (c, col) {
                    col = $.extend({ "property": null, "visible": true, "controls": false, "type": "text", "readonly": false, "required": false }, col);
                    if (col.visible) {
                        if (col.controls) {
                            result.append($("<td/>", { "class": "controls" }).css("vertical-align", "middle").html(getEditDeleteControls()));
                        } else {
                            if (col.property) {
                                var val = item[col.property];
                                var cell = $("<td/>", { "data-property": col.property, "data-type": col.type, "data-readonly": col.readonly, "data-required": col.required });
                                if (col.type == "checkbox") {
                                    cell.html($("<input/>", { "type": "checkbox" }).prop("checked", val).prop("disabled", true));
                                } else {
                                    //do the same for select and text
                                    cell.html(val);
                                }
                                result.append(cell);
                            }
                        }
                    }
                });
                return result;
            }

            //initial load
            loadTable();

            $this.on("click", ".command-link", function (e) {
                e.preventDefault();
                var link = $(this);
                var row = link.closest("tr");
                var command = link.data("command");
                switch (command) {
                    case "add":
                        addRow(row);
                        break;
                    case "edit":
                        editRow(row);
                        break;
                    case "delete":
                        deleteRow(row);
                        break;
                    case "save":
                        saveRow(row);
                        break;
                    case "cancel":
                        cancelRow(row);
                        break;
                }
            });

        });
    }
}(jQuery));