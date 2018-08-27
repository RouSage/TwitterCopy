using Microsoft.EntityFrameworkCore;
using System;
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
                .Include(fs => fs.Followers)
                .Include(fg => fg.Following)
                .Include(l => l.Likes)
                .Include(t => t.Tweets)
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
                .Include(fg => fg.Following)
                    .ThenInclude(fu => fu.User)
                        .ThenInclude(ft => ft.Tweets)
                .Include(fs => fs.Followers)
                .Include(t => t.Tweets)
                .FirstOrDefaultAsync(x => x.Id.ToString().Equals(userId));
        }
    }
}
