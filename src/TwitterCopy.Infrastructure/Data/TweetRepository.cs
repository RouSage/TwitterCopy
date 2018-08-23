using System;
using System.Collections.Generic;
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

        public Task<IEnumerable<Tweet>> GetTweetsByUserId(string userId)
        {
            throw new NotImplementedException();
        }
    }
}
