using System.Collections.Generic;
using System.Threading.Tasks;
using TwitterCopy.Core.Entities;

namespace TwitterCopy.Core.Interfaces
{
    public interface ITweetService
    {
        Task<Tweet> GetTweetAsync(int tweetId);
        Task<Tweet> GetTweetWithRepliesAsync(int tweetId);
        Task AddTweet(string text, TwitterCopyUser user);
        Task DeleteTweet(Tweet tweetToDelete);
        Task<int> UpdateLikes(int tweetId, TwitterCopyUser user);
        Task<int> UpdateRetweets(int tweetId, TwitterCopyUser user);
        Task AddReplyAsync(string replyText, TwitterCopyUser user, Tweet replyTo);
        Task<Tweet> GetTweetWithRepliesForEditingAsync(int tweetId);
        Task<Tweet> GetTweetForDeletion(int tweetId);
    }
}
