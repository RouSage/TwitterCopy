using System.Collections.Generic;
using System.Threading.Tasks;
using TwitterCopy.Core.Entities;
using TwitterCopy.Core.Entities.TweetAggregate;

namespace TwitterCopy.Core.Interfaces
{
    public interface ITweetService
    {
        Task<List<Tweet>> GetUserTweetsAsync(string userId);
        Task<Tweet> GetTweet(int tweetId);
        Task AddTweet(string text, TwitterCopyUser user);
    }
}
