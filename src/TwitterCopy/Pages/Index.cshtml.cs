using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TwitterCopy.Core.Entities;
using TwitterCopy.Core.Interfaces;
using TwitterCopy.Models;

namespace TwitterCopy.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ITweetService _tweetService;
        private readonly UserManager<TwitterCopyUser> _userManager;
        private readonly IMapper _mapper;

        public IndexModel(
            IUserService userService,
            ITweetService tweetService,
            UserManager<TwitterCopyUser> userManager,
            IMapper mapper)
        {
            _userService = userService;
            _tweetService = tweetService;
            _userManager = userManager;
            _mapper = mapper;
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

            // TODO: Add Retweets to the Feed too
            var feedTweets = user.Tweets
                .Union(user.Following
                    .SelectMany(x => x.User.Tweets)
                )
                .OrderByDescending(t => t.PostedOn)
                .ToList();

            FeedTweets = _mapper.Map<List<TweetViewModel>>(feedTweets);
            CurrentUser = _mapper.Map<ProfileViewModel>(user);

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

            var tweetVM = _mapper.Map<TweetViewModel>(tweetToDelete);

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

        public async Task<IActionResult> OnGetUpdateLikesAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get current authenticated user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var updatedLikeCount = await _tweetService.UpdateLikes(id.Value, user);
            
            return new JsonResult(updatedLikeCount);
        }

        public async Task<IActionResult> OnPostFollowAsync(string userSlug)
        {
            if (userSlug == null)
            {
                return NotFound();
            }

            var userToFollow = await _userService.GetProfileOwnerWithFollowersForEditAsync(userSlug);
            if (userToFollow == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            if (userSlug == currentUser.Slug)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return new JsonResult(new { Message = "You can't follow yourself." });
            }

            bool alreadyFollowing = userToFollow.Followers
                .Any(f => f.FollowerId.Equals(currentUser.Id));
            if (alreadyFollowing)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return new JsonResult(new { Message = "You can't follow the user you're already following." });
            }

            userToFollow.Followers.Add(new UserToUser
            {
                User = userToFollow,
                Follower = currentUser
            });

            await _userService.UpdateUserAsync(userToFollow);

            return new JsonResult(new
            {
                Count = userToFollow.Followers.Count,
                Slug = userToFollow.Slug,
                CurrentUserSlug = currentUser.Slug
            });
        }

        public async Task<IActionResult> OnPostUnfollowAsync(string userSlug)
        {
            if (userSlug == null)
            {
                return NotFound();
            }

            var userToUnfollow = await _userService.GetProfileOwnerWithFollowersForEditAsync(userSlug);
            if (userToUnfollow == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            if (userSlug == currentUser.Slug)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return new JsonResult(new { Message = "You can't unfollow yourself." });
            }

            bool alreadyFollowing = userToUnfollow.Followers
                .Any(f => f.FollowerId.Equals(currentUser.Id));
            if (!alreadyFollowing)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return new JsonResult(new { Message = "You can't unfollow the user you're alredy unfollowing." });
            }

            userToUnfollow.Followers.Remove(userToUnfollow.Followers
                .FirstOrDefault(x => x.FollowerId.Equals(currentUser.Id)));
            await _userService.UpdateUserAsync(userToUnfollow);

            return new JsonResult(new
            {
                Count = userToUnfollow.Followers.Count,
                Slug = userToUnfollow.Slug,
                CurrentUserSlug = currentUser.Slug
            });
        }

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
    }
}
