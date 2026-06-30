// ========================
// ChatView.js
// ========================

(function () {
    const hub = $.connection ?.chatHub;
    if (!hub) {
        console.error('SignalR chatHub not available for ChatView');
        return;
    }

    // DOM
    const roomListEl = document.getElementById('roomList');
    const roomSearchEl = document.getElementById('roomSearch');
    const chatMessagesEl = document.getElementById('chatMessages');
    const typingIndicatorEl = document.getElementById('typingIndicator');
    const chatInputEl = document.getElementById('chatInput');
    const chatSubmitBtn = document.getElementById('chatSubmit');
    const chatRoomNameEl = document.getElementById('chatRoomName');
    const chatRoomSubtitleEl = document.getElementById('chatRoomSubtitle');
    const chatRoomIconEl = document.getElementById('chatRoomIcon');

    // State
    let rooms = [];
    let filteredRooms = [];
    let currentRoomId = null;
    let currentRoomMeta = null;
    let currentUserRoom = null;

    const typingTimeouts = {};
    const typingDisplayDuration = 3000;

    // ----- Helpers -----
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

    if (typeof window.formatDate !== 'function') {
        window.formatDate = function (date, fmt) {
            const d = new Date(date);
            return d.toLocaleString();
        };
    }

    // ----- Room list -----
    function renderRooms(list) {
        if (!roomListEl) return;
        roomListEl.innerHTML = '';

        list.forEach(r => {
            const item = document.createElement('div');
            item.className = 'chat-room-item';
            item.setAttribute('data-room-id', r.room);

            const color = r.roomColor || '#0d6efd';
            const iconClass = r.roomIcon || 'bi bi-chat-dots-fill';

            item.innerHTML = `
                <div class="chat-room-icon" style="background-color:${escapeHtml(color)};">
                    <i class="${escapeHtml(iconClass)}"></i>
                </div>
                <div class="chat-room-info">
                    <div class="chat-room-name">${escapeHtml(r.roomName || r.room)}</div>
                    <div class="chat-room-subtitle text-muted">
                        ${escapeHtml(r.lastMessagePreview || r.room)}
                    </div>
                </div>
                ${
                r.unreadCount && r.unreadCount > 0
                    ? `<div class="chat-room-badge badge bg-danger rounded-pill">${r.unreadCount}</div>`
                    : ''
                }
            `;

            roomListEl.appendChild(item);
        });

        updateActiveRoomHighlight();
    }

    function updateActiveRoomHighlight() {
        if (!roomListEl) return;
        Array.from(roomListEl.querySelectorAll('.chat-room-item')).forEach(el => {
            const id = el.getAttribute('data-room-id');
            if (id === currentRoomId) el.classList.add('active');
            else el.classList.remove('active');
        });
    }

    function loadRooms() {
        $.getJSON(window.getApiRootPath() + '/api/chat/rooms', function (data) {
            rooms = data || [];
            filteredRooms = rooms.slice();
            renderRooms(filteredRooms);
        }).fail(err => {
            console.error('Failed to load rooms', err);
        });
    }

    if (roomSearchEl) {
        roomSearchEl.addEventListener('input', function () {
            const term = this.value.toLowerCase();
            filteredRooms = rooms.filter(r =>
                (r.roomName || r.room || '').toLowerCase().includes(term)
            );
            renderRooms(filteredRooms);
        });
    }

    if (roomListEl) {
        roomListEl.addEventListener('click', function (e) {
            const item = e.target.closest('.chat-room-item');
            if (!item) return;

            const roomId = item.getAttribute('data-room-id');
            if (!roomId) return;

            const roomMeta = rooms.find(r => r.room === roomId) || { room: roomId };
            selectRoom(roomId, roomMeta);
        });
    }

    // ----- Typing UI -----
    function showTypingIndicator(userId, displayName) {
        if (!typingIndicatorEl) return;
        const safeName = escapeHtml(displayName || 'Someone');

        typingIndicatorEl.innerHTML = `${safeName} is typing…`;
        typingIndicatorEl.style.display = 'block';

        if (typingTimeouts[userId]) clearTimeout(typingTimeouts[userId]);
        typingTimeouts[userId] = setTimeout(() => {
            hideTypingIndicatorForUser(userId);
        }, typingDisplayDuration);
    }

    function hideTypingIndicatorForUser(userId) {
        if (!typingIndicatorEl) return;

        if (typingTimeouts[userId]) {
            clearTimeout(typingTimeouts[userId]);
            delete typingTimeouts[userId];
        }

        if (Object.keys(typingTimeouts).length === 0) {
            typingIndicatorEl.style.display = 'none';
            typingIndicatorEl.innerHTML = '';
        }
    }

    function hideLocalTypingIndicator() {
        const localUserId = window.user ?.EmployeeId;
        if (!localUserId) return;
        hideTypingIndicatorForUser(localUserId);
    }

    // ----- Bubbles -----
    function renderReplyHtml(options) {
        const { sender, alias, timestamp, messageHtml, isYou } = options;
        const fulltimestamp = formatDate(timestamp, "MMM DD, YYYY HH:mm");
        const displaytimestamp = formatDate(timestamp, "DD MMM HH:mm");

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
                    ${escapeHtml(alias)}
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
                    ${escapeHtml(alias)}
                </div>
                <div class="direct-chat-text">${messageHtml}</div>
            </div>`;
        }
    }

    function addMessage(message) {
        if (!chatMessagesEl) return;

        const displayName = message.SenderDisplayName;
        const textmessage = message.Message;
        const alias = getAliasFromUser(displayName);
        const isYou = displayName === (window.user ?.DisplayName);

        if (message.CreatedBy) hideTypingIndicatorForUser(message.CreatedBy);

        const wrapper = document.createElement('div');
        wrapper.innerHTML = renderReplyHtml({
            sender: displayName,
            alias,
            timestamp: message.CreatedDate,
            messageHtml: escapeHtml(textmessage),
            isYou
        });

        chatMessagesEl.appendChild(wrapper.firstElementChild);
        setTimeout(() => {
            chatMessagesEl.scrollTop = chatMessagesEl.scrollHeight;
        }, 0);
    }

    function clearMessages() {
        if (!chatMessagesEl) return;
        chatMessagesEl.innerHTML = '';
    }

    // ----- Room selection & join -----
    function selectRoom(roomId, roomMetaFromList) {
        if (currentRoomId === roomId) return;

        currentRoomId = roomId;
        currentRoomMeta = roomMetaFromList || null;
        updateActiveRoomHighlight();
        updateRoomHeader();

        joinRoom(roomId);
    }

    function updateRoomHeader() {
        if (!chatRoomNameEl) return;

        if (!currentRoomId) {
            chatRoomNameEl.textContent = 'Select a project to start chatting';
            chatRoomSubtitleEl.textContent = '';
            chatRoomIconEl.style.display = 'none';
            return;
        }

        const name = currentRoomMeta ?.roomName || currentRoomMeta ?.RoomName || currentRoomId;
        const iconClass = currentRoomMeta ?.roomIcon || currentRoomMeta ?.RoomIcon || 'bi bi-chat-dots-fill';
        const color = currentRoomMeta ?.roomColor || currentRoomMeta ?.RoomColor || '#0d6efd';

        chatRoomNameEl.textContent = name;
        chatRoomSubtitleEl.textContent = currentRoomId;

        chatRoomIconEl.innerHTML = `<i class="${escapeHtml(iconClass)}"></i>`;
        chatRoomIconEl.style.backgroundColor = color;
        chatRoomIconEl.style.display = 'flex';
    }

    function joinRoom(room) {
        if (!hub.server) {
            console.error('SignalR chat server not available.');
            return;
        }
        if (!room) return;

        try {
            hub.server.joinRoom(room);
        } catch (err) {
            console.error('joinRoom failed', err);
        }

        const empId = window.user ?.EmployeeId;
        if (empId && hub.server.joinUserRoom) {
            try {
                hub.server.joinUserRoom(empId);
            } catch (e) {
                console.error('joinUserRoom failed', e);
            }
        }

        clearMessages();
    }

    // ----- Sending + typing -----
    function sendMessage() {
        if (!chatInputEl) return;
        const msg = chatInputEl.value.trim();
        if (!msg || !currentRoomId) return;

        if (!hub.server ?.sendMessage) return;

        const empId = window.user ?.EmployeeId;
        const displayName = window.user ?.DisplayName || 'You';
        if (!empId) return;

        try {
            hub.server.sendMessage(currentRoomId, empId, displayName, msg);
        } catch (err) {
            console.error('sendMessage failed', err);
        }

        hideLocalTypingIndicator();
        chatInputEl.value = '';
        chatInputEl.focus();
    }

    function sendTypingNotification() {
        if (!currentRoomId) return;
        if (!hub.server ?.userTyping) return;

        const empId = window.user ?.EmployeeId;
        const displayName = window.user ?.DisplayName;
        if (!empId || !displayName) return;

        try {
            hub.server.userTyping(currentRoomId, empId, displayName);
        } catch (e) {
            console.warn('userTyping call failed', e);
        }
    }

    // ----- SignalR callbacks (chaining with NavbarChat) -----
    const prevReceive = hub.client.receiveMessage;
    const prevLoadHistory = hub.client.loadHistory;
    const prevSetRoomMeta = hub.client.setRoomMeta;
    const prevUserTyping = hub.client.userTyping;
    const prevLoadUnread = hub.client.loadUnread;

    hub.client.receiveMessage = function (message) {
        const room = message.ProjectNo;
        if (room === currentRoomId) {
            addMessage(message);
        }

        // update left list preview/unread
        const r = rooms.find(x => x.room === room);
        if (r) {
            r.lastMessagePreview = message.Message;
            if (room !== currentRoomId) {
                r.unreadCount = (r.unreadCount || 0) + 1;
            } else {
                r.unreadCount = 0;
            }
            renderRooms(filteredRooms);
        }

        if (typeof prevReceive === 'function') prevReceive(message);
    };

    hub.client.loadHistory = function (messages) {
        clearMessages();
        (messages || []).forEach(addMessage);
        if (typeof prevLoadHistory === 'function') prevLoadHistory(messages);
    };

    hub.client.setRoomMeta = function (meta) {
        currentRoomMeta = meta;
        updateRoomHeader();
        if (typeof prevSetRoomMeta === 'function') prevSetRoomMeta(meta);
    };

    hub.client.userTyping = function (room, userId, displayName) {
        if (room === currentRoomId && userId !== window.user ?.EmployeeId) {
            showTypingIndicator(userId, displayName);
        }
        if (typeof prevUserTyping === 'function') prevUserTyping(room, userId, displayName);
    };

    hub.client.loadUnread = function (messages, count) {
        // This page mainly uses /api/chat/rooms for left list,
        // but if you want to sync, you can integrate here.
        if (typeof prevLoadUnread === 'function') prevLoadUnread(messages, count);
    };

    // ----- Init -----
    function init() {
        loadRooms();

        if (chatSubmitBtn) {
            $(chatSubmitBtn)
                .off('click.chatView')
                .on('click.chatView', function (e) {
                    e.preventDefault();
                    sendMessage();
                });
        }

        if (chatInputEl) {
            chatInputEl.addEventListener('keydown', function (e) {
                if (e.key === 'Enter' && !e.shiftKey) {
                    e.preventDefault();
                    sendMessage();
                }
            });

            chatInputEl.addEventListener('input', function () {
                if (chatInputEl.value.trim().length > 0) {
                    sendTypingNotification();
                } else {
                    hideLocalTypingIndicator();
                }
            });
        }
    }

    if ($.connection ?.hub && $.connection.hub.state === $.signalR.connectionState.connected) {
        init();
    } else if ($.connection ?.hub) {
        $.connection.hub.start().done(init);
    }
})();