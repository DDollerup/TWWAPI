using Microsoft.EntityFrameworkCore;

namespace TWWAPI.Models
{
    public class TWWContext : DbContext
    {
        public DbSet<About> Abouts { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Newsletter> Newsletters { get; set; }
        public DbSet<Comment> Comments { get; set; }

        public TWWContext(DbContextOptions options) : base(options) { }


    }
}
