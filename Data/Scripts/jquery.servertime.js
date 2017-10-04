(function ($) {
    $.fn.servertime = function (options) {
        return this.each(function () {
            var $this = $(this);

            var opt = $.extend({}, { "url": null, "interval": 1000 * 60 * 10, "format": "h:mm:ss A", "ontick": null }, $this.data(), options);
            
            var getServerTime = function () {
                /// <summary>Gets the current server time.</summary>
                /// <returns value="jQuery.Deferred().promise()"></returns>

                return $.ajax({
                    "url": opt.url,
                    "dataType": "jsonp"
                });
            };
            
            var diff = null;
            var clockInterval = 250;
            var clockTimer = null;
            var refreshTimer = null;

            var getAdjustedTime = function () {
                /// <summary>Adjusts local time</summary>
                /// <returns value="moment()"></returns>

                return moment().add(diff, 'ms');
            };
            
            var displayTime = function () {
                /// <summary>Displays the server time in this html element.</summary>
                /// <returns value="jQuery.Deferred().promise()"></returns>

                var def = $.Deferred();
                var adjusted = getAdjustedTime();
                $this.html(adjusted.format(opt.format));
                def.resolve(adjusted);
                return def.promise();
            };

            var stop = function () {
                //clear both timers
                if (clockTimer !== null)
                    clearInterval(clockTimer);
                if (refreshTimer !== null)
                    clearInterval(refreshTimer);
            };

            var refresh = function () {
                //clear the clockTimer only
                if (clockTimer !== null)
                    clearInterval(clockTimer);

                getServerTime().done(function (data) {
                    //use the time from the server
                    diff = moment(data.ServerTime).diff(moment(), 'ms');
                }).fail(function () {
                    //when there is an error fall back to local system time
                    diff = 0;
                    console.log("using local system time");
                }).always(function () {
                    //show the time
                    displayTime().done(opt.ontick);

                    //start incrementing by one second
                    clockTimer = setInterval(function () {
                        displayTime().done(opt.ontick);
                    }, clockInterval);
                });
            };

            var updateLocalTime = function () {
                var def = $.Deferred();
                var m = moment();
                $this.html(m.format(opt.format));
                def.resolve();
                return def.promise();
            };

            var start = function () {
                if (opt.url === null) {
                    console.log("local time mode");
                    updateLocalTime();
                    clockTimer = setInterval(function () {
                        updateLocalTime().done(opt.ontick);
                    }, clockInterval);
                } else {
                    //clear the refreshTimer only
                    if (refreshTimer !== null)
                        clearInterval(refreshTimer);

                    //get the server time and display
                    refresh();

                    //refresh the server time every 10 minutes
                    refreshTimer = setInterval(refresh, opt.interval);
                }
            };

            start();

            $this.on("stop", function () {
                stop();
            }).on("start", function () {
                start();
            });
        });
    };
}(jQuery));