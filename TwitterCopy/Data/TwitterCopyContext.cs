﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
        public DbSet<Like> Likes { get; set; }
        public DbSet<Retweet> Retweets { get; set; }

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

            builder.Entity<Like>()
                .HasOne(t => t.Tweet)
                .WithMany(l => l.Likes)
                .HasForeignKey("TweetId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Like>()
                .HasOne(u => u.User)
                .WithMany(l => l.Likes)
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Retweet>()
                .HasOne(t => t.Tweet)
                .WithMany(r => r.Retweets)
                .HasForeignKey("TweetId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Retweet>()
                .HasOne(u => u.User)
                .WithMany(r => r.Retweets)
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
