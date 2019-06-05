(function ($) {
    $.fn.feeds = function () {
        return this.each(function () {
            var $this = $(this);

            var console = $this.data("console") === true;

            if (!console) {
                var feedTable = $this.dataTable({
                    "stateSave": true,
                    "columnDefs": [
                        { "width": "300px", "targets": [0] },
                        {
                            "width": "305px",
                            "orderable": false,
                            "targets": [2]
                        },
                        { "visible": false, "targets": [3, 4] }
                    ],
                    "order": [[1, "asc"]],
                    "pagingType": "full_numbers",
                    "autoWidth": false
                });
            }

            var ajaxUrl = function () {
                return $this.data("ajaxurl");
            };

            var feedType = function () {
                var result = $('.feed-types', $this).find('input:checked').val();
                return result;
            };

            var getMode = function () {
                var result = '';
                switch (feedType()) {
                    case 'Python':
                        result = 'text/x-python';
                        break;
                    default:
                        result = 'text/x-sql';
                        break;
                }
                return result;
            };

            var setRunError = function (msg) {
                $('.run-output', $this).html($("<div/>", { "class": "alert alert-danger", "role": "alert" }).html(msg));
                return false;
            };

            var bufferContent = function (buffer) {
                return $('<pre/>').html(buffer);
            };

            var dataContent = function (data) {

                var result = [];

                $.each(data, function (i, v) {
                    var table = $("<table/>", { "class": "table" });
                    table.append("<thead/>").append("<tbody/>");

                    var headers = v.Headers;
                    var items = v.Items;
                    var row = null;

                    if (headers !== null && headers.length > 0) {
                        row = $('<tr/>');
                        $.each(headers, function (i, v) {
                            row.append($('<th/>').html(v));
                        });
                        $("thead", table).append(row);
                    }

                    if (items !== null && items.length > 0) {
                        $.each(items, function (i, v) {
                            var row = $('<tr/>');
                            $.each(v, function (x, y) {
                                row.append($('<td/>').html(y));
                            });
                            $("tbody", table).append(row);
                        });
                    } else {
                        row = $('<tr/>', { 'colspan': Math.max(headers.length, 1) });
                        row.append($('<td/>', { 'class': 'nodata' }).html('No results to display.'));
                        $("tbody", table).append(row);
                    }

                    result.push(table);
                });

                return result;
            };

            var runFeed = function (type, script, qs) {
                if (script === '')
                    return setRunError('Please enter a script.');
                $.ajax({
                    url: ajaxUrl(),
                    data: { "Command": "run-script", "FeedType": type, "Query": script, "DefaultParameters": qs },
                    type: 'POST',
                    dataType: 'json',
                    success: function (data, textStatus, jqXHR) {
                        if (data.Error !== '')
                            setRunError(data.Error);
                        else {
                            if (data.Parameters) {
                                var params = $("<div/>", { "class": "parameters" });
                                params.html("<strong>Parameters:</strong> " + data.Parameters);
                                $('.run-output', $this).html(params);
                            }

                            var div = $("<div/>");
                            var count = 0;

                            if (data.Buffer) {
                                div.append(
                                    $("<div/>", { "class": "panel panel-default" }).append(
                                        $("<div/>", { "class": "panel-heading" }).html("Buffer")
                                    ).append(
                                        $("<div/>", { "class": "panel-body" }).append(bufferContent(data.Buffer))
                                    )
                                );
                                count++;
                            }

                            if (data.Html) {
                                div.append(
                                    $("<div/>", { "class": "panel panel-default" }).append(
                                        $("<div/>", { "class": "panel-heading" }).html("HTML")
                                    ).append(
                                        $("<div/>", { "class": "panel-body" }).html(data.Html)
                                    )
                                );
                                count++;
                            }

                            if (data.Data !== null) {
                                div.append(
                                    $("<div/>", { "class": "panel panel-default" }).append(
                                        $("<div/>", { "class": "panel-heading" }).html("Data")
                                    ).append(
                                        dataContent(data.Data)
                                    )
                                );
                                count++;
                            }

                            if (count === 0)
                                div.html($('<div class="nodata">No results to display.</div>'));

                            $('.run-output', $this).append(div);
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        setRunError(errorThrown);
                    },
                    complete: function (jqXHR, textStatus) {
                        $(".working", $this).html("");
                        $(".run-output", $this).show();
                        $(".run-feed", $this).prop("disabled", false);
                    }
                });
            };

            var editor;

            $('.feed-query', $this).each(function () {
                editor = CodeMirror.fromTextArea(this, {
                    'mode': getMode(),
                    'indentWithTabs': true,
                    'smartIndent': true,
                    'lineNumbers': true,
                    'matchBrackets': true,
                    'autofocus': false
                });
            });

            $this.on('change', '.feed-types input', function (event) {
                editor.setOption("mode", getMode());
            }).on('click', '.run-feed', function (event) {
                event.preventDefault();
                var query = editor.getValue();

                if (query === "")
                    return;

                var qs = $(".query-string", $this).val();

                $(".working", $this).html("working...");
                $(".run-feed", $this).prop("disabled", true);
                runFeed(feedType(), query, qs);
            }).on('keyup', '.launcher-querystring', function (event) {
                var qs = $(".launcher-querystring", $this).val();
                var href = $(".link-href", $this).val();
                var baseurl = $(".link-baseurl", $this).val();
                if (qs.indexOf("?") !== 0)
                    qs = '?' + qs;
                $(".launcher-run", $this).attr("href", href + qs);
                $(".launcher-run", $this).text(baseurl + href + qs);
            });
        });
    };
}(jQuery));