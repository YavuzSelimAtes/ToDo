namespace ToDoApi.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public int Score { get; set; }
        public int DailyTasks { get; set; }
        public int WeeklyTasks{ get; set; }
        public int MonthlyTasks { get; set; }
        public int DailyTasksCompleted { get; set; }
        public int WeeklyTasksCompleted { get; set; }
        public int MonthlyTasksCompleted { get; set; }
    }
}