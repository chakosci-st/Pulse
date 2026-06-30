// ~/scripts/utils.js

window.FormUtils = (function () {
    /**
     * Generate a GUID/UUID v4
     * @returns {string}
     */
    function generateGUID() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    /**
     * Deep clone an object (simple version)
     * @param {Object} obj
     * @returns {Object}
     */
    function deepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }

    /**
     * Parse a JSON string safely
     * @param {string} str
     * @returns {any|null}
     */
    function safeJsonParse(str) {
        try {
            return JSON.parse(str);
        } catch {
            return null;
        }
    }

    /**
     * Get a field by name from an array of fields
     * @param {Array} fields
     * @param {string} name
     * @returns {Object|null}
     */
    function getFieldByName(fields, name) {
        return fields.find(f => f.name === name) || null;
    }

    /**
     * Replace all occurrences of {param} in a string with a value
     * @param {string} str
     * @param {string} param
     * @param {string} value
     * @returns {string}
     */
    function replaceParam(str, param, value) {
        const re = new RegExp(`{${param}}`, 'g');
        return str.replace(re, encodeURIComponent(value));
    }

    /**
     * Debounce a function
     * @param {Function} func
     * @param {number} wait
     * @returns {Function}
     */
    function debounce(func, wait) {
        let timeout;
        return function (...args) {
            clearTimeout(timeout);
            timeout = setTimeout(() => func.apply(this, args), wait);
        };
    }

    /**
     * Capitalize the first letter of a string
     * @param {string} str
     * @returns {string}
     */
    function capitalize(str) {
        if (!str) return '';
        return str.charAt(0).toUpperCase() + str.slice(1);
    }

    // Expose API
    return {
        generateGUID,
        deepClone,
        safeJsonParse,
        getFieldByName,
        replaceParam,
        debounce,
        capitalize
    };
})();