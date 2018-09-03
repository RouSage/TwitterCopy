using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterCopy.Core.Entities;
using TwitterCopy.Core.Entities.TweetAggregate;
using TwitterCopy.Core.Interfaces;

namespace TwitterCopy.Core.Services
{
    public class TweetService : ITweetService
    {
        private readonly ITweetRepository _tweetRepository;
        private readonly IRepository<Like> _likeRepository;

        public TweetService(ITweetRepository tweetRepository, IRepository<Like> likeRepository)
        {
            _tweetRepository = tweetRepository;
            _likeRepository = likeRepository;
        }

        public async Task AddTweet(string text, TwitterCopyUser user)
        {
            var tweet = new Tweet
            {
                Text = text,
                User = user
            };
            _tweetRepository.Add(tweet);
            await _tweetRepository.SaveAsync();
        }

        public async Task DeleteTweet(Tweet tweetToDelete)
        {
            _tweetRepository.Delete(tweetToDelete);
            await _tweetRepository.SaveAsync();
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

        public async Task<int> UpdateLikes(int tweetId, TwitterCopyUser user)
        {
            var tweet = await _tweetRepository.GetTweetWithLikes(tweetId);

            // Apply the user and tweet object from above to the new Like
            var like = new Like
            {
                Tweet = tweet,
                User = user,
            };

            // Check if the user already has like on this tweet
            var dupe = tweet.Likes.FirstOrDefault(x => x.UserId.Equals(user.Id));
            if (dupe == null)
            {
                // If no duplicate was found
                // Add new like to the database
                _likeRepository.Add(like);
                tweet.LikeCount++;
            }
            else
            {
                // If duplicate was found then
                // Delete dupe instead of like because
                // like doesn't have Id values
                _likeRepository.Delete(dupe);
                tweet.LikeCount--;
            }

            // Update Tweet entity because LikeCount property was changed
            _tweetRepository.Update(tweet);
            await _tweetRepository.SaveAsync();

            return tweet.LikeCount;
        }
    }
}
