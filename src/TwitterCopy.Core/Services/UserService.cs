using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using TwitterCopy.Core.Entities;
using TwitterCopy.Core.Interfaces;

namespace TwitterCopy.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IHostingEnvironment _hostingEnvironment;

        public UserService(
            IUserRepository userRepository,
            IHostingEnvironment hostingEnvironment)
        {
            _userRepository = userRepository;
            _hostingEnvironment = hostingEnvironment;
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

        public bool CheckFollower(TwitterCopyUser user, Guid followerId)
        {
            return user.Followers.Any(f => f.FollowerId.Equals(followerId));
        }

        public async Task<string> UploadImage(IFormFile image, string folder)
        {
            var imageFileName = Guid.NewGuid().ToString().Replace("-", "") + Path.GetExtension(image.FileName);

            var imageFilePath = Path.Combine(_hostingEnvironment.WebRootPath, $"images\\{folder}", imageFileName);
            using (var stream = new FileStream(imageFilePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return imageFileName;
        }

        public void RemoveImage(string imageName, string folder)
        {
            var imageFilePath = Path.Combine(_hostingEnvironment.WebRootPath, $"images\\{folder}", imageName);
            File.Delete(imageName);
        }
    }
}
