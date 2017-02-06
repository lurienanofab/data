(function ($) {
	$.fn.inlab = function (options) {
		return this.each(function () {
			var $this = $(this);

			var opt = $.extend({}, {
				"mode": "ajax",
				"interval": 10000,
				"url": "//ssel-apps.eecs.umich.edu/data/feed/currently-in-lab/jsonp",
				"room": "all"
			}, $this.data(), options);

			$this.data("options", opt);

			var timer;
			var current;

			var getAreaName = function (room) {
				switch (room) {
					case "wetchem":
						return "Wet Chemistry";
					case "cleanroom":
						return "Clean Room";
					default:
						return "All";
				}
			}

			var display = function () {
				if (current) {
					var div = $("<div/>", { "class": "row inlab-list" });
					var room = [];

					$.each(current, function (index, item) {
						var include = true;

						if (getAreaName(opt.room) != "All")
							include = getAreaName(opt.room) == item.AreaName;

						if (include)
							room.push(item);
					});

					if (room.length == 0)
						div.append($("<div/>", { "class": "col-xs-12 inlab-nodata" }).html("There are currently no users in room: " + getAreaName(opt.room)));
					else {
						$.each(room, function (index, item) {
							var time = moment(item.EventTime, "M/D/YYYY h:mm:ss A");
							var duration = moment().diff(time, 'seconds') / 60 / 60;
							div.append($("<div/>", { "class": "col-lg-2 col-md-3 col-sm-3 col-xs-6" }).html($("<div/>", { "class": "inlab-item" }).html(item.FullName).append($("<div/>", { "class": "inlab-time" }).html(duration.toFixed(2) + ' hours'))));
						})
					}

					$this.html($("<h4/>").html(getAreaName(opt.room) + " Users")).append(div);

				}
			}

			var sync = function () {
				$.getJSON(opt.url, function (data) {
					if (data) {
						current = data.Data.default;
						//console.log("[" + moment().format("YYYY-MM-DD HH:mm:ss") + "] inlab: found " + current.length + " records");
						display();
					}
				});
			}

			var start = function () {
				if (opt.interval > 0)
					window.setInterval(sync, opt.interval);
			}

			sync();
			start();
		});
	}
}(jQuery));