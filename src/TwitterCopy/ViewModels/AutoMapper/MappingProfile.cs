using AutoMapper;
using System.Linq;
using TwitterCopy.Core.Entities;
using TwitterCopy.ViewModels.AutoMapper;

namespace TwitterCopy.Models.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Tweet, TweetViewModel>()
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User.Avatar))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.User.Slug))
                .ForMember(dest => dest.LikeCount, opt => opt.MapFrom(src => src.Likes.Count))
                .ForMember(dest => dest.RetweetCount, opt => opt.MapFrom(src => src.Retweets.Count))
                .ForMember(dest => dest.IsRetweet, opt => opt.UseValue(false))
                .ForMember(dest => dest.RetweetDate, opt => opt.MapFrom(src => src.PostedOn))
                .ForMember(dest => dest.RepliesFrom, opt => opt.MapFrom(src => src.RepliesFrom.Select(t => t.ReplyFrom)))
                .ForMember(dest => dest.RepliesTo, opt => opt.MapFrom(src => src.RepliesTo.Select(t => t.ReplyTo)))
                .ForMember(dest => dest.ReplyCount, opt => opt.MapFrom(src => src.RepliesFrom.Count));
            CreateMap<Retweet, TweetViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TweetId))
                .ForMember(dest => dest.LikeCount, opt => opt.MapFrom(src => src.Tweet.Likes.Count))
                .ForMember(dest => dest.PostedOn, opt => opt.MapFrom(src => src.Tweet.PostedOn))
                .ForMember(dest => dest.RetweetCount, opt => opt.MapFrom(src => src.Tweet.Retweets.Count))
                .ForMember(dest => dest.ReplyCount, opt => opt.MapFrom(src => src.Tweet.RepliesFrom.Count))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Tweet.Text))
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Tweet.User.Slug))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Tweet.User.UserName))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Tweet.User.Avatar))
                .ForMember(dest => dest.IsRetweet, opt => opt.UseValue(true))
                .ForMember(dest => dest.RetweetUserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.RetweetSlug, opt => opt.MapFrom(src => src.User.Slug));
            CreateMap<TwitterCopyUser, ProfileViewModel>()
                .ForMember(dest => dest.FollowersCount, opt => opt.MapFrom(src => src.Followers.Count))
                .ForMember(dest => dest.FollowingCount, opt => opt.MapFrom(src => src.Following.Count))
                .ForMember(dest => dest.TweetCount, opt => opt.ResolveUsing<TweetCountResolver>())
                .ForMember(dest => dest.LikesCount, opt => opt.MapFrom(src => src.Likes.Count))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.JoinDate, opt => opt.MapFrom(src => src.RegisterDate.ToString("MMMM yyyy")));
            CreateMap<TwitterCopyUser, ProfileInputModel>()
                .ForMember(dest => dest.Avatar, opt => opt.Ignore())
                .ForMember(dest => dest.Banner, opt => opt.Ignore());
        }
    }
}
