(function ($) {
    $.fn.analogclock = function (options) {
        return this.each(function () {
            var $this = $(this);

            var canvas = $("<canvas/>").appendTo($this)[0];
            var context = canvas.getContext("2d");
            console.log(context);
        });
    };
}(jQuery));