using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace TwitterCopy.Areas.Identity
{
    // Add profile data for application users by adding properties to the TwitterCopyUser class
    public class TwitterCopyUser : IdentityUser<Guid>
    {
    }
}
