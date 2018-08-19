using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TwitterCopy.Models
{
    public class ProfileInputModel
    {
        [Required(ErrorMessage = "Name can't be blank.")]
        [StringLength(50, MinimumLength = 2)]
        public string UserName { get; set; }

        [StringLength(1000)]
        public string Bio { get; set; }

        public string Location { get; set; }

        [Url(ErrorMessage = "Url is not valid.")]
        [DataType(DataType.Url)]
        public string Website { get; set; }

        public IFormFile Avatar { get; set; }
    }
}
