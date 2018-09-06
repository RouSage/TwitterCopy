using AutoMapper;
using TwitterCopy.Core.Entities;
using TwitterCopy.Core.Entities.TweetAggregate;

namespace TwitterCopy.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Tweet, TweetViewModel>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.User.Slug))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User.Avatar));
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
