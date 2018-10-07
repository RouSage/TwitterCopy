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
    public class FollowingModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly UserManager<TwitterCopyUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public FollowingModel(
            IUserService userService,
            UserManager<TwitterCopyUser> userManager,
            IMapper mapper,
            ILogger<FollowingModel> logger)
        {
            _userService = userService;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public ProfileViewModel ProfileUser { get; set; }

        public IList<UserToUser> Following { get; set; }

        public ProfileInputModel Input { get; set; }

        public PaginationViewModel Pagination { get; set; }

        public async Task<IActionResult> OnGetAsync(string slug, int p = 1)
        {
            if(string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            _logger.LogInformation("Getting User by slug ({SLUG})", slug);
            var profileOwner = await _userService.GetProfileOwnerAsync(slug);
            if(profileOwner == null)
            {
                _logger.LogWarning("User with slug ({SLUG}) NOT FOUND", slug);
                return NotFound();
            }

            _logger.LogInformation("Getting authenticated User");
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("Authenticated User NOT FOUND");
                return NotFound();
            }

            ViewData["CurrentUserId"] = currentUser.Id.ToString();
            ViewData["IsYourself"] = profileOwner.Slug == currentUser.Slug;
            ViewData["IsFollowed"] = profileOwner.Followers
                .Any(x => x.FollowerId.Equals(currentUser.Id));

            Pagination = new PaginationViewModel
            {
                Count = profileOwner.Following.Count(),
                CurrentPage = p
            };

            ProfileUser = _mapper.Map<ProfileViewModel>(profileOwner);
            Input = _mapper.Map<ProfileInputModel>(profileOwner);
            Following = profileOwner.Following
                .Skip((p - 1) * Pagination.PageSize)
                .Take(Pagination.PageSize)
                .ToList();

            return Page();
        }
    }
}