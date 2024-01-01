using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ProgrammerAl.CommentsApi.DB.Entities;

public class UserCommentsEntity
{
    [Key, Required, NotNull]
    public string? Id { get; set; }

    [Required, NotNull]
    public string? Comments { get; set; }
}
