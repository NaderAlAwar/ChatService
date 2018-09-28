using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ChatService.DataContracts;

namespace ChatService.Storage.Memory
{
    public class InMemoryProfileStore : IProfileStore
    {
        private readonly ConcurrentDictionary<string, UserProfile> data = new ConcurrentDictionary<string, UserProfile>();
        public Task AddProfile(UserProfile profile)
        {
            ValidateArgument(profile);

            if (!data.TryAdd(profile.Username, profile))
            {
                throw new DuplicateProfileException($"Cannot create a profile for user {profile.Username} because it already exists");
            }
            return Task.CompletedTask;
        }

        public Task<UserProfile> GetProfile(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            if (!data.TryGetValue(username, out UserProfile profile))
            {
                throw new ProfileNotFoundException($"Could not find a profile for user {username}");
            }
            return Task.FromResult(profile);
        }

        public Task UpdateProfile(UserProfile profile)
        {
            ValidateArgument(profile);

            data[profile.Username] = profile;
            return Task.CompletedTask;
        }

        public Task<bool> TryDelete(string username)
        {
            bool found = data.TryRemove(username, out _);
            return Task.FromResult(found);
        }

        private void ValidateArgument(UserProfile profile)
        {
            ProfileUtils.Validate(profile);
        }
    }
}
