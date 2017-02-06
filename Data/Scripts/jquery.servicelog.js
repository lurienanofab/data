(function ($) {
    $.fn.servicelog = function () {
        return this.each(function () {
            var $this = $(this);

            $this.css({ 'width': ($this.outerWidth() - 50) + 'px' });

            var stats = $('<div/>')
				.addClass('stats')
				.css({ 'margin-bottom': '5px' })
				.prependTo($this);

            var options = {
                ajaxUrl: $('.ajax-url', $this).val(),
                serviceName: $('.service-name', $this).val()
            };

            var tbl = $('.service-log-table', $this).dataTable({
                'bProcessing': true,
                'bServerSide': true,
                'sAjaxDataProp': 'aaData',
                'sAjaxSource': options.ajaxUrl,
                'aaSorting': [[1, 'desc'], [0, 'asc']],
                'aoColumns': [
                    { 'sTitle': 'ID', 'bVisible': false },
                    { 'sTitle': 'Date/Time', 'sWidth': '130px' },
                    { 'sTitle': 'Subject', 'sWidth': '80px' },
                    { 'sTitle': 'Level', 'sWidth': '80px' },
                    { 'sTitle': 'Message' }
                ],
                'fnDrawCallback': function (oSettings) {
                    var data = this.fnGetData();
                    if (data.length > 0) {
                        var now = new Date();
                        var mostRecentEvent = new Date(data[0][1]);
                        var diff = Math.max((now.getTime() - mostRecentEvent.getTime()) / 1000, 0);
                        var diffText = '';
                        if (diff >= 60)
                            diffText = (diff / 60).toFixed(2) + ' minutes';
                        else
                            diffText = diff.toFixed(2) + ' seconds';
                        var diffCss = (diff >= 420) ? { 'color': '#ff0000', 'font-weight': 'bold' } : { 'color': '#008000', 'font-weight': 'normal' };
                        stats.html('').append(
							$('<div/>').css(diffCss).html('Last event occurred ' + diffText + ' ago.')
						);
                    }
                }
            });

            var refreshInterval = 60000; //every minute

            var refreshData = function () {
                //console.log('auto-refresh');
                tbl.fnReloadAjax();
                setTimeout(refreshData, refreshInterval);
            }

            setTimeout(refreshData, refreshInterval);
        });
    }
}(jQuery));