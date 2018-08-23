using System.Threading.Tasks;
using TwitterCopy.Core.Entities.TweetAggregate;

namespace TwitterCopy.Core.Interfaces
{
    public interface ITweetService
    {
        Task AddLikeAsync(int tweetId, Like like);
        Task AddRetweetAsync(int tweetId, Retweet retweet);
    }
}
