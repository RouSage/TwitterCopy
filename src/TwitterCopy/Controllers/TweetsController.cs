using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TwitterCopy.Core.Entities;
using TwitterCopy.Core.Interfaces;
using TwitterCopy.Models;

namespace TwitterCopy.Controllers
{
    public class TweetsController : Controller
    {
        private readonly UserManager<TwitterCopyUser> _userManager;
        private readonly ITweetService _tweetService;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly ILogger _logger;

        public TweetsController(
            UserManager<TwitterCopyUser> userManager,
            ITweetService tweetService,
            IMapper mapper,
            IUserService userService,
            ILogger<TweetsController> logger)
        {
            _userManager = userManager;
            _tweetService = tweetService;
            _mapper = mapper;
            _userService = userService;
            _logger = logger;
        }

        public async Task<IActionResult> UpdateLikes(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Getting authenticated User");
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Authenticated User NOT FOUND");
                return NotFound(user);
            }

            _logger.LogInformation("Updating Likes for Tweet ({ID})", id.Value);
            var updatedLikeCount = await _tweetService.UpdateLikes(id.Value, user);

            return Json(updatedLikeCount);
        }

        /// <summary>
        /// Provides tweet for the delete tweet modal dialog
        /// </summary>
        /// <param name="id">Tweet's id</param>
        /// <returns>Tweet info in JSON</returns>
        [HttpGet]
        public async Task<IActionResult> GetTweet(int? id)
        {
            _logger.LogInformation("Getting Tweet ({ID})", id.Value);
            var tweetToDelete = await _tweetService.GetTweetAsync(id.Value);
            if (tweetToDelete == null)
            {
                _logger.LogWarning("Tweet ({ID}) NOT FOUND", id.Value);
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { Message = "Requested Tweet was not found" });
            }

            _logger.LogInformation("Getting authenticated User Id");
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("User id not found. Please log in to the website.");
            }

            if (!tweetToDelete.UserId.ToString().Equals(userId))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Json(new
                {
                    Message = "You cannot delete Tweet which doesn't belong to you."
                });
            }

            var tweetVM = _mapper.Map<DeleteTweetViewModel>(tweetToDelete);

            _logger.LogInformation("Returning _DeleteTweetPopUp partial view for Tweet ({ID})", tweetVM.Id);
            return PartialView("_DeleteTweetPopUp", tweetVM);
        }

        /// <summary>
        /// Deletes tweet from the database
        /// </summary>
        /// <param name="id">Tweet's id</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Getting Tweet ({ID})", id.Value);
            var tweetToDelete = await _tweetService.GetTweetForDeletion(id.Value);
            if (tweetToDelete == null)
            {
                _logger.LogWarning("Tweet ({ID}) NOT FOUND", id.Value);
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { Message = "Requested Tweet was not found" });
            }

            _logger.LogInformation("Deleting Tweet ({ID})", id.Value);
            await _tweetService.DeleteTweet(tweetToDelete);

            return Json(new { Message = "Tweet deleted successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> Retweet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Getting authenticated User");
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("Authenticated User NOT FOUND");
                return NotFound(currentUser);
            }

            _logger.LogInformation("Updating Retweet for Tweet ({ID})", id.Value);
            var updatedRetweetCount = await _tweetService.UpdateRetweets(id.Value, currentUser);

            return Json(updatedRetweetCount);
        }

        [HttpGet]
        public async Task<IActionResult> GetStatus(string slug, int? tweetId)
        {
            if (string.IsNullOrEmpty(slug) || tweetId == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Getting User by slug ({SLUG})", slug);
            var profileOwner = await _userService.GetProfileOwnerWithFollowersAsync(slug);
            if (profileOwner == null)
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

            _logger.LogInformation("Getting Tweet ({ID})", tweetId.Value);
            var tweet = await _tweetService.GetTweetWithRepliesAsync(tweetId.Value);
            if (tweet == null)
            {
                _logger.LogWarning("Tweet ({ID}) NOT FOUND");
                return NotFound();
            }

            var tweetVM = _mapper.Map<TweetViewModel>(tweet);

            ViewData["CurrentUserSlug"] = currentUser.Slug;
            ViewData["IsYourself"] = profileOwner.Slug == currentUser.Slug;
            ViewData["IsFollowed"] = profileOwner.Followers
                .Any(x => x.FollowerId.Equals(currentUser.Id));

            return PartialView("_TweetPopUp", tweetVM);
        }

        [HttpPost]
        public async Task<IActionResult> Reply(string replyText, int? tweetId)
        {
            if (tweetId == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Getting Tweet ({ID})", tweetId.Value);
            var replyTo = await _tweetService.GetTweetWithRepliesForEditingAsync(tweetId.Value);
            if (replyTo == null)
            {
                _logger.LogWarning("Tweet ({ID}) NOT FOUND", tweetId.Value);
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { Message = "Requested tweet was not found. Apparently it was deleted." });
            }

            _logger.LogInformation("Getting authenticated User");
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Authenticated User NOT FOUND");
                return NotFound(user);
            }

            _logger.LogInformation("Inserting new Reply ({REPLY}) to Tweet ({ID})", replyText, replyTo.Id);
            // AddReplyAsync return created reply as a Tweet entity
            var replyFrom = await _tweetService.AddReplyAsync(replyText, user, replyTo);
            // Map the Tweet entity to the TweetViewModel
            var reply = _mapper.Map<TweetViewModel>(replyFrom);

            ViewData["CurrentUserSlug"] = user.Slug;

            return PartialView("_Tweet", reply);
        }
    }
}