using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;

        public IndexModel(
            IUserService userService,
            ITweetService tweetService,
            UserManager<TwitterCopyUser> userManager,
            IMapper mapper,
            ILogger<IndexModel> logger)
        {
            _userService = userService;
            _tweetService = tweetService;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
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
            _logger.LogInformation("Getting User entity {ID}", userId);
            var user = await _userService.GetUserAndFeedMainInfoAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User ({ID}) NOT FOUND", userId);
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

            ViewData["CurrentUserSlug"] = user.Slug;

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

            _logger.LogInformation("Getting authenticated User");
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("User NOT FOUND");
                return NotFound();
            }

            _logger.LogInformation("Inserting new Tweet entity with text ({TEXT}) and User Id ({ID})", Tweet.Text, user.Id);
            await _tweetService.AddTweet(Tweet.Text, user);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostFollowAsync(string userSlug)
        {
            if (string.IsNullOrEmpty(userSlug))
            {
                return NotFound();
            }

            _logger.LogInformation("Getting User by slug ({SLUG})", userSlug);
            var userToFollow = await _userService.GetProfileOwnerWithFollowersForEditAsync(userSlug);
            if (userToFollow == null)
            {
                _logger.LogWarning("User with slug ({SLUG}) NOT FOUND", userSlug);
                return NotFound(userToFollow);
            }

            _logger.LogInformation("Getting authenticated User");
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("Authenticated User NOT FOUND");
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

            _logger.LogInformation("Inserting new follower ({FOLLOWER}) to the User ({ID})", currentUser.Id, userToFollow.Id);
            userToFollow.Followers.Add(new UserToUser
            {
                User = userToFollow,
                Follower = currentUser
            });

            _logger.LogInformation("Updating User ({ID}) entity", userToFollow.Id);
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

            _logger.LogInformation("Getting User by slug ({SLUG})", userSlug);
            var userToUnfollow = await _userService.GetProfileOwnerWithFollowersForEditAsync(userSlug);
            if (userToUnfollow == null)
            {
                _logger.LogWarning("User with slug ({SLUG}) NOT FOUND", userSlug);
                return NotFound(userToUnfollow);
            }

            _logger.LogInformation("Getting authenticated User");
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("Authenticated User NOT FOUND");
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

            _logger.LogInformation("Removing follower ({FOLLOWER}) from User ({ID})", currentUser.Id, userToUnfollow.Id);
            userToUnfollow.Followers.Remove(userToUnfollow.Followers
                .FirstOrDefault(x => x.FollowerId.Equals(currentUser.Id)));

            _logger.LogInformation("Updating User ({ID}) entity", userToUnfollow.Id);
            await _userService.UpdateUserAsync(userToUnfollow);

            return new JsonResult(new
            {
                userToUnfollow.Followers.Count,
                userToUnfollow.Slug,
                CurrentUserSlug = currentUser.Slug
            });
        }
    }
}
