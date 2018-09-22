using System.Threading.Tasks;
using TwitterCopy.Core.Entities;

namespace TwitterCopy.Core.Interfaces
{
    public interface ITweetRepository : IRepository<Tweet>
    {
        Task<Tweet> GetTweetWithUserAsync(int tweetId);
        Task<Tweet> GetWithRepliesAsync(int tweetId);
        Task<Tweet> GetWithRepliesForEditingAsync(int tweetId);
        Task<Tweet> GetTweetWithLikesAsync(int tweetId);
        Task<Tweet> GetTweetWithRetweetsAsync(int tweetId);
        Task<Tweet> GetTweetForDeletion(int tweetId);
    }
}
