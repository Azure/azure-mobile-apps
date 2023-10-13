using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Datasync.CosmosDb;

namespace api.Db;

public class TodoItem : CosmosTableData
{
    [Required, MinLength(1)]
    public string Title { get; set; } = "";

    public bool IsComplete { get; set; }
}