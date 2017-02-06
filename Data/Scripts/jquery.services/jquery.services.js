(function ($) {
    $.fn.services = function () {
        return this.each(function () {
            var $this = $(this);

            $(".log-table", $this).dataTable({
                "order": [[0, "desc"]],
                "columnDefs": [
                    { "targets": [0], "width": "120px" },
                    { "targets": [1], "width": "80px" },
                    { "targets": [2], "width": "200px" }
                ],
                "initComplete": function (settings, json) {
                    $(".loading", $this).hide();
                    $(".table-container", $this).show();
                }
            });

            $(".date-picker", $this).datepicker();

        });
    }
}(jQuery));