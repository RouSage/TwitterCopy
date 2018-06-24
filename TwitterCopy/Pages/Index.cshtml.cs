using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterCopy.Areas.Identity;
using TwitterCopy.Data;
using TwitterCopy.Models;

namespace TwitterCopy.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly TwitterCopyContext _context;
        private readonly UserManager<TwitterCopyUser> _userManager;

        public IndexModel(UserManager<TwitterCopyUser> userManager, TwitterCopyContext context)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Tweet Tweet { get; set; }

        public TwitterCopyUser TwitterCopyUser { get; set; }
        public IList<Tweet> FeedTweets { get; set; }

        public async Task<IActionResult> OnGet()
        {
            TwitterCopyUser = await _context.Users
                .AsNoTracking()
                .Include(t => t.Tweets)
                .Include(f => f.Following)
                .FirstOrDefaultAsync(x => x.UserName.Equals(User.Identity.Name));

            if(TwitterCopyUser == null)
            {
                return NotFound();
            }

            if (TwitterCopyUser.Tweets != null)
            {
                FeedTweets = GetTweets(TwitterCopyUser);
            }

            return Page();
        }

        // Add tweets
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Tweet.User == null)
            {
                Tweet.User = await _userManager.FindByNameAsync(User.Identity.Name);
            }
            Tweet.PostedOn = DateTime.Now;

            if (await TryUpdateModelAsync<Tweet>(
                Tweet,
                "tweet",
                t => t.Text, t => t.User, t => t.PostedOn))
            {
                _context.Tweets.Add(Tweet);
                await _context.SaveChangesAsync();

                return RedirectToPage();
            }

            return null;
        }

        private IList<Tweet> GetTweets(TwitterCopyUser user)
        {
            //var currentUser = _context.Users
            //    .AsNoTracking()
            //    .Include(t => t.Tweets)
            //    .Include(f => f.Following)
            //    .FirstOrDefault(x => x.Id.Equals(user.Id));

            //var followingTweets = currentUser.Following
            //    .Where(u => currentUser.Id.Equals(u.FollowerId))
            //    .Select(t => t.User.Tweets
            //        .OrderByDescending(x => x.PostedOn));

            return _context.Tweets
                .AsNoTracking()
                .Include(u=>u.User)
                .Where(u => u.UserId.Equals(user.Id))
                .OrderByDescending(t => t.PostedOn)
                .ToList();
        }
    }
}
