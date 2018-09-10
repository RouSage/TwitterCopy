using System.Collections.Generic;
using System.Threading.Tasks;
using TwitterCopy.Core.Entities;

namespace TwitterCopy.Core.Interfaces
{
    public interface ITweetRepository : IRepository<Tweet>
    {
        Task<List<Tweet>> GetTweetsByUserIdAsync(string userId);
        Task<Tweet> GetTweetWithUserAsync(int tweetId);
        Task<Tweet> GetTweetWithUserAndRepliesAsync(int tweetId);
        Task<Tweet> GetTweetWithUserAndRepliesForEditingAsync(int tweetId);
        Task<Tweet> GetTweetWithLikesAsync(int tweetId);
        Task<Tweet> GetTweetWithRetweetsAsync(int tweetId);
    }
}
