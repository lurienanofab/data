(function ($) {
    $.fn.commandLine = function () {
        return this.each(function () {
            var $this = $(this);

            var prevCmd = null;

            var currentLine = $(".current-line", $this);

            var scrollToBottom = function () {
                $("html, body").scrollTop($(document).height());
            }

            var initCurrentLine = function () {
                currentLine.val("");
                currentLine.focus();
            }

            var clearProcessed = function () {
                $(".processed-commands", $this).html("");
            }

            var doCommand = function () {
                var cmd = $("<div>").text(currentLine.val()).html(); //converts html entities

                initCurrentLine();

                if (cmd == "clear") {
                    clearProcessed();
                    return;
                } else if (cmd == "redo") {
                    cmd = prevCmd;
                }
                else
                    prevCmd = cmd;

                if (!cmd)
                    return;

                $(".processed-commands", $this).append($("<div/>").addClass("command-text").html(cmd));

                $(".processed-commands", $this).append($("<div/>").addClass("command-loading").html("&nbsp;"));

                var result = { "Success": false, "Message": null };

                currentLine.prop("disabled", true);

                setTimeout(function () {
                    $.ajax({
                        "url": "/data/command/process",
                        "type": "POST",
                        "data": { "cmd": cmd },
                        "success": function (data, textStatus, jqXHR) {
                            result = $.extend({}, result, data);
                        },
                        "error": function (jqXHR, textStatus, errorThrown) {
                            result.Success = false;
                            result.Message = errorThrown;
                        },
                        "complete": function (jqXHR, textStatus) {

                            $(".command-loading", $this).remove();

                            if (result.Success)
                                $(".processed-commands", $this).append($("<div/>").addClass("command-result").html(result.Message));
                            else
                                $(".processed-commands", $this).append($("<div/>").addClass("command-error").html(result.Message));

                            if (result.Data) {
                                var table = $("<table/>").addClass("command-line-table");
                                var thead = $("<thead/>").appendTo(table);

                                var first = result.Data[0];
                                var r = $("<tr/>");
                                for (k in first) {
                                    r.append($("<th/>").html(k));
                                }
                                thead.append(r);

                                var tbody = $("<tbody/>").appendTo(table);
                                $.each(result.Data, function (index, item) {
                                    var r = $("<tr/>");
                                    for (k in first) {
                                        r.append($("<td/>").html(item[k]));
                                    }
                                    tbody.append(r);
                                });

                                $(".processed-commands", $this).append(table);
                            }

                            currentLine.prop("disabled", false);
                            scrollToBottom();
                        }
                    });
                }, 100);
            }

            initCurrentLine();

            $("body").on("click", function (e) {
                currentLine.focus();
            });

            currentLine.on("keydown", function (e) {
                if (e.keyCode == 13) {
                    doCommand();
                }
                else if (e.keyCode == 38) {
                    if (prevCmd) {
                        currentLine.val(prevCmd);
                        scrollToBottom();
                    }
                }
            }).on("click", function (e) {
                e.stopPropagation();
            });

            $this.on("click", ".processed-commands", function (e) {
                e.stopPropagation();
            });
        })
    }
}(jQuery));