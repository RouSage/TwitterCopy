using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace TwitterCopy.Pages.Profiles
{
    public static class ManageProfileNavPages
    {
        public static string Index => "Index";

        public static string Following => "Following";

        public static string Followers => "Followers";

        public static string Likes => "Likes";

        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);

        public static string FollowingNavClass(ViewContext viewContext) => PageNavClass(viewContext, Following);

        public static string FollowersNavClass(ViewContext viewContext) => PageNavClass(viewContext, Followers);

        public static string LikesNavClass(ViewContext viewContext) => PageNavClass(viewContext, Likes);

        public static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
