window.currentUserCode = "*";

function ClearField() {
    $('input').val('');
    $('textarea').val('');

    window.formFields = [];
    if (typeof renderFieldsList === 'function') renderFieldsList();
    if (typeof renderPreviewForm === 'function') renderPreviewForm();
    if (typeof renderFormStructure === 'function') renderFormStructure();
    $('#addFieldForm')[0].reset();
    $('#fieldType').trigger('change');
    $('#editFieldIdx').val('');
    $('#addFieldBtn').show();
    $('#updateFieldBtn, #cancelEditBtn').hide();

    $('#fieldReadAccess').val('*');
    $('#fieldWriteAccess').val('*');

    if (window.RulesBuilder) $('#rulesList').empty();
}


document.addEventListener('DOMContentLoaded', function () {
    // Form Preview Card
    var previewToggleBtn = document.getElementById('toggleFormPreview');
    var previewBody = document.getElementById('formPreviewBody');
    var previewChevron = document.getElementById('formPreviewChevron');

    previewToggleBtn.addEventListener('click', function () {
        var isVisible = !previewBody.classList.contains('d-none');
        if (isVisible) {
            previewBody.classList.add('d-none');
            previewChevron.style.transform = 'rotate(180deg)';
            previewToggleBtn.setAttribute('aria-label', 'Show');
        } else {
            previewBody.classList.remove('d-none');
            previewChevron.style.transform = 'rotate(0deg)';
            previewToggleBtn.setAttribute('aria-label', 'Minimize');
        }
    });

    // Form Structure Card
    var structureToggleBtn = document.getElementById('toggleFormStructure');
    var structureBody = document.getElementById('formStructureBody');
    var structureChevron = document.getElementById('formStructureChevron');

    structureToggleBtn.addEventListener('click', function () {
        var isVisible = !structureBody.classList.contains('d-none');
        if (isVisible) {
            structureBody.classList.add('d-none');
            structureChevron.style.transform = 'rotate(180deg)';
            structureToggleBtn.setAttribute('aria-label', 'Show');
        } else {
            structureBody.classList.remove('d-none');
            structureChevron.style.transform = 'rotate(0deg)';
            structureToggleBtn.setAttribute('aria-label', 'Minimize');
        }
    });

    // Copy JSON to clipboard
    var copyBtn = document.getElementById('copyFormJsonBtn');
    if (copyBtn) {
        copyBtn.addEventListener('click', function () {
            var jsonText = document.getElementById('formStructure').textContent;
            if (navigator.clipboard) {
                navigator.clipboard.writeText(jsonText)
                    .then(function () {
                        alert('Form JSON copied to clipboard!');
                    })
                    .catch(function (err) {
                        alert('Failed to copy: ' + err);
                    });
            } else {
                // Fallback for older browsers
                var textarea = document.createElement('textarea');
                textarea.value = jsonText;
                document.body.appendChild(textarea);
                textarea.select();
                document.execCommand('copy');
                document.body.removeChild(textarea);
                alert('Form JSON copied to clipboard!');
            }
        });
    }

    // Import JSON from file
    var importBtn = document.getElementById('importBtn');
    var importInput = document.getElementById('importJsonInput');
    if (importBtn && importInput) {
        importBtn.addEventListener('click', function () {
            importInput.value = ''; // Reset file input
            importInput.click();
        });

        importInput.addEventListener('change', function () {
            var file = importInput.files[0];
            if (!file) return;
            var reader = new FileReader();
            reader.onload = function (e) {
                try {
                    var json = JSON.parse(e.target.result);
                    if (!Array.isArray(json)) {
                        alert('Invalid JSON: Root should be an array of fields.');
                        return;
                    }
                    window.formFields = json;
                    if (typeof renderFieldsList === 'function') renderFieldsList();
                    if (typeof renderPreviewForm === 'function') renderPreviewForm();
                    if (typeof renderFormStructure === 'function') renderFormStructure();
                    alert('Form structure imported successfully!');
                } catch (err) {
                    alert('Failed to import JSON: ' + err.message);
                }
            };
            reader.readAsText(file);
        });
    }


    // Export JSON as a file
    var exportBtn = document.getElementById('exportBtn');
    if (exportBtn) {
        exportBtn.addEventListener('click', function () {
            var jsonText = document.getElementById('formStructure').textContent;
            var blob = new Blob([jsonText], { type: "application/json" });
            var url = URL.createObjectURL(blob);
            var a = document.createElement('a');
            a.href = url;
            a.download = "form-structure.json";
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            URL.revokeObjectURL(url);
        });
    }
});




