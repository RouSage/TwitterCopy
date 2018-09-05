using System;
using System.Threading.Tasks;
using TwitterCopy.Core.Entities;

namespace TwitterCopy.Core.Interfaces
{
    public interface IUserService
    {
        Task<TwitterCopyUser> GetProfileOwnerAsync(string slug);
        Task<TwitterCopyUser> GetProfileOwnerWithFollowersAsync(string slug);
        Task<TwitterCopyUser> GetProfileOwnerWithFollowersForEditAsync(string userSlug);
        Task<TwitterCopyUser> GetUserAndFeedMainInfoAsync(string userId);
        Task UpdateUserAsync(TwitterCopyUser user);
        bool CheckFollower(TwitterCopyUser user, Guid followerId);
    }
}
