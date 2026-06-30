resetHeroDecorations();

$(document).ready(function () {
    var table = $("#fieldsTable").DataTable({
        processing: true,
        responsive: true,
        serverSide: true,
        ajax: {
            url: getApiRootPath() + "/api/fields/datatables",
            type: "POST",
            contentType: "application/json",
            data: function (d) {
                d.isActive = $("#isActiveFilter").val();

                if (d.order && d.order.length > 0) {
                    var sortIndex = d.order[0].column;
                    d.sortBy = sortIndex === 0 ? "FIELDTITLE" : "FORMLINKEDCOUNT";
                    d.sortDirection = d.order[0].dir;
                }

                return JSON.stringify(d);
            }
        },
        dom: ' <"search"f><"top"l>rt<"bottom"ip><"clear">',
        lengthMenu: [[10, 25, 50, 100, -1], ["Entries per page: 10", "Entries per page: 25", "Entries per page: 50", "Entries per page: 100", "Entries per page: All"]],
        initComplete: function () {
            var api = this.api();
            var $tableContainer = $(api.table().container());
            var $toolbar = $(".table-filter-toolbar");

            $tableContainer.find(".dt-paging").appendTo(".card-tools-fieldsTable-pagination");
            $tableContainer.find(".dt-search").appendTo(".card-tools-fieldsTable-filter");
            $tableContainer.find(".dt-length").appendTo(".card-tools-fieldsTable-length");
            $tableContainer.find(".dt-info").appendTo(".card-tools-fieldsTable-size");

            $toolbar.find(".dt-search").addClass("toolbar-search__control");
            $toolbar.find(".dt-search label").addClass("toolbar-search__label");
            $toolbar.find(".dt-search input").addClass("form-control").attr("type", "search");
            $toolbar.find(".dt-length select").addClass("form-select");

            document.querySelectorAll(".toolbar-search__label").forEach(function (el) { el.remove(); });
            $("label[for='dt-length-0']").remove();
        },
        language: {
            lengthMenu: "_MENU_",
            search: "",
            searchPlaceholder: "Search...",
            emptyTable: "No data found.",
            processing: "<div>Loading data please wait...</div>",
            paginate: {
                previous: "«",
                next: "»"
            }
        },
        columns: [
            {
                data: "title",
                render: function (value, type, data, meta) {
                    var isActive = data.isActive === true || data.IsActive === true;
                    var linkedCount = data.FormLinkedCount || data.formLinkedCount || 0;
                    var status = isActive
                        ? '<span class="badge text-bg-success mr-2" style="margin-right:5px">Active</span>'
                        : '<span class="badge text-bg-danger mr-2" style="margin-right:5px">Inactive</span>';

                    var code = data.id || data.FieldSysId || "";
                    var name = data.name || data.FieldName || "";
                    var titleText = data.title || data.FieldTitle || "Untitled";
                    var fieldType = data.type || data.FieldType || "-";
                    var createdDate = data.createdDate || data.CreatedDate;
                    var modifiedDate = data.modifiedDate || data.ModifiedDate || createdDate;
                    var formattedCreateDate = createdDate ? moment(createdDate).format("MMM DD, YYYY HH:mm") : "-";
                    var relativeModifiedTime = modifiedDate ? moment(modifiedDate).fromNow() : "-";

                    return '' +
                        '<div class="card request-card mb-2">' +
                        '  <div class="card-body">' +
                        '    <div class="d-lg-flex align-items-center">' +
                        '      <div class="flex-fill pr-lg-3">' +
                        '        <div class="d-flex flex-wrap align-items-center mb-1">' +
                                     status +
                        '          <strong>' + titleText + '</strong>' +
                        '        </div>' +
                        '        <div class="small text-muted mb-1">Code: <strong>' + code + '</strong></div>' +
                        '        <div class="small text-muted mb-1">Name: ' + name + '</div>' +
                        '        <div class="small text-muted mb-1">Type: ' + fieldType + '</div>' +
                        '        <div class="small text-muted mb-1">Created: ' + formattedCreateDate + '</div>' +
                        '      </div>' +
                        '      <div class="flex-fill pr-lg-3 text-center">' +
                        '        <div class="field-thumb" data-row="' + meta.row + '" style="width:140px;height:90px;background:#f8f9fa;border-radius:8px;display:flex;align-items:center;justify-content:center; margin:0 auto;">' +
                        '          <span class="spinner-border spinner-border-sm"></span>' +
                        '        </div>' +
                        '        <div class="small text-muted mt-1">Preview</div>' +
                        '        <div class="small text-muted mb-1">Linked Forms: <strong>' + linkedCount + '</strong></div>' +
                        '      </div>' +
                        '      <div class="request-actions d-flex flex-column align-items-lg-end mt-3 mt-lg-0">' +
                        '        <div class="mb-1 d-flex gap-2 justify-content-lg-end">' +
                        '          <a title="Display" href="/templates/fields/display/' + code + '" class="btn btn-outline-secondary rounded-circle d-inline-flex align-items-center justify-content-center p-0" style="width: 40px; height: 40px;"><i class="bi bi-eye"></i></a>' +
                        '          <a title="Config" href="/templates/fields/edit/' + code + '" class="btn btn-outline-primary rounded-circle d-inline-flex align-items-center justify-content-center p-0" style="width: 40px; height: 40px;"><i class="bi bi-gear"></i></a>' +
                        '        </div>' +
                        '        <small class="text-muted">Updated ' + relativeModifiedTime + '</small>' +
                        '      </div>' +
                        '    </div>' +
                        '  </div>' +
                        '</div>';
                }
            }
        ]
    });

    $("#fieldsTable").on("draw.dt", function () {
        var tableData = table.rows({ page: "current" }).data().toArray();

        $("#fieldsTable .field-thumb").each(function () {
            var rowIdx = $(this).data("row");
            var row = tableData[rowIdx] || {};
            var field = {
                title: row.title || row.FieldTitle || "Untitled",
                name: row.name || row.FieldName || "field",
                type: row.type || row.FieldType || "text",
                placeholder: row.placeholder || row.Placeholder || "",
                tooltip: row.tooltip || row.Tooltip || "",
                isrequired: false,
                readAccess: "*",
                writeAccess: "*",
                minLength: row.minLength || row.MinLength || undefined,
                maxLength: row.maxLength || row.MaxLength || undefined,
                caseOption: row.caseOption || row.CaseOption || undefined,
                validate: row.validate || row.FieldValidate || undefined,
                datasource: row.datasource || row.DataSource || undefined,
                datasourceParamField: row.datasourceParamField || row.DataSourceParamField || undefined,
                fileTypes: row.fileTypes || row.FileType || undefined,
                fileMaxSize: row.fileMaxSize || row.FileMaxSize || undefined,
                urlIsParameter: row.urlIsParameter === true || row.urlIsParam === true || row.UrlIsParameter === true,
                urlDefaultPattern: row.urlDefaultPattern || row.UrlDefaultPattern || undefined,
                defaultValue: row.defaultValue || row.DefaultValue || undefined,
                defaultClobValue: row.defaultClobValue || row.DefaultClobValue || undefined,
                options: row.options || row.Options || [],
                rules: row.rules || row.Rules || []
            };

            $(this).data("fieldPreview", field);

            var $thumb = $(this);
            $.generateFormThumbnail([field], { userCode: "*", mode: "READONLY" }, function (imgData) {
                $thumb.html('<img src="' + imgData + '" style="max-width:100%;max-height:80px;border-radius:6px;box-shadow:0 1px 4px rgba(0,0,0,0.07);">');
            });
        });
    });

    $("#fieldsTable").on("click", ".field-thumb", function () {
        var field = $(this).data("fieldPreview");
        if (!field) {
            return;
        }

        var modalElement = document.getElementById("fieldPreviewModal");

        if (!window._fieldPreviewModal) {
            if (window.bootstrap && window.bootstrap.Modal) {
                window._fieldPreviewModal = new window.bootstrap.Modal(modalElement);
            } else if ($.fn.modal) {
                window._fieldPreviewModal = {
                    show: function () { $(modalElement).modal("show"); },
                    hide: function () { $(modalElement).modal("hide"); }
                };
            }
        }

        if (!window._fieldPreviewModal) {
            toastr.error("Preview modal is unavailable.");
            return;
        }

        FieldsDesigner.renderFieldPreviewInModes(field, "#fieldPreviewWrite", "#fieldPreviewRead");
        window._fieldPreviewModal.show();
    });

    $("#isActiveFilter").change(function () {
        table.ajax.reload();
    });
});
