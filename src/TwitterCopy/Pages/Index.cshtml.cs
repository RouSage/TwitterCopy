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
    }
}
