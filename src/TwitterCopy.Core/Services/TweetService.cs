using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterCopy.Core.Entities;
using TwitterCopy.Core.Interfaces;

namespace TwitterCopy.Core.Services
{
    public class TweetService : ITweetService
    {
        private readonly ITweetRepository _tweetRepository;
        private readonly IRepository<Like> _likeRepository;
        private readonly IRepository<Retweet> _retweetRepository;

        public TweetService(
            ITweetRepository tweetRepository,
            IRepository<Like> likeRepository,
            IRepository<Retweet> retweetRepository)
        {
            _tweetRepository = tweetRepository;
            _likeRepository = likeRepository;
            _retweetRepository = retweetRepository;
        }

        public async Task AddReply(string replyText, TwitterCopyUser user, Tweet replyTo)
        {
            var replyFrom = new Tweet
            {
                Text = replyText,
                User = user
            };
            _tweetRepository.Add(replyFrom);

            var reply = new TweetToTweet
            {
                ReplyTo = replyTo,
                ReplyFrom = replyFrom
            };

            // Automatically adds new TweetToTweet entity to the replyFrom's property RepliesTo
            replyTo.RepliesFrom.Add(reply);

            _tweetRepository.Update(replyTo);
            await _tweetRepository.SaveAsync();
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

        public List<Tweet> GetFeed(TwitterCopyUser user)
        {
            return user.Tweets
                .Concat(user.Following
                    .SelectMany(ft => ft.User.Tweets))
                .Concat(user.Retweets
                    .Select(rt => rt.Tweet))
                .ToList();
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

        public async Task<Tweet> GetTweetWithAuthorAndRepliesAsync(int tweetId)
        {
            return await _tweetRepository.GetTweetWithUserAndRepliesAsync(tweetId);
        }

        public async Task<Tweet> GetTweetWithUserAndRepliesForEditingAsync(int tweetId)
        {
            return await _tweetRepository.GetTweetWithUserAndRepliesForEditingAsync(tweetId);
        }

        public async Task<List<Tweet>> GetUserTweetsAsync(string userId)
        {
            return await _tweetRepository.GetTweetsByUserIdAsync(userId);
        }

        public async Task<int> UpdateLikes(int tweetId, TwitterCopyUser user)
        {
            var tweet = await _tweetRepository.GetTweetWithLikesAsync(tweetId);

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
            }
            else
            {
                // If duplicate was found then
                // Delete dupe instead of like because
                // like doesn't have Id values
                _likeRepository.Delete(dupe);
            }

            await _likeRepository.SaveAsync();

            return tweet.Likes.Count;
        }

        public async Task<int> UpdateRetweets(int tweetId, TwitterCopyUser user)
        {
            var tweet = await _tweetRepository.GetTweetWithRetweetsAsync(tweetId);

            var retweet = new Retweet
            {
                Tweet = tweet,
                User = user
            };

            var dupe = tweet.Retweets.FirstOrDefault(x => x.TweetId == tweet.Id && x.UserId == user.Id);
            if (dupe == null)
            {
                // If no duplicate was found
                // Add new retweet to the database
                _retweetRepository.Add(retweet);
            }
            else
            {
                // If duplicate was found
                // Delete dupe instead of retweer because
                // retweet doesn't have Id value
                _retweetRepository.Delete(dupe);
            }

            await _retweetRepository.SaveAsync();

            return tweet.Retweets.Count;
        }
    }
}
