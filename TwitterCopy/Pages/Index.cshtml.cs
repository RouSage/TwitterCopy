using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

            FeedTweets = GetTweets(CurrentUser.Id);

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

        public async Task<IActionResult> OnGetUpdateLikesAsync(int? id)
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
            if (dupe == null)
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

            //_context.Update<Tweet>(tweet);
            await _context.SaveChangesAsync();

            return new JsonResult(tweet.LikeCount);
        }

        public async Task<IActionResult> OnPostFollowAsync(string userSlug)
        {
            if (userSlug == null)
            {
                return NotFound();
            }

            var userToFollow = await _context.Users
                .Include(f => f.Followers)
                .FirstOrDefaultAsync(u => u.Slug == userSlug);
            if (userToFollow == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return NotFound();
            }

            if (userSlug == currentUser.Slug)
            {
                return NotFound("You cannot follow yourself");
            }

            bool alreadyFollowing = await _context.Users
                .AnyAsync(x => x.Followers
                    .Any(u => u.FollowerId.Equals(currentUser.Id)));
            if (alreadyFollowing)
            {
                return NotFound("You cannot follow the User you are already following");
            }

            userToFollow.Followers.Add(new UserToUser
            {
                User = userToFollow,
                Follower = currentUser
            });

            _context.Update(userToFollow);
            await _context.SaveChangesAsync();

            return new JsonResult(userToFollow.Followers.Count);
        }

        public async Task<IActionResult> OnPostUnfollowAsync(string userSlug)
        {
            if (userSlug == null)
            {
                return NotFound();
            }

            var userToUnfollow = await _context.Users
                .Include(f => f.Followers)
                .FirstOrDefaultAsync(u => u.Slug == userSlug);
            if (userToUnfollow == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return NotFound();
            }

            if (userSlug == currentUser.Slug)
            {
                return NotFound("You cannot unfollow yourself");
            }

            bool alreadyFollowing = await _context.Users
                .AnyAsync(x => x.Followers
                    .Any(u => u.FollowerId.Equals(currentUser.Id)));
            if (!alreadyFollowing)
            {
                return NotFound("You cannot unfollow the User you are already unfollowing");
            }

            userToUnfollow.Followers.Remove(userToUnfollow.Followers
                .FirstOrDefault(x => x.FollowerId.Equals(currentUser.Id)));
            await _context.SaveChangesAsync();

            return new JsonResult(userToUnfollow.Followers.Count);
        }

        public async Task<IActionResult> OnPostRetweetAsync(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var tweet = await _context.Tweets
                .Include(r => r.Retweets)
                .FirstOrDefaultAsync(t => t.Id.Equals(id));
            if(tweet == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if(currentUser == null)
            {
                return NotFound();
            }

            var retweet = new Retweet
            {
                Tweet = tweet,
                User = currentUser,
                RetweetDate = DateTime.UtcNow
            };

            var dupe = await _context.Retweets.FirstOrDefaultAsync(x => x.TweetId == tweet.Id && x.UserId == currentUser.Id);
            if (dupe == null)
            {
                // If no duplicate was found
                // Add new retweet to the database
                _context.Retweets.Add(retweet);
                tweet.RetweetCount++;
            }
            else
            {
                // If duplicate was found in the Retweets table
                // Delete dupe instead of retweer because
                // retweet doesn't have Id value
                _context.Retweets.Remove(dupe);
                tweet.RetweetCount--;
            }

            //_context.Update<Tweet>(tweet);
            await _context.SaveChangesAsync();

            return new JsonResult(tweet.RetweetCount);
        }

        /// <summary>
        /// Returns all the user's tweets
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private IList<TweetModel> GetTweets(Guid userId)
        {
            var user = _context.Users
                .AsNoTracking()
                .Include(t => t.Tweets)
                .Include(f => f.Following)
                    .ThenInclude(u => u.User)
                        .ThenInclude(t=>t.Tweets)
                .FirstOrDefault(x => x.Id.Equals(userId));

            var currentUserTweets = user.Tweets
                .Select(x => new TweetModel
                {
                    Id = x.Id,
                    AuthorName = x.User.UserName,
                    AuthorSlug = x.User.Slug,
                    LikeCount = x.LikeCount,
                    RetweetCount = x.RetweetCount,
                    PostedOn = x.PostedOn,
                    Text = x.Text
                });

            var followingTweets = user.Following
                .SelectMany(x => x.User.Tweets
                    .Select(t => new TweetModel
                    {
                        Id = t.Id,
                        AuthorName = t.User.UserName,
                        AuthorSlug = t.User.Slug,
                        LikeCount = t.LikeCount,
                        RetweetCount = t.RetweetCount,
                        PostedOn = t.PostedOn,
                        Text = t.Text
                    }));

            var feed = currentUserTweets.Concat(followingTweets)
                .OrderByDescending(t => t.PostedOn)
                .ToList();

            return feed;
        }
    }
}
