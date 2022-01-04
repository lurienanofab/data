(function ($) {
    $.fn.billing = function (options) {
        var $this = $(this);

        var opt = $.extend({}, {
            'command': null,
            'path': null,
            'startPeriod': null,
            'endPeriod': null,
            'isTemp': false,
            'clientId': 0,
            'resourceId': 0,
            'roomId': 0,
            'itemId': 0,
            'delete': true
        }, options);

        var getRecord = function () {
            switch (opt.command) {
                case "Tool":
                    return opt.resourceId;
                case "Room":
                    return opt.roomId;
                case "Store":
                    return opt.itemId;
                default:
                    return 0;
            }
        };

        var getModel = function () {
            var result = {
                ClientID: opt.clientId,
                BillingCategory: opt.command
            };

            switch (opt.path) {
                case "process/data/clean":
                    result.StartDate = opt.startPeriod;
                    result.EndDate = opt.endPeriod;
                    break;
                case "process/data":
                    result.Period = opt.startPeriod;
                    result.Record = getRecord();
                    break;
                case "process/step1":
                    result.Period = opt.startPeriod;
                    result.Record = getRecord();
                    result.Delete = opt.delete;
                    result.IsTemp = opt.isTemp;
                    break;
                case "process/step2":
                case "process/step3":
                case "process/step4":
                    result.Period = opt.startPeriod;
                    result.Command = opt.command;
                    break;
            }

            return result;
        };

        var def = $.Deferred();

        var model = getModel();

        $.ajax({
            'url': '/webapi/billing/' + opt.path,
            'type': 'POST',
            'data': model,
            'success': function (data, textStatus, jqXHR) {
                $this.html('<div class="api-message success">' + data.LogText + '</div>');
                def.resolve(data, textStatus, jqXHR);
            },
            'error': function (jqXHR, textStatus, errorThrown) {
                var errmsg = errorThrown;

                if (jqXHR.responseJSON && jqXHR.responseJSON.ExceptionMessage)
                    errmsg = jqXHR.responseJSON.ExceptionMessage;

                $this.html('<div class="api-message error">' + errmsg + '</div>');

                def.reject(jqXHR, textStatus, errorThrown);
            }
        });

        return def.promise();
    };
}(jQuery));