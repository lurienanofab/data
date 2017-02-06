$(document).ready(function () {

    dataTableOptions = (typeof dataTableOptions == 'undefined') ? {} : dataTableOptions;
    $('.datatable').dataTable(dataTableOptions);


    $(".client-access").access();

    $('.address-table').datagrid({
        'getFields': function () {
            return [
                { 'name': 'AddressID', 'className': 'address-id', 'visible': false },
                { 'name': 'AddressType', 'className': 'address-type' },
                { 'name': 'AttentionLine', 'className': 'address-attention-line' },
                { 'name': 'Street1', 'className': 'address-street1', 'required': true },
                { 'name': 'Street2', 'className': 'address-street2' },
                { 'name': 'City', 'className': 'address-city', 'required': true },
                { 'name': 'State', 'className': 'address-state', 'required': true },
                { 'name': 'Zip', 'className': 'address-zip', 'required': true },
                { 'name': 'Country', 'className': 'address-country' }
            ];
        },
        'disableFooter': function (instance) {
            var select = instance.getFooter().getCell('address-type').find('select');
            return select.find('option').length == 0;
        },
        'onAddRow': function (instance, row) {
            var select = instance.getFooter().getCell('address-type').find('select');
            var option = select.find('option:selected');
            row.getCell('address-type').html(option.text());
            option.remove();
        },
        'onRemoveRow': function (instance, row) {
            var text = row.getCell('address-type').text();
            var value = text.toLowerCase();
            var select = instance.getFooter().getCell('address-type').find('select');
            var option = $('<option/>').html(text).val(value);
            select.append(option);
            instance.sortSelect(select);
        }
    });

    $('.manager-table').datagrid({
        'allowCopy': false,
        'allowEdit': false,
        'getFields': function () {
            return [
                { 'name': 'ClientOrgID', 'className': 'client-org-id', 'visible': false },
                { 'name': 'DisplayName', 'className': 'display-name' }
            ];
        },
        'disableFooter': function (instance) {
            var select = instance.getFooter().getCell('display-name').find('select');
            return select.find('option').length == 0;
        },
        'onAddRow': function (instance, row) {
            var select = instance.getFooter().getCell('display-name').find('select');
            var option = select.find('option:selected');
            row.getCell('client-org-id').html(option.val());
            row.getCell('display-name').html(option.text());
            option.remove();
        },
        'onRemoveRow': function (instance, row) {
            var value = row.getCell('client-org-id').text();
            var text = row.getCell('display-name').text();
            var option = $('<option/>').html(text).val(value);
            var select = instance.getFooter().getCell('display-name').find('select');
            select.append(option);
            instance.sortSelect(select);
        }
    });

    //$('.service-log').servicelog();
    $(".services").services();

    $("#active_orgs").val($('.OrgID').val()); // set the organization from the previous selected page

    $('.feed').feeds();

    $('.news').news();

    $('.data-utility').utility();

    $('.account-assignment').assignaccounts({
        'init': function (instance) {

            var initManagerCombo = function () {
                //$('.manager-clientorg-id', instance).combobox();
            };

            var selectedValue = instance.Settings.Data.OrgID;
            var selectedText = $('.org-select .autodd', instance).find('option[value="' + selectedValue + '"]').text();

            $('.org-select .autodd', instance)
				//.combobox()
				.change(function () {
				    if (selectedValue != this.value) {
				        selectedValue = this.value;
				        instance.Settings.Data.ManagerClientOrgID = null;
				        instance.UpdateManagers(selectedValue, initManagerCombo);
				    }
				});

            //$('.org-select .ui-combobox-input').css({ 'width': $('.autodd', instance).outerWidth() + 'px' });
            $('.org-select .ui-combobox-input').val(selectedText);

            instance.UpdateManagers(selectedValue, initManagerCombo);
        }
    });

    $('.section.client').each(function () {
        var $this = $(this);
        var search = $('.search', $this).val();
        var tbl = $('.client-table', $this);
        tbl.dataTable({
            'aoColumns': [
                { 'sWidth': '60px' },
                { 'sWidth': '100px' },
                null,
                { 'sWidth': '60px' },
                { 'bVisible': false }
            ],
            'sPaginationType': 'full_numbers'
        });

        tbl.fnFilter(search);
    });

    $('.section.account').each(function () {
        var $this = $(this);
        var search = $('.search', $this).val();
        var tbl = $('.account-table', $this);
        tbl.dataTable({
            'aaSorting': [
                [1, 'asc'],
                [2, 'asc']
            ],
            'aoColumns': [
                { 'sWidth': '60px' },
                { 'sWidth': '220px' },
                null,
                { 'sWidth': '60px' },
                { 'sWidth': '60px' },
                { 'bVisible': false }
            ],
            'sPaginationType': 'full_numbers'
        });
    });

    $(".paging_full_numbers").css({
        "-webkit-touch-callout": "none",
        "-webkit-user-select": "none",
        "-khtml-user-select": "none",
        "-moz-user-select": "none",
        "-ms-user-select": "none",
        "user-select": "none"
    });
});




//test
var test = [
	{
	    "processInfoId": 678,
	    "lines": [
            {
                "processInfoLineId": 1,
                "param": { "processInfoLineParamId": "123", "value": "500" }
            },
            {
                "processInfoLineId": 1,
                "param": { "processInfoLineParamId": "123", "value": "200" }
            },
            {
                "processInfoLineId": 2,
                "param": { "ProcessInfoLineParamId": "123", "value": "5" }
            }
	    ]
	},
    {
        "processInfoId": 678,
        "lines": [{
            "processInfoLineId": 1, "param": { "processInfoLineParamId": "123", "value": "150" }
        }, { "processInfoLineId": 2, "param": { "processInfoLineParamId": "123", "value": "3" } }]
    }];