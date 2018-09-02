using System.Collections.Generic;
using System.Threading.Tasks;
using TwitterCopy.Core.Entities;
using TwitterCopy.Core.Entities.TweetAggregate;
using TwitterCopy.Core.Interfaces;

namespace TwitterCopy.Core.Services
{
    public class TweetService : ITweetService
    {
        private readonly ITweetRepository _tweetRepository;

        public TweetService(ITweetRepository tweetRepository)
        {
            _tweetRepository = tweetRepository;
        }

        public async Task AddTweet(string text, TwitterCopyUser user)
        {
            var tweet = new Tweet
            {
                Text = text,
                User = user
            };
            await _tweetRepository.AddAsync(tweet);
        }

        public async Task DeleteTweet(Tweet tweetToDelete)
        {
            await _tweetRepository.DeleteAsync(tweetToDelete);
        }

        public async Task<Tweet> GetTweetAsync(int tweetId)
        {
            return await _tweetRepository.GetByIdAsync(tweetId);
        }

        /// <summary>
        /// Always returns Tweet with the User
        /// </summary>
        /// <param name="tweetId"></param>
        /// <returns></returns>
        public async Task<Tweet> GetTweetWithAuthor(int tweetId)
        {
            return await _tweetRepository.GetTweetWithUserAsync(tweetId);
        }

        public async Task<List<Tweet>> GetUserTweetsAsync(string userId)
        {
            return await _tweetRepository.GetTweetsByUserIdAsync(userId);
        }
    }
}
