// ----------------------------------
// Helpers
// ----------------------------------
function getInitials(name) {
    if (!name) return '';
    const words = name.trim().split(/\s+/);
    if (words.length === 1) {
        return words[0].substring(0, 2).toUpperCase();
    }
    // Use first letter of up to 3 words
    return words.slice(0, 3).map(w => w[0]).join('').toUpperCase();
}

// ----------------------------------
// Core avatar builder (returns avatars, does NOT touch container)
// ----------------------------------
function buildAvatarElements(users, settings) {
    users = users || [];

    // Transform data if provided
    if (typeof settings.transform === 'function') {
        users = settings.transform(users);
    }

    // Helpers (duplicated from original render logic)
    function pluralize(word, count) {
        return count === 1 ? word : word + 's';
    }

    function sortUsers(list) {
        if (!settings.sort) return list;
        let sorted = list.slice();
        if (typeof settings.sort === 'string') {
            sorted.sort((a, b) => {
                let aVal = (a[settings.sort] || '').toLowerCase();
                let bVal = (b[settings.sort] || '').toLowerCase();
                return aVal.localeCompare(bVal);
            });
        } else if (typeof settings.sort === 'function') {
            sorted.sort(settings.sort);
        }
        return sorted;
    }

    function getUserUrl(user) {
        if (!settings.userInformationUrl || !user.id) return null;
        if (typeof settings.userInformationUrl === 'function') {
            return settings.userInformationUrl(user);
        }
        return settings.userInformationUrl.replace('{id}', encodeURIComponent(user.id));
    }

    function handleAvatarClick(url) {
        if (!url) return;
        if (settings.userInformationTarget === '_blank') {
            window.open(url, '_blank');
        } else {
            window.location.href = url;
        }
    }

    function getDisplayInitials(user) {
        if (user && user.initials) {
            return String(user.initials).trim().toUpperCase();
        }

        return getInitials(user && user.name);
    }

    // Apply sorting
    users = sortUsers(users);

    var total = users.length;
    var visibleUsers = users.slice(0, settings.maxVisible);
    var extraCount = Math.max(0, total - settings.maxVisible);
    var extraUsers = users.slice(settings.maxVisible);

    // Build group container
    var $group = $('<div class="avatar-group avatar-group--stacked"></div>').css({
        '--avatar-size': settings.avatarSize + 'px',
        '--avatar-spacing': settings.avatarSpacing + 'px',
        '--avatar-bg': settings.backgroundColor,
        '--avatar-color': settings.fontColor,
        '--avatar-label-bg': settings.labelBackgroundColor,
        '--avatar-label-color': settings.labelFontColor
    });

    // Visible avatars
    visibleUsers.forEach(function (user) {
        var $avatar = $('<div class="avatar avatar--sm"></div>')
            .attr('data-tippy-content', user.name)
            .attr('title', user.name)
            .css('cursor', getUserUrl(user) ? 'pointer' : 'default');

        var $avatarContainer = $('<div class="avatar__container" role="img"></div>')
            .attr('aria-label', user.name)
            .attr('aria-hidden', 'false');

        var initials = getDisplayInitials(user);
        var initialsClass = 'avatar__initials avatar__initials--sm';
        if (initials.length >= 3) {
            initialsClass += ' avatar__initials--small';
        }

        var $initials = $('<span>')
            .addClass(initialsClass)
            .text(initials || '?');

        if (user.avatarUrl) {
            var showAvatarImage = function ($image) {
                $avatarContainer.removeClass('avatar__container--image-error');
                $image.removeClass('avatar__image--error').show();
                $initials.hide();
            };

            var showAvatarInitials = function ($image) {
                $avatarContainer.addClass('avatar__container--image-error');
                $image.addClass('avatar__image--error').hide();
                $initials.show();
            };

            var $img = $('<img>')
                .addClass('avatar__image avatar__image--custom')
                .attr('alt', 'avatar')
                .on('load', function () {
                    showAvatarImage($(this));
                })
                .on('error', function () {
                    showAvatarInitials($(this));
                });

            $avatarContainer.append($img);
            $avatarContainer.append($initials);

            $img.attr('src', user.avatarUrl);

            if ($img[0] && $img[0].complete) {
                if ($img[0].naturalWidth > 0) {
                    showAvatarImage($img);
                } else {
                    showAvatarInitials($img);
                }
            }
        } else {
            $avatarContainer.append($initials);
        }

        $avatar.append($avatarContainer);

        // Click / key handlers for avatar
        var userUrl = getUserUrl(user);
        if (userUrl) {
            $avatar.on('click', function (e) {
                e.preventDefault();
                handleAvatarClick(userUrl);
            });
            $avatar.attr('tabindex', 0)
                .attr('role', 'button')
                .on('keydown', function (e) {
                    if (e.key === 'Enter' || e.key === ' ') {
                        e.preventDefault();
                        handleAvatarClick(userUrl);
                    }
                });
        }

        $group.append($avatar);
    });

    // "+X" avatar
    if (extraCount > 0) {
        var $more = $('<div class="avatar avatar--sm"></div>')
            .attr('data-tippy-content', 'More ' + pluralize(settings.label, extraCount))
            .css('cursor', settings.onMoreClick ? 'pointer' : 'default');
        var $moreContainer = $('<div class="avatar__container avatar__container--label" aria-hidden="true"></div>');
        var moreText = '+' + extraCount;
        var moreInitialsClass = 'avatar__initials avatar__initials--sm';
        if (moreText.length >= 3) {
            moreInitialsClass += ' avatar__initials--small';
        }
        $moreContainer.append(
            $('<span>')
                .addClass(moreInitialsClass)
                .text(moreText)
        );
        $more.append($moreContainer);

        if (settings.onMoreClick) {
            $more.on('click', function (e) {
                e.preventDefault();
                settings.onMoreClick(extraUsers, e);
            });
            $more.attr('tabindex', 0)
                .attr('role', 'button')
                .on('keydown', function (e) {
                    if (e.key === 'Enter' || e.key === ' ') {
                        e.preventDefault();
                        settings.onMoreClick(extraUsers, e);
                    }
                });
        }

        $group.append($more);
    }

    // Label
    if (settings.showLabel) {
        var labelText = total + ' ' + pluralize(settings.label, total);
        var $label = $('<span>')
            .addClass('avatar-group__text avatar-group__text--stacked')
            .text(labelText)
            .css('cursor', settings.onLabelClick ? 'pointer' : 'default');

        if (settings.onLabelClick) {
            $label.on('click', function (e) {
                e.preventDefault();
                settings.onLabelClick(users, e);
            });
            $label.attr('tabindex', 0)
                .attr('role', 'button')
                .on('keydown', function (e) {
                    if (e.key === 'Enter' || e.key === ' ') {
                        e.preventDefault();
                        settings.onLabelClick(users, e);
                    }
                });
        }

        $group.append($label);
    }

    // Result object you can reuse
    var $avatars = $group.find('.avatar'); // includes normal avatars + "+X" one
    var $visibleAvatars = $avatars.slice(0, visibleUsers.length);

    return {
        $group,          // the whole group element
        $avatars,        // all avatar elements (including "+X")
        $visibleAvatars, // only visible user avatars
        users,           // all users after transform/sort
        visibleUsers,
        extraUsers
    };
}

