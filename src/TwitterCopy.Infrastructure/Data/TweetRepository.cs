using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterCopy.Core.Entities.TweetAggregate;
using TwitterCopy.Core.Interfaces;

namespace TwitterCopy.Infrastructure.Data
{
    public class TweetRepository : EfRepository<Tweet>, ITweetRepository
    {
        public TweetRepository(TwitterCopyContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Tweet>> GetTweetsByUserIdAsync(string userId)
        {
            return await _context.Tweets
                .AsNoTracking()
                .Where(x => x.UserId.ToString().Equals(userId))
                .ToListAsync();
        }
    }
}
