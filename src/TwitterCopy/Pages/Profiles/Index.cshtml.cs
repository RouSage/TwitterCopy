using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TwitterCopy.Data;
using TwitterCopy.Entities;
using TwitterCopy.Models;

namespace TwitterCopy.Pages.Profiles
{
    public class IndexModel : PageModel
    {
        private readonly TwitterCopyContext _context;
        private readonly UserManager<TwitterCopyUser> _userManager;
        private readonly IHostingEnvironment _hostingEnvironment;

        public IndexModel(UserManager<TwitterCopyUser> userManager, TwitterCopyContext context,
            IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        public IList<TweetModel> Tweets { get; set; }

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

            var profileOwner = await _context.Users
                .AsNoTracking()
                .Include(f => f.Followers)
                .Include(f => f.Following)
                .Include(l => l.Likes)
                .FirstOrDefaultAsync(u => u.Slug.Equals(slug));
            if (profileOwner == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if(currentUser == null)
            {
                return NotFound();
            }

            Tweets = await _context.Tweets
                .AsNoTracking()
                .Where(t => t.UserId.Equals(profileOwner.Id))
                .Select(x => new TweetModel
                {
                    Id = x.Id,
                    AuthorName = x.User.UserName,
                    AuthorSlug = x.User.Slug,
                    AuthorAvatar = x.User.Avatar,
                    PostedOn = x.PostedOn,
                    Text = x.Text,
                    LikeCount = x.LikeCount,
                    RetweetCount = x.RetweetCount
                })
                .OrderByDescending(p => p.PostedOn)
                .ToListAsync();

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
                TweetsCount = Tweets.Count,
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
                    if(modelError.Value.Errors.Count > 0)
                    {
                        Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        return new JsonResult(new { Message = modelError.Value.Errors[0].ErrorMessage });
                    }
                }
            }

            if(postedData == null)
            {
                return NotFound("No data posted.");
            }

            var userToUpdate = await _userManager.GetUserAsync(User);
            if(userToUpdate == null)
            {
                return NotFound("User not found.");
            }

            userToUpdate.UserName = postedData.UserName;
            userToUpdate.Bio = postedData.Bio;
            userToUpdate.Location = postedData.Location;
            userToUpdate.Website = postedData.Website;

            if(postedData.Avatar?.Length > 0)
            {
                var avatarFileName = Guid.NewGuid().ToString().Replace("-", "") + Path.GetExtension(postedData.Avatar.FileName);

                var avatarFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "images\\profile-images", avatarFileName);
                using (var stream = new FileStream(avatarFilePath, FileMode.Create))
                {
                    await postedData.Avatar.CopyToAsync(stream);
                }

                userToUpdate.Avatar = avatarFileName;
            }

            await _context.SaveChangesAsync();

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

            var profileOwner = await _context.Users
                .AsNoTracking()
                .Include(f => f.Followers)
                .FirstOrDefaultAsync(u => u.Slug == slug);
            if(profileOwner == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if(currentUser == null)
            {
                return NotFound();
            }

            var tweet = await _context.Tweets
                .AsNoTracking()
                .Select(t => new TweetModel
                {
                    Id = t.Id,
                    AuthorName = t.User.UserName,
                    AuthorSlug = t.User.Slug,
                    AuthorAvatar = t.User.Avatar,
                    LikeCount = t.LikeCount,
                    PostedOn = t.PostedOn,
                    RetweetCount = t.RetweetCount,
                    Text = t.Text
                })
                .FirstOrDefaultAsync(t => t.Id == tweetId);
            if(tweet == null)
            {
                return NotFound();
            }

            var viewData = new ViewDataDictionary(
                new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary()) { { "TweetModel", tweet } };
            viewData.Model = tweet;

            viewData["IsYourself"] = profileOwner.Slug == currentUser.Slug;
            viewData["IsFollowed"] = profileOwner.Followers
                .Any(x => x.FollowerId.Equals(currentUser.Id));

            var result = new PartialViewResult
            {
                ViewName = "_TweetPopUp",
                ViewData = viewData,
            };

            //return new JsonResult(tweet);
            return result;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if(user == null)
            {
                return NotFound();
            }

            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "images\\profile-images", Vm.Avatar.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await Vm.Avatar.CopyToAsync(stream);
            }

            user.Avatar = Vm.Avatar.FileName;
            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index", new { slug = user.Slug });
        }
    }
}
