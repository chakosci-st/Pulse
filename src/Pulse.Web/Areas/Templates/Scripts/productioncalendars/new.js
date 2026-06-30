function ClearField() {
    $('input').val('');
}

function getFirstSaturday(year) {
    var date = new Date(year, 0, 1); // Jan 1
    var day = date.getDay();
    var offset = (6 - day + 7) % 7; // 6 is Saturday
    date.setDate(1 + offset);
    return date;
}

function refreshDatePicker(year) {

    if (/^\d{4}$/.test(year) && year >= 1999 && year <= 2100) {
        var start = new Date(year, 0, 1);  // Jan 1
        var end = new Date(year, 0, 31);   // Jan 31
        $('#Week1End').datepicker('setStartDate', start);
        $('#Week1End').datepicker('setEndDate', end);
        $('#Week1End').prop('disabled', false);

        // Set to first Saturday of January
        var firstSaturday = getFirstSaturday(year);
        var formatted = firstSaturday.getFullYear() + '-' +
            String(firstSaturday.getMonth() + 1).padStart(2, '0') + '-' +
            String(firstSaturday.getDate()).padStart(2, '0');
        //$('#Week1End').datepicker('update', formatted);
        $('#Week1End').val(formatted);
    } else {
        $('#Week1End').datepicker('setStartDate', null);
        $('#Week1End').datepicker('setEndDate', null);
        $('#Week1End').val('');
        $('#Week1End').prop('disabled', true);
    }
}

$(document).ready(function () {
    var currentYear = new Date().getFullYear();
 
    // Initialize datepicker but disable input initially
    $('#Week1End').datepicker({
        format: 'yyyy-mm-dd',
        autoclose: true,
        todayHighlight: true,
        startDate: null,
        endDate: null,
        orientation: "bottom",
        daysOfWeekDisabled: [0, 1, 2, 3, 4, 5] // Only Saturdays enabled
    });
 
    $('#CalendarYear').on('input change', function () {
        refreshDatePicker($(this).val());
    });


    $('#CalendarYear').val(currentYear);
    refreshDatePicker(currentYear);


    // Initialize jQuery Validate
    $('#productioncalendarForm').validate({
        // Add your validation rules if needed
        rules: {
            CalendarYear: { required: true },
            Week1End: { required: true },
        },
        messages: {
            CalendarYear: { required: "" },
            Week1End: { required: "" },
        },
        errorElement: 'span',
        errorPlacement: function (error, element) {
            $('[id*=-error]').hide()
        },
        highlight: function (element) {
            $(element).addClass('is-invalid').removeClass('is-valid');
            $(element).next('.invalid-feedback').show();
        },
        unhighlight: function (element) {
            $(element).removeClass('is-invalid').addClass('is-valid');
            $(element).next('.invalid-feedback').hide();
        },
        submitHandler: function (form) {
            // This function runs only if the form is valid

            // Prepare FormData
            var formData = new FormData();

            // Collect productionCalendar data (example)
            var productionCalendar = {
                CalendarYear: $('#CalendarYear').val(),
                Week1End: $('#Week1End').val(),
            };

            formData.append("productioncalendar", JSON.stringify(productionCalendar));

            // AJAX call
            $.ajax({
                url: getApiRootPath() + '/api/productionCalendars',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                // xhrFields: { withCredentials: true }, //** REMOVED**
                success: function () {

                    toastr.success(`Production Calendar for ${$('#CalendarYear').val()} is successfully generated!`)
                    ClearField();
                },
                error: function (xhr) {
                    if (xhr.responseText.indexOf('ORA-00001: unique constraint (NPITRACK.PRODUCTGROUPS_PK) violated') > 0) {
                        toastr.error('Product Group code already exist!');
                    } else {
                        toastr.error('Error: ' + xhr.responseText);
                    }
                },

            });

            // Prevent default form submission
            return false;
        }
    });

});