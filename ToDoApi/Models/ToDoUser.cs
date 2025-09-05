using System.ComponentModel.DataAnnotations.Schema;

namespace ToDoApi.Models
{
    [Table("Users")]
    public class ToDoUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public int Score { get; set; }
        public int DailyTasks { get; set; }
        public int WeeklyTasks { get; set; }
        public int MonthlyTasks { get; set; }
        public int DailyTasksCompleted { get; set; }
        public int WeeklyTasksCompleted { get; set; }
        public int MonthlyTasksCompleted { get; set; }
        public ICollection<ToDoItem> ToDoItems { get; set; }
    }
}
