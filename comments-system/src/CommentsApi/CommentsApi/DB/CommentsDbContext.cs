using Microsoft.EntityFrameworkCore;

using ProgrammerAl.CommentsApi.DB.Entities;

namespace ProgrammerAl.CommentsApi.DB;

public class CommentsDbContext : DbContext
{
    public CommentsDbContext(DbContextOptions<CommentsDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserCommentsEntity> UserComments { get; set; }
}
