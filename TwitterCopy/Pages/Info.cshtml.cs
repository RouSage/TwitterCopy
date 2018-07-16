using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TwitterCopy.Pages
{
    public class InfoModel : PageModel
    {
        [TempData]
        public string Message { get; set; }

        public void OnGet()
        {

        }
    }
}