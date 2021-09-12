using System;
using System.Threading.Tasks;
using API.Extensions;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class PresenceHub : Hub
    {
        private readonly SimplePresenceTracker _tracker;

        public PresenceHub(SimplePresenceTracker tracker)
        {
            _tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {
            await _tracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);
            await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());

            var currentUsers = _tracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await _tracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);            
            await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());

            var currentUsers = _tracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers", currentUsers);

            await base.OnDisconnectedAsync(exception);
        }
    }
}