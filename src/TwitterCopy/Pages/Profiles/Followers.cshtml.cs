using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterCopy.Core.Entities;
using TwitterCopy.Core.Interfaces;
using TwitterCopy.Models;

namespace TwitterCopy.Pages.Profiles
{
    public class FollowersModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly UserManager<TwitterCopyUser> _userManager;
        private readonly IMapper _mapper;

        public FollowersModel(
            IUserService userService,
            UserManager<TwitterCopyUser> userManager,
            IMapper mapper)
        {
            _userService = userService;
            _userManager = userManager;
            _mapper = mapper;
        }

        public ProfileViewModel ProfileUser { get; set; }

        public List<UserToUser> Followers { get; set; } = new List<UserToUser>();

        [BindProperty]
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
            if (currentUser == null)
            {
                return NotFound();
            }

            ViewData["CurrentUserId"] = currentUser.Id.ToString();
            ViewData["IsYourself"] = profileOwner.Slug == currentUser.Slug;
            ViewData["IsFollowed"] = profileOwner.Followers
                .Any(x => x.FollowerId.Equals(currentUser.Id));

            ProfileUser = _mapper.Map<ProfileViewModel>(profileOwner);
            Followers = profileOwner.Followers.ToList();
            Input = _mapper.Map<ProfileInputModel>(profileOwner);


            return Page();
        }
    }
}