using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Pulse.Core.Entities;
using Pulse.Core.Interfaces;

namespace Pulse.Web.Hubs
{
    [HubName("chatHub")]
    public class ChatHub : Hub
    {
        private readonly IProjectChatService _projectchatService;

        public ChatHub(IProjectChatService projectchatService)
        {
            _projectchatService = projectchatService;
        }

        // Join a project room
        public void JoinRoom(string room)
        {
            System.Diagnostics.Debug.WriteLine($"JoinRoom called with room={room}, connId={Context.ConnectionId}");

            Groups.Add(Context.ConnectionId, room);

            var history = _projectchatService
                .GetByProjectAsync(room)
                .GetAwaiter()
                .GetResult()
                .OrderBy(c => c.CreatedDate);

            Clients.Caller.loadHistory(history);

            var roommeta = _projectchatService
                .GetRoomMetaAsync(room)
                .GetAwaiter()
                .GetResult();

            Clients.Caller.setRoomMeta(roommeta);
        }

        // Join the user-specific room (for personal notifications)
        public void JoinUserRoom(string userId)
        {
            System.Diagnostics.Debug.WriteLine($"JoinUserRoom called with user={userId}, connId={Context.ConnectionId}");
            Groups.Add(Context.ConnectionId, userId);

            // Upon joining, immediately send unread list + count to this user
            GetUnreadNotification(userId);
        }

        // Leave a project room (optional)
        public void LeaveRoom(string room)
        {
            Groups.Remove(Context.ConnectionId, room);
        }

        // Send message to specific project room
        public void SendMessage(string room, string userId, string displayname, string message)
        {
            var transactiondate = DateTime.Now;

            var roommeta = _projectchatService
                .GetRoomMetaAsync(room)
                .GetAwaiter()
                .GetResult();

            var chatMessage = new ProjectChat
            {
                ProjectNo = room,
                RoomName = roommeta.RoomName,
                RoomIcon = roommeta.RoomIcon,
                RoomColor = roommeta.RoomColor,
                SenderDisplayName = displayname,
                Message = message,
                CreatedBy = userId,
                CreatedDate = transactiondate
            };

            // Persist message
            _projectchatService.AddAsync(chatMessage).GetAwaiter().GetResult();

            // 1) Broadcast message to all connections in this project room
            Clients.Group(room).receiveMessage(chatMessage);

            // 2) Recompute unread totals per user in this project
            var participants = _projectchatService
                .GetParticipantsByRoomAsync(room)     // returns IList<string> of userIds
                .GetAwaiter()
                .GetResult();

            foreach (var participantUserId in participants)
            {
                var unreadTotalForUser = _projectchatService
                    .GetUnreadByUserAsync(participantUserId)
                    .GetAwaiter()
                    .GetResult()
                    .Count();

                Clients.Group(participantUserId).loadUnreadCount(unreadTotalForUser);
            }
        }

        // Notify "others in room" someone is typing
        public void UserTyping(string room, string userId, string displayName)
        {
            Clients.OthersInGroup(room).userTyping(room, userId, displayName);
        }

        // Per-room unread count (if you need it for a single room)
        public Task GetUnreadNotificationCount(string room, string readerUserId)
        {
            var notificationCount = _projectchatService
                .GetByProjectAsync(room, readerUserId)
                .GetAwaiter()
                .GetResult()
                .Count(c => c.Viewed == 0);

            return Clients.Group(room)
                .UpdateNotificationCounter(room, readerUserId, notificationCount);
        }

        // All unread messages for this user across rooms
        public void GetUnreadNotification(string userId)
        {
            var unreadmessages = _projectchatService
                .GetUnreadByUserAsync(userId)
                .GetAwaiter()
                .GetResult()
                .OrderByDescending(m => m.CreatedDate)
                .ToList();

            Clients.Caller.loadUnread(unreadmessages, unreadmessages.Count());

            // Also send total unread count if you want
            Clients.Caller.loadUnreadCount(unreadmessages.Count());
        }
    }
}