using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TwitterCopy.Core.Entities;
using TwitterCopy.Core.Entities.TweetAggregate;
using TwitterCopy.Core.Interfaces;
using TwitterCopy.Infrastructure.Data;
using TwitterCopy.Models;

namespace TwitterCopy.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ITweetService _tweetService;
        private readonly UserManager<TwitterCopyUser> _userManager;

        public IndexModel(IUserService userService, ITweetService tweetService,
            UserManager<TwitterCopyUser> userManager)
        {
            _userService = userService;
            _tweetService = tweetService;
            _userManager = userManager;
        }

        [BindProperty]
        public TweetViewModel Tweet { get; set; }

        public ProfileViewModel CurrentUser { get; set; }

        public List<TweetViewModel> FeedTweets { get; set; } = new List<TweetViewModel>();

        /// <summary>
        /// Provides data for the view
        /// - User
        /// - Feed for the user
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGet()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userService.GetUserAndFeedMainInfoAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            CurrentUser = new ProfileViewModel
            {
                Avatar = user.Avatar,
                FollowersCount = user.Followers.Count,
                FollowingCount = user.Following.Count,
                Id = user.Id.ToString(),
                Slug = user.Slug,
                TweetsCount = user.Tweets.Count,
                UserName = user.UserName
            };

            // TODO: Add Retweets to the Feed too
            FeedTweets = user.Tweets
                .Select(x => new TweetViewModel
                {
                    AuthorAvatar = user.Avatar,
                    AuthorName = user.UserName,
                    AuthorSlug = user.Slug,
                    Id = x.Id,
                    LikeCount = x.LikeCount,
                    PostedOn = x.PostedOn,
                    RetweetCount = x.RetweetCount,
                    Text = x.Text
                })
                .Union(user.Following
                    .SelectMany(x => x.User.Tweets
                        .Select(t => new TweetViewModel
                        {
                            AuthorAvatar = t.User.Avatar,
                            AuthorName = t.User.UserName,
                            AuthorSlug = t.User.Slug,
                            Id = t.Id,
                            LikeCount = t.LikeCount,
                            PostedOn = t.PostedOn,
                            RetweetCount = t.RetweetCount,
                            Text = t.Text
                        }))
                )
                .OrderByDescending(t => t.PostedOn)
                .ToList();

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

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            await _tweetService.AddTweet(Tweet.Text, user);

            return RedirectToPage();
        }

        /// <summary>
        /// Provides tweet for the modal dialog
        /// </summary>
        /// <param name="id">Tweet's id</param>
        /// <returns>Tweet info in JSON</returns>
        public async Task<IActionResult> OnGetTweetAsync(int? id)
        {
            var tweetToDelete = await _tweetService.GetTweetWithAuthor(id.Value);
            if (tweetToDelete == null)
            {
                return NotFound("Tweet to delete not found.");
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("User id not found. Please log in to the website.");
            }

            if (!tweetToDelete.UserId.ToString().Equals(userId))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return new JsonResult(new
                {
                    Message = "You cannot delete Tweet which doesn't belong to you."
                });
            }

            var tweetVM = new TweetViewModel
            {
                Id = tweetToDelete.Id,
                AuthorName = tweetToDelete.User.UserName,
                AuthorSlug = tweetToDelete.User.Slug,
                AuthorAvatar = tweetToDelete.User.Avatar,
                Text = tweetToDelete.Text,
                PostedOn = tweetToDelete.PostedOn
            };

            return new JsonResult(tweetVM);
        }

        /// <summary>
        /// Deletes tweet from the database
        /// </summary>
        /// <param name="id">Tweet's id</param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostDeleteAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tweetToDelete = await _tweetService.GetTweetAsync(id.Value);
            if (tweetToDelete == null)
            {
                return NotFound("Tweet to delete not found.");
            }

            await _tweetService.DeleteTweet(tweetToDelete);

            return RedirectToPage();
        }

        //public async Task<IActionResult> OnGetUpdateLikesAsync(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    // Get current authenticated user
        //    var user = await _userManager.GetUserAsync(User);
        //    if(user == null)
        //    {
        //        return NotFound();
        //    }
        //    // Get tweet with the given Id
        //    var tweet = await _context.Tweets
        //        .Include(l => l.Likes)
        //        .FirstOrDefaultAsync(t => t.Id == id);
        //    if(tweet == null)
        //    {
        //        return NotFound();
        //    }

        //    // Apply the user and tweet object from above to the new Like
        //    var like = new Like
        //    {
        //        Tweet = tweet,
        //        User = user,
        //        DateLiked = DateTime.UtcNow
        //    };

        //    // Check if the user already has like on this tweet
        //    var dupe = await _context.Likes.FirstOrDefaultAsync(x => x.TweetId == tweet.Id && x.UserId == user.Id);
        //    if (dupe == null)
        //    {
        //        // If no duplicate was found
        //        // Add new like to the database
        //        _context.Likes.Add(like);
        //        tweet.LikeCount++;
        //    }
        //    else
        //    {
        //        // If duplicate was found in the Likes table
        //        // Delete dupe instead of like because
        //        // like doesn't have Id values
        //        _context.Likes.Remove(dupe);
        //        tweet.LikeCount--;
        //    }

        //    //_context.Update<Tweet>(tweet);
        //    await _context.SaveChangesAsync();

        //    return new JsonResult(tweet.LikeCount);
        //}

        //public async Task<IActionResult> OnPostFollowAsync(string userSlug)
        //{
        //    if (userSlug == null)
        //    {
        //        return NotFound();
        //    }

        //    var userToFollow = await _context.Users
        //        .Include(f => f.Followers)
        //        .FirstOrDefaultAsync(u => u.Slug == userSlug);
        //    if (userToFollow == null)
        //    {
        //        return NotFound();
        //    }

        //    var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
        //    if (currentUser == null)
        //    {
        //        return NotFound();
        //    }

        //    if (userSlug == currentUser.Slug)
        //    {
        //        Response.StatusCode = (int)HttpStatusCode.Forbidden;
        //        return new JsonResult(new { Message = "You can't follow yourself." });
        //    }

        //    bool alreadyFollowing = userToFollow.Followers
        //        .Any(f => f.FollowerId.Equals(currentUser.Id));
        //    if (alreadyFollowing)
        //    {
        //        Response.StatusCode = (int)HttpStatusCode.Forbidden;
        //        return new JsonResult(new { Message = "You can't follow the user you're already following." });
        //    }

        //    userToFollow.Followers.Add(new UserToUser
        //    {
        //        User = userToFollow,
        //        Follower = currentUser
        //    });

        //    _context.Update(userToFollow);
        //    await _context.SaveChangesAsync();

        //    return new JsonResult(new
        //    {
        //        Count = userToFollow.Followers.Count,
        //        Slug = userToFollow.Slug,
        //        CurrentUserSlug = currentUser.Slug
        //    });
        //}

        //public async Task<IActionResult> OnPostUnfollowAsync(string userSlug)
        //{
        //    if (userSlug == null)
        //    {
        //        return NotFound();
        //    }

        //    var userToUnfollow = await _context.Users
        //        .Include(f => f.Followers)
        //        .FirstOrDefaultAsync(u => u.Slug == userSlug);
        //    if (userToUnfollow == null)
        //    {
        //        return NotFound();
        //    }

        //    var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
        //    if (currentUser == null)
        //    {
        //        return NotFound();
        //    }

        //    if (userSlug == currentUser.Slug)
        //    {
        //        Response.StatusCode = (int)HttpStatusCode.Forbidden;
        //        return new JsonResult(new { Message = "You can't unfollow yourself." });
        //    }

        //    bool alreadyFollowing = await _context.Users
        //        .AnyAsync(x => x.Followers
        //            .Any(u => u.FollowerId.Equals(currentUser.Id)));
        //    if (!alreadyFollowing)
        //    {
        //        Response.StatusCode = (int)HttpStatusCode.Forbidden;
        //        return new JsonResult(new { Message = "You can't unfollow the user you're alredy unfollowing." });
        //    }

        //    userToUnfollow.Followers.Remove(userToUnfollow.Followers
        //        .FirstOrDefault(x => x.FollowerId.Equals(currentUser.Id)));
        //    await _context.SaveChangesAsync();

        //    return new JsonResult(new
        //    {
        //        Count = userToUnfollow.Followers.Count,
        //        Slug = userToUnfollow.Slug,
        //        CurrentUserSlug = currentUser.Slug
        //    });
        //}

        //public async Task<IActionResult> OnPostRetweetAsync(int? id)
        //{
        //    if(id == null)
        //    {
        //        return NotFound();
        //    }

        //    var tweet = await _context.Tweets
        //        .Include(r => r.Retweets)
        //        .FirstOrDefaultAsync(t => t.Id.Equals(id));
        //    if(tweet == null)
        //    {
        //        return NotFound();
        //    }

        //    var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
        //    if(currentUser == null)
        //    {
        //        return NotFound();
        //    }

        //    var retweet = new Retweet
        //    {
        //        Tweet = tweet,
        //        User = currentUser,
        //        RetweetDate = DateTime.UtcNow
        //    };

        //    var dupe = await _context.Retweets.FirstOrDefaultAsync(x => x.TweetId == tweet.Id && x.UserId == currentUser.Id);
        //    if (dupe == null)
        //    {
        //        // If no duplicate was found
        //        // Add new retweet to the database
        //        _context.Retweets.Add(retweet);
        //        tweet.RetweetCount++;
        //    }
        //    else
        //    {
        //        // If duplicate was found in the Retweets table
        //        // Delete dupe instead of retweer because
        //        // retweet doesn't have Id value
        //        _context.Retweets.Remove(dupe);
        //        tweet.RetweetCount--;
        //    }

        //    //_context.Update<Tweet>(tweet);
        //    await _context.SaveChangesAsync();

        //    return new JsonResult(tweet.RetweetCount);
        //}

        /// <summary>
        /// Returns all the user's tweets
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        //private IList<TweetViewModel> GetTweets(string userId)
        //{
        //    var user = _context.Users
        //        .AsNoTracking()
        //        .Include(t => t.Tweets)
        //        .Include(r => r.Retweets)
        //            .ThenInclude(t => t.Tweet)
        //        .Include(f => f.Following)
        //            .ThenInclude(u => u.User)
        //                .ThenInclude(t => t.Tweets)
        //        .FirstOrDefault(x => x.Id.ToString().Equals(userId));

        //    var currentUserTweets = user.Tweets
        //        .Select(x => new TweetViewModel
        //        {
        //            Id = x.Id,
        //            AuthorName = x.User.UserName,
        //            AuthorSlug = x.User.Slug,
        //            AuthorAvatar = x.User.Avatar,
        //            LikeCount = x.LikeCount,
        //            RetweetCount = x.RetweetCount,
        //            PostedOn = x.PostedOn,
        //            Text = x.Text
        //        })
        //        .Union(user.Retweets
        //            .Select(x => new TweetViewModel
        //            {
        //                Id = x.Tweet.Id,
        //                AuthorName = x.Tweet.User.UserName,
        //                AuthorSlug = x.Tweet.User.Slug,
        //                AuthorAvatar = x.Tweet.User.Avatar,
        //                LikeCount = x.Tweet.LikeCount,
        //                PostedOn = x.RetweetDate,
        //                RetweetCount = x.Tweet.RetweetCount,
        //                Text = x.Tweet.Text
        //            })
        //        );

        //    var followingTweets = user.Following
        //        .SelectMany(x => x.User.Tweets
        //            .Select(t => new TweetViewModel
        //            {
        //                Id = t.Id,
        //                AuthorName = t.User.UserName,
        //                AuthorSlug = t.User.Slug,
        //                AuthorAvatar = t.User.Avatar,
        //                LikeCount = t.LikeCount,
        //                RetweetCount = t.RetweetCount,
        //                PostedOn = t.PostedOn,
        //                Text = t.Text
        //            }));

        //    return currentUserTweets.Concat(followingTweets)
        //            .OrderByDescending(t => t.PostedOn)
        //            .ToList();
        //}
    }
}
