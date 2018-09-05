using System.Collections.Generic;
using System.Threading.Tasks;
using TwitterCopy.Core.Entities.TweetAggregate;

namespace TwitterCopy.Core.Interfaces
{
    public interface ITweetRepository : IRepository<Tweet>
    {
        Task<List<Tweet>> GetTweetsByUserIdAsync(string userId);
        Task<Tweet> GetTweetWithUserAsync(int tweetId);
        Task<Tweet> GetTweetWithLikes(int tweetId);
        Task<Tweet> GetTweetWithRetweets(int tweetId);
    }
}
