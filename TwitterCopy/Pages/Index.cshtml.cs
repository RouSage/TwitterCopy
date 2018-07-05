using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public TweetModel Tweet { get; set; }

        public TwitterCopyUser CurrentUser { get; set; }

        public IList<TweetModel> FeedTweets { get; set; }

        public class TweetModel
        {
            public int Id { get; set; }

            [Required]
            [StringLength(280)]
            public string Text { get; set; }

            public string AuthorName { get; set; }

            public string AuthorSlug { get; set; }

            [DataType(DataType.DateTime)]
            public DateTime PostedOn { get; set; } = DateTime.Now;
        }

        /// <summary>
        /// Provides data for the view
        /// - User
        /// - Feed for the user
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGet()
        {
            CurrentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if(CurrentUser == null)
            {
                return NotFound();
            }

            FeedTweets = GetTweets(CurrentUser);

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

            if (await TryUpdateModelAsync<TweetModel>(
                Tweet,
                "tweet",
                t => t.Text, t => t.AuthorName, t => t.PostedOn))
            {
                var newTweet = new Tweet
                {
                    Text = Tweet.Text,
                    User = await _userManager.FindByNameAsync(User.Identity.Name),
                    PostedOn = Tweet.PostedOn
                };

                _context.Tweets.Add(newTweet);
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
        public async Task<IActionResult> OnGetTweetAsync(int? id)
        {
            var tweetToDelete = await _context.Tweets
                .Include(u => u.User)
                .Select(x => new TweetModel
                {
                    Id = x.Id,
                    AuthorName = x.User.UserName,
                    AuthorSlug = x.User.Slug,
                    Text = x.Text,
                    PostedOn = x.PostedOn
                })
                .FirstOrDefaultAsync(t => t.Id == id);

            if(tweetToDelete == null)
            {
                return NotFound();
            }

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
                .AsNoTracking()
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
        private IList<TweetModel> GetTweets(TwitterCopyUser user)
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
                .Where(u => u.UserId.Equals(user.Id))
                .Select(x => new TweetModel
                {
                    Id = x.Id,
                    Text = x.Text,
                    PostedOn = x.PostedOn,
                    AuthorName = x.User.UserName,
                    AuthorSlug = x.User.Slug
                })
                .OrderByDescending(t => t.PostedOn)
                .ToList();
        }
    }
}
