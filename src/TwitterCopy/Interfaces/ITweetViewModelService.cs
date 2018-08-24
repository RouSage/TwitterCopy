using System.Collections.Generic;
using System.Threading.Tasks;
using TwitterCopy.Models;

namespace TwitterCopy.Interfaces
{
    public interface ITweetViewModelService
    {
        Task<IEnumerable<TweetViewModel>> GetFeedForUser(string userId);
    }
}
