(function ($) {
    $.fn.access = function () {
        return this.each(function () {
            var $this = $(this);

            var badgeTable = $(".badge-table", $this).dataTable({
                bStateSave: true,
                sPaginationType: "full_numbers",
                aoColumnDefs: [
                    { sWidth: "40px", aTargets: [0] },
                    { sWidth: "100px", aTargets: [1] },
                    { bVisible: false, aTargets: [8] }
                ]
            });

            var cardTable = $(".card-table", $this).dataTable({
                bStateSave: true,
                sPaginationType: "full_numbers",
                aoColumnDefs: [
                    { sWidth: "40px", aTargets: [0] }
                ]
            });

            var eventTable = $(".event-table", $this).dataTable({
                bStateSave: true,
                sPaginationType: "full_numbers",
                aoColumnDefs: [
                    { sWidth: "40px", aTargets: [0] },
                    { iDataSort: 6, aTargets: [5] },
                    { bVisible: false, aTargets: [6] },
                    { sWidth: "100px", aTargets: [7] }
                ]
            });

            $(".paging_full_numbers", $this).css({
                "-webkit-touch-callout": "none",
                "-webkit-user-select": "none",
                "-khtml-user-select": "none",
                "-moz-user-select": "none",
                "-ms-user-select": "none",
                "user-select": "none"
            });

            var activeTab = function () {
                var result = 0;
                var val = $(".active-tab", $this).val();
                if (val != "") {
                    switch (val) {
                        case "cards":
                            result = 1;
                            break;
                        case "events":
                            result = 2;
                            break;
                        default: //badges
                            result = 0;
                            break;
                    }
                }
                return result;
            }

            $(".tabs", $this).tabs({
                active: activeTab(),
                activate: function (e, ui) {
                    var url = ui.newPanel.find(".redirect-url").val();
                    setTimeout(function () {
                        window.location = url;
                    }, 1000);
                    //$(".active-tab", $this).val(ui.newTab.index());
                }
            });

            $(".datepicker", $this).datepicker();

            $(".in-area", $this).on("change", function (e) {
                var prevSearch = badgeTable.fnSettings().oPreviousSearch.sSearch;
                var newSearch = "";
                if ($(this).prop("checked"))
                    newSearch = "inlab " + prevSearch;
                else
                    newSearch = prevSearch.replace("inlab", "").trim();
                badgeTable.fnFilter(newSearch);
            });
        });
    }
}(jQuery));