using AutoMapper;
using System.Linq;
using TwitterCopy.Core.Entities;

namespace TwitterCopy.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Tweet, TweetViewModel>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.User.Slug))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User.Avatar))
                .ForMember(dest => dest.IsRetweet, opt => opt.UseValue(false))
                .ForMember(dest => dest.RetweetDate, opt => opt.MapFrom(src => src.PostedOn))
                .ForMember(dest => dest.RepliesFrom, opt => opt.MapFrom(src => src.RepliesFrom.Select(t => t.ReplyFrom)));
            CreateMap<Retweet, TweetViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Tweet.Id))
                .ForMember(dest => dest.LikeCount, opt => opt.MapFrom(src => src.Tweet.LikeCount))
                .ForMember(dest => dest.PostedOn, opt => opt.MapFrom(src => src.Tweet.PostedOn))
                .ForMember(dest => dest.RetweetCount, opt => opt.MapFrom(src => src.Tweet.RetweetCount))
                .ForMember(dest => dest.ReplyCount, opt => opt.MapFrom(src => src.Tweet.ReplyCount))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Tweet.Text))
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Tweet.User.Slug))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Tweet.User.UserName))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Tweet.User.Avatar))
                .ForMember(dest => dest.IsRetweet, opt => opt.UseValue(true));
            CreateMap<TwitterCopyUser, ProfileViewModel>()
                .ForMember(dest => dest.FollowersCount, opt => opt.MapFrom(src => src.Followers.Count))
                .ForMember(dest => dest.FollowingCount, opt => opt.MapFrom(src => src.Following.Count))
                .ForMember(dest => dest.TweetsCount, opt => opt.MapFrom(src => src.Tweets.Count))
                .ForMember(dest => dest.LikesCount, opt => opt.MapFrom(src => src.Likes.Count))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.JoinDate, opt => opt.MapFrom(src => src.RegisterDate.ToString("MMMM yyyy")));
            CreateMap<TwitterCopyUser, ProfileInputModel>()
                .ForMember(dest => dest.Avatar, opt => opt.Ignore())
                .ForMember(dest => dest.Banner, opt => opt.Ignore());
        }
    }
}
