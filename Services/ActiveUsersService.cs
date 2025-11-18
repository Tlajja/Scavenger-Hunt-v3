using System.Collections.Concurrent;

namespace PhotoScavengerHunt.Services
{
    public class ActiveUsersService
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _connections
            = new();

        public int AddConnection(string userId, string connectionId)
        {
            var isNewUser = false;

            var connections = _connections.GetOrAdd(userId, _ =>
            {
                isNewUser = true;
                return new ConcurrentDictionary<string, byte>();
            });

            connections.TryAdd(connectionId, 0);

            return isNewUser ? _connections.Count : -1;
        }

        public int RemoveConnection(string userId, string connectionId)
        {
            if (!_connections.TryGetValue(userId, out var connections))
                return -1;

            connections.TryRemove(connectionId, out _);

            if (connections.IsEmpty)
            {
                _connections.TryRemove(userId, out _);
                return _connections.Count;
            }

            return -1;
        }
    }
}