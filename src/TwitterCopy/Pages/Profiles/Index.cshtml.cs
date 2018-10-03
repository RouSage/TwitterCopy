using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterCopy.Core.Entities;
using TwitterCopy.Core.Interfaces;
using TwitterCopy.Models;

namespace TwitterCopy.Pages.Profiles
{
    public class IndexModel : PageModel
    {
        private readonly ITweetService _tweetSevice;
        private readonly IUserService _userService;
        private readonly UserManager<TwitterCopyUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public IndexModel(
            ITweetService tweetService,
            IUserService userService,
            UserManager<TwitterCopyUser> userManager,
            IMapper mapper,
            ILogger<IndexModel> logger)
        {
            _tweetSevice = tweetService;
            _userService = userService;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public List<TweetViewModel> Tweets { get; set; } = new List<TweetViewModel>();

        public ProfileViewModel Profile { get; set; }

        public ProfileInputModel Input { get; set; }

        public async Task<IActionResult> OnGetAsync(string slug)
        {
            if (slug == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Get User by slug ({SLUG})", slug);
            var profileOwner = await _userService.GetProfileOwnerAsync(slug);
            if (profileOwner == null)
            {
                _logger.LogWarning("User with slug ({SLUG}) NOT FOUND", slug);
                return NotFound();
            }

            _logger.LogInformation("Getting authenticated User");
            var currentUser = await _userManager.GetUserAsync(User);
            if(currentUser == null)
            {
                _logger.LogWarning("Authenticated User NOT FOUND");
                return NotFound();
            }

            var userTweets = _mapper.Map<IEnumerable<TweetViewModel>>(profileOwner.Tweets);
            var userRetweets = _mapper.Map<IEnumerable<TweetViewModel>>(profileOwner.Retweets);

            Tweets = userTweets
                .Concat(userRetweets)
                .OrderByDescending(rt => rt.RetweetDate)
                //.ThenByDescending(p=>p.PostedOn)
                .ToList();
            Profile = _mapper.Map<ProfileViewModel>(profileOwner);
            Input = _mapper.Map<ProfileInputModel>(profileOwner);

            ViewData["CurrentUserSlug"] = currentUser.Slug;
            ViewData["IsYourself"] = profileOwner.Slug == currentUser.Slug;
            ViewData["IsFollowed"] = profileOwner.Followers
                .Any(x => x.FollowerId.Equals(currentUser.Id));

            return Page();
        }
    }
}
