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

        public TwitterCopyUser TwitterCopyUser { get; set; }

        public IList<Tweet> FeedTweets { get; set; }

        [BindProperty]
        public Tweet Tweet { get; set; }

        public async Task<IActionResult> OnGet()
        {
            TwitterCopyUser = await _context.Users
                .Include(t => t.Tweets)
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
            return _context.Tweets
                .Where(u => u.UserId.Equals(user.Id))
                .OrderByDescending(t => t.PostedOn)
                .ToList();
        }
    }
}
