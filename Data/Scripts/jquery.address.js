(function ($) {
    $.fn.address = function () {
        return this.each(function () {
            var $this = $(this);

            var opt = $.extend({}, { "apiurl": null }, $this.data());

            var saveAddress = function (item) {
                return $.ajax({
                    "url": opt.apiurl,
                    "type": "POST",
                    "data": $.extend({}, { "AddressID": item.data("id"), "AddressType": item.data("type") }, getEditObject(item))
                });
            }

            var getEditProperty = function (item, name) {
                return $(".edit-addr-form [data-property='" + name + "']", item).val();
            }

            var getEditObject = function (item) {
                var result = {};
                $(".edit-addr-form input", item).each(function () {
                    result[$(this).data("property")] = $(this).val();
                });
                return result;
            }

            var setDisplayProperty = function (item, name, value) {
                var obj = $(".display-addr [data-property='" + name + "']", item);
                obj.html(value);
            }

            var setEditProperty = function (item, name, value) {
                var obj = $(".edit-addr-form [data-property='" + name + "']", item);
                obj.val(value);
            }

            var copyAddress = function (item) {
                var result = null;

                var edit = $(".edit-addr-form", item);

                if (edit.length > 0) {
                    result = {};
                    $("input", edit).each(function () {
                        result[$(this).data("property")] = $(this).data("origvalue");
                    });
                }

                return result;
            }

            var appendToUrl = function (url, data) {
                var result = url;
                var next = result.indexOf("?") === -1 ? "?" : "&";
                $.each(data, function (key, value) {
                    result += next + key + "=" + value;
                    next = "&";
                });
                return result;
            }

            var deleteAddress = function (item) {
                return $.ajax({
                    "url": appendToUrl(opt.apiurl, { "AddressID": item.data("id"), "AddressType": item.data("type") }),
                    "type": "DELETE"
                });
            }

            $(".address-item", $this).each(function () {
                //start of address-item loop

                var item = $(this);

                if (item.data("id") == 0) {
                    $(".new-addr", item).show();
                    $(".display-addr", item).hide();
                } else {
                    $(".new-addr", item).hide();
                    $(".display-addr", item).show();
                }

                item.on("click", ".edit-addr", function (e) {
                    $(".display-addr", item).hide();
                    $(".edit-addr-form", item).show();
                }).on("click", ".delete-addr", function (e) {
                    deleteAddress(item).done(function (data) {
                        if (data) {
                            item.data("id", 0);
                            $(".new-addr", item).show();
                            $(".display-addr", item).hide();
                            $(".edit-addr-form", item).hide();
                            $(".edit-addr-form input", item).each(function () {
                                var input = $(this);
                                input.val("");
                                input.data("origval", "");
                            });
                        }
                    });
                }).on("click", ".save-addr", function (e) {
                    saveAddress(item).done(function (data) {
                        item.data("id", data.AddressID);
                        setDisplayProperty(item, "Attention", data.Attention);
                        setDisplayProperty(item, "StreetAddress1", data.StreetAddress1);
                        setDisplayProperty(item, "StreetAddress2", data.StreetAddress2);
                        setDisplayProperty(item, "City", data.City);
                        setDisplayProperty(item, "State", data.State);
                        setDisplayProperty(item, "Zip", data.Zip);
                        setDisplayProperty(item, "Country", data.Country);

                        $(".edit-addr-form input", item).each(function () {
                            var input = $(this);
                            input.data("origvalue", input.val());
                        });

                        $(".display-addr", item).show();
                        $(".edit-addr-form", item).hide();
                    });
                }).on("click", ".cancel-addr", function (e) {

                    if (item.data("id") == 0) {
                        $(".new-addr", item).show();
                    } else {
                        $(".edit-addr-form input", item).each(function () {
                            var input = $(this);
                            input.val(input.data("origvalue"));
                        });

                        $(".display-addr", item).show();
                    }
                    $(".edit-addr-form", item).hide();
                }).on("click", ".add-addr", function (e) {
                    $(".new-addr", item).hide();
                    $(".edit-addr-form", item).show();
                }).on("click", ".copy-addr", function (e) {
                    e.preventDefault();
                    var type = $(this).data("type");
                    var addr = copyAddress($(".address-item[data-type='" + type + "']", $this));

                    if (addr != null) {
                        setEditProperty(item, "Attention", addr.Attention);
                        setEditProperty(item, "StreetAddress1", addr.StreetAddress1);
                        setEditProperty(item, "StreetAddress2", addr.StreetAddress2);
                        setEditProperty(item, "City", addr.City);
                        setEditProperty(item, "State", addr.State);
                        setEditProperty(item, "Zip", addr.Zip);
                        setEditProperty(item, "Country", addr.Country);
                    }

                    $(".new-addr", item).hide();
                    $(".edit-addr-form", item).show();
                });
                //end of address-item loop
            });



        })
    }
}(jQuery));