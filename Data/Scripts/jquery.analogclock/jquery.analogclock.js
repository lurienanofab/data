(function ($) {
    $.fn.analogclock = function (options) {
        return this.each(function () {
            var $this = $(this);

            if (typeof moment != "function") {
                $this.html($("<div/>", { "class": "alert alert-danger", "role": "alert" }).css({ "font-size": "10pt" }).html("moment.js is required to run this plugin."));
                return;
            }

            var opt = $.extend({}, {
                "seed": null, //the initial clock time
                "start": false, //starts the clock when true, just sets the time when false
                "ontick": null, //function to call on each tick (once per second)
                "onsync": null, //function to call for synchronizing
                "onerror": null, //function to call when an error occurs
                "syncInterval": 3600000 //one hour, if this is zero the sync will happen once when onsync is a function
            }, options);

            var timer = null;
            var nextSync = null; //the local time when the next sync should occur
            var current = opt.seed; //the server time retuned by the sync function
            var offset = 0; //the difference in ms between server and local time

            if (!$this.hasClass("analog-clock"))
                $this.addClass("analog-clock");

            var handleError = function (message) {
                clearTimeout(timer);
                if (typeof opt.onerror == "function")
                    opt.onerror(message);
            }

            var localTime = function () {
                return moment().milliseconds();
            }

            var setNextSync = function (d) {
                if (opt.syncInterval > 0) {
                    if (nextSync == null) nextSync = moment().add('ms', -1)
                    nextSync.add('ms', opt.syncInterval);
                    console.log("current time: " + current.format("M/D/YYYY h:mm:ss A") + " [offset: " + offset + " ms], next sync: " + nextSync.format("M/D/YYYY h:mm:ss A"));
                }
            }

            var sync = function (callback) {
                if (typeof opt.onsync == "function") {
                    opt.onsync(function (data) {
                        //We are using moment (http://momentjs.com) here because it gracefully handles browser
                        //compatibility issues (IE9 does not recognize the value from the server as a valid date).

                        if (data == null) {
                            handleError("data is null");
                            return;
                        }

                        if (data == "") {
                            handleError("data is an empty string");
                            return;
                        }

                        var m = moment(data);

                        if (m.isValid()) {
                            current = m;

                            var tsp = moment().toDate();
                            var performance = window.performance || window.mozPerformance || window.msPerformance || window.webkitPerformance || {};
                            tsp.setTime(tsp.getTime() + performance.timing.loadEventStart - performance.timing.navigationStart)
                            console.log(tsp);



                            offset = moment().diff(current, 'ms');
                            setNextSync(current);
                            if (typeof callback == "function")
                                callback();
                        }
                        else {
                            handleError("Cannot convert data into a moment object [" + data.toString() + "]");
                        }
                    })
                }
                else {
                    setNextSync(current);
                    if (typeof callback == "function")
                        callback();
                }
            }

            var tick = function (callback) {

                if (current == null) {
                    handleError("current is null");
                    return;
                }

                var seconds = current.seconds();
                var sdegree = seconds * 6;
                var srotate = "rotate(" + sdegree + "deg)";

                var hours = current.hours();
                var mins = current.minutes();
                var hdegree = hours * 30 + (mins / 2);
                var hrotate = "rotate(" + hdegree + "deg)";

                var mdegree = mins * 6;
                var mrotate = "rotate(" + mdegree + "deg)";

                $(".sec", $this).css({ "-moz-transform": srotate, "-webkit-transform": srotate });
                $(".min", $this).css({ "-moz-transform": mrotate, "-webkit-transform": mrotate });
                $(".hour", $this).css({ "-moz-transform": hrotate, "-webkit-transform": hrotate });

                if (typeof opt.ontick == "function") {
                    if (current.isValid())
                        opt.ontick(current);
                    else
                        handleError("A problem occurred: current server time is not valid");
                }

                if (opt.syncInterval > 0 && moment().diff(nextSync, 'ms') >= 0)
                    sync(callback);
                else{
                    if (typeof callback == "function")
                        callback();
                }
            }

            sync(function () {
                tick(function () {
                    if (opt.start) {
                        if (current == null || !current.isValid()) {
                            handleError("A problem occurred: current server time is not valid");
                            return;
                        }

                        //timer = setInterval(function () {
                        //    current = moment().add('ms', offset);
                        //    tick()
                        //}, 1000);

                        timer = setInterval(function () {
                            current = current.add('ms', 1000);
                            tick()
                        }, 1000);
                    }
                })
            });
            
        });
    }
}(jQuery));