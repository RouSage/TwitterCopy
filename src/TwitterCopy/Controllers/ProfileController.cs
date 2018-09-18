using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using TwitterCopy.Core.Entities;
using TwitterCopy.Core.Interfaces;
using TwitterCopy.Models;

namespace TwitterCopy.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<TwitterCopyUser> _userManager;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public ProfileController(
            UserManager<TwitterCopyUser> userManager,
            IUserService userService,
            IMapper mapper)
        {
            _userManager = userManager;
            _userService = userService;
            _mapper = mapper;
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

            var userToUpdate = await _userManager.GetUserAsync(User);
            if (userToUpdate == null)
            {
                return NotFound("User not found.");
            }

            _userService.RemoveImage(avatar, Globals.AvatarsFolder);
            userToUpdate.Avatar = Globals.DefaultAvatar;

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

            var userToUpdate = await _userManager.GetUserAsync(User);
            if (userToUpdate == null)
            {
                return NotFound("User not found.");
            }

            _userService.RemoveImage(banner, Globals.BannersFolder);
            userToUpdate.Banner = Globals.DefaultBanner;

            await _userService.UpdateUserAsync(userToUpdate);

            return Json(new
            {
                Message = "Avatar removed successfully",
                Banner = string.Concat("/images/profile-banners/", userToUpdate.Banner)
            });
        }
    }
}