using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterCopy.Core.Entities.TweetAggregate;
using TwitterCopy.Core.Interfaces;
using TwitterCopy.Interfaces;
using TwitterCopy.Models;

namespace TwitterCopy.Services
{
    public class TweetViewModelService : ITweetViewModelService
    {
        private readonly ITweetRepository _tweetRepository;

        public TweetViewModelService(ITweetRepository tweetRepository)
        {
            _tweetRepository = tweetRepository;
        }

        public async Task<IEnumerable<TweetViewModel>> GetFeedForUser(string userId)
        {
            var tweets = await _tweetRepository.GetTweetsByUserIdAsync(userId);

            return CreateViewModelFromTweets(tweets);
        }

        private IEnumerable<TweetViewModel> CreateViewModelFromTweets(IEnumerable<Tweet> tweets)
        {
            return tweets.Select(x => new TweetViewModel
            {
                AuthorAvatar = x.User.Avatar,
                AuthorName = x.User.UserName,
                AuthorSlug = x.User.Slug,
                Id = x.Id,
                LikeCount = x.LikeCount,
                PostedOn = x.PostedOn,
                RetweetCount = x.RetweetCount,
                Text = x.Text
            });
        }
    }
}
