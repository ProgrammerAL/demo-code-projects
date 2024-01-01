using Microsoft.EntityFrameworkCore;

using ProgrammerAl.CommentsApi.DB.Entities;

namespace ProgrammerAl.CommentsApi.DB.Repositories;

public interface ICommentsRepository
{
    ValueTask StoreNewCommentAsync(string comments);
}

public class CommentsRepository : ICommentsRepository
{
    private readonly CommentsDbContext _dbContext;

    public CommentsRepository(CommentsDbContext dbContextFactory)
    {
        _dbContext = dbContextFactory;
    }

    public async ValueTask StoreNewCommentAsync(string comments)
    {
        _ = await _dbContext.UserComments.AddAsync(new UserCommentsEntity
        {
            Id = Guid.NewGuid().ToString(),
            Comments = comments
        });

        await _dbContext.SaveChangesAsync();
    }
}
