using Microsoft.AspNetCore.Datasync.EFCore;
using System.ComponentModel.DataAnnotations;

namespace CosmosTodoService.Db
{
    /// <summary>
    /// The fields in this class must match the fields in Models/TodoItem.cs
    /// for the TodoApp.Data project.
    /// </summary>
    public class TodoItem : ETagEntityTableData
    {
        [Required, MinLength(1)]
        public string Title { get; set; } = "";

        public bool IsComplete { get; set; }
    }
}