using System.Threading.Tasks;
using TwitterCopy.Core.Entities.TweetAggregate;
using TwitterCopy.Core.Interfaces;

namespace TwitterCopy.Core.Services
{
    public class TweetService : ITweetService
    {
        private readonly IRepository<Tweet> _tweetRepository;

        public TweetService(IRepository<Tweet> tweetRepository)
        {
            _tweetRepository = tweetRepository;
        }

        public async Task AddLikeAsync(int tweetId, Like like)
        {
            var tweet = await _tweetRepository.GetByIdAsync(tweetId);

            tweet.AddLike(like);

            await _tweetRepository.UpdateAsync(tweet);
        }

        public async Task AddRetweetAsync(int tweetId, Retweet retweet)
        {
            var tweet = await _tweetRepository.GetByIdAsync(tweetId);

            tweet.AddRetweet(retweet);

            await _tweetRepository.UpdateAsync(tweet);
        }
    }
}
