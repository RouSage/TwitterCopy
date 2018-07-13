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

        public bool IsYourself { get; set; }

        public class TweetModel
        {
            public int Id { get; set; }

            public string Text { get; set; }

            public string AuthorName { get; set; }

            public string AuthorSlug { get; set; }

            public int LikeCount { get; set; }

            [DataType(DataType.DateTime)]
            public DateTime PostedOn { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return NotFound();
            }

            var profileOwner = await _userManager.FindByNameAsync(userName);
            if (profileOwner == null)
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
                .OrderByDescending(p=>p.PostedOn)
                .ToListAsync();

            ProfileUser = profileOwner;

            IsYourself = profileOwner.UserName.Equals(User.Identity.Name, StringComparison.InvariantCultureIgnoreCase);
            
            return Page();
        }

        public async Task<IActionResult> OnGetLikedAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get current authenticated user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            // Get tweet with the given Id
            var tweet = await _context.Tweets
                .Include(l => l.Likes)
                .FirstOrDefaultAsync(t => t.Id == id);

            // Apply the user and tweet object from above to the new Like
            var like = new Like
            {
                Tweet = tweet,
                User = user,
                DateLiked = DateTime.UtcNow
            };

            // Check if the user already has like on this tweet
            var dupe = await _context.Likes.FirstOrDefaultAsync(x => x.TweetId == tweet.Id && x.UserId == user.Id);
            if(dupe == null)
            {
                // If no duplicate was found
                // Add new like to the database
                _context.Likes.Add(like);
                tweet.LikeCount++;
            }
            else
            {
                // If duplicate was found in the Likes table
                // Delete dupe instead of like because
                // like doesn't have Id values
                _context.Likes.Remove(dupe);
                tweet.LikeCount--;
            }

            _context.Update<Tweet>(tweet);
            await _context.SaveChangesAsync();

            return new JsonResult(tweet.LikeCount);
        }
    }
}
