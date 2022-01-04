(function ($) {
    $.fn.utility = function (options) {
        return this.each(function () {
            var $this = $(this);

            var working = false;

            var opts = $.extend({}, { "apiUrl": "" }, options, $this.data());

            var getProgressImage = function () {
                return '<table style="margin-bottom: 5px;"><tr><td><img src="//ssel-apps.eecs.umich.edu/static/images/ajax-loader-2.gif" /></td><td style="font-style: italic; color: #808080;">Working, please wait...</td></tr></table>';
            };

            var getStartPeriodYear = function () {
                return $('.start-period-year', $this).val();
            };

            var getStartPeriodMonth = function () {
                return $('.start-period-month', $this).val();
            };

            var getEndPeriodYear = function () {
                return $('.end-period-year', $this).val();
            };

            var getEndPeriodMonth = function () {
                return $('.end-period-month', $this).val();
            };

            var getStartPeriod = function () {
                var y = getStartPeriodYear();
                var m = getStartPeriodMonth();
                var result = m + "/1/" + y;
                return result;
            };

            var getEndPeriod = function () {
                var y = getEndPeriodYear();
                var m = getEndPeriodMonth();
                var temp = m + "/1/" + y;
                var d = new Date(temp);
                d.setMonth(d.getMonth() + 1);
                var result = d.getMonth() + 1 + "/1/" + d.getFullYear();
                return result;
            };

            var getIsTemp = function () {
                return $('.billing-step1-istemp', $this).is(':checked');
            };

            var getClientID = function () {
                return $('.client-id', $this).val();
            };

            var getResourceID = function () {
                return $('.resource-id', $this).val();
            };

            var getRoomID = function () {
                return $('.room-id', $this).val();
            };

            var getItemID = function () {
                return $('.item-id', $this).val();
            };

            var getDelete = function () {
                return $('.delete', $this).prop("checked");
            };

            var executeCommand = function (command, path, target) {
                target.billing({
                    'command': command,
                    'path': path,
                    'startPeriod': getStartPeriod(),
                    'endPeriod': getEndPeriod(),
                    'isTemp': getIsTemp(),
                    'clientId': getClientID(),
                    'resourceId': getResourceID(),
                    'roomId': getRoomID(),
                    'itemId': getItemID(),
                    'delete': getDelete()
                }).always(function () {
                    working = false;
                });
            };

            var getSendEmailModel = function (emailType) {
                switch (emailType) {
                    case "financial-manager":
                        return { "Period": getStartPeriod(), "Message": "", "IncludeManager": true };
                    case "user-apportionment":
                        return { "Period": getStartPeriod(), "Message": "", "NoEmail": false };
                    default:
                        return null;
                }
            };

            var executeSendEmail = function (emailType, target) {
                var model = getSendEmailModel(emailType);

                $.ajax({
                    "url": opts.apiUrl + "email/" + emailType,
                    "method": "POST",
                    "data": model
                }).done(function (data) {
                    target.html($("<div/>", { "class": "api-message success" }).html("Emails sent: " + data));
                }).fail(function (jqXHR, textStatus, errorThrown) {
                    var errmsg = errorThrown;

                    if (jqXHR.responseJSON && jqXHR.responseJSON.ExceptionMessage)
                        errmsg = jqXHR.responseJSON.ExceptionMessage;

                    target.html($("<div/>", { "class": "api-message error" }).html(errmsg));
                }).always(function () {
                    working = false;
                });
            };

            $this.on('click', '.command-link', function (event) {
                event.preventDefault();

                if (!working) {
                    working = true;
                    var link = $(this);
                    var li = link.closest('li');
                    var info = li.find('.info');
                    var command = $(this).data('command');
                    var path = $(this).data('path');
                    if (command !== '') {
                        info.html(getProgressImage()).css({ 'display': 'block' });
                        executeCommand(command, path, info);
                    }
                }
                else
                    console.log('ignored');
            }).on("click", ".email-link", function (e) {
                event.preventDefault();

                if (!working) {
                    working = true;
                    var link = $(this);
                    var li = link.closest('li');
                    var info = li.find('.info');
                    var emailType = $(this).data('email-type');
                    if (emailType !== '') {
                        info.html(getProgressImage()).css({ 'display': 'block' });
                        executeSendEmail(emailType, info);
                    }
                }
            });
        });
    };
}(jQuery));