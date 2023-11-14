using Microsoft.EntityFrameworkCore;
using AoS.Models;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;

public class AoSDbContext : DbContext
{
    public DbSet<Activity> Activites { get; set; }
    public DbSet<Memory> Memories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<User> Users { get; set; }

    public AoSDbContext(DbContextOptions<AoSDbContext> context) : base(context)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Activity>().HasData(new Activity[]
        {
            new Activity {
                ActivityId = 1,
                UserId = 1,
                Description = "Tell each other 5 things you like about them while giving them a big hug.",
                IsUsed = false
            },
        });

        modelBuilder.Entity<Memory>().HasData(new Memory[]
        {
            new Memory {
                MemoryId = 1,
                UserId = 1,
                ActivityId = 1,
                Description = "I told my partner that I liked their smile, their laugh, the way they take care of other people was really special, I like how patient they are with me, and that I think we have really great communication that we've put effort into.",
                Date = new DateTime()
            },
        });

        modelBuilder.Entity<Tag>().HasData(new Tag[]
        {
            new Tag { TagId = 1, Label = "Acts of Service"},
            new Tag { TagId = 2, Label = "Quality Time"},
            new Tag { TagId = 3, Label = "Words of Affirmation"},
            new Tag { TagId = 4, Label = "Physical Touch"},
            new Tag { TagId = 5, Label = "Receiving Gifts"}
        });

        modelBuilder.Entity<User>().HasData(new User[]
        {
            new User {
                UserId = 1,
                UID = "user1",
                FirstName = "James",
                LastName = "Johnson",
                Email = "jjohnsonson@gmail.com",
                ImageUrl = "https://rugby.vlaanderen/wp-content/uploads/2018/03/Anonymous-Profile-pic.jpg"
            },
        });
    }
}