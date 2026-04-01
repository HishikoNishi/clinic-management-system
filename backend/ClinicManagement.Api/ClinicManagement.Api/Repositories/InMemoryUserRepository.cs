using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicManagement.Api.Models;

namespace ClinicManagement.Api.Repositories
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly ConcurrentDictionary<Guid, User> _users = new();

        public Task<IEnumerable<User>> GetAllAsync()
        {
            return Task.FromResult(_users.Values.AsEnumerable());
        }

        public Task<User?> GetByIdAsync(Guid id)
        {
            _users.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }

        public Task<User?> GetByUsernameAsync(string username)
        {
            var user = _users.Values.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(user);
        }

        public Task AddAsync(User user)
        {
            if (user.Id == Guid.Empty)
            {
                user.Id = Guid.NewGuid();
            }

            _users[user.Id] = user;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(User user)
        {
            if (_users.ContainsKey(user.Id))
            {
                _users[user.Id] = user;
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            _users.TryRemove(id, out _);
            return Task.CompletedTask;
        }
    }
}
