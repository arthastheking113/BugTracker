using BugTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BugTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<CustomUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Project> Project { get; set; }
        public DbSet<Ticket> Ticket { get; set; }
        public DbSet<TicketAttachment> Attachment { get; set; }
        public DbSet<Comment> Comment { get; set; }
        public DbSet<Invite> Invite { get; set; }
        public DbSet<Priority> Priority { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<Status> Status { get; set; }
        public DbSet<TicketHistory> TicketHistory { get; set; }
        public DbSet<TicketType> TicketType { get; set; }
        public DbSet<Company> Company { get; set; }
        public DbSet<BugTracker.Models.Inbox> Inbox { get; set; }
        public DbSet<BugTracker.Models.ProjectAttachment> ProjectAttachment { get; set; }
        public DbSet<BugTracker.Models.Reply> Reply { get; set; }
        public DbSet<BugTracker.Models.InboxNotification> InboxNotification { get; set; }
        public DbSet<BugTracker.Models.WelcomeNotification> WelcomeNotification { get; set; }
    }
}
