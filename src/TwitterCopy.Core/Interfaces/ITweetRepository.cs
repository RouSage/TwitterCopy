using System.Collections.Generic;
using System.Threading.Tasks;
using TwitterCopy.Core.Entities.TweetAggregate;

namespace TwitterCopy.Core.Interfaces
{
    public interface ITweetRepository : IRepository<Tweet>
    {
        Task<IEnumerable<Tweet>> GetTweetsByUserIdAsync(string userId);
    }
}
