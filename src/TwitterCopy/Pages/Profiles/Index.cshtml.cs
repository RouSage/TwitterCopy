using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly UserManager<TwitterCopyUser> _userManager;

        public IndexModel(ITweetService tweetService, IUserService userService,
            IHostingEnvironment hostingEnvironment, UserManager<TwitterCopyUser> userManager)
        {
            _tweetSevice = tweetService;
            _userService = userService;
            _hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
        }

        public List<TweetViewModel> Tweets { get; set; } = new List<TweetViewModel>();

        public ProfileViewModel Profile { get; set; }

        public ProfileInputModel Input { get; set; }

        public class AvatarVm
        {
            public IFormFile Avatar { get; set; }
        }

        [BindProperty]
        public AvatarVm Vm { get; set; }

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

            Tweets = profileOwner.Tweets
                .Select(x => new TweetViewModel
            {
                AuthorAvatar = profileOwner.Avatar,
                AuthorName = profileOwner.UserName,
                AuthorSlug = profileOwner.Slug,
                Id = x.Id,
                LikeCount = x.LikeCount,
                PostedOn = x.PostedOn,
                RetweetCount = x.RetweetCount,
                Text = x.Text
            }).ToList();

            ViewData["IsYourself"] = profileOwner.Slug == currentUser.Slug;
            ViewData["IsFollowed"] = profileOwner.Followers
                .Any(x => x.FollowerId.Equals(currentUser.Id));

            Profile = new ProfileViewModel
            {
                Id = profileOwner.Id.ToString(),
                UserName = profileOwner.UserName,
                Slug = profileOwner.Slug,
                Bio = profileOwner.Bio,
                Location = profileOwner.Location,
                Website = profileOwner.Website,
                FollowersCount = profileOwner.Followers.Count,
                FollowingCount = profileOwner.Following.Count,
                LikesCount = profileOwner.Likes.Count,
                TweetsCount = profileOwner.Tweets.Count,
                Avatar = profileOwner.Avatar,
                JoinDate = profileOwner.RegisterDate.ToString("MMMM yyyy")
            };

            Input = new ProfileInputModel
            {
                UserName = profileOwner.UserName,
                Bio = profileOwner.Bio,
                Location = profileOwner.Location,
                Website = profileOwner.Website,
            };

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
                var avatarFileName = Guid.NewGuid().ToString().Replace("-", "") + Path.GetExtension(postedData.Avatar.FileName);
                
                var avatarFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "images\\profile-images", avatarFileName);
                using (var stream = new FileStream(avatarFilePath, FileMode.Create))
                {
                    await postedData.Avatar.CopyToAsync(stream);
                }

                userToUpdate.Avatar = avatarFileName;
            }

            await _userService.UpdateUserAsync(userToUpdate);

            return new JsonResult(new
            {
                UserName = userToUpdate.UserName,
                Bio = userToUpdate.Bio,
                Location = userToUpdate.Location,
                Website = userToUpdate.Website,
                Avatar = string.Concat("/images/profile-images/", userToUpdate.Avatar)
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

            var tweet = await _tweetSevice.GetTweet(tweetId.Value);
            if (tweet == null)
            {
                return NotFound();
            }

            var tweetVM = new TweetViewModel
            {
                AuthorAvatar = tweet.User.Avatar,
                AuthorName = tweet.User.UserName,
                AuthorSlug = tweet.User.Slug,
                Id = tweet.Id,
                LikeCount = tweet.LikeCount,
                PostedOn = tweet.PostedOn,
                RetweetCount = tweet.RetweetCount,
                Text = tweet.Text
            };

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
    }
}
