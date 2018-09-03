using AutoMapper;
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
        private readonly IMapper _mapper;

        public IndexModel(
            ITweetService tweetService,
            IUserService userService,
            IHostingEnvironment hostingEnvironment,
            UserManager<TwitterCopyUser> userManager,
            IMapper mapper)
        {
            _tweetSevice = tweetService;
            _userService = userService;
            _hostingEnvironment = hostingEnvironment;
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

            Tweets = _mapper.Map<List<TweetViewModel>>(profileOwner.Tweets);
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
                var avatarFileName = Guid.NewGuid().ToString().Replace("-", "") + Path.GetExtension(postedData.Avatar.FileName);
                
                var avatarFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "images\\profile-images", avatarFileName);
                using (var stream = new FileStream(avatarFilePath, FileMode.Create))
                {
                    await postedData.Avatar.CopyToAsync(stream);
                }

                userToUpdate.Avatar = avatarFileName;
            }
            else
            {
                userToUpdate.Avatar = Globals.DefaultAvatar;
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

            var avatarFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "images\\profile-images", avatar);
            System.IO.File.Delete(avatarFilePath);

            userToUpdate.Avatar = Globals.DefaultAvatar;

            await _userService.UpdateUserAsync(userToUpdate);

            return new JsonResult(new
            {
                Message = "Avatar removed successfully",
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

            var tweet = await _tweetSevice.GetTweetWithAuthor(tweetId.Value);
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
    }
}
