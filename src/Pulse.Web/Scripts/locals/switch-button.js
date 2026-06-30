(function ($) {
    var idCounter = 0;

    $.fn.statusSwitch = function (optionsOrMethod) {
        var defaults = {
            checked: false,
            onText: 'ON',
            offText: 'OFF',
            name: 'statusSwitch',
            size: 'md' // 'sm', 'md', 'lg'
        };

        var sizeMap = {
            sm: { height: 20, knob: 14, knobOffset: 2, padding: 18, font: 12 },
            md: { height: 30, knob: 24, knobOffset: 3, padding: 26, font: 16 },
            lg: { height: 40, knob: 32, knobOffset: 4, padding: 36, font: 20 }
        };

        var methods = {
            init: function (options) {
                var settings = $.extend({}, defaults, options);
                var size = sizeMap[settings.size] || sizeMap['md'];

                return this.each(function () {
                    var $container = $(this);
                    idCounter++;
                    var switchId = settings.name + '-' + idCounter;

                    // Generate HTML for custom switch
                    var html = `
                        <input type="checkbox" id="${switchId}" name="${settings.name}" style="display:none;" ${settings.checked ? 'checked' : ''}>
                        <label for="${switchId}" class="status-switch-label" style="cursor:pointer;display:inline-block;">
                            <span class="status-switch-pill">
                                <span class="status-switch-text"></span>
                            </span>
                        </label>
                    `;
                    $container.html(html);

                    // Add styles (only once)
                    if (!$('#status-switch-style').length) {
                        $('head').append(`
                            <style id="status-switch-style">
                                .status-switch-label {
                                    user-select: none;
                                }
                                .status-switch-pill {
                                    display: inline-block;
                                    background: #dee2e6;
                                    border-radius: 999px;
                                    position: relative;
                                    transition: background 0.3s;
                                    vertical-align: middle;
                                    box-sizing: border-box;
                                }
                                .status-switch-pill .status-switch-text {
                                    position: relative;
                                    z-index: 2;
                                    font-weight: bold;
                                    color: #fff;
                                    pointer-events: none;
                                    white-space: nowrap;
                                    transition: color 0.3s;
                                }
                                .status-switch-pill::before {
                                    content: "";
                                    position: absolute;
                                    background: #fff;
                                    border-radius: 50%;
                                    transition: left 0.3s;
                                    box-shadow: 0 1px 3px rgba(0,0,0,0.1);
                                    z-index: 3;
                                }
                                input[type="checkbox"]:checked + .status-switch-label .status-switch-pill {
                                    background: #198754;
                                }
                                input[type="checkbox"]:checked + .status-switch-label .status-switch-text {
                                    color: #fff;
                                }
                            </style>
                        `);
                    }

                    var $pill = $container.find('.status-switch-pill');
                    var $text = $container.find('.status-switch-text');
                    var $checkbox = $container.find('input[type=checkbox]');

                    // Set font size and height
                    $pill.css({
                        height: size.height + 'px',
                        'font-size': size.font + 'px',
                        'padding-left': size.padding + 'px',
                        'padding-right': size.padding + 'px',
                        'line-height': size.height + 'px',
                        'min-width': (size.knob * 2 + size.padding * 2) + 'px'
                    });
                    $text.css({
                        height: size.height + 'px',
                        'line-height': size.height + 'px'
                    });

                    // Set knob size and position via inline style
                    var knobStyle = `
                        .status-switch-pill {
                            --knob-width: ${size.knob}px;
                            --knob-offset: ${size.knobOffset}px;
                        }
                        .status-switch-pill::before {
                            width: var(--knob-width);
                            height: var(--knob-width);
                            left: var(--knob-offset);
                            top: var(--knob-offset);
                        }
                        input[type="checkbox"]:checked + .status-switch-label .status-switch-pill::before {
                            left: calc(100% - var(--knob-width) - var(--knob-offset));
                        }
                    `;
                    $container.find('style.status-switch-size').remove();
                    $container.append(`<style class="status-switch-size">${knobStyle}</style>`);

                    // Store for later
                    $container.data('statusSwitchCheckbox', $checkbox);

                    // Update text and style
                    function update() {
                        if ($checkbox.is(':checked')) {
                            $text.text(settings.onText);
                        } else {
                            $text.text(settings.offText);
                        }
                    }

                    // Initial update
                    update();

                    // Bind event
                    $checkbox.on('change', update);

                    // Also update when label is clicked (for accessibility)
                    $container.find('.status-switch-label').on('click', function () {
                        setTimeout(update, 10);
                    });

                    // Expose update for external calls
                    $container.data('statusSwitchUpdate', update);
                });
            },
            setChecked: function (checked) {
                return this.each(function () {
                    var $container = $(this);
                    var $checkbox = $container.data('statusSwitchCheckbox');
                    if ($checkbox) {
                        $checkbox.prop('checked', checked).trigger('change');
                    }
                });
            },
            getChecked: function () {
                var $container = this.first();
                var $checkbox = $container.data('statusSwitchCheckbox');
                return $checkbox ? $checkbox.is(':checked') : undefined;
            },
            update: function () {
                return this.each(function () {
                    var update = $(this).data('statusSwitchUpdate');
                    if (update) update();
                });
            }
        };

        if (typeof optionsOrMethod === 'string' && methods[optionsOrMethod]) {
            return methods[optionsOrMethod].apply(this, Array.prototype.slice.call(arguments, 1));
        } else {
            return methods.init.apply(this, arguments);
        }
    };
}(jQuery));