// ----------------------------------
// Public API: load and render into container, return avatars
// ----------------------------------
function loadAndRenderAvatarGroup(options) {
    var settings = $.extend({
        dataSource: null,
        container: null,
        maxVisible: 4,
        avatarSize: 32,
        avatarSpacing: 12,
        label: 'Participant',
        emptyText: 'No participants', 
        showLabel: true,
        backgroundColor: '#e0f2fe',
        fontColor: '#075985',
        labelBackgroundColor: '#bbb',
        labelFontColor: '#fff',
        sort: null,
        userInformationUrl: null, // e.g. '/user/{id}' or function(user) { return ... }
        userInformationTarget: '_self',
        onMoreClick: null,   // function(extraUsers, event)
        onLabelClick: null,  // function(allUsers, event)
        transform: null      // function(rawData) { return mappedData; }
    }, options);

    if (!settings.dataSource || !settings.container) {
        console.error('dataSource and container are required.');
        return null;
    }

    var $container = $(settings.container);
    $container.empty().append('<div class="avatar-group__loading">Loading...</div>');

    function renderEmpty() {
        $container.empty().append(
            $('<div class="avatar-group__empty"></div>').text(settings.emptyText)
        );
        return null;
    }

    function renderError(message) {
        $container.empty().append(
            $('<div class="avatar-group__error"></div>').text(message || 'Failed to load avatars.')
        );
        return null;
    }

    function finalizeWithResult(result) {
        if (!result || !result.users || result.users.length === 0) {
            return renderEmpty();
        }

        $container.empty().append(result.$group);

        // Initialize Tippy.js tooltips for all avatars in this group
        if (typeof tippy === "function") {
            tippy(result.$group.find('.avatar').toArray(), {
                theme: 'light-border',
                animation: 'shift-away',
                delay: [100, 0],
                placement: 'top'
            });
        }

        // Return the result so caller can access avatars
        return result;
    }

    // ----------------------
    // Synchronous data source (array)
    // ----------------------
    if (Array.isArray(settings.dataSource)) {
        // Build avatars immediately
        var result = buildAvatarElements(settings.dataSource, settings);
        return finalizeWithResult(result);
    }

    // ----------------------
    // Asynchronous data source (AJAX)
    // ----------------------
    if (typeof settings.dataSource === 'object' && settings.dataSource.url) {
        // Return a Promise so caller can get the avatars when ready
        return $.ajax({
            url: settings.dataSource.url,
            method: settings.dataSource.method || 'GET',
            data: settings.dataSource.data || {},
            dataType: 'json'
        }).then(function (users) {
            // If no users returned
            if (!users || !users.length) {
                return renderEmpty();
            }
            var result = buildAvatarElements(users, settings);
            return finalizeWithResult(result);
        }).catch(function (xhr, status, error) {
            return renderError('Failed to load avatars.');
        });
    }

    // Invalid dataSource
    return renderError('Invalid dataSource provided.');
}

// ----------------------------------
// Example usage
// ----------------------------------
// 1) Synchronous (array):
// var res = loadAndRenderAvatarGroup({
//     dataSource: [
//         { id: 1, name: 'John Doe', avatarUrl: null },
//         { id: 2, name: 'Jane Smith', avatarUrl: '/img/jane.png' }
//     ],
//     container: '#avatars-container',
//     maxVisible: 4
// });
// console.log(res.$avatars);

// 2) Asynchronous (AJAX, returns a Promise):
// loadAndRenderAvatarGroup({
//     dataSource: { url: '/api/users' },
//     container: '#avatars-container',
//     maxVisible: 4
// }).then(function (res) {
//     if (res) {
//         console.log(res.$avatars);
//     }
// });