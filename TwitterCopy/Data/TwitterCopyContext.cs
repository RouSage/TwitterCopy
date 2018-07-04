using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using TwitterCopy.Models;

namespace TwitterCopy.Data
{
    public class TwitterCopyContext : IdentityDbContext<TwitterCopyUser, TwitterCopyRole, Guid>
    {
        public TwitterCopyContext(DbContextOptions<TwitterCopyContext> options)
            : base(options)
        {
        }

        public DbSet<Tweet> Tweets { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.Entity<Tweet>().ToTable("Tweet");

            builder.Entity<Tweet>()
                .HasOne(x => x.User)
                .WithMany(t => t.Tweets)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserToUser>()
                .HasKey(k => new { k.UserId, k.FollowerId });

            builder.Entity<UserToUser>()
                .HasOne(l => l.User)
                .WithMany(a => a.Followers)
                .HasForeignKey(l => l.UserId);

            builder.Entity<UserToUser>()
                .HasOne(l => l.Follower)
                .WithMany(a => a.Following)
                .HasForeignKey(l => l.FollowerId);
        }
    }
}
