﻿using Microsoft.EntityFrameworkCore;
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

        public async Task<List<Tweet>> GetTweetsByUserIdAsync(string userId)
        {
            return await _context.Tweets
                .AsNoTracking()
                .Where(x => x.UserId.ToString() == userId)
                .OrderByDescending(x => x.PostedOn)
                .ToListAsync();
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

        public async Task<Tweet> GetTweetWithUserAndRepliesAsync(int tweetId)
        {
            return await _context.Tweets
                .AsNoTracking()
                .Include(u => u.User)
                .Include(r => r.RepliesFrom)
                .Include(r => r.RepliesTo)
                .FirstOrDefaultAsync(x => x.Id == tweetId);
        }

        public async Task<Tweet> GetTweetWithUserAndRepliesForEditingAsync(int tweetId)
        {
            return await _context.Tweets
                .Include(u => u.User)
                .Include(r => r.RepliesFrom)
                .Include(r => r.RepliesTo)
                .FirstOrDefaultAsync(x => x.Id == tweetId);
        }

        public async Task<Tweet> GetTweetWithUserAsync(int tweetId)
        {
            return await _context.Tweets
                .AsNoTracking()
                .Include(u => u.User)
                .FirstOrDefaultAsync(t => t.Id == tweetId);
        }
    }
}
