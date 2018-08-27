using System.Threading.Tasks;
using TwitterCopy.Core.Entities;

namespace TwitterCopy.Core.Interfaces
{
    public interface IUserRepository : IRepository<TwitterCopyUser>
    {
        Task<TwitterCopyUser> GetUserWithTweetsAsync(string userId);
        Task<TwitterCopyUser> GetUserWithAllInfoAsync(string slug);
        Task<TwitterCopyUser> GetUserWithFollowersAsync(string slug);
        Task<TwitterCopyUser> GetUserMainInfoWithFollowingTweetsAsync(string userId);
    }
}
