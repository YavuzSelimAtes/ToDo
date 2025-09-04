using System.ComponentModel.DataAnnotations.Schema;

namespace ToDoApi.Models
{
    [Table("ToDoItems")]
    public class ToDoItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public State State { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? RepeatDayOfWeek { get; set; }
        public int? RepeatDayOfMonth { get; set; }

        public required string Category { get; set; }
        public bool IsTemplate { get; set; } = false;
        public int? ParentTaskId { get; set; }

        public int UserId { get; set; }
        public ToDoUser User { get; set; }
    }
}
