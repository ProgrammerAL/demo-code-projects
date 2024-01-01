using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackFunctionsApp;

public class ServiceConfig
{
    [Required, NotNull]
    public string? Environment { get; set; }

    [Required, NotNull]
    public string? Version { get; set; }
}

public class StorageConfig
{
    [Required, NotNull]
    public string? Endpoint { get; set; }

    [Required, NotNull]
    public string? TableName { get; set; }
}
