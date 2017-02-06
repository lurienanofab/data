(function ($) {
    $.fn.databaseReport = function (options) {
        return this.each(function () {
            var $this = $(this);

            var opt = $.extend({}, {"type": "client", "showedit": false}, $this.data(), options);

            var doCommand = function (command, type) {
                var base = '?';
                var url = base + 'type=' + type + '&search=';
                switch (command) {
                    case 'search':
                        var text = $("." + type + "-search-text", $this).val();
                        url += text;
                        break;
                }
                window.location = url;
            }

            $.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {

                //this function is called every time the table is drawn

                var result = true;
                var operation = "and";

                $(".filter-buttons", $this).each(function () {
                    var container = $(this);
                    var selected = null;
                    var column = container.data("column")

                    //find the selected filter-link
                    $(".filter-link", container).each(function () {
                        if ($(this).data("selected")) {
                            selected = $(this);
                            return false;
                        }
                    });

                    if (selected) {
                        var value = selected.data("value");

                        if (operation == "or")
                            result = result || data[column] == value;
                        else
                            result = result && data[column] == value;
                    }
                });

                return result;
            });

            var idColumn = { "width": "60px" };
            var usernameColumn = { "width": "60px" };
            var nameColumn = null;
            var historyColumn = { "width": "60px", "orderable": false, "searchable": false };
            var activeColumn = { "width": "60px", "orderable": true, "searchable": false };
            var searchColumn = { "visible": false, "orderable": false, "searchable": true };

            var tableConfigs = {
                "client": {
                    "pagingType": "full_numbers",
                    "columns": [
                        idColumn,
                        usernameColumn,
                        nameColumn,
                        activeColumn,
                        searchColumn,
                        historyColumn
                    ]
                },
                "client-org": {
                    "pagingType": "full_numbers",
                    "columns": [
                        idColumn,
                        null, //client name
                        { "width": "150px" }, //email
                        { "width": "150px" }, //org name
                        { "width": "80px", "orderable": true, "searchable": false }, //manager checkbox
                        searchColumn,
                        { "width": "100px", "orderable": true, "searchable": false }, //fin manager checkbox
                        searchColumn,
                        activeColumn,
                        searchColumn,
                        { "width": "80px", "orderable": false, "searchable": false }, //links
                        historyColumn
                    ]
                },
                "client-account": {
                    "pagingType": "full_numbers",
                    "columns": [
                        idColumn,
                        null, //client name
                        { "width": "300px" }, //account name
                        { "width": "100px" }, //shortcode
                        { "width": "180px" }, //org name
                        { "width": "80px", "orderable": true, "searchable": false }, //manager checkbox
                        searchColumn,
                        activeColumn,
                        searchColumn,
                        historyColumn
                    ]
                },
                "client-manager": {
                    "pagingType": "full_numbers",
                    "columns": [
                        idColumn,
                        { "width": "300px" }, //client name
                        { "width": "300px" }, //manager name
                        activeColumn,
                        searchColumn,
                        historyColumn
                    ]
                },
                "org": {
                    "pagingType": "full_numbers",
                    "columns": [
                        idColumn,
                        null, //org name
                        { "width": "200px" }, //org type
                        activeColumn,
                        searchColumn,
                        { "width": "70px", "orderable": false, "searchable": false }, //links
                        historyColumn
                    ]
                },
                "account": {
                    "pagingType": "full_numbers",
                    "columns": [
                        idColumn,
                        null,
                        { "width": "260px" }, //number
                        { "width": "100px" }, //shortcode
                        { "width": "100px" }, //account type
                        { "width": "250px" }, //org name
                        activeColumn,
                        searchColumn,
                        historyColumn
                    ]
                }
            };

            if (opt.showedit) {
                tableConfigs["org"].columns.push({ "width": "40px", "orderable": false, "searchable": false });
            }

            var table = $("." + opt.type + "-search-table", $this).dataTable(tableConfigs[opt.type]);

            $this.on("click", ".command", function (e) {
                e.preventDefault();
                var type = $(this).closest(".input-group").data("type");
                var command = $(this).data("command");
                $(".message", $this).html("");
                doCommand(command, type);
            }).on("keydown", ".search", function (e) {
                if ((e.keyCode || e.which) == 13) {
                    e.preventDefault();
                    var type = $(this).closest(".input-group").data("type");
                    $(".message", $this).html("");
                    doCommand("search", type);
                }
            }).on("focus", ".search", function (e) {
                $(this).select();
            }).on("click", ".filter-link", function (e) {
                e.preventDefault();

                var link = $(this);
                var container = link.closest(".filter-buttons");

                if (link.data("selected")) {
                    //this one is already selected so unselect both
                    container.find(".filter-link").removeClass("selected").data("selected", false);
                } else {
                    //unselect both and then select this one
                    container.find(".filter-link").removeClass("selected").data("selected", false);
                    link.addClass("selected").data("selected", true);
                }

                //redraw to apply filter
                table.api().draw();
            });

        });
    }
}(jQuery));