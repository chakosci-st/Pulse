var pulseJwtToken = null;
var pulseTokenExpiry = null;
var currentUser = null;
var appPath = getAppRootPath();
var apiPath = getApiRootPath();

function initJwtToken() {
    return $.ajax({
        url: getAppRootPath() + '/auth/token',
        type: 'GET',
        dataType: 'json',
        async: false
    }).done(function (res) {
        pulseJwtToken = res.access_token;
        currentUser = parseJwt(pulseJwtToken);
        //console.log('Current user payload:', currentUser);
        // Optional: decode exp from JWT to track expiry client-side
    }).fail(function (xhr) {
        console.error('Failed to get access token', xhr.status, xhr.responseText);
    });
}


function parseJwt(token) {
    if (!token) return null;
    var parts = token.split('.');
    if (parts.length !== 3) return null;

    var payload = parts[1];

    // Base64url -> Base64
    payload = payload.replace(/-/g, '+').replace(/_/g, '/');
    // Pad with '='
    while (payload.length % 4) {
        payload += '=';
    }

    try {
        var json = atob(payload);
        return JSON.parse(json);
    } catch (e) {
        console.error('Failed to parse JWT payload', e);
        return null;
    }
}


function applyPulseJwtHeader(xhr, url) {
    var requestUrl = url || '';

    if (requestUrl.indexOf(apiPath) !== 0) {
        return true;
    }

    if (!pulseJwtToken) {
        console.warn('Retrying... ' + requestUrl);
        initJwtToken();

        if (!pulseJwtToken) {
            console.warn('Access token not ready; blocking API request to ' + requestUrl);
            return false;
        }
    }

    xhr.setRequestHeader('Authorization', 'Bearer ' + pulseJwtToken);
    return true;
}


// Wrap request-specific beforeSend so API calls still receive JWT auth.
$.ajaxPrefilter(function (options, originalOptions, jqXHR) {
    var originalBeforeSend = options.beforeSend;

    options.beforeSend = function (xhr, settings) {
        var effectiveSettings = settings || options;
        var requestUrl = effectiveSettings.url || options.url || '';

        if (!applyPulseJwtHeader(xhr, requestUrl)) {
            return false;
        }

        if ($.isFunction(originalBeforeSend)) {
            return originalBeforeSend.call(this, xhr, effectiveSettings);
        }

        return true;
    };
});

$(function () {
    initJwtToken();
});