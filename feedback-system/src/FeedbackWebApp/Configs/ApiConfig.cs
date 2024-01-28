using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FeedbackWebApp.Configs;

public class ApiConfig
{
    [Required(AllowEmptyStrings = false), NotNull]
    public string? FeedbackEndpoint { get; set; }
}
