using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterCopy.Core.Entities;
using TwitterCopy.Core.Interfaces;

namespace TwitterCopy.Infrastructure.Data
{
    public class TweetRepository : EfRepository<Tweet>, ITweetRepository
    {
        public TweetRepository(TwitterCopyContext context) : base(context)
        {
        }

        public async Task<Tweet> GetTweetWithLikesAsync(int tweetId)
        {
            return await _context.Tweets
                .Include(l => l.Likes)
                .FirstOrDefaultAsync(x => x.Id == tweetId);
        }

        public async Task<Tweet> GetTweetWithRetweetsAsync(int tweetId)
        {
            return await _context.Tweets
                .Include(r => r.Retweets)
                .FirstOrDefaultAsync(x => x.Id == tweetId);
        }

        public async Task<Tweet> GetWithRepliesAsync(int tweetId)
        {
            return await _context.Tweets
                .AsNoTracking()
                .Include(u => u.User)
                .Include(l => l.Likes)
                .Include(rt => rt.Retweets)
                .Include(r => r.RepliesFrom).ThenInclude(rf => rf.ReplyFrom).ThenInclude(tu => tu.User)
                .Include(r => r.RepliesFrom).ThenInclude(rf => rf.ReplyFrom).ThenInclude(l => l.Likes)
                .Include(r => r.RepliesFrom).ThenInclude(rf => rf.ReplyFrom).ThenInclude(rt => rt.Retweets)
                .Include(r => r.RepliesFrom).ThenInclude(rf => rf.ReplyFrom).ThenInclude(rt => rt.RepliesFrom)
                .Include(r => r.RepliesTo).ThenInclude(rt => rt.ReplyTo).ThenInclude(tu => tu.User)
                .Include(r => r.RepliesTo).ThenInclude(rt => rt.ReplyTo).ThenInclude(l => l.Likes)
                .Include(r => r.RepliesTo).ThenInclude(rt => rt.ReplyTo).ThenInclude(rt => rt.Retweets)
                .Include(r => r.RepliesTo).ThenInclude(rt => rt.ReplyTo).ThenInclude(rf => rf.RepliesFrom)
                .FirstOrDefaultAsync(x => x.Id == tweetId);
        }

        public async Task<Tweet> GetWithRepliesForEditingAsync(int tweetId)
        {
            return await _context.Tweets
                .Include(u => u.User)
                .Include(r => r.RepliesFrom)
                .Include(r => r.RepliesTo).ThenInclude(rt => rt.ReplyTo)
                .FirstOrDefaultAsync(x => x.Id == tweetId);
        }

        public async Task<Tweet> GetAsync(int tweetId)
        {
            return await _context.Tweets
                .AsNoTracking()
                .Include(u => u.User)
                .FirstOrDefaultAsync(t => t.Id == tweetId);
        }

        public async Task<Tweet> GetTweetForDeletion(int tweetId)
        {
            return await _context.Tweets
                .Include(r => r.Retweets)
                .Include(rt => rt.RepliesTo)
                .Include(rf => rf.RepliesFrom)
                .FirstOrDefaultAsync(t => t.Id == tweetId);
        }
    }
}
