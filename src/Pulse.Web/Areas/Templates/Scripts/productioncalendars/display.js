 
$(document).ready(function () {

    var table = $('#productioncalendarTable').DataTable({
        "processing": true, "responsive": true,
        "serverSide": false, "paging": false,
        scrollCollapse: true,
        scrollY: '50vh',
        "ajax": {
            "url": getApiRootPath() + "/api/productionCalendars/GetByYear?year=" + id,
            "type": "GET",
            "contentType": "application/json"
        },
        "dom": '<"search"Bf><"top"l>rt<"bottom"ip><"clear">',
        "initComplete": function () {

            $('.dt-search').appendTo('.card-tools-productioncalendarTable-filter');
            $('.dt-info').appendTo('.card-tools-productioncalendarTable-size');
            $('.dt-buttons').appendTo('.card-tools-productioncalendarTable-buttons');
        },
        "language": {
            "search": "",
            "searchPlaceholder": "Search...",
            "emptyTable": "No data found.",
            'processing': '<div>Loading data please wait...  </div>'
        },
        buttons: [
            {
                extend: 'copy',
                text: '<i class="bi bi-files"></i>',
                className: 'btn btn-custom-dt',
                titleAttr: 'Copy'
            },
            {
                extend: 'csv',
                text: '<i class="bi bi-filetype-csv"></i>',
                className: 'btn btn-custom-dt',
                titleAttr: 'Export as CSV'
            },
            {
                extend: 'excel',
                text: '<i class="bi bi-file-earmark-excel"></i>',
                className: 'btn btn-custom-dt',
                titleAttr: 'Export as Excel'
            },
            {
                extend: 'pdf',
                text: '<i class="bi bi-file-earmark-pdf"></i>',
                className: 'btn btn-custom-dt',
                titleAttr: 'Export as PDF'
            },
            {
                extend: 'print',
                text: '<i class="bi bi-printer"></i>',
                className: 'btn btn-custom-dt',
                titleAttr: 'Print'
            }
        ],
        "columns": [
            {
                "data": "fiscalDate",
                "render": function (value, type, data) {
                    return moment(value, "YYYY-MM-DD").format("YYYY-MM-DD");
                }
            },
            {
                "data": "calendarWorkWeek",
                "render": function (value, type, data) {
                    return `WW${id}${value}`;
                }
            },
            {
                "data": "calendarMonth",
                "render": function (value, type, data) {
                    var returnvalue = "";

                    switch (value) {
                        case "01":
                            returnvalue = "JANUARY"
                            break;
                        case "02":
                            returnvalue = "FEBRUARY"
                            break;
                        case "03":
                            returnvalue = "MARCH"
                            break;
                        case "04":
                            returnvalue = "APRIL"
                            break;
                        case "05":
                            returnvalue = "MAY"
                            break;
                        case "06":
                            returnvalue = "JUNE"
                            break;
                        case "07":
                            returnvalue = "JULY"
                            break;
                        case "08":
                            returnvalue = "AUGUST"
                            break;
                        case "09":
                            returnvalue = "SEPTEMBER"
                            break;
                        case "10":
                            returnvalue = "OCTOBER"
                            break;
                        case "11":
                            returnvalue = "NOVEMBER"
                            break;
                        default:
                            returnvalue = "DECEMBER"
                            break;
                    }

                    return returnvalue;
                }
            },
            {
                "data": "calendarQuarter",
                "render": function (value, type, data) {
                    var returnvalue = "";

                    switch (value) {
                        case "01":
                            returnvalue = "1ST"
                            break;
                        case "02":
                            returnvalue = "2ND"
                            break;
                        case "03":
                            returnvalue = "3RD"
                            break;
                        default:
                            returnvalue = "4TH"
                            break;

                    }

                    return returnvalue;
                }
            }

        ]
    });


});