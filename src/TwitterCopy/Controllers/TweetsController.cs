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
    public class TweetsController : Controller
    {
        private readonly UserManager<TwitterCopyUser> _userManager;
        private readonly ITweetService _tweetService;
        private readonly IMapper _mapper;

        public TweetsController(
            UserManager<TwitterCopyUser> userManager,
            ITweetService tweetService,
            IMapper mapper)
        {
            _userManager = userManager;
            _tweetService = tweetService;
            _mapper = mapper;
        }

        public async Task<IActionResult> UpdateLikes(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(user);
            }

            var updatedLikeCount = await _tweetService.UpdateLikes(id.Value, user);

            return Json(updatedLikeCount);
        }

        /// <summary>
        /// Provides tweet for the modal dialog
        /// </summary>
        /// <param name="id">Tweet's id</param>
        /// <returns>Tweet info in JSON</returns>
        public async Task<IActionResult> GetTweet(int? id)
        {
            var tweetToDelete = await _tweetService.GetTweetWithAuthor(id.Value);
            if (tweetToDelete == null)
            {
                return NotFound("Tweet to delete not found.");
            }

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

            var tweetVM = _mapper.Map<TweetViewModel>(tweetToDelete);

            return Json(tweetVM);
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

            var tweetToDelete = await _tweetService.GetTweetAsync(id.Value);
            if (tweetToDelete == null)
            {
                return NotFound("Tweet to delete not found.");
            }

            await _tweetService.DeleteTweet(tweetToDelete);
            // TODO: return success message as json and rewrite JS
            return View();
        }

        public async Task<IActionResult> Retweet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound(currentUser);
            }

            var updatedRetweetCount = await _tweetService.UpdateRetweets(id.Value, currentUser);

            return Json(updatedRetweetCount);
        }
    }
}