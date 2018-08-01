﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterCopy.Data;
using TwitterCopy.Entities;
using TwitterCopy.Models;

namespace TwitterCopy.Pages.Profiles
{
    public class FollowersModel : PageModel
    {
        private readonly TwitterCopyContext _context;
        private readonly UserManager<TwitterCopyUser> _userManager;

        public FollowersModel(TwitterCopyContext context, UserManager<TwitterCopyUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public ProfileViewModel ProfileUser { get; set; }

        public IList<TwitterCopyUser> Followers { get; set; }

        [BindProperty]
        public ProfileInputModel Input { get; set; }

        public async Task<IActionResult> OnGetAsync(string slug)
        {
            if (slug == null)
            {
                return NotFound();
            }

            var profileOwner = await _context.Users
                .AsNoTracking()
                .Include(f => f.Following)
                .Include(f => f.Followers)
                    .ThenInclude(u => u.Follower)
                .Include(t => t.Tweets)
                .Include(l => l.Likes)
                .FirstOrDefaultAsync(u => u.Slug.Equals(slug));
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

            ProfileUser = new ProfileViewModel
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
                TweetsCount = profileOwner.Tweets.Count
            };

            Followers = profileOwner.Followers
                .Select(t => t.Follower)
                .ToList();

            Input = new ProfileInputModel
            {
                UserName = profileOwner.UserName,
                Bio = profileOwner.Bio,
                Location = profileOwner.Location,
                Website = profileOwner.Website
            };

            return Page();
        }
    }
}