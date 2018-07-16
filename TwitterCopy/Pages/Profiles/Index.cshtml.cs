using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterCopy.Data;
using TwitterCopy.Models;

namespace TwitterCopy.Pages.Profiles
{
    public class IndexModel : PageModel
    {
        private readonly TwitterCopyContext _context;
        private readonly UserManager<TwitterCopyUser> _userManager;

        public IndexModel(UserManager<TwitterCopyUser> userManager, TwitterCopyContext context)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<TweetModel> Tweets { get; set; }

        public TwitterCopyUser ProfileUser { get; set; }

        public bool IsFollowed { get; set; }

        public async Task<IActionResult> OnGetAsync(string userName)
        {
            if (userName == null)
            {
                return NotFound();
            }

            var profileOwner = await _context.Users
                .AsNoTracking()
                .Include(f => f.Followers)
                .FirstOrDefaultAsync(u => u.UserName == userName);
            if (profileOwner == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if(currentUser == null)
            {
                return NotFound();
            }

            Tweets = await _context.Tweets
                .AsNoTracking()
                .Where(t => t.UserId == profileOwner.Id)
                .Select(x => new TweetModel
                {
                    Id = x.Id,
                    AuthorName = x.User.UserName,
                    AuthorSlug = x.User.Slug,
                    PostedOn = x.PostedOn,
                    Text = x.Text,
                    LikeCount = x.LikeCount
                })
                .OrderByDescending(p => p.PostedOn)
                .ToListAsync();

            ProfileUser = profileOwner;

            IsFollowed = profileOwner.Followers
                .Any(x => x.FollowerId.Equals(currentUser.Id));

            return Page();
        }
    }
}
