using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Datasync.EFCore;

namespace api.Db;

public class TodoItem : ETagEntityTableData
{
    [Required, MinLength(1)]
    public string Title { get; set; } = "";

    public bool IsComplete { get; set; }
}