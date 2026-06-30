function normalizeIsActive(value) {
    if (value === true || value === 1) {
        return true;
    }

    if (typeof value === "string") {
        var normalized = value.toLowerCase();
        return normalized === "true" || normalized === "1";
    }

    return false;
}

$(document).ready(function () {
    $.ajax({
        url: getApiRootPath() + "/api/fields",
        type: "GET",
        contentType: "application/json",
        success: function (response) {
            var rows = Array.isArray(response) ? response : [];
            var activeCount = 0;
            var inactiveCount = 0;
            var byType = {};

            rows.forEach(function (row) {
                var fieldType = row.type || row.FieldType || "unknown";
                var isActive = normalizeIsActive(row.isActive === undefined ? row.IsActive : row.isActive);

                if (!byType[fieldType]) {
                    byType[fieldType] = { total: 0, active: 0, inactive: 0 };
                }

                byType[fieldType].total += 1;
                if (isActive) {
                    byType[fieldType].active += 1;
                    activeCount += 1;
                } else {
                    byType[fieldType].inactive += 1;
                    inactiveCount += 1;
                }
            });

            $("#totalFields").text(rows.length);
            $("#activeFields").text(activeCount);
            $("#inactiveFields").text(inactiveCount);

            var grid = $("#fieldTypeGrid");
            grid.empty();

            if (Object.keys(byType).length === 0) {
                grid.html('<div class="col-12 text-muted">No field records found.</div>');
                return;
            }

            Object.keys(byType).sort().forEach(function (type) {
                var bucket = byType[type];
                grid.append(
                    '<div class="col-lg-4 col-md-6 col-12">' +
                    '  <div class="card h-100 border-0 shadow-sm">' +
                    '    <div class="card-body">' +
                    '      <div class="text-uppercase small text-muted mb-2">' + type + '</div>' +
                    '      <h4 class="mb-2">' + bucket.total + '</h4>' +
                    '      <div class="small text-muted">Active: ' + bucket.active + '</div>' +
                    '      <div class="small text-muted">Inactive: ' + bucket.inactive + '</div>' +
                    '    </div>' +
                    '  </div>' +
                    '</div>'
                );
            });
        },
        error: function () {
            toastr.error("Unable to load field reports.");
        }
    });
});
