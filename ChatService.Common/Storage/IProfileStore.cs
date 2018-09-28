using System;
using ChatService.DataContracts;
using System.Threading.Tasks;

namespace ChatService.Storage
{
    public interface IProfileStore
    {
        /// <summary>
        /// Will fetch the profile associated with the given username from storage
        /// </summary>
        /// <returns>The user profile</returns>
        /// <exception cref="StorageErrorException">If we fail to retrieve the profile from storage</exception>
        /// <exception cref="ProfileNotFoundException">If the profile does not exists</exception>
        /// <exception cref="ArgumentNullException">If the given username is null or empty</exception>
        Task<UserProfile> GetProfile(string username);

        /// <summary>
        /// Adds the user profile to storage if it does not exists.
        /// </summary>
        /// <param name="profile"></param>
        /// <exception cref="DuplicateProfileException"> If the user profile already exists</exception>
        /// <exception cref="StorageErrorException">If we fail to write the profile to storage</exception>
        /// <exception cref="ArgumentNullException">If the given profile is null </exception>
        /// <exception cref="ArgumentException">If the given profile is invalid</exception>
        Task AddProfile(UserProfile profile);

        /// <summary>
        /// Will update the user profile in storage. If the profile does not exists, an exception will be thrown.
        /// </summary>
        /// <param name="profile"></param>
        /// <exception cref="StorageErrorException">If we fail to write the profile to storage</exception>
        /// <exception cref="StorageConflictException">Race condition detected</exception>
        /// <exception cref="ProfileNotFoundException">If the profile does not exists</exception>
        /// <exception cref="ArgumentNullException">If the given profile is null </exception>
        /// <exception cref="ArgumentException">If the given profile is invalid</exception>
        Task UpdateProfile(UserProfile profile);

        /// <summary>
        /// Will delete the profile associated with the given username from storage if it exists.
        /// </summary>
        /// <exception cref="StorageErrorException">If we fail to retrieve the profile from storage</exception>
        /// <exception cref="ProfileNotFoundException">If the profile does not exists</exception>
        /// <exception cref="StorageConflictException">Race condition detected</exception>
        /// <exception cref="ArgumentNullException">If the given username is null or empty</exception>
        Task<bool> TryDelete(string username);
    }
}