$(document).ready(function () {
 

    document.addEventListener('DOMContentLoaded', function () {
        // Form Preview Card
        var previewToggleBtn = document.getElementById('toggleFormPreview');
        var previewBody = document.getElementById('formPreviewBody');
        var previewChevron = document.getElementById('formPreviewChevron'); 

        if (previewToggleBtn && previewBody && previewChevron) {
            previewToggleBtn.addEventListener('click', function () {
                var isVisible = !previewBody.classList.contains('d-none');
                if (isVisible) {
                    previewBody.classList.add('d-none');
                    previewChevron.style.transform = 'rotate(180deg)';
                    previewToggleBtn.setAttribute('aria-label', 'Show'); 
                } else {
                    previewBody.classList.remove('d-none');
                    previewChevron.style.transform = 'rotate(0deg)';
                    previewToggleBtn.setAttribute('aria-label', 'Minimize'); 
                }
 
            });
        }

        // Form Structure Card
        var structureToggleBtn = document.getElementById('toggleFormStructure');
        var structureBody = document.getElementById('formStructureBody');
        var structureChevron = document.getElementById('formStructureChevron');

        if (structureToggleBtn && structureBody && structureChevron) {
            structureToggleBtn.addEventListener('click', function () {
                var isVisible = !structureBody.classList.contains('d-none');
                if (isVisible) {
                    structureBody.classList.add('d-none');
                    structureChevron.style.transform = 'rotate(180deg)';
                    structureToggleBtn.setAttribute('aria-label', 'Show');
                    structureToggleBtn.title = 'Show';
                } else {
                    structureBody.classList.remove('d-none');
                    structureChevron.style.transform = 'rotate(0deg)';
                    structureToggleBtn.setAttribute('aria-label', 'Minimize');
                    structureToggleBtn.title = 'Minimize';
                }
            });
        }


        // Copy JSON to clipboard
        var copyBtn = document.getElementById('copyFormJsonBtn');
        if (copyBtn) {
            copyBtn.addEventListener('click', function () {
                var jsonText = document.getElementById('formStructure').textContent;
                if (navigator.clipboard) {
                    navigator.clipboard.writeText(jsonText)
                        .then(function () {
                            alert('Form JSON copied to clipboard!');
                        })
                        .catch(function (err) {
                            alert('Failed to copy: ' + err);
                        });
                } else {
                    // Fallback for older browsers
                    var textarea = document.createElement('textarea');
                    textarea.value = jsonText;
                    document.body.appendChild(textarea);
                    textarea.select();
                    document.execCommand('copy');
                    document.body.removeChild(textarea);
                    alert('Form JSON copied to clipboard!');
                }
            });
        }


        // Import JSON from file
        var importBtn = document.getElementById('importBtn');
        var importInput = document.getElementById('importJsonInput');
        if (importBtn && importInput) {
            importBtn.addEventListener('click', function () {
                importInput.value = ''; // Reset file input
                importInput.click();
            });

            importInput.addEventListener('change', function () {
                var file = importInput.files[0];
                if (!file) return;
                var reader = new FileReader();
                reader.onload = function (e) {
                    try {
                        var json = JSON.parse(e.target.result);
                        if (!Array.isArray(json)) {
                            alert('Invalid JSON: Root should be an array of fields.');
                            return;
                        }
                        window.formFields = json;
                        if (typeof renderFieldsList === 'function') renderFieldsList();
                        if (typeof renderPreviewForm === 'function') renderPreviewForm();
                        if (typeof renderFormStructure === 'function') renderFormStructure();
                        alert('Form structure imported successfully!');
                    } catch (err) {
                        alert('Failed to import JSON: ' + err.message);
                    }
                };
                reader.readAsText(file);
            });
        }


        // Export JSON as a file
        var exportBtn = document.getElementById('exportBtn');
        if (exportBtn) {
            exportBtn.addEventListener('click', function () {
                var jsonText = document.getElementById('formStructure').textContent;
                var blob = new Blob([jsonText], { type: "application/json" });
                var url = URL.createObjectURL(blob);
                var a = document.createElement('a');
                a.href = url;
                a.download = "form-structure.json";
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                URL.revokeObjectURL(url);
            });
        }
    });


    $('#buttonClear').on('click', function () {
        if (confirm('Are you sure you want to clear all values?')) {
            ClearField();
        }
    })

 


});