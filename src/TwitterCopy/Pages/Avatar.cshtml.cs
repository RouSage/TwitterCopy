using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Threading.Tasks;

namespace TwitterCopy.Pages
{
    public class AvatarModel : PageModel
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public AvatarModel(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public class AvatarVM
        {
            public IFormFile Avatar { get; set; }
        }

        [BindProperty]
        public AvatarVM Vm { get; set; }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "images\\profile-images", Vm.Avatar.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await Vm.Avatar.CopyToAsync(stream);
            }

            return Page();
        }
    }
}