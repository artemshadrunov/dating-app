using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    public interface IPresenceTracker
    {
        Task UserConnected(string username, string connectionId);
        
        Task UserDisconnected(string username, string connectionId);

        Task<string[]> GetOnlineUsers();
    }


    // WARNING: Suits only for single server. Use Reddis for multiple servers.
    public class SimplePresenceTracker : IPresenceTracker
    {
        private static readonly Dictionary<string, List<string>> _onlineUsers = new Dictionary<string, List<string>>();

        public Task UserConnected(string username, string connectionId)
        {
            lock(_onlineUsers)
            {
                if (_onlineUsers.ContainsKey(username))
                {
                    _onlineUsers[username].Add(connectionId);
                }
                else
                {
                    _onlineUsers.Add(username, new List<string>(){connectionId});
                }
            }

            return Task.CompletedTask;
        }

        public Task UserDisconnected(string username, string connectionId)
        {
            lock(_onlineUsers)
            {
                if (!_onlineUsers.ContainsKey(username))
                    return Task.CompletedTask;

                var connectionIds = _onlineUsers[username];
                connectionIds.Remove(connectionId);
                if (connectionIds.Count == 0)
                    _onlineUsers.Remove(username);
            }

            return Task.CompletedTask;
        }

        public Task<string[]> GetOnlineUsers()
        {
            lock(_onlineUsers)
            {
                return Task.FromResult(_onlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray());
            }
        }
    }
}