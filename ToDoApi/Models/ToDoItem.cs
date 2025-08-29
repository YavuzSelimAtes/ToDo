using System.ComponentModel.DataAnnotations.Schema;

namespace ToDoApi.Models
{
    [Table("ToDoItems")]
    public class ToDoItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsCompleted { get; set; }


        public required string Category { get; set; }

        public int UserId { get; set; }
        public ToDoUser User { get; set; }
    }
}
