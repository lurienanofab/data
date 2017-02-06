(function ($) {
    $.fn.report = function (options) {
        return this.each(function () {
            var $this = $(this);

            var opt = $.extend({}, { "alias": null }, $this.data(), options);

            var getAlert = function (type, text) {
                return $("<div/>", { "class": "alert alert-" + type, "role": "alert" }).html(text);
            }

            //https://gist.github.com/jasonrhodes/2321581
            var getProperty = function (propertyName, object) {
                var parts = propertyName.split("."),
                    length = parts.length,
                    property = object || this;

                for (var i = 0; i < length; i++) {
                    if (property) property = property[parts[i]];
                }

                return property;
            }

            var getReportConfig = function () {
                return $.ajax({
                    "url": "http://lnf-jgett.eecs.umich.edu/data/feed/reports/" + opt.alias + "/cfg",
                    "jsonp": "callback",
                    "dataType": "jsonp"
                });
            }

            var getReportData = function (cfg) {
                $.ajax({
                    "url": cfg.feed,
                    "jsonp": "callback",
                    "dataType": "jsonp"
                }).done(function (data) {
                    renderReport(data, cfg);
                }).fail(function (xhr) {
                    $this.html(getAlert("danger", xhr.responseText));
                });
            }

            var renderReport = function (data, cfg) {
                var panel = $("<div/>", { "class": "panel panel-default" });
                var heading = $("<div/>", { "class": "panel-heading" }).appendTo(panel);
                var title = $("<h3/>", { "class": "panel-title" }).appendTo(heading);
                var body = $("<div/>", { "class": "panel-body" }).appendTo(panel);
                var table = $("<table/>", { "class": "table" + (cfg.striped ? " table-striped" : "") }).appendTo(body);
                var thead = $("<thead/>").appendTo(table);
                var tbody = $("<tbody/>").appendTo(table);

                var rows = getProperty(cfg.rows, data);

                if (cfg.columns) {
                    var tr = $("<tr/>").appendTo(thead);
                    $.each(cfg.columns, function (index, item) {
                        $("<th/>").html(item.title ? item.title : item.data).appendTo(tr);
                    });
                } else {
                    //use the first row to define the columns
                    if (rows.length > 0) {
                        $.each(rows[0], function (key, value) {
                            $("<th/>").html(key).appendTo(tr);
                        });
                    } else {
                        //no data and no columns
                        $this.html($("<em/>", { "class": "text-muted" }).html("No data was found for this report."));
                        return;
                    }
                }

                $.each(rows, function (r, row) {
                    var tr = $("<tr/>").appendTo(tbody);
                    if (cfg.columns) {
                        $.each(cfg.columns, function (c, col) {
                            $("<td/>").html(row[col.data]).appendTo(tr);
                        });
                    } else {
                        $.each(row, function (key, value) {
                            $("<td/>").html(value).appendTo(tr);
                        });
                    }
                });

                title.html(cfg.title);

                $this.html(panel);

                if (cfg.datatables) {
                    if (typeof cfg.datatables == "object")
                        table.dataTable(cfg.datatables);
                    else
                        table.dataTable();
                }
            }

            if (!opt.alias) {
                $this.html(getAlert("danger", "Missing required option: alias"));
                return;
            }

            getReportConfig().done(function (cfg) {
                if (cfg.error)
                    $this.html(getAlert("danger", cfg.errorMessage));
                else
                    getReportData(cfg);
            }).fail(function (xhr) {
                console.log(xhr);
                $this.html(getAlert("danger", xhr.responseText));
            });
        });
    }
}(jQuery));