using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TwitterCopy.Core.Entities;
using TwitterCopy.Core.Interfaces;

namespace TwitterCopy.Infrastructure.Data
{
    public class UserRepository : EfRepository<TwitterCopyUser>, IUserRepository
    {
        public UserRepository(TwitterCopyContext context)
            : base(context)
        {
        }

        public Task<TwitterCopyUser> GetUserWithTweetsAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<TwitterCopyUser> GetUserWithAllInfoAsync(string slug)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(t => t.Tweets).ThenInclude(rf => rf.RepliesFrom)
                .Include(t => t.Tweets).ThenInclude(rt => rt.RepliesTo).ThenInclude(rtt => rtt.ReplyTo).ThenInclude(rtu => rtu.User)
                .Include(l => l.Likes).ThenInclude(lt => lt.Tweet)
                .Include(rt => rt.Retweets).ThenInclude(rtt => rtt.Tweet).ThenInclude(tu => tu.User)
                .Include(rt => rt.Retweets).ThenInclude(rtt => rtt.Tweet).ThenInclude(rf => rf.RepliesFrom)
                .Include(fs => fs.Followers).ThenInclude(u => u.Follower)
                .Include(fg => fg.Following).ThenInclude(u => u.User)
                .FirstOrDefaultAsync(u => u.Slug.Equals(slug));
        }

        public async Task<TwitterCopyUser> GetUserWithFollowersAsync(string slug)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(fs => fs.Followers)
                .FirstOrDefaultAsync(s => s.Slug.Equals(slug));
        }

        public async Task<TwitterCopyUser> GetUserMainInfoWithFollowingTweetsAsync(string userId)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(t => t.Tweets).ThenInclude(l => l.Retweets)
                .Include(t => t.Tweets).ThenInclude(l => l.Likes)
                .Include(t => t.Tweets).ThenInclude(rf => rf.RepliesFrom)
                .Include(t => t.Tweets).ThenInclude(rt => rt.RepliesTo).ThenInclude(rtt => rtt.ReplyTo).ThenInclude(rtu => rtu.User)
                .Include(rt => rt.Retweets).ThenInclude(rtt => rtt.Tweet).ThenInclude(tu => tu.User)
                .Include(fg => fg.Following).ThenInclude(fu => fu.User).ThenInclude(ft => ft.Tweets).ThenInclude(rf => rf.RepliesFrom)
                .Include(fg => fg.Following).ThenInclude(fu => fu.User).ThenInclude(ft => ft.Tweets).ThenInclude(rf => rf.RepliesTo).ThenInclude(rtt => rtt.ReplyTo)
                .Include(fg => fg.Following).ThenInclude(fu => fu.User).ThenInclude(ft => ft.Tweets).ThenInclude(l => l.Likes)
                .Include(fs => fs.Followers)
                .FirstOrDefaultAsync(x => x.Id.ToString().Equals(userId));
        }

        public async Task<TwitterCopyUser> GetUserWithFollowersForEditAsync(string userSlug)
        {
            return await _context.Users
                .Include(fs => fs.Followers)
                .FirstOrDefaultAsync(s => s.Slug.Equals(userSlug));
        }
    }
}
