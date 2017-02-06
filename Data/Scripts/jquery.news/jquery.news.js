(function ($) {
    $.fn.news = function () {
        return this.each(function () {
            var $this = $(this);

            var newsSlideShowTimer = null;
            var newsSlideShowIndex = 0;

            var startClock = function () {
                setInterval(function () {
                    $(".datetime", $this).html(moment().format("dddd[, ]MMMM[ ]D[, ]YYYY[ ]h[:]mm[:]ss[ ]A"));
                }, 500);
            };

            var getNews = function () {
                $.ajax({
                    "url": "/data/api/news",
                    "dataType": "json"
                }).done(function (data) {

                    data.push({ "Description": "this is a test" });

                    if (newsSlideShowTimer)
                        clearInterval(newsSlideShowTimer);

                    newsSlideShowIndex = 0;

                    showSlide(data);

                    newsSlideShowTimer = setInterval(function () {
                        showSlide(data);
                    }, 1000 * 10);
                });
            };

            var getFileNameWithoutExtension = function (fileName) {
                var matches = fileName.match(/(.\w+)\./);

                if (!matches)
                    return fileName;

                if (matches.length === 2)
                    return matches[1];
                else
                    return fileName;
            };

            var showSlide = function (data) {
                var item = data[newsSlideShowIndex];

                var slide = $("td.slide", $this);
                var slideWidth = slide.width() - 40; //-40 for padding

                var existing = $(".slide-content", slide);

                existing.fadeOut(2000, function () {
                    var slideContent = "";
                    if (item.Description)
                        slideContent = item.Description;
                    else {
                        console.log(item);
                        var fileNameWithoutExtension = getFileNameWithoutExtension(item.ImageFileName);

                        var imageUrl = "//ssel-sched.eecs.umich.edu/news/item/image/" + item.NewsID + "/" + fileNameWithoutExtension;
                        slideContent = $("<img/>", { "src": imageUrl });
                    }

                    var div = $("<div/>", { "class": "slide-content" }).css({ "left": "-" + slideWidth + "px", "width": slideWidth + "px" }).html(slideContent);

                    $(".slide", $this).append(div);

                    div.animate({ left: '0px' }, 2000, 'easeOutQuint', function () {
                        existing.remove();
                    });
                });

                newsSlideShowIndex++;
                if (newsSlideShowIndex >= data.length)
                    newsSlideShowIndex = 0;
            };

            var startNews = function () {
                setInterval(getNews, 1000 * 60 * 5);
            };

            startClock();
            getNews();
            startNews();

            $(window).on("resize", function (e) {
                var titleHeight = 0;
                var windowHeight = $(window).outerHeight();

                $(".title", $this).each(function () {
                    titleHeight += $(this).outerHeight();
                });

                $(".body", $this).outerHeight(windowHeight - titleHeight);
            }).trigger("resize");

        });
    };
}(jQuery));