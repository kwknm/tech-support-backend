using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechSupport.Database.Entities;

namespace TechSupport.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<User>().Property(x=> x.FirstName).IsRequired();
        builder.Entity<User>().Property(x=> x.LastName).IsRequired();
    }
    
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<IssueType> IssueTypes { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Metadata> Metadatas { get; set; }
    public DbSet<Faq> Faqs { get; set; }
};