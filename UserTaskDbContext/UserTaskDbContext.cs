using Microsoft.EntityFrameworkCore;
using DataBalkTaskLisit.Entities;
namespace DataBalkTaskLisit.UserTaskDbContext
{
    public class UserTaskDbContext : DbContext
    {
        //private readonly IConfiguration _configuration;
        public UserTaskDbContext(DbContextOptions<UserTaskDbContext> options) : base(options)
        {
           
        }
        public DbSet<User> Users => Set<User>();
        public DbSet<TasksList> Tasks => Set<TasksList>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.Tasks)
                .WithOne(t => t.user)
                .HasForeignKey(t => t.UserId);
        }
    }
}
