using System.Threading.Tasks;
using TwitterCopy.Core.Entities;
using TwitterCopy.Core.Interfaces;

namespace TwitterCopy.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<TwitterCopyUser> GetUserAndFeedMainInfoAsync(string userId)
        {
            return await _userRepository.GetUserMainInfoWithFollowingTweetsAsync(userId);
        }

        public async Task<TwitterCopyUser> GetProfileOwnerAsync(string slug)
        {
            return await _userRepository.GetUserWithAllInfoAsync(slug);
        }

        public async Task<TwitterCopyUser> GetProfileOwnerWithFollowersAsync(string slug)
        {
            return await _userRepository.GetUserWithFollowersAsync(slug);
        }

        public async Task UpdateUserAsync(TwitterCopyUser user)
        {
            _userRepository.Update(user);
            await _userRepository.SaveAsync();
        }

        public async Task<TwitterCopyUser> GetProfileOwnerWithFollowersForEditAsync(string userSlug)
        {
            return await _userRepository.GetUserWithFollowersForEditAsync(userSlug);
        }
    }
}
