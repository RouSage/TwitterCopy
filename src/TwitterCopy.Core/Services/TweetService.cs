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

        /// <summary>
        /// Creates new Tweet entity from 'replyText' and
        /// populates RepliesTo property with 'replyTo',
        /// and with its previous replies.
        /// </summary>
        /// <param name="replyText"></param>
        /// <param name="user"></param>
        /// <param name="replyTo"></param>
        /// <returns></returns>
        public async Task AddReplyAsync(string replyText, TwitterCopyUser user, Tweet replyTo)
        {
            var replyFrom = new Tweet
            {
                Text = replyText,
                User = user,
                RepliesTo = new List<TweetToTweet>()
            };
            
            // Add main replyTo
            replyFrom.RepliesTo.Add(new TweetToTweet
            {
                ReplyFrom = replyFrom,
                ReplyTo = replyTo
            });
            // Add reply to the RepliesTo property of the replyTo tweet
            // so that you reply not to one tweet but
            // to the whole 'conversation'
            foreach (var tweet in replyTo.RepliesTo)
            {
                replyFrom.RepliesTo.Add(new TweetToTweet
                {
                    ReplyFrom = replyFrom,
                    ReplyTo = tweet.ReplyTo
                });
            }

            _tweetRepository.Add(replyFrom);
            await _tweetRepository.SaveAsync();
        }

        /// <summary>
        /// Creates a new Tweet entity and saves it to the database
        /// </summary>
        /// <param name="text"></param>
        /// <param name="user"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Removes a Tweet entity from the database and breaks all
        /// relations for 'RepliesFrom' and 'RepliesTo' properties
        /// </summary>
        /// <param name="tweetToDelete"></param>
        /// <returns></returns>
        public async Task DeleteTweet(Tweet tweetToDelete)
        {
            if(tweetToDelete.RepliesTo.Count > 0)
            {
                tweetToDelete.RepliesTo.Clear();
            }
            if(tweetToDelete.RepliesFrom.Count > 0)
            {
                tweetToDelete.RepliesFrom.Clear();
            }
            _tweetRepository.Delete(tweetToDelete);
            await _tweetRepository.SaveAsync();
        }

        public async Task<Tweet> GetTweetForDeletion(int tweetId)
        {
            return await _tweetRepository.GetTweetForDeletion(tweetId);
        }

        /// <summary>
        /// Always returns a Tweet with the User
        /// </summary>
        /// <param name="tweetId"></param>
        /// <returns></returns>
        public async Task<Tweet> GetTweetAsync(int tweetId)
        {
            return await _tweetRepository.GetAsync(tweetId);
        }

        /// <summary>
        /// Returns a Tweet with all info about replies
        /// </summary>
        /// <param name="tweetId"></param>
        /// <returns></returns>
        public async Task<Tweet> GetTweetWithRepliesAsync(int tweetId)
        {
            return await _tweetRepository.GetWithRepliesAsync(tweetId);
        }

        /// <summary>
        /// Returns a Tweet with needed info about replies for editing
        /// </summary>
        /// <param name="tweetId"></param>
        /// <returns></returns>
        public async Task<Tweet> GetTweetWithRepliesForEditingAsync(int tweetId)
        {
            return await _tweetRepository.GetWithRepliesForEditingAsync(tweetId);
        }

        /// <summary>
        /// Remove a 'Like' from the Tweet if it's already exists
        /// or add new if it's not
        /// </summary>
        /// <param name="tweetId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Remove a 'Retweet' from the Tweet if it's already exists
        /// or add new if it's not
        /// </summary>
        /// <param name="tweetId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
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
