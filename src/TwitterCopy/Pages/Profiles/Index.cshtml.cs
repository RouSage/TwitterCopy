using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public IndexModel(
            ITweetService tweetService,
            IUserService userService,
            UserManager<TwitterCopyUser> userManager,
            IMapper mapper)
        {
            _tweetSevice = tweetService;
            _userService = userService;
            _userManager = userManager;
            _mapper = mapper;
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

            var profileOwner = await _userService.GetProfileOwnerAsync(slug);
            if (profileOwner == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if(currentUser == null)
            {
                return NotFound();
            }

            ViewData["IsYourself"] = profileOwner.Slug == currentUser.Slug;
            ViewData["IsFollowed"] = profileOwner.Followers
                .Any(x => x.FollowerId.Equals(currentUser.Id));

            var userTweets = _mapper.Map<IEnumerable<TweetViewModel>>(profileOwner.Tweets);
            var followingTweets = _mapper.Map<IEnumerable<TweetViewModel>>(profileOwner.Following.SelectMany(ut => ut.User.Tweets));
            var userRetweets = _mapper.Map<IEnumerable<TweetViewModel>>(profileOwner.Retweets);

            Tweets = userTweets
                .Concat(followingTweets)
                .Concat(userRetweets)
                .OrderByDescending(rt => rt.RetweetDate)
                //.ThenByDescending(p=>p.PostedOn)
                .ToList();
            Profile = _mapper.Map<ProfileViewModel>(profileOwner);
            Input = _mapper.Map<ProfileInputModel>(profileOwner);

            return Page();
        }

        public async Task<IActionResult> OnPostEditUserAsync(ProfileInputModel postedData)
        {
            if (!ModelState.IsValid)
            {
                foreach (var modelError in ModelState)
                {
                    if (modelError.Value.Errors.Count > 0)
                    {
                        Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        return new JsonResult(new { Message = modelError.Value.Errors[0].ErrorMessage });
                    }
                }
            }

            if (postedData == null)
            {
                return NotFound("No data posted.");
            }

            var userToUpdate = await _userManager.GetUserAsync(User);
            if (userToUpdate == null)
            {
                return NotFound("User not found.");
            }

            userToUpdate.UserName = postedData.UserName;
            userToUpdate.Bio = postedData.Bio;
            userToUpdate.Location = postedData.Location;
            userToUpdate.Website = postedData.Website;

            if (postedData.Avatar?.Length > 0)
            {
                userToUpdate.Avatar = await _userService.UploadImage(postedData.Avatar, Globals.AvatarsFolder);
            }
            else
            {
                userToUpdate.Avatar = Globals.DefaultAvatar;
            }

            if (postedData.Banner?.Length > 0)
            {
                userToUpdate.Banner = await _userService.UploadImage(postedData.Banner, Globals.BannersFolder);
            }
            else
            {
                userToUpdate.Banner = Globals.DefaultBanner;
            }

            await _userService.UpdateUserAsync(userToUpdate);

            return new JsonResult(new
            {
                userToUpdate.UserName,
                userToUpdate.Bio,
                userToUpdate.Location,
                userToUpdate.Website,
                Avatar = string.Concat("/images/profile-images/", userToUpdate.Avatar),
                Banner = string.Concat("/images/profile-banners/", userToUpdate.Banner)
            });
        }

        public async Task<IActionResult> OnPostRemoveAvatarAsync(string avatar)
        {
            if (string.Equals(avatar, Globals.DefaultAvatar))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return new JsonResult(new { Message = "You cannot remove the default avatar." });
            }

            var userToUpdate = await _userManager.GetUserAsync(User);
            if (userToUpdate == null)
            {
                return NotFound("User not found.");
            }

            _userService.RemoveImage(avatar, Globals.AvatarsFolder);
            userToUpdate.Avatar = Globals.DefaultAvatar;

            await _userService.UpdateUserAsync(userToUpdate);

            return new JsonResult(new
            {
                Message = "Avatar removed successfully",
                Avatar = string.Concat("/images/profile-images/", userToUpdate.Avatar)
            });
        }

        public async Task<IActionResult> OnPostRemoveBannerAsync(string banner)
        {
            if (string.Equals(banner, Globals.DefaultBanner))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return new JsonResult(new { Message = "You cannot remove the default banner." });
            }

            var userToUpdate = await _userManager.GetUserAsync(User);
            if (userToUpdate == null)
            {
                return NotFound("User not found.");
            }

            _userService.RemoveImage(banner, Globals.BannersFolder);
            userToUpdate.Banner = Globals.DefaultBanner;

            await _userService.UpdateUserAsync(userToUpdate);

            return new JsonResult(new
            {
                Message = "Avatar removed successfully",
                Banner = string.Concat("/images/profile-banners/", userToUpdate.Banner)
            });
        }

        public async Task<IActionResult> OnGetStatusAsync(string slug, int? tweetId)
        {
            if (string.IsNullOrEmpty(slug) || tweetId == null)
            {
                return NotFound();
            }

            var profileOwner = await _userService.GetProfileOwnerWithFollowersAsync(slug);
            if (profileOwner == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            var tweet = await _tweetSevice.GetTweetWithAuthorAndRepliesAsync(tweetId.Value);
            if (tweet == null)
            {
                return NotFound();
            }

            var tweetVM = _mapper.Map<TweetViewModel>(tweet);

            var viewData = new ViewDataDictionary(
                new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary()) { { "TweetViewModel", tweetVM } };
            viewData.Model = tweetVM;

            viewData["IsYourself"] = profileOwner.Slug == currentUser.Slug;
            viewData["IsFollowed"] = profileOwner.Followers
                .Any(x => x.FollowerId.Equals(currentUser.Id));

            var result = new PartialViewResult
            {
                ViewName = "_TweetPopUp",
                ViewData = viewData,
            };

            return result;
        }

        public async Task<IActionResult> OnPostReplyAsync(string replyText, int? tweetId)
        {
            if(tweetId == null)
            {
                return NotFound();
            }

            var replyTo = await _tweetSevice.GetTweetWithUserAndRepliesForEditingAsync(tweetId.Value);
            if(replyTo == null)
            {
                return NotFound(replyTo);
            }

            var user = await _userManager.GetUserAsync(User);
            if(user == null)
            {
                return NotFound(user);
            }

            await _tweetSevice.AddReply(replyText, user, replyTo);

            return new JsonResult(replyTo.ReplyCount);
        }
    }
}
