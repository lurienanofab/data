(function ($) {
    $.fn.account = function (options) {
        return this.each(function () {

            var $this = $(this);

            var opt = $.extend({}, {
                "baseurl": "/",
                "addressDefaults": {
                    "attention": "",
                    "address-line1": "",
                    "address-line2": "",
                    "city": "",
                    "state": "",
                    "zip": "",
                    "country": "US"
                },
                "templates": {
                    "manager-list": null
                }
            }, options, $this.data());

            var internalOrg = $(".internal-org", $this);
            var externalOrg = $(".external-org", $this);
            var accountInfo = $(".account-info", $this);
            var addressList = $(".address-list", $this);
            var managerList = $(".manager-list", $this);

            function failHandler(jqXHR, textStatus, errorThrown) {
                var errmsg = "An error occured.";

                if (jqXHR) {
                    if (jqXHR.responseJSON) {
                        errmsg = jqXHR.responseJSON.ExceptionMessage || jqXHR.responseJSON.error;
                    } else {
                        console.log(jqXHR);
                        console.log(textStatus);
                        console.log(errorThrown);
                        errmsg = jqXHR.statusText;
                    }
                }

                alert(errmsg);
            }

            var beginEdit = function (addr) {
                if (addr.length > 0) {

                    var addressId = addr.data("address-id");

                    var editField = function (key) {
                        var field = $("." + key, addr);
                        var value = field.html();
                        field.data("original-value", value);

                        if (addressId === -1)
                            v = opt.addressDefaults[key];
                        else
                            v = value;

                        field.html($("<input/>", { "type": "text", "class": "form-control input-sm " + key + "-input", "value": v }));
                    };

                    $(".controls .add-addr", addr).hide();
                    $(".controls .edit-delete", addr).hide();
                    $(".controls .update-cancel", addr).show();

                    editField("attention");
                    editField("address-line1");
                    editField("address-line2");
                    editField("city");
                    editField("state");
                    editField("zip");
                    editField("country");
                }
            };

            var cancelEdit = function (addr) {
                if (addr.length > 0) {
                    var cancelField = function (key) {
                        var field = $("." + key, addr);
                        var value = field.data("original-value");
                        field.html(value);
                    };

                    $(".controls .update-cancel", addr).hide();

                    var addressId = addr.data("address-id");
                    if (addressId === -1) {
                        $(".controls .edit-delete", addr).hide();
                        $(".controls .add-addr", addr).show();
                    } else {
                        $(".controls .add-addr", addr).hide();
                        $(".controls .edit-delete", addr).show();
                    }

                    cancelField("attention");
                    cancelField("address-line1");
                    cancelField("address-line2");
                    cancelField("city");
                    cancelField("state");
                    cancelField("zip");
                    cancelField("country");
                }
            };

            var populateAvailableManagers = function (managers) {
                $(".available-managers", managerList).html($.map(managers, function (value, index) {
                    return $("<option/>", { "value": value.ClientOrgID }).html(value.LName + ", " + value.FName);
                }));
            };

            $this.on("hidden.bs.modal", ".chartfield-modal", function (e) {
                // Runs when the Chart Field Lookup dialog is closed for any reason

                var modal = $(this);

                // clear all fields so if another shortcode is queried there won't be any left over data
                $(".fund", modal).val("");
                $(".department", modal).val("");
                $(".program", modal).val("");
                $(".class", modal).val("");
                $(".project", modal).val("");
                $(".shortcode", modal).val("");
                $(".description", modal).html("");
            }).on("click", ".save-chartfields", function (e) {
                // Runs when the user clicks the "Use these values" button on the Chart Field Lookup dialog

                var modal = $(".chartfield-modal", $this);

                // set the internal org chart field textbox values to whatever is in the modal textboxes
                $(".fund", internalOrg).val($(".fund", modal).val());
                $(".department", internalOrg).val($(".department", modal).val());
                $(".program", internalOrg).val($(".program", modal).val());
                $(".class", internalOrg).val($(".class", modal).val());
                $(".project", internalOrg).val($(".project", modal).val());
                $(".shortcode", internalOrg).val($(".shortcode", modal).val());

                // hide the modal
                modal.modal('hide');

                // get the new values to update the session object
                if (internalOrg.length > 0) {
                    var update = [];
                    update.push({ "field": "ChartFields.Fund", "value": $(".fund", internalOrg).val() });
                    update.push({ "field": "ChartFields.Department", "value": $(".department", internalOrg).val() });
                    update.push({ "field": "ChartFields.Program", "value": $(".program", internalOrg).val() });
                    update.push({ "field": "ChartFields.Class", "value": $(".class", internalOrg).val() });
                    update.push({ "field": "ChartFields.Project", "value": $(".project", internalOrg).val() });
                    update.push({ "field": "ChartFields.ShortCode", "value": $(".shortcode", internalOrg).val() });

                    $.ajax({
                        "url": opt.baseurl + "ajax/account/update",
                        "method": "POST",
                        "data": JSON.stringify(update),
                        "contentType": "application/json; charset=UTF-8"
                    }).done(function (data) {
                        console.log(data);
                    }).fail(failHandler);
                }

            }).on("change", "[data-update='true']", function (e) {
                // Runs when the user changes any input with the data-update attribute set to true

                var input = $(this);
                var field = input.data("field");
                var value = input.val();
                var hasError = false;
                var errmsg = "";

                input.closest(".form-group").removeClass("has-error");
                input.closest(".form-group").find(".help-block").html("").hide();

                if (input.prop("type") === "text") {
                    if (input.data("required") && value.length === 0) {
                        hasError = true;
                        errmsg = "Required";
                    } else {
                        var maxlen = parseInt(input.prop("maxlength")) || 0;

                        if (input.data("require-maxlen") && maxlen > 0) {
                            if (value.length !== maxlen) {
                                hasError = true;
                                errmsg = "Length required: " + maxlen;
                            }
                        }
                    }
                }

                if (hasError) {
                    input.closest(".form-group").find(".help-block").html(errmsg).show();
                    input.closest(".form-group").addClass("has-error");
                } else {
                    $.ajax({
                        "url": opt.baseurl + "ajax/account/update",
                        "method": "POST",
                        "data": JSON.stringify([{ "field": field, "value": value }]),
                        "contentType": "application/json; charset=UTF-8"
                    }).done(function (data) {
                        console.log(data);
                    }).fail(failHandler);
                }
            });

            internalOrg.on("click", ".shortcode-lookup", function (e) {
                // Runs when the user clicks the "Lookup" button next to the shortcode textbox

                var modal = $(".chartfield-modal", $this);
                var value = parseInt($(".shortcode", internalOrg).val());
                var shortcode = null;

                if (!isNaN(value)) {
                    shortcode = new String(1000000 + value).substr(1);
                }

                if (shortcode) {
                    $(".shortcode-form-group", internalOrg).removeClass("has-error");
                    $(".shortcode", internalOrg).val(shortcode);
                    $(".status-open", modal).hide();
                    $(".status-terminated", modal).hide();
                    $(".status-notfound", modal).hide();

                    $.ajax({
                        "url": opt.baseurl + "api/shortcode/" + shortcode
                    }).done(function (data) {
                        if (data) {
                            $(".fund", modal).val(data.fundCode);
                            $(".department", modal).val(data.deptID);
                            $(".program", modal).val(data.programCode);
                            $(".class", modal).val(data.class);
                            $(".project", modal).val(data.projectGrant);
                            $(".shortcode", modal).val(data.shortCode);
                            $(".description", modal).html(data.shortCodeDescription);

                            if (data.shortCodeStatus === "O")
                                $(".status-open", modal).show();
                            else if (data.shortCodeStatus === "T")
                                $(".status-terminated", modal).show();
                        } else {
                            $(".status-notfound", modal).show();
                        }

                        modal.modal("show");
                    }).fail(failHandler);
                } else {
                    $(".shortcode-form-group", internalOrg).addClass("has-error");
                }
            });

            addressList.on("click", ".edit-addr", function (e) {
                // Runs when the user clicks the address edit icon

                e.preventDefault();
                var addr = $(this).closest(".address-item");
                beginEdit(addr);

            }).on("click", ".delete-addr", function (e) {
                // Runs when the user clicks the address delete icon

                e.preventDefault();

                var addr = $(this).closest(".address-item");

                if (addr.length > 0) {
                    var model = {
                        "address-id": addr.data("address-id"),
                        "address-type": addr.data("address-type")
                    };

                    $.ajax({
                        "url": opt.baseurl + "ajax/account/delete/address",
                        "method": "POST",
                        "data": model
                    }).done(function (data) {
                        if (data.delete) {
                            addr.data("address-id", -1);
                            $(".attention", addr).data("original-value", "");
                            $(".address-line1", addr).data("original-value", "");
                            $(".address-line2", addr).data("original-value", "");
                            $(".city", addr).data("original-value", "");
                            $(".state", addr).data("original-value", "");
                            $(".zip", addr).data("original-value", "");
                            $(".country", addr).data("original-value", "");
                        }
                        cancelEdit(addr);
                    }).fail(failHandler);
                }

            }).on("click", ".update-addr", function (e) {
                // Runs when the user clicks the address update icon

                e.preventDefault();

                var addr = $(this).closest(".address-item");

                if (addr.length > 0) {
                    var model = {
                        "address-id": addr.data("address-id"),
                        "address-type": addr.data("address-type"),
                        "attention": $(".attention-input", addr).val(),
                        "address-line1": $(".address-line1-input", addr).val(),
                        "address-line2": $(".address-line2-input", addr).val(),
                        "city": $(".city-input", addr).val(),
                        "state": $(".state-input", addr).val(),
                        "zip": $(".zip-input", addr).val(),
                        "country": $(".country-input", addr).val()
                    };

                    $.ajax({
                        "url": opt.baseurl + "ajax/account/update/address",
                        "method": "POST",
                        "data": model
                    }).done(function (data) {
                        addr.data("address-id", data.addressId);
                        $(".attention", addr).data("original-value", model["attention"]);
                        $(".address-line1", addr).data("original-value", model["address-line1"]);
                        $(".address-line2", addr).data("original-value", model["address-line2"]);
                        $(".city", addr).data("original-value", model["city"]);
                        $(".state", addr).data("original-value", model["state"]);
                        $(".zip", addr).data("original-value", model["zip"]);
                        $(".country", addr).data("original-value", model["country"]);
                        cancelEdit(addr);
                    }).fail(failHandler);
                }

            }).on("click", ".cancel-addr", function (e) {
                e.preventDefault();
                var addr = $(this).closest(".address-item");
                cancelEdit(addr);
            }).on("click", ".add-addr", function (e) {
                var addr = $(this).closest(".address-item");
                beginEdit(addr);
            });

            managerList.on("click", ".add-manager", function (e) {
                var clientOrgId = $(".available-managers", managerList).val();

                $.ajax({
                    "url": opt.baseurl + "ajax/account/add/manager",
                    "method": "POST",
                    "data": { "client-org-id": clientOrgId }
                }).done(function (data) {
                    if (data.add) {
                        // reload the managers list
                        $(".manager-list-items", managerList).html(opt.templates["manager-list"](data.managers));

                        // repopulate available managers
                        populateAvailableManagers(data.available);
                    }
                }).fail(failHandler);

            }).on("click", ".delete-manager", function (e) {
                e.preventDefault();

                var mgr = $(this).closest(".manager-item");

                if (mgr.length > 0) {
                    $.ajax({
                        "url": opt.baseurl + "ajax/account/delete/manager",
                        "method": "POST",
                        "data": { "client-org-id": mgr.data("client-org-id") }
                    }).done(function (data) {
                        if (data.delete) {
                            // reload the managers list
                            $(".manager-list-items", managerList).html(opt.templates["manager-list"](data.managers));

                            // repopulate available managers
                            populateAvailableManagers(data.available);
                        }
                    }).fail(failHandler);
                }
            });

            var formatDate = function (v, f) {
                var m = moment(v);
                if (m.isValid())
                    return m.format(f);
                else
                    return "";
            };

            var formatNumber = function (v, f) {
                if (null === v || isNaN(v))
                    return "";
                else
                    return numeral(v).format(f);
            };

            var refresh = function () {
                $this.hide();

                $.ajax({
                    "url": opt.baseurl + "ajax/account/edit"
                }).done(function (data) {
                    if (internalOrg.length > 0) {
                        $(".account", internalOrg).val(data.ChartFields.Account);
                        $(".fund", internalOrg).val(data.ChartFields.Fund);
                        $(".department", internalOrg).val(data.ChartFields.Department);
                        $(".program", internalOrg).val(data.ChartFields.Program);
                        $(".class", internalOrg).val(data.ChartFields.Class);
                        $(".project", internalOrg).val(data.ChartFields.Project);
                        $(".shortcode", internalOrg).val(data.ChartFields.ShortCode);
                    }

                    if (externalOrg.length > 0) {
                        $(".account-number", externalOrg).val(new String(data.AccountNumber).substr(8));
                        $(".invoice-number", externalOrg).val(data.InvoiceNumber);
                        $(".invoice-line1", externalOrg).val(data.InvoiceLine1);
                        $(".invoice-line2", externalOrg).val(data.InvoiceLine2);
                        $(".po-end-date", externalOrg).val(formatDate(data.PoEndDate, "M/D/YYYY h:mm:ss A"));
                        $(".po-initial-funds", externalOrg).val(formatNumber(data.PoInitialFunds, "0.00"));
                    }

                    $(".account-name", accountInfo).val(data.AccountName);
                    $(".funding-sources option[value='" + data.FundingSourceID + "']", accountInfo).prop("selected", true);
                    $(".technical-fields option[value='" + data.TechnicalFieldID + "']", accountInfo).prop("selected", true);
                    $(".special-topics option[value='" + data.SpecialTopicID + "']", accountInfo).prop("selected", true);
                    $(".account-types input[value='" + data.AccountTypeID + "']", accountInfo).prop("checked", true);

                    $.each(data.Addresses, function (key, value) {
                        var addr = $(".address-item[data-address-type='" + key + "']", addressList);
                        if (addr.length > 0) {
                            if (value) {
                                addr.data("address-id", value.AddressID);
                                $(".attention", addr).html(value.Attention);
                                $(".address-line1", addr).html(value.AddressLine1);
                                $(".address-line2", addr).html(value.AddressLine2);
                                $(".city", addr).html(value.City);
                                $(".state", addr).html(value.State);
                                $(".zip", addr).html(value.Zip);
                                $(".country", addr).html(value.Country);

                                $(".controls .add-addr", addr).hide();
                                $(".controls .edit-delete", addr).show();
                            } else {
                                // -1 means address is undefined, 0 means we are adding a new address
                                addr.data("address-id", -1);
                            }
                        }
                    });

                    $(".manager-list-items", managerList).html(opt.templates["manager-list"](data.Managers));

                }).fail(failHandler).always(function () {
                    $this.show();
                });
            };

            var getData = function () {
                var result = {
                    "AccountName": $(".account-name", accountInfo).val(),
                    "AccountNumber": "",
                    "ShortCode": "",
                    "FundingSourceID": $(".funding-sources", accountInfo).val(),
                    "TechnicalFieldID": $(".technical-fields", accountInfo).val(),
                    "SpecialTopicID": $(".special-topics", accountInfo).val(),
                    "AccountTypeID": $(".account-types input:checked", accountInfo).val(),
                    "InvoiceNumber": "",
                    "InvoiceLine1": "",
                    "InvoiceLine2": "",
                    "PoEndDate": null,
                    "PoInitialFunds": null,
                    "ChartFields": null
                };

                if (internalOrg.length > 0) {
                    result.ShortCode = $(".shortcode", internalOrg).val();
                    result.ChartFields = {
                        "Account": $(".account", internalOrg).val(),
                        "Fund": $(".fund", internalOrg).val(),
                        "Department": $(".department", internalOrg).val(),
                        "Program": $(".program", internalOrg).val(),
                        "Class": $(".class", internalOrg).val(),
                        "Project": $(".project", internalOrg).val(),
                        "ShortCode": $(".shortcode", internalOrg).val()
                    };
                } else if (externalOrg.length > 0) {
                    result.AccountNumber = $(".account-number", externalOrg).val();
                    result.InvoiceNumber = $(".invoice-number", externalOrg).val();
                    result.InvoiceLine1 = $(".invoice-line1", externalOrg).val();
                    result.InvoiceLine2 = $(".invoice-line2", externalOrg).val();
                    result.PoEndDate = $(".po-end-date", externalOrg).val();
                    result.PoInitialFunds = $(".po-initial-funds", externalOrg).val();
                }

                return result;
            };

            refresh();
        });
    };
}(jQuery));