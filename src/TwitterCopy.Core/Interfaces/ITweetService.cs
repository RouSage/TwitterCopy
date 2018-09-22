using System.Collections.Generic;
using System.Threading.Tasks;
using TwitterCopy.Core.Entities;

namespace TwitterCopy.Core.Interfaces
{
    public interface ITweetService
    {
        Task<List<Tweet>> GetUserTweetsAsync(string userId);
        Task<Tweet> GetTweetWithAuthor(int tweetId);
        Task<Tweet> GetTweetWithAuthorAndRepliesAsync(int tweetId);
        Task AddTweet(string text, TwitterCopyUser user);
        Task<Tweet> GetTweetAsync(int tweetId);
        Task DeleteTweet(Tweet tweetToDelete);
        Task<int> UpdateLikes(int tweetId, TwitterCopyUser user);
        Task<int> UpdateRetweets(int tweetId, TwitterCopyUser user);
        Task AddReplyAsync(string replyText, TwitterCopyUser user, Tweet replyTo);
        Task<Tweet> GetTweetWithUserAndRepliesForEditingAsync(int tweetId);
        Task<Tweet> GetTweetForDeletion(int tweetId);
    }
}
