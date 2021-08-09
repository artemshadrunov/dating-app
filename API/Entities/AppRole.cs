using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppRole : IdentityRole<int>
    {
        public ICollection<AppUserRole> UserRoles { get; set; }
    }

    public static class RoleNames
    {
        public const string Admin = "Admin";
        public const string Moderator = "Moderator";
        public const string Member = "Member";
    }
}