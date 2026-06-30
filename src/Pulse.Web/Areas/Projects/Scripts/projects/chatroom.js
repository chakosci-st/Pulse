// ========================
// Chatroom.js (Navbar + multi-popups)
// ========================

window.NavbarChat = (function () {
    const hub = $.connection ?.chatHub;
    if (!hub) {
        console.error('SignalR chatHub not available for NavbarChat');
        return {};
    }

    const chatUnreadMessages = document.getElementById('unreadChatRecent');
    const typingDisplayDuration = 3000;

    // room -> popup object { room, root, body, typingEl, input, closeBtn, typingTimeouts }
    const chatPopups = {};

    // Gap between popups (in px)
    const POPUP_GAP = 12;

    // -------- Helpers --------
    function escapeHtml(text) {
        if (text == null) return '';
        return String(text)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function getAliasFromUser(u) {
        if (!u) return '';
        return u
            .split(/\s+/)
            .map(part => part[0] || '')
            .join('')
            .substring(0, 3)
            .toUpperCase();
    }

    function getProfilePhotoUrl(userId) {
        if (!userId) return '';
        return `/Settings/Profile/Photo/${encodeURIComponent(userId)}`;
    }

    function renderAvatarHtml(sender, alias, senderId) {
        const photoUrl = getProfilePhotoUrl(senderId);
        const safeAlias = escapeHtml(alias);

        if (!photoUrl) {
            return `<span class="chat-avatar-alias">${safeAlias}</span>`;
        }

        return `
            <img src="${photoUrl}"
                 alt="${escapeHtml(sender || alias || 'User')}"
                 class="chat-avatar-photo"
                 onerror="this.style.display='none'; if (this.nextElementSibling) { this.nextElementSibling.style.display='flex'; }" />
            <span class="chat-avatar-alias" style="display:none;">${safeAlias}</span>
        `;
    }

    if (typeof window.formatDate !== 'function') {
        window.formatDate = function (date, fmt) {
            const d = new Date(date);
            return d.toLocaleString();
        };
    }

    // -------- Layout multiple popups side-by-side --------
    function layoutPopups() {
        // Get all visible popups in DOM order
        const popups = Array.from(document.querySelectorAll('.chat-popup-window'))
            .filter(p => p.style.display !== 'none');

        let currentRight = 20;

        popups.forEach(popupEl => {
            const rect = popupEl.getBoundingClientRect();
            const width = rect.width || 320; // fallback

            popupEl.style.position = 'fixed';
            popupEl.style.bottom = '0px';
            popupEl.style.right = currentRight + 'px';

            currentRight += width + POPUP_GAP;
        });
    }

    // -------- Bubble rendering (shared) --------
    function renderReplyHtml(options) {
        const {
            sender,
            alias,
            senderId,
            timestamp,
            messageHtml,
            isYou
        } = options;

        const fulltimestamp = formatDate(timestamp, "MMM DD, YYYY HH:mm");
        const displaytimestamp = formatDate(timestamp, "DD MMM HH:mm");
        const avatarHtml = renderAvatarHtml(sender, alias, senderId);

        if (isYou) {
            return `
            <div class="direct-chat-msg end">
                <div class="direct-chat-infos clearfix">
                    <span class="direct-chat-name float-end">${escapeHtml(sender)}</span>
                    <span class="direct-chat-timestamp float-start" title="${escapeHtml(fulltimestamp)}">
                        ${escapeHtml(displaytimestamp)}
                    </span>
                </div>
                <div class="direct-chat-img chat-avatar chat-avatar-msg me-2" title="${escapeHtml(sender)}">
                    ${avatarHtml}
                </div>
                <div class="direct-chat-text">${messageHtml}</div>
            </div>`;
        } else {
            return `
            <div class="direct-chat-msg">
                <div class="direct-chat-infos clearfix">
                    <span class="direct-chat-name float-start">${escapeHtml(sender)}</span>
                    <span class="direct-chat-timestamp float-end" title="${escapeHtml(fulltimestamp)}">
                        ${escapeHtml(displaytimestamp)}
                    </span>
                </div>
                <div class="direct-chat-img chat-avatar chat-avatar-msg me-2" title="${escapeHtml(sender)}">
                    ${avatarHtml}
                </div>
                <div class="direct-chat-text">${messageHtml}</div>
            </div>`;
        }
    }

    // -------- Popup chat --------
    function createChatPopup(room, roomMeta) {
        if (!room) return null;
        if (chatPopups[room]) return chatPopups[room];

        const popup = document.createElement('div');
        popup.className = 'chat-popup-window';
        popup.setAttribute('data-room', room);

        const titleHtml = roomMeta ?.roomName
            ? `<i class="${escapeHtml(roomMeta.roomIcon || '')}"></i> ${escapeHtml(roomMeta.roomName)}`
            : escapeHtml(room);

        popup.innerHTML = `
            <div class="chat-popup-header">
                <span class="chat-popup-title">${titleHtml}</span>
                <button type="button" class="chat-popup-close btn btn-sm btn-link text-light">&times;</button>
            </div>
            <div class="chat-popup-body">
                <div class="chat-popup-messages"></div>
                <div class="chat-popup-typing text-muted fs-7" style="display:none;"></div>
            </div>
            <div class="chat-popup-footer">
                <input type="text" class="form-control chat-popup-input" placeholder="Type a message..." />
            </div>
        `;

        document.body.appendChild(popup);

        const body = popup.querySelector('.chat-popup-messages');
        const typingEl = popup.querySelector('.chat-popup-typing');
        const input = popup.querySelector('.chat-popup-input');
        const closeBtn = popup.querySelector('.chat-popup-close');

        const popupObj = {
            room,
            root: popup,
            body,
            typingEl,
            input,
            closeBtn,
            typingTimeouts: {} // userId -> timeout
        };

        closeBtn.addEventListener('click', () => {
            popup.remove();
            delete chatPopups[room];
            layoutPopups();   // reposition remaining popups
        });

        input.addEventListener('keydown', e => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                const text = input.value.trim();
                if (text) {
                    sendPopupMessage(popupObj, text);
                }
            }
        });

        input.addEventListener('input', () => {
            if (input.value.trim().length > 0) {
                sendTypingNotificationForRoom(room);
            } else {
                hideTypingIndicatorInPopupForUser(popupObj, window.user ?.EmployeeId);
            }
        });

        chatPopups[room] = popupObj;
        return popupObj;
    }

    function openChatPopup(room, roomMeta) {
        const popup = createChatPopup(room, roomMeta);

        try {
            if (hub.server ?.joinRoom) {
                hub.server.joinRoom(room);
            }
        } catch (e) {
            console.error('joinRoom for popup failed', e);
        }

        popup.root.style.display = 'block';
        popup.input.focus();

        // Let layout settle, then position
        setTimeout(layoutPopups, 0);
    }

    function addMessageToPopup(room, message) {
        const popup = chatPopups[room];
        if (!popup) return;

        const displayName = message.SenderDisplayName;
        const textmessage = message.Message;
        const alias = getAliasFromUser(displayName);
        const senderId = message.CreatedBy || message.SenderId || message.UserId;
        const isYou = displayName === (window.user ?.DisplayName);

        if (message.CreatedBy) {
            hideTypingIndicatorInPopupForUser(popup, message.CreatedBy);
        }

        const wrapper = document.createElement('div');
        wrapper.innerHTML = renderReplyHtml({
            sender: displayName,
            alias,
            senderId,
            timestamp: message.CreatedDate,
            messageHtml: escapeHtml(textmessage),
            isYou
        });

        popup.body.appendChild(wrapper.firstElementChild);
        popup.body.scrollTop = popup.body.scrollHeight;
    }

    function showTypingIndicatorInPopup(popup, userId, displayName) {
        if (!popup || !popup.typingEl) return;

        const safeName = escapeHtml(displayName || 'Someone');
        popup.typingEl.innerHTML = `${safeName} is typing…`;
        popup.typingEl.style.display = 'block';

        if (popup.typingTimeouts[userId]) {
            clearTimeout(popup.typingTimeouts[userId]);
        }
        popup.typingTimeouts[userId] = setTimeout(() => {
            hideTypingIndicatorInPopupForUser(popup, userId);
        }, typingDisplayDuration);
    }

    function hideTypingIndicatorInPopupForUser(popup, userId) {
        if (!popup || !popup.typingEl) return;

        if (popup.typingTimeouts[userId]) {
            clearTimeout(popup.typingTimeouts[userId]);
            delete popup.typingTimeouts[userId];
        }

        if (Object.keys(popup.typingTimeouts).length === 0) {
            popup.typingEl.style.display = 'none';
            popup.typingEl.innerHTML = '';
        }
    }

    function sendPopupMessage(popup, text) {
        if (!hub.server ?.sendMessage) return;
        const empId = window.user ?.EmployeeId;
        const displayName = window.user ?.DisplayName || 'You';
        if (!empId) return;

        popup.input.value = '';
        hideTypingIndicatorInPopupForUser(popup, empId);

        try {
            hub.server.sendMessage(popup.room, empId, displayName, text);
        } catch (e) {
            console.error('sendMessage from popup failed', e);
        }
    }

    // -------- Unread dropdown --------
    function addUnreadMessage(message) {
        if (!chatUnreadMessages) return;

        const displayName = message.SenderDisplayName;
        const textmessage = message.Message;
        const timestamp = moment(message.CreatedDate).fromNow();
        const alias = getAliasFromUser(displayName);
        const senderId = message.CreatedBy || message.SenderId || message.UserId;
        const avatarHtml = renderAvatarHtml(displayName, alias, senderId);

        const wrapper = document.createElement('div');
        wrapper.classList.add('justify-content-start');
        wrapper.innerHTML = `
            <a href="/projects/${escapeHtml(message.ProjectNo)}/details" 
               class="dropdown-item unread-chat-item" 
               data-project-no="${escapeHtml(message.ProjectNo)}"
               data-room-name="${escapeHtml(message.RoomName || '')}"
               data-room-icon="${escapeHtml(message.RoomIcon || '')}"
               data-room-color="${escapeHtml(message.RoomColor || '')}">
                <div class="d-flex">
                    <div class="flex-shrink-0">
                        <div class="chat-avatar chat-avatar-medium me-2" title="${escapeHtml(displayName)}">
                            ${avatarHtml}
                        </div>
                    </div>
                    <div class="flex-grow-1">
                        <h3 class="dropdown-item-title">
                            ${escapeHtml(displayName)}
                            <span class="float-end fs-7 text-danger">
                                <i class="bi bi-star-fill"></i>
                            </span>
                        </h3>
                        <div>
                            <small>
                                ${message.RoomIcon ? `<i class="${escapeHtml(message.RoomIcon)}"></i> ` : ''}
                                ${escapeHtml(message.RoomName || message.ProjectNo)}
                            </small>
                        </div>
                        <p class="fs-7">${escapeHtml(textmessage)}</p>
                        <p class="fs-7 text-secondary">
                            <i class="bi bi-clock-fill me-1"></i> ${escapeHtml(timestamp)}
                        </p>
                    </div>
                </div>
            </a>
            <div class="dropdown-divider"></div>
        `;

        chatUnreadMessages.appendChild(wrapper);
    }

    function updateUnreadList(messages, count) {
        $('#unreadChatRecent').html('');
        const $cnt = $('#unreadChatCount');
        const $userMenuCount = $('#userMenuMessagesCount');

        if (count === 0) {
            $cnt.hide();
            if ($userMenuCount.length) {
                $userMenuCount.text('0');
            }
            return;
        }

        $cnt.show().html(count);
        if ($userMenuCount.length) {
            $userMenuCount.text(Number(count).toLocaleString('en-US'));
        }

        const maxMsgCount = 5;
        let shown = 0;

        (messages || []).forEach(msg => {
            if (shown < maxMsgCount) {
                addUnreadMessage(msg);
                shown++;
            }
        });
    }

    function requestUnread() {
        const empId = window.user ?.EmployeeId;
        if (!empId || !hub.server ?.getUnreadNotification) return;
        hub.server.getUnreadNotification(empId);
    }

    function joinUserRoom() {
        const empId = window.user ?.EmployeeId;
        if (!empId || !hub.server ?.joinUserRoom) return;
        try {
            hub.server.joinUserRoom(empId);
        } catch (e) {
            console.error('NavbarChat joinUserRoom failed', e);
        }
    }

    function sendTypingNotificationForRoom(room) {
        if (!hub.server ?.userTyping) return;
        const empId = window.user ?.EmployeeId;
        const displayName = window.user ?.DisplayName;
        if (!empId || !displayName) return;

        try {
            hub.server.userTyping(room, empId, displayName);
        } catch (e) {
            console.warn('userTyping call failed', e);
        }
    }

    // -------- Wire SignalR callbacks (chaining) --------
    const prevReceive = hub.client.receiveMessage;
    const prevLoadHistory = hub.client.loadHistory;
    const prevLoadUnread = hub.client.loadUnread;
    const prevUserTyping = hub.client.userTyping;
    const prevLoadUnreadCount = hub.client.loadUnreadCount;

    hub.client.receiveMessage = function (message) {
        const room = message.ProjectNo;
        if (chatPopups[room]) {
            addMessageToPopup(room, message);
        }
        if (typeof prevReceive === 'function') prevReceive(message);
    };

    hub.client.loadHistory = function (messages) {
        if (messages && messages.length > 0) {
            const room = messages[0].ProjectNo;
            const popup = chatPopups[room];
            if (popup) {
                popup.body.innerHTML = '';
                messages.forEach(msg => addMessageToPopup(room, msg));
            }
        }
        if (typeof prevLoadHistory === 'function') prevLoadHistory(messages);
    };

    hub.client.loadUnread = function (messages, count) {
        updateUnreadList(messages, count);
        if (typeof prevLoadUnread === 'function') prevLoadUnread(messages, count);
    };

    hub.client.userTyping = function (room, userId, displayName) {
        if (userId === window.user ?.EmployeeId) {
            if (typeof prevUserTyping === 'function') prevUserTyping(room, userId, displayName);
            return;
        }

        const popup = chatPopups[room];
        if (popup) {
            showTypingIndicatorInPopup(popup, userId, displayName);
        }
        if (typeof prevUserTyping === 'function') prevUserTyping(room, userId, displayName);
    };

    hub.client.loadUnreadCount = function (count) {
        const $cnt = $('#unreadChatCount');
        const $userMenuCount = $('#userMenuMessagesCount');
        if ($cnt.length) {
            if (!count || count === 0) {
                $cnt.hide();
            } else {
                $cnt.show().text(count);
            }
        }
        if ($userMenuCount.length) {
            $userMenuCount.text((!count || count === 0 ? 0 : Number(count)).toLocaleString('en-US'));
        }
        if (typeof prevLoadUnreadCount === 'function') prevLoadUnreadCount(count);
    };

    function init() {
        if ($.connection ?.hub && $.connection.hub.state === $.signalR.connectionState.connected) {
            joinUserRoom();
            requestUnread();
        } else if ($.connection ?.hub) {
            $.connection.hub.start().done(() => {
                joinUserRoom();
                requestUnread();
            });
        }

        if (chatUnreadMessages) {
            chatUnreadMessages.addEventListener('click', function (e) {
                const link = e.target.closest('.unread-chat-item');
                if (!link) return;

                e.preventDefault();
                const projectNo = link.getAttribute('data-project-no');
                const roomName = link.getAttribute('data-room-name');
                const roomIcon = link.getAttribute('data-room-icon');
                const roomColor = link.getAttribute('data-room-color');

                const roomMeta = {
                    room: projectNo,
                    roomName,
                    roomIcon,
                    roomColor
                };

                if (projectNo) {
                    openChatPopup(projectNo, roomMeta);
                }
            });
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    return {
        refreshUnread: requestUnread,
        openPopup: openChatPopup
    };
})();