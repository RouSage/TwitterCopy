using AutoMapper;
using TwitterCopy.Core.Entities;
using TwitterCopy.Models;

namespace TwitterCopy.ViewModels.AutoMapper
{
    public class TweetCountResolver : IValueResolver<TwitterCopyUser, ProfileViewModel, int>
    {
        public int Resolve(TwitterCopyUser source, ProfileViewModel destination, int destMember, ResolutionContext context)
        {
            return source.Tweets.Count + source.Retweets.Count;
        }
    }
}
