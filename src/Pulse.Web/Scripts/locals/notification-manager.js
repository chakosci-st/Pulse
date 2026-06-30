(function (window, document) {
    'use strict';

    function escapeHtml(value) {
        if (value == null) {
            return '';
        }

        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function formatDisplayDate(value) {
        if (!value) {
            return '--';
        }

        var date = new Date(value);
        if (Number.isNaN(date.getTime())) {
            return value;
        }

        return date.toLocaleString('en-US', {
            month: 'short',
            day: 'numeric',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    function toDateTimeLocalValue(value) {
        if (!value) {
            return '';
        }

        var date = new Date(value);
        if (Number.isNaN(date.getTime())) {
            return '';
        }

        var pad = function (number) {
            return String(number).padStart(2, '0');
        };

        return [
            date.getFullYear(),
            '-',
            pad(date.getMonth() + 1),
            '-',
            pad(date.getDate()),
            'T',
            pad(date.getHours()),
            ':',
            pad(date.getMinutes())
        ].join('');
    }

    function buildPayload(options, form) {
        var formData = new FormData(form);
        return {
            notificationSysId: formData.get('notificationSysId') || null,
            title: formData.get('title') || '',
            message: formData.get('message') || '',
            recipients: formData.get('recipients') || '',
            notificationDate: formData.get('when') || '',
            expiryDate: formData.get('endswhen') || null,
            projectNo: options.projectNo || '',
            entityType: options.entityType,
            entitySysId: options.entitySysId
        };
    }

    function sortItems(items) {
        return (items || []).slice().sort(function (left, right) {
            return new Date(right.notificationDate || right.createdDate) - new Date(left.notificationDate || left.createdDate);
        });
    }

    window.PulseNotificationManagers = window.PulseNotificationManagers || {};

    function renderItem(item, options, fallbackContextLabel) {
        var actionMarkup = '';
        var canEdit = Boolean(options.canEdit) && Boolean(item.canEdit || item.canManage);
        var canDelete = Boolean(options.canDelete) && Boolean(item.canDelete || item.canManage);

        if (canEdit || canDelete) {
            var actions = [];
            if (canEdit) {
                actions.push('<button type="button" class="btn btn-outline-secondary btn-sm" data-notification-action="edit" data-notification-id="' + escapeHtml(item.notificationSysId) + '">Edit</button>');
            }
            if (canDelete) {
                actions.push('<button type="button" class="btn btn-outline-danger btn-sm" data-notification-action="delete" data-notification-id="' + escapeHtml(item.notificationSysId) + '">Delete</button>');
            }

            actionMarkup = [
                '<div class="d-flex gap-2">',
                actions.join(''),
                '</div>'
            ].join('');
        }

        return [
            '<div class="border rounded-3 p-3 bg-light-subtle mb-2" data-notification-item="', escapeHtml(item.notificationSysId), '">',
            '<div class="d-flex justify-content-between align-items-start gap-3">',
            '<div class="flex-grow-1">',
            '<div class="fw-semibold text-dark">', escapeHtml(item.title || 'Notification'), '</div>',
            '<div class="text-muted small mt-1">', escapeHtml(item.message || ''), '</div>',
            '<div class="d-flex flex-wrap gap-2 mt-2">',
            '<span class="badge text-bg-light border">', escapeHtml(formatDisplayDate(item.notificationDate || item.createdDate)), '</span>',
            item.recipients ? '<span class="badge text-bg-light border">Recipients: ' + escapeHtml(item.recipients) + '</span>' : '',
            item.expiryDate ? '<span class="badge text-bg-light border">Ends: ' + escapeHtml(formatDisplayDate(item.expiryDate)) + '</span>' : '',
            '</div>',
            '<div class="text-muted small mt-2">',
            'Context: ', escapeHtml(item.contextLabel || fallbackContextLabel || 'General'), ' • By ', escapeHtml(item.createdByDisplayName || item.createdBy || ''),
            '</div>',
            '</div>',
            actionMarkup,
            '</div>',
            '</div>'
        ].join('');
    }

    function createManager(options) {
        var state = {
            items: [],
            modal: null,
            form: document.querySelector(options.formSelector || '#notificationEditorForm')
        };

        var listElement = document.querySelector(options.listSelector);
        var emptyElement = document.querySelector(options.emptySelector);
        var addButton = options.addButtonSelector ? document.querySelector(options.addButtonSelector) : null;
        var modalElement = document.querySelector(options.modalSelector || '#notificationEditorModal');
        var titleElement = modalElement ? modalElement.querySelector('.modal-title') : null;
        var submitButton = modalElement ? modalElement.querySelector('#notificationEditorSubmitBtn') : null;

        if (!listElement || !emptyElement || !state.form || !modalElement) {
            return null;
        }

        state.modal = window.bootstrap ? new window.bootstrap.Modal(modalElement) : null;

        function setLoading(message) {
            emptyElement.classList.remove('d-none');
            emptyElement.textContent = message;
            listElement.innerHTML = '';
        }

        function render() {
            state.items = sortItems(state.items);

            if (!state.items.length) {
                emptyElement.classList.remove('d-none');
                emptyElement.textContent = options.emptyText || 'No notifications found.';
                listElement.innerHTML = '';
                return;
            }

            emptyElement.classList.add('d-none');
            listElement.innerHTML = state.items.map(function (item) {
                return renderItem(item, options, options.contextLabel);
            }).join('');
        }

        function load() {
            if (typeof options.onLoadStart === 'function') {
                options.onLoadStart();
            }

            setLoading(options.loadingText || 'Loading notifications...');

            return fetch(getApiRootPath() + '/api/notifications/entity/' + encodeURIComponent(options.entityType) + '/' + encodeURIComponent(options.entitySysId), {
                method: 'GET',
                headers: {
                    'Authorization': window.pulseJwtToken ? 'Bearer ' + window.pulseJwtToken : ''
                }
            })
                .then(function (response) {
                    if (!response.ok) {
                        throw new Error('Failed to load notifications');
                    }

                    return response.json();
                })
                .then(function (payload) {
                    state.items = payload.data || [];
                    render();
                })
                .catch(function () {
                    emptyElement.classList.remove('d-none');
                    emptyElement.textContent = options.errorText || 'Unable to load notifications right now.';
                    listElement.innerHTML = '';
                })
                .finally(function () {
                    if (typeof options.onLoadEnd === 'function') {
                        options.onLoadEnd();
                    }
                });
        }

        function fillForm(item) {
            state.form.reset();
            state.form.querySelector('[name="notificationSysId"]').value = item && item.notificationSysId ? item.notificationSysId : '';
            state.form.querySelector('[name="title"]').value = item && item.title ? item.title : '';
            state.form.querySelector('[name="message"]').value = item && item.message ? item.message : '';
            state.form.querySelector('[name="recipients"]').value = item && item.recipients ? item.recipients : '';
            state.form.querySelector('[name="when"]').value = item && item.notificationDate ? toDateTimeLocalValue(item.notificationDate) : toDateTimeLocalValue(new Date());
            state.form.querySelector('[name="endswhen"]').value = item && item.expiryDate ? toDateTimeLocalValue(item.expiryDate) : '';
        }

        function openCreate() {
            fillForm(null);
            if (titleElement) {
                titleElement.textContent = options.createTitle || 'Add notification';
            }
            if (submitButton) {
                submitButton.textContent = options.createButtonLabel || 'Save notification';
            }
            state.modal && state.modal.show();
        }

        function openEdit(notificationId) {
            var item = state.items.find(function (entry) { return entry.notificationSysId === notificationId; });
            if (!item) {
                return;
            }

            fillForm(item);
            if (titleElement) {
                titleElement.textContent = options.editTitle || 'Edit notification';
            }
            if (submitButton) {
                submitButton.textContent = options.editButtonLabel || 'Update notification';
            }
            state.modal && state.modal.show();
        }

        function save(event) {
            event.preventDefault();
            var payload = buildPayload(options, state.form);
            var notificationId = payload.notificationSysId;
            var method = notificationId ? 'PUT' : 'POST';
            var url = getApiRootPath() + '/api/notifications' + (notificationId ? '/' + encodeURIComponent(notificationId) : '/add');

            if (!payload.notificationDate) {
                window.alert('Notification date is required.');
                return;
            }

            if (submitButton) {
                submitButton.disabled = true;
            }

            fetch(url, {
                method: method,
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': window.pulseJwtToken ? 'Bearer ' + window.pulseJwtToken : ''
                },
                body: JSON.stringify(payload)
            })
                .then(function (response) {
                    if (!response.ok) {
                        throw new Error('Failed to save notification');
                    }

                    return response.json();
                })
                .then(function () {
                    state.modal && state.modal.hide();
                    return load();
                })
                .catch(function () {
                    window.alert('Unable to save the notification right now.');
                })
                .finally(function () {
                    if (submitButton) {
                        submitButton.disabled = false;
                    }
                });
        }

        function remove(notificationId) {
            if (!window.confirm('Delete this notification?')) {
                return;
            }

            fetch(getApiRootPath() + '/api/notifications/' + encodeURIComponent(notificationId), {
                method: 'DELETE',
                headers: {
                    'Authorization': window.pulseJwtToken ? 'Bearer ' + window.pulseJwtToken : ''
                }
            })
                .then(function (response) {
                    if (!response.ok) {
                        throw new Error('Failed to delete notification');
                    }

                    return response.json();
                })
                .then(function () {
                    return load();
                })
                .catch(function () {
                    window.alert('Unable to delete the notification right now.');
                });
        }

        if (addButton && options.canAdd) {
            addButton.addEventListener('click', openCreate);
        }

        state.form.addEventListener('submit', save);
        listElement.addEventListener('click', function (event) {
            var actionButton = event.target.closest('[data-notification-action]');
            if (!actionButton) {
                return;
            }

            var action = actionButton.getAttribute('data-notification-action');
            var notificationId = actionButton.getAttribute('data-notification-id');
            if (!notificationId) {
                return;
            }

            if (action === 'edit') {
                openEdit(notificationId);
                return;
            }

            if (action === 'delete') {
                remove(notificationId);
            }
        });

        return {
            load: load,
            openCreate: openCreate,
            setItems: function (items) {
                state.items = items || [];
                render();
            }
        };
    }

    window.PulseNotificationManager = {
        init: function (options) {
            var manager = createManager(options || {});
            if (manager) {
                if (options && options.name) {
                    window.PulseNotificationManagers[options.name] = manager;
                }
                manager.load();
            }
            return manager;
        },
        open: function (name) {
            var manager = window.PulseNotificationManagers[name];
            if (manager && typeof manager.openCreate === 'function') {
                manager.openCreate();
            }
        },
        formatDateValue: formatDisplayDate
    };
})(window, document);