(function ($) {

    function Report(url, args) {
        this.title = args.title || "Report: " + url;
        this.defaultDateFormat = args.defaultDateFormat || "M/D/YYYY h:mm:ss A";
        this.criteria = args.criteria || [];
        this.dataType = args.dataType || "html";
        this.feed = args.feed || "feed";
        this.rows = args.rows || "rows";
        this.columns = args.columns || [];
        this.export = args.export || [];
        this.striped = args.striped;
        this.autoRun = args.autoRun;
        this.datatables = args.datatables || null;
    }

    $.fn.report = function (options) {
        return this.each(function () {
            var $this = $(this);

            var opt = $.extend({}, { "url": "report.json", "onRun": null }, options, $this.data());

            //http://stackoverflow.com/questions/901115/how-can-i-get-query-string-values-in-javascript
            var getParameterByName = function (name) {
                name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
                var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
                    results = regex.exec(location.search);
                return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
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

            var onRun = function () {
                if (typeof opt.onRun == 'function')
                    opt.onRun();
            }

            $.ajax({
                'url': opt.url,
                'dataType': 'json'
            }).done(function (data, textStatus, jqXHR) {

                var report = new Report(opt.url, data);

                $("head title").text(report.title);
                $(".report-title", $this).html(report.title);

                var getInput = function (i) {
                    var result = null;

                    //try to get a value from the querystring
                    var val = getParameterByName(i.id) || i.default || null;
                    
                    var date = new Date();
                    var thisPeriod = new Date(date.getFullYear(), date.getMonth(), 1);
                    var nextPeriod = new Date(thisPeriod.getFullYear(), thisPeriod.getMonth() + 1, 1);
                    var prevPeriod = new Date(thisPeriod.getFullYear(), thisPeriod.getMonth() - 1, 1);
                    val = val.replace("{{ThisPeriod}}", moment(thisPeriod).format(report.defaultDateFormat));
                    val = val.replace("{{NextPeriod}}", moment(nextPeriod).format(report.defaultDateFormat));
                    val = val.replace("{{PrevPeriod}}", moment(prevPeriod).format(report.defaultDateFormat));

                    if (i.type == "select") {
                        result = $("<select/>", { "id": i.id, "name": i.id, "class": "form-control" });
                        if ($.isArray(i.items)) {
                            $.each(i.items, function (index, item) {
                                var selectOpt = $("<option/>", { "value": item.value }).html(item.text);
                                if (val && item.value === val)
                                    selectOpt.prop('selected', true);
                                result.append(selectOpt);
                            });
                        }
                    } else {

                        if (i.type == "date" && val) {
                            var m = moment(new Date(val));
                            val = m.format(report.defaultDateFormat);
                        }

                        result = $("<input/>", { "type": "text", "id": i.id, "name": i.id, "class": "form-control" }).val(val);
                    }
                    return result;
                }

                if ($.isArray(report.criteria) && report.criteria.length > 0) {
                    $.each(report.criteria, function (index, item) {
                        $(".criteria", $this).append(
                            $("<div/>", { "class": "form-group" })
                                .append($("<label/>", { "for": item.id }).html(item.label))
                                .append(getInput(item))
                        );
                    });
                }

                var getUrlWithParams = function (url) {
                    var result = url;
                    if ($.isArray(report.criteria) && report.criteria.length > 0) {
                        $.each(report.criteria, function (index, item) {
                            var value = $("#" + item.id).val();
                            if (item.type == "date" && item.format)
                                value = moment(new Date(value)).format(item.format);
                            result = result.replace("[" + item.id + "]", value);
                        });
                    }
                    return result;
                }

                var validate = function () {
                    var result = true;
                    if ($.isArray(report.criteria) && report.criteria.length > 0) {
                        $.each(report.criteria, function (index, item) {
                            var field = $("#" + item.id, $this);
                            field.attr("placeholder", "");
                            field.closest(".form-group").removeClass("has-error");
                            var value = field.val();
                            if (item.required && (!value || (item.type == "select" && value == "-1"))) {
                                result = false;
                                if (item.type == "select") {
                                    //nothing to do here, adding the has-error class will give it a red border
                                } else {
                                    field.attr("placeholder", "Required");
                                }
                                field.closest(".form-group").addClass("has-error");
                            } else {
                                if (item.type == "date") {
                                    if (!moment(new Date(value)).isValid()) {
                                        result = false;
                                        field.val('').attr("placeholder", "Enter a valid date");
                                        field.closest(".form-group").addClass("has-error");
                                    }
                                }
                            }
                        });
                    }
                    return result;
                }

                var run = function () {
                    if (validate()) {
                        $(".output", $this).html($("<img/>", { "src": "//ssel-apps.eecs.umich.edu/static/images/ajax-loader.gif", "alt": "loading..." }));

                        $.ajax({
                            'url': getUrlWithParams(report.feed),
                            'dataType': report.dataType
                        }).done(function (d) {
                            setTimeout(function () {
                                var table = null;
                                if (report.dataType == 'html') {
                                    var html = d;
                                    $(".output", $this).html(html);
                                    table = $(".output table", $this).eq(0);
                                    if ($("tbody tr", table).length == 0) {
                                        var colspan = $("thead tr th", table).length;
                                        $("tbody", table).html($("<tr/>").html($("<td/>", { "class": "no-data", "colspan": colspan }).html("No records found")));
                                    }
                                } else if (report.dataType == 'jsonp') {
                                    var rows = getProperty(report.rows, d);
                                    if ($.isArray(rows) && rows.length > 0) {
                                        table = $("<table/>", { "class": "table" }).append("<thead/>").append("<tbody/>");

                                        //add the header row
                                        $("thead", table).append("<tr/>");

                                        var cols = report.columns || [];

                                        if (cols.length == 0) {
                                            //get the columns from the first row
                                            $.each(rows[0], function (k, v) {
                                                cols.push({ "data": k });
                                            });
                                        }

                                        //add header row cells
                                        $.each(cols, function (c, col) {
                                            var title = col.title || col.data;
                                            var attr = col.headerStyle ? { "style": col.headerStyle } : null;
                                            $("thead tr", table).append($("<th/>", attr).html(title));
                                        });

                                        //add the data rows
                                        $.each(rows, function (r, row) {
                                            var tr = $("<tr/>");
                                            $.each(cols, function (c, col) {
                                                var val = getProperty(col.data, row);
                                                if (col.type == "date" && col.format) {
                                                    var f = col.format || opt.defaultDateFormat;
                                                    var m = moment(new Date(val));
                                                    //adding a hidden span here for easy sorting by date
                                                    val = '<span style="display: none;">' + m.format("YYYYMMDDHHmmss") + '</span>' + m.format(f);
                                                } else if (col.type == "currency") {
                                                    val = "$" + parseFloat(val).toFixed(2);
                                                }
                                                var attr = col.itemStyle ? { "style": col.itemStyle } : null;
                                                tr.append($("<td/>", attr).html(val));
                                            });
                                            $("tbody", table).append(tr);
                                        });

                                        $(".output", $this).html(table);
                                    } else {
                                        $(".output", $this).html($("<div/>", { "class": "no-data" }).html("No records found"));
                                    }
                                }

                                //apply bootstrap table stipe if option is specified
                                if (table && report.striped)
                                    table.addClass("table-striped");

                                //create export links per the report config
                                $(".exports", $this).html('');
                                if ($.isArray(report.export) && report.export.length > 0) {
                                    var exp = $("<div/>", { "class": "export" }).append("Export: ");
                                    var count = 0;
                                    $.each(report.export, function (index, item) {
                                        var href = getUrlWithParams(item.url);
                                        var link = $("<a/>", { "href": href }).html(item.label);
                                        if (count > 0) exp.append(" | ");
                                        exp.append(link);
                                        count++;
                                    });
                                    $(".exports", $this).html(exp);
                                }

                                //get the url for this report (for bookmarking, sending via email, etc)
                                console.log(window.location.search);
                                var url = window.location.href.replace(window.location.search, '');
                                var delim = '?';
                                if ($.isArray(report.criteria)) {
                                    $.each(report.criteria, function (index, item) {
                                        url += delim + item.id + '=[' + item.id + ']';
                                        delim = '&';
                                    });
                                }

                                var reportUrl = getUrlWithParams(url);

                                reportUrl += delim + 'run=1';
                                delim = '&';

                                //add back any extra querystring params not defined by critera
                                var pairs = window.location.search.replace('?', '').split('&');
                                $.each(pairs, function (index, item) {
                                    var kv = item.split('=');
                                    var key = kv[0];
                                    if (reportUrl.indexOf(key) == -1) {
                                        reportUrl += delim + item;
                                        delim = '&';
                                    }
                                });

                                if (reportUrl)
                                    $(".report-url", $this).html($("<a/>", {
                                        "href": reportUrl,
                                        "data-toggle": "tooltip",
                                        "data-placement": "right",
                                        "title": "Use this link to refresh, bookmark, share via email, etc."
                                    }).html(reportUrl));

                                //datatables support
                                if (report.datatables && table) {

                                    //report.datatables is either a true boolean or an datatables config object
                                    var dt = (typeof report.datatables == 'boolean') ? {} : report.datatables;

                                    var cfg = $.extend({}, {
                                        "initComplete": function () {
                                            onRun();
                                        }
                                    }, dt);

                                    table.dataTable(cfg);
                                } else {
                                    onRun();
                                }

                            }, 500);
                        });
                    }
                }

                //run the report when the button is clicked or ENTER key is pressed
                $("form", $this).on("click", ".run", function (e) {
                    run();
                }).on("keydown", function (e) {
                    if (e.which == 13) run();
                });

                //automatically run the report if specified by the report config or in the querystring
                if (report.autoRun || getParameterByName('run') == '1')
                    run();

            }).fail(function (jqXHR, textStatus, errorThrown) {
                if (jqXHR.status == 404) {
                    $this.html($("<div/>", { "class": "alert alert-danger", "role": "alert" })
                        .append(jqXHR.status + ' ' + jqXHR.statusText + ': ' + opt.url)).css("margin-top", "20px");
                } else {
                    $this.html($("<div/>", { "class": "alert alert-danger", "role": "alert" })
                        .append(jqXHR.status + ' ' + jqXHR.statusText)).css("margin-top", "20px");
                }
            }).always(function () {
                $this.show();
            });
        });
    };
}(jQuery));