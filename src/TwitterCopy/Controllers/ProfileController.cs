﻿using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;
using TwitterCopy.Core.Entities;
using TwitterCopy.Core.Interfaces;
using TwitterCopy.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace TwitterCopy.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<TwitterCopyUser> _userManager;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public ProfileController(
            UserManager<TwitterCopyUser> userManager,
            IUserService userService,
            IMapper mapper,
            ILogger<ProfileController> logger)
        {
            _userManager = userManager;
            _userService = userService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(ProfileInputModel postedData)
        {
            if (!ModelState.IsValid)
            {
                foreach (var modelError in ModelState)
                {
                    if (modelError.Value.Errors.Count > 0)
                    {
                        Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        return Json(new { Message = modelError.Value.Errors[0].ErrorMessage });
                    }
                }
            }

            if (postedData == null)
            {
                return NotFound("No data posted.");
            }

            _logger.LogInformation("Getting authenticated User");
            var userToUpdate = await _userManager.GetUserAsync(User);
            if (userToUpdate == null)
            {
                _logger.LogInformation("Authenticated User NOT FOUND");
                return NotFound("User not found.");
            }

            if(userToUpdate.Slug.Equals(postedData.Slug, StringComparison.InvariantCultureIgnoreCase))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json(new { Message = "You can't edit other User's profile." });
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

            _logger.LogInformation("Updating User ({ID})", userToUpdate.Id);
            await _userService.UpdateUserAsync(userToUpdate);

            return Json(new
            {
                userToUpdate.UserName,
                userToUpdate.Bio,
                userToUpdate.Location,
                userToUpdate.Website,
                Avatar = string.Concat("/images/profile-images/", userToUpdate.Avatar),
                Banner = string.Concat("/images/profile-banners/", userToUpdate.Banner)
            });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveAvatar(string avatar)
        {
            if (string.Equals(avatar, Globals.DefaultAvatar))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json(new { Message = "You cannot remove the default avatar." });
            }

            _logger.LogInformation("Getting authenticated User");
            var userToUpdate = await _userManager.GetUserAsync(User);
            if (userToUpdate == null)
            {
                _logger.LogWarning("Authenticated User NOT FOUND");
                return NotFound("User not found.");
            }

            _userService.RemoveImage(avatar, Globals.AvatarsFolder);
            userToUpdate.Avatar = Globals.DefaultAvatar;

            _logger.LogInformation("Updating User ({ID}) entity", userToUpdate.Id);
            await _userService.UpdateUserAsync(userToUpdate);

            return Json(new
            {
                Message = "Avatar removed successfully",
                Avatar = string.Concat("/images/profile-images/", userToUpdate.Avatar)
            });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveBanner(string banner)
        {
            if (string.Equals(banner, Globals.DefaultBanner))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json(new { Message = "You cannot remove the default banner." });
            }

            _logger.LogInformation("Getting authenticated User");
            var userToUpdate = await _userManager.GetUserAsync(User);
            if (userToUpdate == null)
            {
                _logger.LogWarning("Authenticated User NOT FOUND");
                return NotFound("User not found.");
            }

            _userService.RemoveImage(banner, Globals.BannersFolder);
            userToUpdate.Banner = Globals.DefaultBanner;

            _logger.LogInformation("Updating User ({ID}) entity", userToUpdate.Id);
            await _userService.UpdateUserAsync(userToUpdate);

            return Json(new
            {
                Message = "Avatar removed successfully",
                Banner = string.Concat("/images/profile-banners/", userToUpdate.Banner)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Follow(string userSlug)
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
                return Json(new { Message = "You can't follow yourself." });
            }

            if (_userService.CheckFollower(userToFollow, currentUser.Id))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json(new { Message = "You can't follow the user you're already following." });
            }

            _logger.LogInformation("Inserting new follower ({FOLLOWER}) to the User ({ID})", currentUser.Id, userToFollow.Id);
            userToFollow.Followers.Add(new UserToUser
            {
                User = userToFollow,
                Follower = currentUser
            });

            _logger.LogInformation("Updating User ({ID}) entity", userToFollow.Id);
            await _userService.UpdateUserAsync(userToFollow);

            return Json(new
            {
                userToFollow.Followers.Count,
                userToFollow.Slug,
                CurrentUserSlug = currentUser.Slug
            });
        }

        [HttpPost]
        public async Task<IActionResult> Unfollow(string userSlug)
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
                return Json(new { Message = "You can't unfollow yourself." });
            }

            if (!_userService.CheckFollower(userToUnfollow, currentUser.Id))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json(new { Message = "You can't unfollow the user you're alredy unfollowing." });
            }

            _logger.LogInformation("Removing follower ({FOLLOWER}) from User ({ID})", currentUser.Id, userToUnfollow.Id);
            userToUnfollow.Followers.Remove(userToUnfollow.Followers
                .FirstOrDefault(x => x.FollowerId.Equals(currentUser.Id)));

            _logger.LogInformation("Updating User ({ID}) entity", userToUnfollow.Id);
            await _userService.UpdateUserAsync(userToUnfollow);

            return Json(new
            {
                userToUnfollow.Followers.Count,
                userToUnfollow.Slug,
                CurrentUserSlug = currentUser.Slug
            });
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });

            return Redirect(returnUrl);
        }
    }
}