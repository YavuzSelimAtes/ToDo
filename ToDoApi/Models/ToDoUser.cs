using System.ComponentModel.DataAnnotations.Schema;

namespace ToDoApi.Models
{
    [Table("Users")]
    public class ToDoUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }

        public ICollection<ToDoItem> ToDoItems { get; set; } = new List<ToDoItem>();
    }
}
