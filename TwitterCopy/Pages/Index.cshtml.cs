using Microsoft.AspNetCore.Authorization;
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

        /// <summary>
        /// Provides data for the view
        /// - User
        /// - Feed for the user
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Inserts new tweet to the database
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Provides tweet for the modal dialog
        /// </summary>
        /// <param name="id">Tweet's id</param>
        /// <returns>Tweet info in JSON</returns>
        public async Task<JsonResult> OnGetTweetAsync(int? id)
        {
            var tweetToDelete = await _context.Tweets
                .Include(u => u.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            return new JsonResult(tweetToDelete);
        }

        /// <summary>
        /// Deletes tweet from the database
        /// </summary>
        /// <param name="id">Tweet's id</param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostDeleteAsync(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            // TODO: check owner with authorization
            var tweetToDelete = await _context.Tweets
                .FirstOrDefaultAsync(t => t.Id == id);

            if(tweetToDelete == null)
            {
                return NotFound();
            }

            _context.Tweets.Remove(tweetToDelete);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        /// <summary>
        /// Returns all the user's tweets
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
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
