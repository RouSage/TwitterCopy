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
                return NotFound(user);
            }

            var userTweets = _mapper.Map<IEnumerable<TweetViewModel>>(user.Tweets);
            var followingTweets = _mapper.Map<IEnumerable<TweetViewModel>>(user.Following.SelectMany(ut => ut.User.Tweets));
            var userRetweets = _mapper.Map<IEnumerable<TweetViewModel>>(user.Retweets);

            FeedTweets = userTweets
                .Concat(followingTweets)
                .Concat(userRetweets)
                .OrderByDescending(rt => rt.RetweetDate)
                //.ThenByDescending(p=>p.PostedOn)
                .ToList();
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

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(user);
            }

            var updatedLikeCount = await _tweetService.UpdateLikes(id.Value, user);
            
            return new JsonResult(updatedLikeCount);
        }

        public async Task<IActionResult> OnPostFollowAsync(string userSlug)
        {
            if (string.IsNullOrEmpty(userSlug))
            {
                return NotFound();
            }

            var userToFollow = await _userService.GetProfileOwnerWithFollowersForEditAsync(userSlug);
            if (userToFollow == null)
            {
                return NotFound(userToFollow);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound(currentUser);
            }

            if (userSlug == currentUser.Slug)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return new JsonResult(new { Message = "You can't follow yourself." });
            }

            if (_userService.CheckFollower(userToFollow, currentUser.Id))
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
                userToFollow.Followers.Count,
                userToFollow.Slug,
                CurrentUserSlug = currentUser.Slug
            });
        }

        public async Task<IActionResult> OnPostUnfollowAsync(string userSlug)
        {
            if (string.IsNullOrEmpty(userSlug))
            {
                return NotFound();
            }

            var userToUnfollow = await _userService.GetProfileOwnerWithFollowersForEditAsync(userSlug);
            if (userToUnfollow == null)
            {
                return NotFound(userToUnfollow);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound(currentUser);
            }

            if (userSlug == currentUser.Slug)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return new JsonResult(new { Message = "You can't unfollow yourself." });
            }

            if (!_userService.CheckFollower(userToUnfollow, currentUser.Id))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return new JsonResult(new { Message = "You can't unfollow the user you're alredy unfollowing."});
            }

            userToUnfollow.Followers.Remove(userToUnfollow.Followers
                .FirstOrDefault(x => x.FollowerId.Equals(currentUser.Id)));
            await _userService.UpdateUserAsync(userToUnfollow);

            return new JsonResult(new
            {
                userToUnfollow.Followers.Count,
                userToUnfollow.Slug,
                CurrentUserSlug = currentUser.Slug
            });
        }

        public async Task<IActionResult> OnPostRetweetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound(currentUser);
            }

            var updatedRetweetCount = await _tweetService.UpdateRetweets(id.Value, currentUser);

            return new JsonResult(updatedRetweetCount);
        }
    }
}
