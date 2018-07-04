using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterCopy.Data;
using TwitterCopy.Models;

namespace TwitterCopy.Pages.Profiles
{
    public class IndexModel : PageModel
    {
        private readonly TwitterCopyContext _context;

        public IndexModel(TwitterCopyContext context)
        {
            _context = context;
        }

        public IList<Tweet> Tweet { get; set; }

        public async Task<ActionResult> OnGetAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return NotFound();
            }

            Tweet = await _context.Tweets
                .Where(t => t.User.UserName.Equals(userName))
                .Include(t => t.User)
                .ToListAsync();

            return Page();
        }
    }
}
